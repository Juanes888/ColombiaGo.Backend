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
    public static ColombiaGoDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ColombiaGoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new ColombiaGoDbContext(options);
    }
}
