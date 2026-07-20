namespace Library.Domain.Entities;

public sealed record Loan(
    string Id,
    string BookId,
    string BorrowerId,
    DateOnly BorrowedOn,
    DateOnly? ReturnedOn);
