using Library.Core.Entities;

namespace Library.Core.Repositories;

public interface ILibraryRepository
{
    Task<IReadOnlyCollection<Book>> GetBooksAsync(CancellationToken cancellationToken = default);

    Task<Book?> GetBookByIdAsync(long bookId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Borrower>> GetBorrowersAsync(CancellationToken cancellationToken = default);

    Task<Borrower?> GetBorrowerByIdAsync(long borrowerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Loan>> GetLoansAsync(CancellationToken cancellationToken = default);
}
