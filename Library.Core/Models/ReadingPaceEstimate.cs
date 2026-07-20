namespace Library.Core.Models;

public sealed record ReadingPaceEstimate(
    long BorrowerId,
    long BookId,
    string BookTitle,
    int PageCount,
    DateOnly BorrowedOn,
    DateOnly ReturnedOn,
    int ReadingDays,
    double PagesPerDay);
