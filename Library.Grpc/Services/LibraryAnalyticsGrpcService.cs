using System.Globalization;
using Grpc.Core;
using Library.Core.Services;

namespace Library.Grpc.Services;

public sealed class LibraryAnalyticsGrpcService : LibraryAnalytics.LibraryAnalyticsBase
{
    private const string DateFormat = "yyyy-MM-dd";
    private readonly ILibraryAnalyticsService _analyticsService;

    public LibraryAnalyticsGrpcService(ILibraryAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public override async Task<MostBorrowedBooksResponse> GetMostBorrowedBooks(
        MostBorrowedBooksRequest request,
        ServerCallContext context)
    {
        var books = await _analyticsService.GetMostBorrowedBooksAsync(request.Limit, GetCancellationToken(context));
        var response = new MostBorrowedBooksResponse();

        response.Books.AddRange(books.Select(book => new BorrowedBookStatistic
        {
            BookId = book.BookId,
            Title = book.Title,
            Author = book.Author,
            BorrowCount = book.BorrowCount
        }));

        return response;
    }

    public override async Task<TopBorrowersResponse> GetTopBorrowers(
        TopBorrowersRequest request,
        ServerCallContext context)
    {
        var from = ParseDate(request.From, nameof(request.From));
        var to = ParseDate(request.To, nameof(request.To));
        var borrowers = await _analyticsService.GetTopBorrowersAsync(from, to, request.Limit, GetCancellationToken(context));
        var response = new TopBorrowersResponse();

        response.Borrowers.AddRange(borrowers.Select(borrower => new BorrowerActivity
        {
            BorrowerId = borrower.BorrowerId,
            Name = borrower.Name,
            BorrowCount = borrower.BorrowCount
        }));

        return response;
    }

    public override async Task<ReadingPaceResponse> EstimateReadingPace(
        ReadingPaceRequest request,
        ServerCallContext context)
    {
        var estimate = await _analyticsService.EstimateReadingPaceAsync(
            request.BorrowerId,
            request.BookId,
            GetCancellationToken(context));

        if (estimate is null)
        {
            return new ReadingPaceResponse
            {
                Found = false,
                BorrowerId = request.BorrowerId,
                BookId = request.BookId,
                Message = "No completed loan was found for this borrower and book."
            };
        }

        return new ReadingPaceResponse
        {
            Found = true,
            BorrowerId = estimate.BorrowerId,
            BookId = estimate.BookId,
            BookTitle = estimate.BookTitle,
            PageCount = estimate.PageCount,
            BorrowedOn = FormatDate(estimate.BorrowedOn),
            ReturnedOn = FormatDate(estimate.ReturnedOn),
            ReadingDays = estimate.ReadingDays,
            PagesPerDay = estimate.PagesPerDay
        };
    }

    public override async Task<AlsoBorrowedBooksResponse> GetAlsoBorrowedBooks(
        AlsoBorrowedBooksRequest request,
        ServerCallContext context)
    {
        var books = await _analyticsService.GetAlsoBorrowedBooksAsync(request.BookId, request.Limit, GetCancellationToken(context));
        var response = new AlsoBorrowedBooksResponse();

        response.Books.AddRange(books.Select(book => new AlsoBorrowedBook
        {
            BookId = book.BookId,
            Title = book.Title,
            Author = book.Author,
            CoBorrowerCount = book.CoBorrowerCount,
            BorrowCount = book.BorrowCount
        }));

        return response;
    }

    private static DateOnly ParseDate(string value, string fieldName)
    {
        if (DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new RpcException(new Status(StatusCode.InvalidArgument, $"{fieldName} must use {DateFormat}."));
    }

    private static string FormatDate(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static CancellationToken GetCancellationToken(ServerCallContext? context) =>
        context?.CancellationToken ?? CancellationToken.None;
}
