using Library.Application.Models;
using Library.Application.Repositories;

namespace Library.Application.Services;

public sealed class LibraryAnalyticsService: ILibraryAnalyticsService
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
        var safeLimit = NormalizeLimit(limit);
        var booksById = (await _repository.GetBooksAsync(cancellationToken))
            .ToDictionary(book => book.Id, StringComparer.OrdinalIgnoreCase);

        return (await _repository.GetLoansAsync(cancellationToken))
            .Where(loan => booksById.ContainsKey(loan.BookId))
            .GroupBy(loan => loan.BookId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var book = booksById[group.Key];
                return new BorrowedBookStatistic(book.Id, book.Title, book.Author, group.Count());
            })
            .OrderByDescending(statistic => statistic.BorrowCount)
            .ThenBy(statistic => statistic.Title, StringComparer.OrdinalIgnoreCase)
            .Take(safeLimit)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<BorrowerActivity>> GetTopBorrowersAsync(
        DateOnly from,
        DateOnly to,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
        {
            throw new ArgumentException("The end date must be on or after the start date.", nameof(to));
        }

        var safeLimit = NormalizeLimit(limit);
        var borrowersById = (await _repository.GetBorrowersAsync(cancellationToken))
            .ToDictionary(borrower => borrower.Id, StringComparer.OrdinalIgnoreCase);

        return (await _repository.GetLoansAsync(cancellationToken))
            .Where(loan => loan.BorrowedOn >= from && loan.BorrowedOn <= to)
            .Where(loan => borrowersById.ContainsKey(loan.BorrowerId))
            .GroupBy(loan => loan.BorrowerId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var borrower = borrowersById[group.Key];
                return new BorrowerActivity(borrower.Id, borrower.Name, group.Count());
            })
            .OrderByDescending(activity => activity.BorrowCount)
            .ThenBy(activity => activity.Name, StringComparer.OrdinalIgnoreCase)
            .Take(safeLimit)
            .ToArray();
    }

    public async Task<ReadingPaceEstimate?> EstimateReadingPaceAsync(
        string borrowerId,
        string bookId,
        CancellationToken cancellationToken = default)
    {
        var book = await _repository.GetBookByIdAsync(bookId, cancellationToken);
        if (book is null)
        {
            return null;
        }

        var loan = (await _repository.GetLoansAsync(cancellationToken))
            .Where(candidate =>
                candidate.BookId.Equals(bookId, StringComparison.OrdinalIgnoreCase) &&
                candidate.BorrowerId.Equals(borrowerId, StringComparison.OrdinalIgnoreCase) &&
                candidate.ReturnedOn.HasValue)
            .OrderByDescending(candidate => candidate.ReturnedOn)
            .ThenByDescending(candidate => candidate.BorrowedOn)
            .FirstOrDefault();

        if (loan?.ReturnedOn is null)
        {
            return null;
        }

        var readingDays = Math.Max(1, loan.ReturnedOn.Value.DayNumber - loan.BorrowedOn.DayNumber);
        var pagesPerDay = Math.Round(book.PageCount / (double)readingDays, 2, MidpointRounding.AwayFromZero);

        return new ReadingPaceEstimate(
            borrowerId,
            book.Id,
            book.Title,
            book.PageCount,
            loan.BorrowedOn,
            loan.ReturnedOn.Value,
            readingDays,
            pagesPerDay);
    }

    public async Task<IReadOnlyCollection<AlsoBorrowedBook>> GetAlsoBorrowedBooksAsync(
        string bookId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var safeLimit = NormalizeLimit(limit);
        var booksById = (await _repository.GetBooksAsync(cancellationToken))
            .ToDictionary(book => book.Id, StringComparer.OrdinalIgnoreCase);

        if (!booksById.ContainsKey(bookId))
        {
            return Array.Empty<AlsoBorrowedBook>();
        }

        var loans = await _repository.GetLoansAsync(cancellationToken);
        var coBorrowerIds = loans
            .Where(loan => loan.BookId.Equals(bookId, StringComparison.OrdinalIgnoreCase))
            .Select(loan => loan.BorrowerId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return loans
            .Where(loan => coBorrowerIds.Contains(loan.BorrowerId))
            .Where(loan => !loan.BookId.Equals(bookId, StringComparison.OrdinalIgnoreCase))
            .Where(loan => booksById.ContainsKey(loan.BookId))
            .GroupBy(loan => loan.BookId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var book = booksById[group.Key];
                var coBorrowerCount = group
                    .Select(loan => loan.BorrowerId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                return new AlsoBorrowedBook(book.Id, book.Title, book.Author, coBorrowerCount, group.Count());
            })
            .OrderByDescending(book => book.CoBorrowerCount)
            .ThenByDescending(book => book.BorrowCount)
            .ThenBy(book => book.Title, StringComparer.OrdinalIgnoreCase)
            .Take(safeLimit)
            .ToArray();
    }

    private static int NormalizeLimit(int limit) => Math.Clamp(limit, 1, 100);
}
