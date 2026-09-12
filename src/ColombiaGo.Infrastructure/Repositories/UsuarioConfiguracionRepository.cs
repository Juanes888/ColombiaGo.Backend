using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;

namespace ColombiaGo.Infrastructure.Repositories;

public class UsuarioConfiguracionRepository : Repository<UsuarioConfiguracion, Guid>, IUsuarioConfiguracionRepository
{
    public UsuarioConfiguracionRepository(ColombiaGoDbContext context) : base(context)
    {
    }
}
