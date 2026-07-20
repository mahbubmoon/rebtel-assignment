namespace Library.Core.Models;

public sealed record BorrowerActivity(
    long BorrowerId,
    string Name,
    int BorrowCount);
