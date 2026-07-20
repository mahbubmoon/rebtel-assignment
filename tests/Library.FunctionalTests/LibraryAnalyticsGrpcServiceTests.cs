using Grpc.Core;
using Library.Core.Services;
using Library.Grpc;
using Library.Grpc.Services;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Sql;
using Xunit;

namespace Library.FunctionalTests;

public sealed class LibraryAnalyticsGrpcServiceTests
{
    [Fact]
    public async Task GetMostBorrowedBooks_ReturnsExpectedGrpcPayload()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = CreateService(database.Repository);

        var response = await service.GetMostBorrowedBooks(new MostBorrowedBooksRequest { Limit = 1 }, null!);

        var book = Assert.Single(response.Books);
        Assert.Equal(1, book.BookId);
        Assert.Equal(4, book.BorrowCount);
    }

    [Fact]
    public async Task GetTopBorrowers_RejectsInvalidDateFormat()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = CreateService(database.Repository);

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetTopBorrowers(
                new TopBorrowersRequest { From = "01-01-2026", To = "2026-06-30", Limit = 3 },
                null!));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task EstimateReadingPace_ReturnsCompletedLoanPace()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = CreateService(database.Repository);

        var response = await service.EstimateReadingPace(
            new ReadingPaceRequest { BorrowerId = 11, BookId = 1 },
            null!);

        Assert.True(response.Found);
        Assert.Equal(7, response.ReadingDays);
        Assert.Equal(50.29, response.PagesPerDay);
    }

    private static LibraryAnalyticsGrpcService CreateService(SqlLibraryRepository repository) =>
        new(new LibraryAnalyticsService(repository));

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
            var databasePath = Path.Combine(Path.GetTempPath(), $"library-functional-tests-{Guid.NewGuid():N}.db");
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
