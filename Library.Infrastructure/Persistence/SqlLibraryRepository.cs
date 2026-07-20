using Library.Core.Entities;
using Library.Core.Repositories;
using Library.Infrastructure.Persistence.Sql;
using Microsoft.Data.Sqlite;

namespace Library.Infrastructure.Persistence;

public sealed class SqlLibraryRepository : ILibraryRepository
{
    private const string DateFormat = "yyyy-MM-dd";
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqlLibraryRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<Book>> GetBooksAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Isbn, Title, Author, PageCount, TotalCopies
            FROM Books
            ORDER BY Title;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var books = new List<Book>();
        while (await reader.ReadAsync(cancellationToken))
        {
            books.Add(ReadBook(reader));
        }

        return books;
    }

    public async Task<Book?> GetBookByIdAsync(long bookId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Isbn, Title, Author, PageCount, TotalCopies
            FROM Books
            WHERE Id = $bookId COLLATE NOCASE
            LIMIT 1;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql);
        command.Parameters.AddWithValue("$bookId", bookId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? ReadBook(reader) : null;
    }

    public async Task<IReadOnlyCollection<Borrower>> GetBorrowersAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Email
            FROM Borrowers
            ORDER BY Name;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var borrowers = new List<Borrower>();
        while (await reader.ReadAsync(cancellationToken))
        {
            borrowers.Add(ReadBorrower(reader));
        }

        return borrowers;
    }

    public async Task<Borrower?> GetBorrowerByIdAsync(long borrowerId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Email
            FROM Borrowers
            WHERE Id = $borrowerId COLLATE NOCASE
            LIMIT 1;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql);
        command.Parameters.AddWithValue("$borrowerId", borrowerId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? ReadBorrower(reader) : null;
    }

    public async Task<IReadOnlyCollection<Loan>> GetLoansAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, BookId, BorrowerId, BorrowedOn, ReturnedOn
            FROM Loans
            ORDER BY BorrowedOn, Id;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var loans = new List<Loan>();
        while (await reader.ReadAsync(cancellationToken))
        {
            loans.Add(ReadLoan(reader));
        }

        return loans;
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static SqliteCommand CreateCommand(SqliteConnection connection, string sql)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        return command;
    }

    private static Book ReadBook(SqliteDataReader reader) =>
        new(
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetInt32(5));

    private static Borrower ReadBorrower(SqliteDataReader reader) =>
        new(
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetString(2));

    private static Loan ReadLoan(SqliteDataReader reader) =>
        new(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2),
            DateOnly.ParseExact(reader.GetString(3), DateFormat),
            reader.IsDBNull(4) ? null : DateOnly.ParseExact(reader.GetString(4), DateFormat));
}
