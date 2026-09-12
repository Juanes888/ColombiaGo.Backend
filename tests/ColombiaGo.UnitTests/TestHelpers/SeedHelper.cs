using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;
using ColombiaGo.Infrastructure.Persistence;

namespace ColombiaGo.UnitTests.TestHelpers;

/// <summary>Minimal, reusable reference data most service tests need (a role, a category, a
/// registered user, a place) — kept small and explicit rather than a giant fixture nobody can
/// read.</summary>
public static class SeedHelper
{
    public static Rol AddRol(this ColombiaGoDbContext context, string nombre = "turista")
    {
        var rol = new Rol { Nombre = nombre };
        context.Roles.Add(rol);
        context.SaveChanges();
        return rol;
    }

    public static Categoria AddCategoria(this ColombiaGoDbContext context, string slug = "culture", string nombre = "Cultura")
    {
        var categoria = new Categoria { Nombre = nombre, Slug = slug, Activa = true };
        context.Categorias.Add(categoria);
        context.SaveChanges();
        return categoria;
    }

    public static Usuario AddUsuario(this ColombiaGoDbContext context, Rol rol, Guid? id = null)
    {
        var usuario = new Usuario(id ?? Guid.NewGuid())
        {
            RolId = rol.Id,
            Rol = rol,
            NombreCompleto = "Camilo Restrepo",
            CorreoElectronico = $"{Guid.NewGuid():N}@colombiago.dev",
            FechaRegistro = DateTimeOffset.UtcNow,
            FechaActualizacion = DateTimeOffset.UtcNow,
        };
        context.Usuarios.Add(usuario);
        context.SaveChanges();
        return usuario;
    }

    public static Lugar AddLugar(
        this ColombiaGoDbContext context,
        Categoria categoria,
        string slug = "comuna-13",
        string nombre = "Comuna 13",
        bool aprobado = true,
        decimal latitud = 6.0m,
        decimal longitud = -75.0m)
    {
        var lugar = new Lugar
        {
            CategoriaId = categoria.Id,
            Categoria = categoria,
            RegistradoPor = null,
            Nombre = nombre,
            Slug = slug,
            Ciudad = "Medellín",
            Region = "Antioquia",
            Descripcion = "Descripción de prueba",
            Direccion = "Dirección de prueba",
            Latitud = latitud,
            Longitud = longitud,
            RangoPrecio = RangoPrecio.Gratis,
            EstadoPublicacion = aprobado ? EstadoPublicacion.Aprobado : EstadoPublicacion.Pendiente,
            FechaRegistro = DateTimeOffset.UtcNow,
            FechaActualizacion = DateTimeOffset.UtcNow,
        };
        context.Lugares.Add(lugar);
        context.SaveChanges();
        return lugar;
    }
}
