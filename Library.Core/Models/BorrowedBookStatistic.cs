namespace Library.Core.Models;

public sealed record BorrowedBookStatistic(
    long BookId,
    string Title,
    string Author,
    int BorrowCount);
