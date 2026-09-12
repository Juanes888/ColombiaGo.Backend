using ColombiaGo.Application.Services;
using ColombiaGo.Infrastructure.Repositories;
using ColombiaGo.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Services;

public class FavoritoServiceTests
{
    [Fact]
    public async Task AgregarLugarAsync_CalledTwice_IsIdempotent()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = new FavoritoService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id));

        var first = await service.AgregarLugarAsync("comuna-13");
        var second = await service.AgregarLugarAsync("comuna-13");

        first.Value.IsFavorite.Should().BeTrue();
        second.Value.IsFavorite.Should().BeTrue();
        context.FavoritosLugares.Count(f => f.UsuarioId == usuario.Id).Should().Be(1);
    }

    [Fact]
    public async Task QuitarLugarAsync_WhenNotFavorited_IsANoOpSuccess()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = new FavoritoService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id));

        var result = await service.QuitarLugarAsync("comuna-13");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task AgregarLugarAsync_ThenQuitar_RemovesTheFavorite()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = new FavoritoService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id));

        await service.AgregarLugarAsync("comuna-13");
        var afterRemove = await service.QuitarLugarAsync("comuna-13");

        afterRemove.Value.IsFavorite.Should().BeFalse();
        context.FavoritosLugares.Count(f => f.UsuarioId == usuario.Id).Should().Be(0);
    }

    [Fact]
    public async Task AgregarLugarAsync_UnknownSlug_ReturnsNotFound()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);

        var service = new FavoritoService(new UnitOfWork(context), new FakeCurrentUser(usuario.Id));

        var result = await service.AgregarLugarAsync("no-existe");

        result.IsFailure.Should().BeTrue();
    }
}
