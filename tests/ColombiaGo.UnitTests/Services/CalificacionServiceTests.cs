using ColombiaGo.Application.DTOs.Calificaciones;
using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Common;
using ColombiaGo.Infrastructure.Repositories;
using ColombiaGo.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Services;

public class CalificacionServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CrearAsync_PuntuacionOutOfRange_ReturnsValidationError(int puntuacion)
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = CreateService(context, usuario.Id);

        var result = await service.CrearAsync("comuna-13", new CrearCalificacionRequest(puntuacion, "texto"));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CrearAsync_SecondReviewByTheSameUser_ReturnsConflict()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = CreateService(context, usuario.Id);

        var first = await service.CrearAsync("comuna-13", new CrearCalificacionRequest(5, "Excelente"));
        var second = await service.CrearAsync("comuna-13", new CrearCalificacionRequest(3, "Cambié de opinión"));

        first.IsSuccess.Should().BeTrue();
        second.IsFailure.Should().BeTrue();
        second.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task CrearAsync_WithAutoAprobarDisabled_StaysPendiente()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = CreateService(context, usuario.Id, autoAprobar: false);

        await service.CrearAsync("comuna-13", new CrearCalificacionRequest(5, "Excelente"));

        context.Calificaciones.Single().EstadoModeracion.Should().Be(ColombiaGo.Domain.Enums.EstadoModeracion.Pendiente);
    }

    [Fact]
    public async Task CrearAsync_WithAutoAprobarEnabled_IsImmediatelyAprobada()
    {
        using var context = InMemoryDbContextFactory.Create();
        var rol = context.AddRol();
        var usuario = context.AddUsuario(rol);
        var categoria = context.AddCategoria();
        context.AddLugar(categoria, "comuna-13");

        var service = CreateService(context, usuario.Id, autoAprobar: true);

        await service.CrearAsync("comuna-13", new CrearCalificacionRequest(5, "Excelente"));

        context.Calificaciones.Single().EstadoModeracion.Should().Be(ColombiaGo.Domain.Enums.EstadoModeracion.Aprobada);
    }

    private static CalificacionService CreateService(
        Infrastructure.Persistence.ColombiaGoDbContext context, Guid userId, bool autoAprobar = false) =>
        new(new UnitOfWork(context), new FakeCurrentUser(userId), new FakeModeracionSettings(autoAprobar));
}
