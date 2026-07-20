namespace Library.Core.Models;

public sealed record AlsoBorrowedBook(
    long BookId,
    string Title,
    string Author,
    int CoBorrowerCount,
    int BorrowCount);
