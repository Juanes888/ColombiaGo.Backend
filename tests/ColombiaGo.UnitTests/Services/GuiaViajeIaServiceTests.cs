using ColombiaGo.Application.DTOs.GuiaViaje;
using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using ColombiaGo.Infrastructure.Repositories;
using ColombiaGo.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Services;

public class GuiaViajeIaServiceTests
{
    private const string RespuestaValida = """
        {"titulo":"Medellín en 2 días","resumen":"Arte y naturaleza.",
         "dias":[
           {"dia":1,"tema":"Centro","paradas":[
             {"slug":"comuna-13","hora":"09:00","duracionMin":120,"costoAprox":30000,"consejo":"Lleva efectivo."},
             {"slug":"inventado","hora":"12:00","duracionMin":60,"costoAprox":99999,"consejo":"No existe."}]},
           {"dia":2,"tema":"Repetido","paradas":[
             {"slug":"comuna-13","hora":"09:00","duracionMin":60,"costoAprox":10000,"consejo":"Repetida."}]},
           {"dia":3,"tema":"Verde","paradas":[
             {"slug":"parque-arvi","hora":"10:00","duracionMin":180,"costoAprox":45000,"consejo":"Lleva chaqueta."}]}],
         "consejosGenerales":["Hidrátate"],"queLlevar":["Protector solar"]}
        """;

    private static GenerarGuiaViajeRequest Pedido(int dias = 3, decimal presupuesto = 500_000) =>
        new("Medellín", dias, presupuesto, ["culture"]);

    private static (ColombiaGoDbContext Context, Guid UserId) Preparar()
    {
        var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var cultura = context.AddCategoria("culture", "Cultura");
        var naturaleza = context.AddCategoria("nature", "Naturaleza");
        context.AddLugar(cultura, "comuna-13", "Comuna 13");
        context.AddLugar(naturaleza, "parque-arvi", "Parque Arví");
        return (context, usuario.Id);
    }

    private static GuiaViajeIaService Crear(ColombiaGoDbContext context, Guid userId, FakeGeminiClient gemini, string idioma = "es") =>
        new(new UnitOfWork(context), new FakeCurrentUser(userId), gemini,
            new FakeIdiomaActual(idioma), TestServices.Localizador(context, idioma, gemini));

    [Fact]
    public async Task GenerarAsync_SinApiKey_FallaConError()
    {
        var (context, userId) = Preparar();
        using var _ = context;

        var result = await Crear(context, userId, new FakeGeminiClient(configurado: false)).GenerarAsync(Pedido());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GuiaIa.SinApiKey");
    }

    [Fact]
    public async Task GenerarAsync_CiudadSinLugares_DevuelveValidacion()
    {
        var (context, userId) = Preparar();
        using var _ = context;

        var result = await Crear(context, userId, new FakeGeminiClient(true, RespuestaValida))
            .GenerarAsync(new GenerarGuiaViajeRequest("Atlantis", 2, 100_000, null));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task GenerarAsync_DescartaSlugsInventadosYRepetidos_YRecalculaElCosto()
    {
        var (context, userId) = Preparar();
        using var _ = context;

        var result = await Crear(context, userId, new FakeGeminiClient(true, RespuestaValida)).GenerarAsync(Pedido());

        result.IsSuccess.Should().BeTrue();
        var guia = result.Value;
        guia.PlanDiario.Should().HaveCount(2); // el día "Repetido" quedó vacío y se descartó
        guia.PlanDiario.Select(d => d.Dia).Should().Equal(1, 2); // renumerados
        guia.PlanDiario.SelectMany(d => d.Paradas).Select(p => p.Lugar.Id)
            .Should().Equal("comuna-13", "parque-arvi");
        guia.CostoTotalEstimado.Should().Be(75_000); // 30.000 + 45.000, no se confía en la IA
        guia.PresupuestoRestante.Should().Be(425_000);
        guia.ExcedePresupuesto.Should().BeFalse();
        context.GuiasViaje.Should().ContainSingle();
    }

    [Fact]
    public async Task GenerarAsync_PresupuestoMenorAlCosto_MarcaExcedePresupuesto()
    {
        var (context, userId) = Preparar();
        using var _ = context;

        var result = await Crear(context, userId, new FakeGeminiClient(true, RespuestaValida)).GenerarAsync(Pedido(presupuesto: 50_000));

        result.Value.ExcedePresupuesto.Should().BeTrue();
        result.Value.PresupuestoRestante.Should().Be(-25_000);
    }

    [Fact]
    public async Task GenerarAsync_RespuestaQueNoEsJson_DevuelveRespuestaInvalida()
    {
        var (context, userId) = Preparar();
        using var _ = context;

        var result = await Crear(context, userId, new FakeGeminiClient(true, "esto no es json")).GenerarAsync(Pedido());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GuiaIa.RespuestaInvalida");
        context.GuiasViaje.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerarAsync_PidiendoEnIngles_PideALaIaResponderEnIngles()
    {
        var (context, userId) = Preparar();
        using var _ = context;
        var gemini = new FakeGeminiClient(true, RespuestaValida);

        await Crear(context, userId, gemini, idioma: "en").GenerarAsync(Pedido());

        gemini.Prompts.Should().Contain(p => p.Contains("Write EVERY text value in English"));
    }

    [Fact]
    public async Task GenerarAsync_ConDiezGuiasEnLaUltimaHora_DevuelveLimite()
    {
        var (context, userId) = Preparar();
        using var _ = context;
        for (var i = 0; i < 10; i++)
            context.GuiasViaje.Add(new GuiaViaje
            {
                UsuarioId = userId, Ciudad = "Medellín", Dias = 1, Presupuesto = 1, Contenido = "{}",
                FechaCreacion = DateTimeOffset.UtcNow.AddMinutes(-5),
            });
        context.SaveChanges();

        var result = await Crear(context, userId, new FakeGeminiClient(true, RespuestaValida)).GenerarAsync(Pedido());

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.TooManyRequests);
    }

    [Fact]
    public async Task ObtenerAsync_GuiaDeOtraPersona_SeReportaComoInexistente()
    {
        var (context, userId) = Preparar();
        using var _ = context;
        var generada = await Crear(context, userId, new FakeGeminiClient(true, RespuestaValida)).GenerarAsync(Pedido());
        var otroUsuario = context.AddUsuario(context.Roles.First());

        var result = await Crear(context, otroUsuario.Id, new FakeGeminiClient(true)).ObtenerAsync(generada.Value.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ListarAsync_DevuelveSoloLasGuiasDeLaPersona()
    {
        var (context, userId) = Preparar();
        using var _ = context;
        await Crear(context, userId, new FakeGeminiClient(true, RespuestaValida)).GenerarAsync(Pedido());
        var otroUsuario = context.AddUsuario(context.Roles.First());

        var mias = await Crear(context, userId, new FakeGeminiClient(true)).ListarAsync();
        var ajenas = await Crear(context, otroUsuario.Id, new FakeGeminiClient(true)).ListarAsync();

        mias.Should().ContainSingle().Which.Titulo.Should().Be("Medellín en 2 días");
        ajenas.Should().BeEmpty();
    }
}
