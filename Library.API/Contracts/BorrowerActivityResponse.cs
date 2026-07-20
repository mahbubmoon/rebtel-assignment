namespace Library.API.Contracts;

public sealed record BorrowerActivityResponse(
    long UserId,
    string Name,
    int BorrowCount);
