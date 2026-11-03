using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.UnitTests.TestHelpers;

/// <summary>
/// One isolated in-memory <see cref="ColombiaGoDbContext"/> per call (a fresh, uniquely named
/// database), so tests never leak state into each other. Verifies the model's structure
/// (relationships, keys, indexes) builds correctly even though the actual SQL generation and
/// Postgres-specific behavior (native enums, triggers) obviously aren't exercised — those are
/// covered by running against a real Postgres instance, not by these unit tests.
/// </summary>
public static class InMemoryDbContextFactory
{
    /// <param name="databaseName">Pasar el mismo nombre a dos llamadas da dos contextos sobre la misma base: sirve para
    /// simular, como en producción, una petición nueva que no tiene nada cargado en memoria.</param>
    public static ColombiaGoDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ColombiaGoDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new ColombiaGoDbContext(options);
    }
}
