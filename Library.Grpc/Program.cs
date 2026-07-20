using Library.Grpc.Services;
using Library.Core.Repositories;
using Library.Core.Services;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Sql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("LibraryDatabase")
                           ?? "Data Source=library.db";
    return new SqliteConnectionFactory(connectionString);
});
builder.Services.AddSingleton<SqlLibraryDatabaseInitializer>();
builder.Services.AddScoped<ILibraryRepository, SqlLibraryRepository>();
builder.Services.AddScoped<ILibraryAnalyticsService, LibraryAnalyticsService>();

var app = builder.Build();

await app.Services.GetRequiredService<SqlLibraryDatabaseInitializer>().InitializeAsync();

app.MapGrpcService<LibraryAnalyticsGrpcService>();
app.MapGet("/",
    () =>
        "Library gRPC service is running. Use the HTTP API project for REST-style access.");

app.Run();
