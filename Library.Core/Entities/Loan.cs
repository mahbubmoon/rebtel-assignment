namespace Library.Core.Entities;

public record Loan(
    long Id,
    long BookId,
    long BorrowerId,
    DateOnly BorrowedOn,
    DateOnly? ReturnedOn);
