using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Auth;

/// <summary>
/// Just-in-time provisioning: the first time a Supabase-authenticated user (identified by their
/// <c>auth.users</c> id) is seen, creates their <see cref="Usuario"/> row with the default
/// "turista" role. Covers the case where Supabase's own <c>handle_new_user</c> trigger (see
/// db/001_schema.sql) hasn't run yet or the project doesn't have it installed — the API never
/// 401s a valid Supabase user just because our side of the profile is missing.
/// </summary>
public sealed class UsuarioProvisioner
{
    private const string RolPorDefecto = "turista";

    private readonly ColombiaGoDbContext _context;

    public UsuarioProvisioner(ColombiaGoDbContext context) => _context = context;

    public async Task<Usuario> GetOrCreateAsync(
        Guid userId, string? email, string? nombreCompleto, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (usuario is not null)
            return usuario;

        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == RolPorDefecto, ct)
            ?? throw new InvalidOperationException(
                $"El rol '{RolPorDefecto}' no existe. Verifica que db/002_seed.sql se haya ejecutado.");

        var nombre = string.IsNullOrWhiteSpace(nombreCompleto)
            ? (string.IsNullOrWhiteSpace(email) ? "Usuario ColombiaGO" : email.Split('@')[0])
            : nombreCompleto;

        usuario = new Usuario(userId)
        {
            RolId = rol.Id,
            Rol = rol,
            NombreCompleto = nombre,
            CorreoElectronico = email ?? string.Empty,
            AceptoTerminos = false,
            Activo = true,
            FechaRegistro = DateTimeOffset.UtcNow,
            FechaActualizacion = DateTimeOffset.UtcNow,
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(ct);

        return usuario;
    }
}
