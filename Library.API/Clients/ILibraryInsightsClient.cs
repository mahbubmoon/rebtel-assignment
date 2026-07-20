using Library.API.Contracts;

namespace Library.API.Clients;

public interface ILibraryInsightsClient
{
    Task<IReadOnlyCollection<BorrowedBookResponse>> GetMostBorrowedBooksAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BorrowerActivityResponse>> GetTopBorrowersAsync(
        DateOnly from,
        DateOnly to,
        int limit,
        CancellationToken cancellationToken = default);

    Task<ReadingPaceResponse?> EstimateReadingPaceAsync(
        long userId,
        long bookId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AlsoBorrowedBookResponse>> GetAlsoBorrowedBooksAsync(
        long bookId,
        int limit,
        CancellationToken cancellationToken = default);
}
