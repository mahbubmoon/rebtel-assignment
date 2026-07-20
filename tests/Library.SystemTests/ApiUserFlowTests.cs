using System.Net;
using System.Net.Http.Json;
using Library.API;
using Library.API.Clients;
using Library.API.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Library.SystemTests;

public sealed class ApiUserFlowTests
{
    [Fact]
    public async Task LibrarianCanReadInventoryUserAndPatternInsights()
    {
        await using var application = CreateApplication();
        var client = application.CreateClient();

        var mostBorrowed = await client.GetFromJsonAsync<BorrowedBookResponse[]>(
            "/api/inventory/most-borrowed-books?limit=1");
        var topBorrowers = await client.GetFromJsonAsync<BorrowerActivityResponse[]>(
            "/api/users/top-borrowers?from=2026-01-01&to=2026-06-30&limit=1");
        var pace = await client.GetFromJsonAsync<ReadingPaceResponse>(
            "/api/users/1/reading-pace?bookId=1");
        var alsoBorrowed = await client.GetFromJsonAsync<AlsoBorrowedBookResponse[]>(
            "/api/books/1/also-borrowed?limit=1");

        Assert.NotNull(mostBorrowed);
        Assert.Equal(1, Assert.Single(mostBorrowed).BookId);
        Assert.NotNull(topBorrowers);
        Assert.Equal(102, Assert.Single(topBorrowers).UserId);
        Assert.NotNull(pace);
        Assert.Equal(50.29, pace.PagesPerDay);
        Assert.NotNull(alsoBorrowed);
        Assert.Equal(2, Assert.Single(alsoBorrowed).BookId);
    }

    [Fact]
    public async Task ReadingPace_ReturnsNotFoundWhenNoCompletedLoanExists()
    {
        await using var application = CreateApplication();
        var client = application.CreateClient();

        var response = await client.GetAsync("/api/users/0/reading-pace?bookId=1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateApplication() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<ILibraryInsightsClient, FakeLibraryInsightsClient>();
                });
            });

    private sealed class FakeLibraryInsightsClient : ILibraryInsightsClient
    {
        public Task<IReadOnlyCollection<BorrowedBookResponse>> GetMostBorrowedBooksAsync(
            int limit,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<BorrowedBookResponse>>(
            [
                new(1, "The Pragmatic Programmer", "Andrew Hunt and David Thomas", 4)
            ]);

        public Task<IReadOnlyCollection<BorrowerActivityResponse>> GetTopBorrowersAsync(
            DateOnly from,
            DateOnly to,
            int limit,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<BorrowerActivityResponse>>(
            [
                new(102, "Grace Hopper", 4)
            ]);

        public Task<ReadingPaceResponse?> EstimateReadingPaceAsync(
            long userId,
            long bookId,
            CancellationToken cancellationToken = default)
        {
            if (userId == 0)
            {
                return Task.FromResult<ReadingPaceResponse?>(null);
            }

            return Task.FromResult<ReadingPaceResponse?>(
                new(userId, bookId, "The Pragmatic Programmer", 352, "2026-01-01", "2026-01-08", 7, 50.29));
        }

        public Task<IReadOnlyCollection<AlsoBorrowedBookResponse>> GetAlsoBorrowedBooksAsync(
            long bookId,
            int limit,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<AlsoBorrowedBookResponse>>(
            [
                new(2, "Clean Code", "Robert C. Martin", 2, 2)
            ]);
    }
}
