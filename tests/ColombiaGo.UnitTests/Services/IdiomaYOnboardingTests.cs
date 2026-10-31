using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Enums;
using ColombiaGo.Infrastructure.Repositories;
using ColombiaGo.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Services;

public class IdiomaYOnboardingTests
{
    // ---- Idiomas / formato ------------------------------------------------------------------

    [Theory]
    [InlineData("zh-CN", "zh")]
    [InlineData("PT-br", "pt")]
    [InlineData("en_US", "en")]
    [InlineData("fr", "fr")]
    [InlineData("de", "es")]
    [InlineData("", "es")]
    [InlineData(null, "es")]
    public void Normalizar_DevuelveUnIdiomaSoportado(string? entrada, string esperado) =>
        Idiomas.Normalizar(entrada).Should().Be(esperado);

    [Theory]
    [InlineData("es", "Gratis")]
    [InlineData("en", "Free")]
    [InlineData("pt", "Grátis")]
    [InlineData("zh", "免费")]
    [InlineData("fr", "Gratuit")]
    public void ToPriceLabel_Gratis_SeTraduce(string idioma, string esperado) =>
        DisplayFormatting.ToPriceLabel(RangoPrecio.Gratis, null, idioma).Should().Be(esperado);

    [Theory]
    [InlineData("es", "Desde $15.000")]
    [InlineData("en", "From $15.000")]
    [InlineData("pt", "A partir de $15.000")]
    [InlineData("zh", "$15.000 起")]
    [InlineData("fr", "À partir de $15.000")]
    public void ToPriceLabel_ConCosto_MantienePesosColombianos(string idioma, string esperado) =>
        DisplayFormatting.ToPriceLabel(RangoPrecio.Economico, 15000, idioma).Should().Be(esperado);

    [Theory]
    [InlineData("es", "2k reseñas")]
    [InlineData("en", "2k reviews")]
    [InlineData("pt", "2k avaliações")]
    [InlineData("zh", "2k 条评价")]
    [InlineData("fr", "2k avis")]
    public void ToReviewsLabel_SeTraduce(string idioma, string esperado) =>
        DisplayFormatting.ToReviewsLabel(2000, idioma).Should().Be(esperado);

    // ---- Localizador de lugares --------------------------------------------------------------

    private const string TraduccionEn = """
        {"traducciones":[{"slug":"comuna-13","nombre":"Comuna 13: Art and Transformation",
          "descripcion":"Discover the story of resilience.","eslogan":"Art, history and transformation",
          "comoLlegar":"Metro + Metrocable","horario":"8am - 6pm"}]}
        """;

    [Fact]
    public async Task Localizador_EnIngles_TraduceGuardaEnCacheYNoVuelveALlamarALaIa()
    {
        using var context = InMemoryDbContextFactory.Create();
        var categoria = context.AddCategoria();
        var lugar = context.AddLugar(categoria, "comuna-13", "Comuna 13: Arte y Transformación");
        var gemini = new FakeGeminiClient(true, TraduccionEn);
        var localizador = TestServices.Localizador(context, "en", gemini);

        var primera = await localizador.ResumenAsync([lugar]);
        var segunda = await localizador.ResumenAsync([lugar]);

        primera.Single().Name.Should().Be("Comuna 13: Art and Transformation");
        segunda.Single().Name.Should().Be("Comuna 13: Art and Transformation");
        gemini.Prompts.Should().HaveCount(1);
        context.LugarTraducciones.Should().ContainSingle(t => t.Idioma == "en");
    }

    [Fact]
    public async Task Localizador_SiLaIaFalla_CaeAlEspanolSinRomperse()
    {
        using var context = InMemoryDbContextFactory.Create();
        var categoria = context.AddCategoria();
        var lugar = context.AddLugar(categoria, "comuna-13", "Comuna 13: Arte y Transformación");
        var localizador = TestServices.Localizador(context, "zh", new FakeGeminiClient(true, "no es json"));

        var resultado = await localizador.ResumenAsync([lugar]);

        resultado.Single().Name.Should().Be("Comuna 13: Arte y Transformación");
        context.LugarTraducciones.Should().BeEmpty();
    }

    [Fact]
    public async Task Localizador_EnEspanol_NuncaLlamaALaIa()
    {
        using var context = InMemoryDbContextFactory.Create();
        var categoria = context.AddCategoria();
        var lugar = context.AddLugar(categoria, "comuna-13");
        var gemini = new FakeGeminiClient(true, TraduccionEn);

        await TestServices.Localizador(context, "es", gemini).ResumenAsync([lugar]);

        gemini.Prompts.Should().BeEmpty();
    }

    [Fact]
    public async Task Localizador_EnIngles_ElPrecioYLasResenasSalenEnIngles()
    {
        using var context = InMemoryDbContextFactory.Create();
        var categoria = context.AddCategoria();
        var lugar = context.AddLugar(categoria, "comuna-13");
        lugar.TotalCalificaciones = 2000;
        var localizador = TestServices.Localizador(context, "en");

        var detalle = await localizador.DetalleAsync(lugar, false, false);

        detalle.Price.Should().Be("Free");
        detalle.ReviewsLabel.Should().Be("2k reviews");
    }

    // ---- Configuración y onboarding -----------------------------------------------------------

    private static ConfiguracionService CrearConfiguracion(
        Infrastructure.Persistence.ColombiaGoDbContext context, Guid userId, string idioma = "es") =>
        new(new UnitOfWork(context), new FakeCurrentUser(userId), new FakeIdiomaActual(idioma));

    [Fact]
    public async Task Configuracion_LaPrimeraVez_NaceConElIdiomaDeLaPeticionYSinOnboarding()
    {
        using var context = InMemoryDbContextFactory.Create();
        var usuario = context.AddUsuario(context.AddRol());

        var config = await CrearConfiguracion(context, usuario.Id, "pt").GetAsync();

        config.Language.Should().Be("pt");
        config.OnboardingCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Configuracion_Actualizar_CambiaIdiomaYMantieneElOnboardingSiNoSeEnvia()
    {
        using var context = InMemoryDbContextFactory.Create();
        var usuario = context.AddUsuario(context.AddRol());
        var servicio = CrearConfiguracion(context, usuario.Id);
        await servicio.CompletarOnboardingAsync();

        var config = await servicio.ActualizarAsync(new ActualizarConfiguracionRequest(true, false, true, "zh-CN"));

        config.Language.Should().Be("zh");
        config.OnboardingCompleted.Should().BeTrue();
        config.Location.Should().BeFalse();
    }

    [Fact]
    public async Task Configuracion_ClienteViejoSinIdioma_NoPisaElIdiomaGuardado()
    {
        using var context = InMemoryDbContextFactory.Create();
        var usuario = context.AddUsuario(context.AddRol());
        var servicio = CrearConfiguracion(context, usuario.Id);
        await servicio.ActualizarAsync(new ActualizarConfiguracionRequest(true, true, false, "fr"));

        var config = await servicio.ActualizarAsync(new ActualizarConfiguracionRequest(false, true, false));

        config.Language.Should().Be("fr");
    }

    // ---- Preferencias ---------------------------------------------------------------------------

    [Fact]
    public async Task Preferencias_Guardar_ReemplazaLasAnteriores()
    {
        using var context = InMemoryDbContextFactory.Create();
        var usuario = context.AddUsuario(context.AddRol());
        context.AddCategoria("food", "Gastronomía");
        context.AddCategoria("culture", "Cultura");
        context.AddCategoria("nature", "Naturaleza");
        var servicio = new PerfilService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id));

        await servicio.GuardarPreferenciasAsync(new GuardarPreferenciasRequest(["food", "culture"]));
        var resultado = await servicio.GuardarPreferenciasAsync(new GuardarPreferenciasRequest(["nature"]));
        var guardadas = await servicio.GetPreferenciasAsync();

        resultado.IsSuccess.Should().BeTrue();
        guardadas.Categorias.Should().Equal("nature");
    }

    [Fact]
    public async Task Preferencias_ConCategoriaInexistente_Falla()
    {
        using var context = InMemoryDbContextFactory.Create();
        var usuario = context.AddUsuario(context.AddRol());
        context.AddCategoria("food", "Gastronomía");
        var servicio = new PerfilService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id));

        var resultado = await servicio.GuardarPreferenciasAsync(new GuardarPreferenciasRequest(["food", "marte"]));

        resultado.IsFailure.Should().BeTrue();
    }
}
