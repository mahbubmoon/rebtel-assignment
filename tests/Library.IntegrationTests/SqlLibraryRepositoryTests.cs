using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Sql;
using Xunit;

namespace Library.IntegrationTests;

public sealed class SqlLibraryRepositoryTests
{
    [Fact]
    public async Task Repository_ReturnsSeededBooksBorrowersAndLoans()
    {
        await using var database = await TestDatabase.CreateAsync();

        var books = await database.Repository.GetBooksAsync();
        var borrowers = await database.Repository.GetBorrowersAsync();
        var loans = await database.Repository.GetLoansAsync();

        Assert.Equal(6, books.Count);
        Assert.Equal(5, borrowers.Count);
        Assert.Equal(15, loans.Count);
    }

    [Fact]
    public async Task Repository_LooksUpBooksCaseInsensitively()
    {
        await using var database = await TestDatabase.CreateAsync();

        var book = await database.Repository.GetBookByIdAsync(1);

        Assert.NotNull(book);
        Assert.Equal("The Pragmatic Programmer", book.Title);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly string _databasePath;

        private TestDatabase(string databasePath, SqlLibraryRepository repository)
        {
            _databasePath = databasePath;
            Repository = repository;
        }

        public SqlLibraryRepository Repository { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"library-tests-{Guid.NewGuid():N}.db");
            var connectionFactory = new SqliteConnectionFactory($"Data Source={databasePath}");

            await new SqlLibraryDatabaseInitializer(connectionFactory).InitializeAsync();

            return new TestDatabase(databasePath, new SqlLibraryRepository(connectionFactory));
        }

        public ValueTask DisposeAsync()
        {
            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }

            return ValueTask.CompletedTask;
        }
    }
}
