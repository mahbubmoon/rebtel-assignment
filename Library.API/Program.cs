using System.Globalization;
using Grpc.Core;
using Library.API.Clients;
using Library.API.Configuration;
using Library.API.Contracts;
using Library.Grpc;
using Microsoft.Extensions.Options;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<GrpcOptions>(builder.Configuration.GetSection(GrpcOptions.SectionName));
builder.Services.AddGrpcClient<LibraryAnalytics.LibraryAnalyticsClient>((serviceProvider, options) =>
{
    var grpcOptions = serviceProvider.GetRequiredService<IOptions<GrpcOptions>>().Value;
    options.Address = new Uri(grpcOptions.LibraryServiceUrl);
});
builder.Services.AddScoped<ILibraryInsightsClient, GrpcLibraryInsightsClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (RpcException exception)
    {
        var statusCode = exception.StatusCode == StatusCode.InvalidArgument
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status502BadGateway;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ErrorResponse(exception.Status.Detail));
    }
});

app.MapGet("/", () => Results.Ok(new { status = "Library API is running" }))
    .WithName("Health");

var api = app.MapGroup("/api");

api.MapGet("/inventory/most-borrowed-books",
        async (int? limit, ILibraryInsightsClient client, CancellationToken cancellationToken) =>
        {
            var books = await client.GetMostBorrowedBooksAsync(limit ?? 10, cancellationToken);
            return Results.Ok(books);
        })
    .WithName("GetMostBorrowedBooks")
    .WithOpenApi();

api.MapGet("/users/top-borrowers",
        async (
            string from,
            string to,
            int? limit,
            ILibraryInsightsClient client,
            CancellationToken cancellationToken) =>
        {
            if (!TryParseDate(from, out var fromDate))
            {
                return Results.BadRequest(new ErrorResponse("Query parameter 'from' must use yyyy-MM-dd."));
            }

            if (!TryParseDate(to, out var toDate))
            {
                return Results.BadRequest(new ErrorResponse("Query parameter 'to' must use yyyy-MM-dd."));
            }

            if (toDate < fromDate)
            {
                return Results.BadRequest(new ErrorResponse("Query parameter 'to' must be on or after 'from'."));
            }

            var borrowers = await client.GetTopBorrowersAsync(fromDate, toDate, limit ?? 10, cancellationToken);
            return Results.Ok(borrowers);
        })
    .WithName("GetTopBorrowers")
    .WithOpenApi();

api.MapGet("/users/{userId}/reading-pace",
        async (
            long userId,
            long bookId,
            ILibraryInsightsClient client,
            CancellationToken cancellationToken) =>
        {
            var estimate = await client.EstimateReadingPaceAsync(userId, bookId, cancellationToken);
            return estimate is null
                ? Results.NotFound(new ErrorResponse("No completed loan was found for this user and book."))
                : Results.Ok(estimate);
        })
    .WithName("EstimateReadingPace")
    .WithOpenApi();

api.MapGet("/books/{bookId}/also-borrowed",
        async (long bookId, int? limit, ILibraryInsightsClient client, CancellationToken cancellationToken) =>
        {
            var books = await client.GetAlsoBorrowedBooksAsync(bookId, limit ?? 10, cancellationToken);
            return Results.Ok(books);
        })
    .WithName("GetAlsoBorrowedBooks")
    .WithOpenApi();

app.Run();

static bool TryParseDate(string value, out DateOnly date) =>
    DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

namespace Library.API
{
    public partial class Program
    {
    }
}
