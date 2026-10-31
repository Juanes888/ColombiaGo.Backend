using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Common;
using ColombiaGo.Infrastructure.Repositories;
using ColombiaGo.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Services;

public class RutaServiceTests
{
    [Fact]
    public async Task AgregarParadaAsync_CalledTwiceForSamePlace_DoesNotDuplicate()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = new RutaService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id), TestServices.Localizador(context));

        await service.AgregarParadaAsync("comuna-13");
        var result = await service.AgregarParadaAsync("comuna-13");

        result.Value.Paradas.Should().ContainSingle();
    }

    [Fact]
    public async Task GetMisParadasAsync_NoDraftYet_ReturnsEmptyRutaWithoutCreatingARow()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);

        var service = new RutaService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id), TestServices.Localizador(context));

        var result = await service.GetMisParadasAsync();

        result.Paradas.Should().BeEmpty();
        context.Rutas.Should().BeEmpty();
    }

    [Fact]
    public async Task QuitarParadaAsync_LastStopRemoved_DeletesTheDraftRuta()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = new RutaService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id), TestServices.Localizador(context));
        await service.AgregarParadaAsync("comuna-13");

        var result = await service.QuitarParadaAsync("comuna-13");

        result.Value.Paradas.Should().BeEmpty();
        context.Rutas.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerarAsync_WithNoStops_ReturnsValidationError()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);

        var service = new RutaService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id), TestServices.Localizador(context));

        var result = await service.GenerarAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task GenerarAsync_StampsGenerationTimeAndEstimatedDuration()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13", latitud: 6.0m, longitud: -75.0m);
        context.AddLugar(categoria, "guatape", latitud: 6.23m, longitud: -75.16m);

        var service = new RutaService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id), TestServices.Localizador(context));
        await service.AgregarParadaAsync("comuna-13");
        await service.AgregarParadaAsync("guatape");

        var result = await service.GenerarAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Generated.Should().BeTrue();
        result.Value.FechaGeneracion.Should().NotBeNull();
        result.Value.TiempoEstimadoMin.Should().Be(90); // 2 stops * 45 min heuristic
        result.Value.Paradas.Should().HaveCount(2);
    }

    [Fact]
    public async Task AgregarParadaAsync_AfterGenerar_StartsANewDraft()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");
        context.AddLugar(categoria, "guatape");

        var service = new RutaService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id), TestServices.Localizador(context));
        await service.AgregarParadaAsync("comuna-13");
        await service.GenerarAsync();

        var afterGenerate = await service.GetMisParadasAsync();
        await service.AgregarParadaAsync("guatape");
        var newDraft = await service.GetMisParadasAsync();

        afterGenerate.Paradas.Should().BeEmpty(); // the generated route is closed, not the draft
        newDraft.Generated.Should().BeFalse();
        newDraft.Paradas.Should().ContainSingle(l => l.Id == "guatape");
    }
}
