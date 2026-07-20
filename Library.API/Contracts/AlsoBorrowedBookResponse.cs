namespace Library.API.Contracts;

public sealed record AlsoBorrowedBookResponse(
    long BookId,
    string Title,
    string Author,
    int CoBorrowerCount,
    int BorrowCount);
