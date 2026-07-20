using Library.Core.Models;

namespace Library.Core.Services;

public interface ILibraryAnalyticsService
{
    Task<IReadOnlyCollection<BorrowedBookStatistic>> GetMostBorrowedBooksAsync(int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BorrowerActivity>> GetTopBorrowersAsync(DateOnly from, DateOnly to, int limit,
        CancellationToken cancellationToken = default);

    Task<ReadingPaceEstimate?> EstimateReadingPaceAsync(long borrowerId, long bookId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AlsoBorrowedBook>> GetAlsoBorrowedBooksAsync(long bookId, int limit,
        CancellationToken cancellationToken = default);
}