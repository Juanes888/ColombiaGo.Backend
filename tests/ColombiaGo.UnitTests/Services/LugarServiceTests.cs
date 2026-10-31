using ColombiaGo.Application.Common;
using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;
using ColombiaGo.Infrastructure.Repositories;
using ColombiaGo.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Services;

public class LugarServiceTests
{
    [Fact]
    public async Task BuscarAsync_FiltersByCategoriaSlug()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var cultura = context.AddCategoria("culture", "Cultura");
        var naturaleza = context.AddCategoria("nature", "Naturaleza");
        context.AddLugar(cultura, "comuna-13", "Comuna 13");
        context.AddLugar(naturaleza, "guatape", "Guatapé");

        var service = CreateService(context, usuario.Id);

        var result = await service.BuscarAsync(new LugarQueryParams { CategoriaSlug = "culture" });

        result.Items.Should().ContainSingle(l => l.Id == "comuna-13");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task BuscarAsync_ExcludesNonAprobadoPlaces()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13", "Comuna 13", aprobado: true);
        context.AddLugar(categoria, "pendiente-1", "Lugar pendiente", aprobado: false);

        var service = CreateService(context, usuario.Id);

        var result = await service.BuscarAsync(new LugarQueryParams());

        result.Items.Should().ContainSingle().Which.Id.Should().Be("comuna-13");
    }

    [Fact]
    public async Task GetPorSlugAsync_UnknownSlug_ReturnsNotFound()
    {
        using var context = InMemoryDbContextFactory.Create();
        var service = CreateService(context, userId: null);

        var result = await service.GetPorSlugAsync("no-existe");

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetPorSlugAsync_PendingPlace_IsNotVisible()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "pendiente-1", aprobado: false);

        var service = CreateService(context, usuario.Id);

        var result = await service.GetPorSlugAsync("pendiente-1");

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetPorSlugAsync_AuthenticatedUserWithFavorite_ReportsIsFavoriteTrue()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        var lugar = context.AddLugar(categoria, "comuna-13");
        context.FavoritosLugares.Add(new FavoritoLugar
        {
            UsuarioId = usuario.Id,
            LugarId = lugar.Id,
            FechaAgregado = DateTimeOffset.UtcNow,
        });
        context.SaveChanges();

        var service = CreateService(context, usuario.Id);

        var result = await service.GetPorSlugAsync("comuna-13");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsFavorite.Should().BeTrue();
        result.Value.IsInRoute.Should().BeFalse();
    }

    [Fact]
    public async Task GetPorSlugAsync_AnonymousUser_DoesNotThrowAndReportsNoFavorite()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        context.AddUsuario(rol); // just to have a valid rol/usuario chain elsewhere
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = CreateService(context, userId: null);

        var result = await service.GetPorSlugAsync("comuna-13");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsFavorite.Should().BeFalse();
        result.Value.IsInRoute.Should().BeFalse();
    }

    private static LugarService CreateService(Infrastructure.Persistence.ColombiaGoDbContext context, Guid? userId)
    {
        var uow = new UnitOfWork(context);
        var currentUser = new FakeCurrentUser(userId);
        return new LugarService(uow, currentUser, TestServices.Localizador(context));
    }
}
