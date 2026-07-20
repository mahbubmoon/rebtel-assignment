namespace Library.Domain.Entities;

public sealed record Book(
    string Id,
    string Isbn,
    string Title,
    string Author,
    int PageCount,
    int TotalCopies);
