namespace Library.Core.Entities;

public record Book(
    long Id,
    string Isbn,
    string Title,
    string Author,
    int PageCount,
    int TotalCopies);
