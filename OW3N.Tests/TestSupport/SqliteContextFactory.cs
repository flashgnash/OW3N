using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OW3N.Tests.TestSupport;

/// <summary>
/// An <see cref="IDbContextFactory{TContext}"/> backed by a single shared in-memory SQLite
/// connection. Every context handed out talks to the same database (the connection is kept
/// open for the lifetime of the factory), which mirrors how the real app uses a factory to
/// create short-lived contexts against one persistent database.
/// </summary>
/// <remarks>
/// <para><b>Why SQLite (in-memory) rather than PostgreSQL for the tests?</b></para>
/// <para>
/// Production runs on PostgreSQL (Npgsql), but the integration tests deliberately use an
/// in-memory SQLite database. This is a considered trade-off, not an accident:
/// </para>
/// <list type="bullet">
///   <item><description><b>Isolation</b> — each test constructs its own factory and therefore
///   its own private database, so tests can never see each other's rows or run order.</description></item>
///   <item><description><b>Hermetic &amp; zero-setup</b> — no PostgreSQL server, Docker container,
///   connection string or credentials are needed, so <c>nix run .#test</c> and the CI <c>test</c>
///   job run anywhere with just the .NET SDK.</description></item>
///   <item><description><b>Speed</b> — an in-memory database plus <see cref="DatabaseFacade.EnsureCreated"/>
///   builds the schema from the EF model in milliseconds per test.</description></item>
///   <item><description><b>Same EF Core surface</b> — the app touches the database exclusively
///   through EF Core and <c>IDbContextFactory&lt;OrdisContext&gt;</c>, so swapping the provider
///   underneath still exercises the same LINQ, mapping and change-tracking code paths.</description></item>
/// </list>
/// <para>
/// The trade-off is fidelity: SQLite is not PostgreSQL. Provider-specific SQL is not exercised
/// (the clearest case is <c>BuffService.SearchTemplatesAsync</c>, whose <c>EF.Functions.ILike</c>
/// name filter is PostgreSQL-only and has no SQLite translation), and the schema is created from
/// the model via <c>EnsureCreated()</c> rather than from the Npgsql migrations under
/// <c>Migrations/</c>. See <c>OW3N.Tests/README.md</c> for the full rationale and how a
/// PostgreSQL-backed run could close those gaps if the behaviour ever becomes critical.
/// </para>
/// </remarks>
public sealed class SqliteContextFactory : IDbContextFactory<OrdisContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<OrdisContext> _options;

    public SqliteContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<OrdisContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        using var ctx = new OrdisContext(_options);
        ctx.Database.EnsureCreated();
    }

    public OrdisContext CreateDbContext() => new(_options);

    public Task<OrdisContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());

    public void Dispose() => _connection.Dispose();
}
