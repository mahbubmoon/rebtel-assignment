using Library.Core.Entities;
using Library.Core.Repositories;
using Library.Core.Services;
using Xunit;

namespace Library.UnitTests;

public sealed class LibraryAnalyticsServiceTests
{
    private static readonly IReadOnlyCollection<Book> Books =
    [
        new(1, "isbn-a", "Alpha", "Author A", 120, 2),
        new(2, "isbn-b", "Beta", "Author B", 300, 2),
        new(3, "isbn-c", "Gamma", "Author C", 200, 2)
    ];

    private static readonly IReadOnlyCollection<Borrower> Borrowers =
    [
        new(11, "Ada", "ada@example.org"),
        new(12, "Grace", "grace@example.org"),
        new(13, "Alan", "alan@example.org")
    ];

    private static readonly IReadOnlyCollection<Loan> Loans =
    [
        new(100, 1, 11, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 5)),
        new(101, 1, 12, new DateOnly(2026, 1, 2), new DateOnly(2026, 1, 8)),
        new(102, 1, 13, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 8)),
        new(104, 2, 11, new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 20)),
        new(105, 2, 12, new DateOnly(2026, 1, 11), new DateOnly(2026, 1, 21)),
        new(106, 3, 12, new DateOnly(2026, 1, 12), new DateOnly(2026, 1, 22)),
        new(107, 3, 13, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 10))
    ];

    [Fact]
    public async Task GetMostBorrowedBooks_ReturnsBooksOrderedByBorrowCount()
    {
        var service = CreateService();

        var results = await service.GetMostBorrowedBooksAsync(limit: 2);

        Assert.Collection(
            results,
            first =>
            {
                Assert.Equal(1, first.BookId);
                Assert.Equal(3, first.BorrowCount);
            },
            second =>
            {
                Assert.Equal(2, second.BookId);
                Assert.Equal(2, second.BorrowCount);
            });
    }

    [Fact]
    public async Task GetTopBorrowers_FiltersByBorrowDateWindow()
    {
        var service = CreateService();

        var results = await service.GetTopBorrowersAsync(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31),
            limit: 3);

        Assert.Collection(
            results,
            first =>
            {
                Assert.Equal(12, first.BorrowerId);
                Assert.Equal(3, first.BorrowCount);
            },
            second =>
            {
                Assert.Equal(11, second.BorrowerId);
                Assert.Equal(2, second.BorrowCount);
            });
    }

    [Fact]
    public async Task EstimateReadingPace_UsesReturnedLoanDuration()
    {
        var service = CreateService();

        var result = await service.EstimateReadingPaceAsync(11, 1);

        Assert.NotNull(result);
        Assert.Equal(4, result.ReadingDays);
        Assert.Equal(30, result.PagesPerDay);
    }

    [Fact]
    public async Task GetAlsoBorrowedBooks_RanksBooksBorrowedBySameReaders()
    {
        var service = CreateService();

        var results = await service.GetAlsoBorrowedBooksAsync(1, limit: 2);

        Assert.Collection(
            results,
            first =>
            {
                Assert.Equal(2, first.BookId);
                Assert.Equal(2, first.CoBorrowerCount);
            },
            second =>
            {
                Assert.Equal(3, second.BookId);
                Assert.Equal(2, second.CoBorrowerCount);
            });
    }

    private static LibraryAnalyticsService CreateService() =>
        new(new FakeLibraryRepository(Books, Borrowers, Loans));

    private sealed class FakeLibraryRepository : ILibraryRepository
    {
        private readonly IReadOnlyCollection<Book> _books;
        private readonly IReadOnlyCollection<Borrower> _borrowers;
        private readonly IReadOnlyCollection<Loan> _loans;

        public FakeLibraryRepository(
            IReadOnlyCollection<Book> books,
            IReadOnlyCollection<Borrower> borrowers,
            IReadOnlyCollection<Loan> loans)
        {
            _books = books;
            _borrowers = borrowers;
            _loans = loans;
        }

        public Task<IReadOnlyCollection<Book>> GetBooksAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_books);

        public Task<Book?> GetBookByIdAsync(long bookId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_books.FirstOrDefault(book => book.Id.Equals(bookId)));

        public Task<IReadOnlyCollection<Borrower>> GetBorrowersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_borrowers);

        public Task<Borrower?> GetBorrowerByIdAsync(long borrowerId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_borrowers.FirstOrDefault(borrower => borrower.Id.Equals(borrowerId)));

        public Task<IReadOnlyCollection<Loan>> GetLoansAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_loans);
    }
}
