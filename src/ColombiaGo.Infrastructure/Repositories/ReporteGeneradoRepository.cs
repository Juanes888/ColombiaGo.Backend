using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;

namespace ColombiaGo.Infrastructure.Repositories;

public class ReporteGeneradoRepository : Repository<ReporteGenerado, int>, IReporteGeneradoRepository
{
    public ReporteGeneradoRepository(ColombiaGoDbContext context) : base(context)
    {
    }
}
