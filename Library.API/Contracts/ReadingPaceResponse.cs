namespace Library.API.Contracts;

public sealed record ReadingPaceResponse(
    long UserId,
    long BookId,
    string BookTitle,
    int PageCount,
    string BorrowedOn,
    string ReturnedOn,
    int ReadingDays,
    double PagesPerDay);
