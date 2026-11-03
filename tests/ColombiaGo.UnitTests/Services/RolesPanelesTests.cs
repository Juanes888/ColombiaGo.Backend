using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Application.DTOs.Negocio;
using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;
using ColombiaGo.Infrastructure.Repositories;
using ColombiaGo.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Services;

/// <summary>Paneles por tipo de cuenta: el negocio publica lugares que esperan moderación y el guía arma su perfil.</summary>
public class RolesPanelesTests
{
    private sealed class FakeGeocoder : IGeocoder
    {
        private readonly (decimal, decimal)? _resultado;

        public FakeGeocoder((decimal, decimal)? resultado) => _resultado = resultado;

        public int Llamadas { get; private set; }

        public Task<(decimal Latitud, decimal Longitud)?> BuscarAsync(
            string direccion, string ciudad, string region, CancellationToken ct = default)
        {
            Llamadas++;
            return Task.FromResult(_resultado);
        }
    }

    private static GuardarLugarRequest Peticion(string nombre = "Café Pergamino", string direccion = "Cra 37 #8A-37") => new(
        Name: nombre,
        Category: "food",
        City: "Medellín",
        Region: "Antioquia",
        Address: direccion,
        Description: "Café de especialidad en El Poblado, con granos de fincas antioqueñas.",
        Tagline: "Café de origen",
        Hours: "8am - 8pm",
        HowToGet: "Metro Poblado",
        PriceRange: RangoPrecio.Economico,
        EntryCost: null,
        ImageUrl: null,
        Latitude: null,
        Longitude: null);

    [Fact]
    public async Task Crear_QuedaPendiente_ConDuenoYCoordenadasDelGeocodificador()
    {
        using var context = InMemoryDbContextFactory.Create();
        var negocio = context.AddUsuario(context.AddRol("negocio"));
        context.AddCategoria("food", "Gastronomía");
        var service = new NegocioService(new UnitOfWork(context), new FakeCurrentUser(negocio.Id, "negocio"), new FakeGeocoder((6.21m, -75.57m)));

        var result = await service.CrearAsync(Peticion());

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("pendiente");
        result.Value.Slug.Should().Be("cafe-pergamino-medellin");
        result.Value.Latitude.Should().Be(6.21m);
        context.Lugares.Single().RegistradoPor.Should().Be(negocio.Id);
    }

    [Fact]
    public async Task Crear_DosConElMismoNombre_GeneraSlugsDistintos()
    {
        using var context = InMemoryDbContextFactory.Create();
        var negocio = context.AddUsuario(context.AddRol("negocio"));
        context.AddCategoria("food", "Gastronomía");
        var service = new NegocioService(new UnitOfWork(context), new FakeCurrentUser(negocio.Id, "negocio"), new FakeGeocoder((6.21m, -75.57m)));

        await service.CrearAsync(Peticion());
        var segundo = await service.CrearAsync(Peticion());

        segundo.Value.Slug.Should().Be("cafe-pergamino-medellin-2");
    }

    [Fact]
    public async Task Crear_DireccionQueNoExiste_DevuelveErrorDeValidacion()
    {
        using var context = InMemoryDbContextFactory.Create();
        var negocio = context.AddUsuario(context.AddRol("negocio"));
        context.AddCategoria("food", "Gastronomía");
        var service = new NegocioService(new UnitOfWork(context), new FakeCurrentUser(negocio.Id, "negocio"), new FakeGeocoder(null));

        var result = await service.CrearAsync(Peticion());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lugar.DireccionNoEncontrada");
        context.Lugares.Should().BeEmpty();
    }

    [Fact]
    public async Task Actualizar_LugarDeOtroNegocio_EsProhibido()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol("negocio");
        var dueno = context.AddUsuario(rol);
        var otro = context.AddUsuario(rol);
        context.AddCategoria("food", "Gastronomía");
        var geocoder = new FakeGeocoder((6.21m, -75.57m));
        var creado = await new NegocioService(new UnitOfWork(context), new FakeCurrentUser(dueno.Id, "negocio"), geocoder)
            .CrearAsync(Peticion());

        var result = await new NegocioService(new UnitOfWork(context), new FakeCurrentUser(otro.Id, "negocio"), geocoder)
            .ActualizarAsync(creado.Value.Id, Peticion("Otro nombre"));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Actualizar_LugarAprobado_VuelveAPendienteSinGeocodificarSiLaDireccionNoCambia()
    {
        using var context = InMemoryDbContextFactory.Create();
        var negocio = context.AddUsuario(context.AddRol("negocio"));
        context.AddCategoria("food", "Gastronomía");
        var geocoder = new FakeGeocoder((6.21m, -75.57m));
        var service = new NegocioService(new UnitOfWork(context), new FakeCurrentUser(negocio.Id, "negocio"), geocoder);
        var creado = await service.CrearAsync(Peticion());
        var lugar = context.Lugares.Single();
        lugar.EstadoPublicacion = EstadoPublicacion.Aprobado;
        context.SaveChanges();

        var result = await service.ActualizarAsync(creado.Value.Id, Peticion("Café Pergamino Poblado"));

        result.Value.Status.Should().Be("pendiente");
        result.Value.Name.Should().Be("Café Pergamino Poblado");
        geocoder.Llamadas.Should().Be(1); // solo al crear
    }

    [Fact]
    public async Task GuiaGuardaSuPerfil_QuedaSinVerificar_YLaSegundaVezActualiza()
    {
        using var context = InMemoryDbContextFactory.Create();
        var guia = context.AddUsuario(context.AddRol("guia"));
        var service = new GuiaService(new UnitOfWork(context), new FakeIdiomaActual(), new FakeCurrentUser(guia.Id, "guia"));
        var peticion = new GuardarGuiaRequest("Laura", "Senderismo", "Medellín", 55000m, ["es", "en", "ES"], "Guía de montaña.", 4, null);

        var creado = await service.GuardarMiPerfilAsync(peticion);
        var editado = await service.GuardarMiPerfilAsync(peticion with { HourlyRate = 60000m });

        creado.Value.Verified.Should().BeFalse();
        creado.Value.Languages.Should().Equal("ES", "EN");
        editado.Value.Id.Should().Be(creado.Value.Id);
        context.Guias.Should().HaveCount(1);
        context.Guias.Single().TarifaHora.Should().Be(60000m);
    }

    [Theory]
    [InlineData("Café Pergamino, Medellín", "cafe-pergamino-medellin")]
    [InlineData("  ¡Bar   Ñandú!  ", "bar-nandu")]
    public void Slugify_QuitaTildesYSimbolos(string entrada, string esperado) =>
        NegocioService.Slugify(entrada).Should().Be(esperado);
}
