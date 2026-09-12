using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class UsuarioRepository : Repository<Usuario, Guid>, IUsuarioRepository
{
    public UsuarioRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public Task<Usuario?> GetByIdWithRolAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id, ct);
}
