using Library.Core.Models;
using Library.Core.Repositories;

namespace Library.Core.Services;

public class LibraryAnalyticsService : ILibraryAnalyticsService
{
    private readonly ILibraryRepository _repository;

    public LibraryAnalyticsService(ILibraryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<BorrowedBookStatistic>> GetMostBorrowedBooksAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1) limit = 10;
        if (limit > 100) limit = 100;

        var booksById = (await _repository.GetBooksAsync(cancellationToken))
            .ToDictionary(b => b.Id);

        var loans = await _repository.GetLoansAsync(cancellationToken);

        return loans
            .Where(l => booksById.ContainsKey(l.BookId))
            .GroupBy(l => l.BookId)
            .Select(g =>
            {
                var book = booksById[g.Key];
                return new BorrowedBookStatistic(book.Id, book.Title, book.Author, g.Count());
            })
            .OrderByDescending(x => x.BorrowCount)
            .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    public async Task<IReadOnlyCollection<BorrowerActivity>> GetTopBorrowersAsync(
        DateOnly from,
        DateOnly to,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
            throw new ArgumentException("to must be >= from", nameof(to));

        limit = Math.Clamp(limit, 1, 100);

        var borrowersById = (await _repository.GetBorrowersAsync(cancellationToken))
            .ToDictionary(b => b.Id);

        return (await _repository.GetLoansAsync(cancellationToken))
            .Where(l => l.BorrowedOn >= from && l.BorrowedOn <= to)
            .Where(l => borrowersById.ContainsKey(l.BorrowerId))
            .GroupBy(l => l.BorrowerId)
            .Select(g =>
            {
                var borrower = borrowersById[g.Key];
                return new BorrowerActivity(borrower.Id, borrower.Name, g.Count());
            })
            .OrderByDescending(x => x.BorrowCount)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    public async Task<ReadingPaceEstimate?> EstimateReadingPaceAsync(
        long borrowerId,
        long bookId,
        CancellationToken cancellationToken = default)
    {
        var book = await _repository.GetBookByIdAsync(bookId, cancellationToken);
        if (book == null)
            return null;

        // pick the most recent completed loan for this user+book
        var loan = (await _repository.GetLoansAsync(cancellationToken))
            .Where(l =>
                l.BookId.Equals(bookId) &&
                l.BorrowerId.Equals(borrowerId) &&
                l.ReturnedOn != null)
            .OrderByDescending(l => l.ReturnedOn)
            .ThenByDescending(l => l.BorrowedOn)
            .FirstOrDefault();

        if (loan?.ReturnedOn == null)
            return null;

        // same-day return still counts as 1 day so we don't divide by zero
        var days = Math.Max(1, loan.ReturnedOn.Value.DayNumber - loan.BorrowedOn.DayNumber);
        var pagesPerDay = Math.Round(book.PageCount / (double)days, 2);

        return new ReadingPaceEstimate(
            borrowerId,
            book.Id,
            book.Title,
            book.PageCount,
            loan.BorrowedOn,
            loan.ReturnedOn.Value,
            days,
            pagesPerDay);
    }

    public async Task<IReadOnlyCollection<AlsoBorrowedBook>> GetAlsoBorrowedBooksAsync(
        long bookId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1) limit = 10;
        if (limit > 100) limit = 100;

        var booksById = (await _repository.GetBooksAsync(cancellationToken))
            .ToDictionary(b => b.Id);

        if (!booksById.ContainsKey(bookId))
            return new List<AlsoBorrowedBook>();

        var loans = await _repository.GetLoansAsync(cancellationToken);

        var readers = loans
            .Where(l => l.BookId.Equals(bookId))
            .Select(l => l.BorrowerId)
            .ToHashSet();

        return loans
            .Where(l => readers.Contains(l.BorrowerId))
            .Where(l => !l.BookId.Equals(bookId))
            .Where(l => booksById.ContainsKey(l.BookId))
            .GroupBy(l => l.BookId)
            .Select(g =>
            {
                var book = booksById[g.Key];
                var sharedReaders = g.Select(l => l.BorrowerId)
                    .Distinct()
                    .Count();

                return new AlsoBorrowedBook(book.Id, book.Title, book.Author, sharedReaders, g.Count());
            })
            .OrderByDescending(b => b.CoBorrowerCount)
            .ThenByDescending(b => b.BorrowCount)
            .ThenBy(b => b.Title, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }
}
