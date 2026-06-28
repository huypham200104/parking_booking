using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using parking_booking_backend.Data;

namespace parking_booking_backend.Tests.Infrastructure;

public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection? _connection;

    private TestDatabase(DbContextOptions<ApplicationDbContext> options, SqliteConnection? connection)
    {
        Options = options;
        _connection = connection;
    }

    public DbContextOptions<ApplicationDbContext> Options { get; }

    public ApplicationDbContext CreateContext() => new(Options);

    public static async Task<TestDatabase> CreateSqliteAsync()
    {
        SQLitePCL.Batteries_V2.Init();

        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection, sqlite => sqlite.UseNetTopologySuite())
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.AmbientTransactionWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new TestDatabase(options, connection);
    }

    public static async Task<TestDatabase> CreateInMemoryAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new TestDatabase(options, null);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
