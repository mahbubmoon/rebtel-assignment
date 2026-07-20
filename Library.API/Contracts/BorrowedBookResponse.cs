namespace Library.API.Contracts;

public sealed record BorrowedBookResponse(
    long BookId,
    string Title,
    string Author,
    int BorrowCount);
