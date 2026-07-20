using Library.API.Contracts;
using Library.Grpc;
using ApiReadingPaceResponse = Library.API.Contracts.ReadingPaceResponse;

namespace Library.API.Clients;

public sealed class GrpcLibraryInsightsClient : ILibraryInsightsClient
{
    private readonly LibraryAnalytics.LibraryAnalyticsClient _client;

    public GrpcLibraryInsightsClient(LibraryAnalytics.LibraryAnalyticsClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyCollection<BorrowedBookResponse>> GetMostBorrowedBooksAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetMostBorrowedBooksAsync(
            new MostBorrowedBooksRequest { Limit = limit },
            cancellationToken: cancellationToken);

        return response.Books
            .Select(book => new BorrowedBookResponse(book.BookId, book.Title, book.Author, book.BorrowCount))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<BorrowerActivityResponse>> GetTopBorrowersAsync(
        DateOnly from,
        DateOnly to,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetTopBorrowersAsync(
            new TopBorrowersRequest
            {
                From = FormatDate(from),
                To = FormatDate(to),
                Limit = limit
            },
            cancellationToken: cancellationToken);

        return response.Borrowers
            .Select(borrower => new BorrowerActivityResponse(borrower.BorrowerId, borrower.Name, borrower.BorrowCount))
            .ToArray();
    }

    public async Task<ApiReadingPaceResponse?> EstimateReadingPaceAsync(
        long userId,
        long bookId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.EstimateReadingPaceAsync(
            new ReadingPaceRequest { BorrowerId = userId, BookId = bookId },
            cancellationToken: cancellationToken);

        if (!response.Found)
        {
            return null;
        }

        return new ApiReadingPaceResponse(
            response.BorrowerId,
            response.BookId,
            response.BookTitle,
            response.PageCount,
            response.BorrowedOn,
            response.ReturnedOn,
            response.ReadingDays,
            response.PagesPerDay);
    }

    public async Task<IReadOnlyCollection<AlsoBorrowedBookResponse>> GetAlsoBorrowedBooksAsync(
        long bookId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAlsoBorrowedBooksAsync(
            new AlsoBorrowedBooksRequest { BookId = bookId, Limit = limit },
            cancellationToken: cancellationToken);

        return response.Books
            .Select(book => new AlsoBorrowedBookResponse(
                book.BookId,
                book.Title,
                book.Author,
                book.CoBorrowerCount,
                book.BorrowCount))
            .ToArray();
    }

    private static string FormatDate(DateOnly date) => date.ToString("yyyy-MM-dd");
}
