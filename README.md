# Library API System

API-driven library analytics system built with .NET 8, ASP.NET Core minimal APIs, gRPC, and a seeded simulated SQL repository.

## Architecture

- `Library.API`: Public HTTP API for librarians.
- `Library.Grpc`: Internal gRPC service used by the API layer.
- `Library.Core`: Business layer and Domain layer, merged for simplicity.
- `Library.Infrastructure`: Simulated SQL persistence with deterministic sample data.
- `tests/*`: Unit, integration, functional, and system tests.

The HTTP API communicates with the service layer through the `LibraryAnalytics` gRPC contract in `Library.Grpc/Protos/library.proto`.

## HTTP Endpoints

Base URL when using the included launch profile: `http://localhost:5217`.

- `GET /api/inventory/most-borrowed-books?limit=10`
- `GET /api/users/top-borrowers?from=2026-01-01&to=2026-06-30&limit=10`
- `GET /api/users/{userId}/reading-pace?bookId={bookId}`
- `GET /api/books/{bookId}/also-borrowed?limit=10`

Useful seeded IDs:

- Users: 11, 12, 13, 14, 15
- Books: 1, 2, 3, 4, 5, 6

## Run Locally

```bash
dotnet restore RebtelAssignment.sln
dotnet run --project Library.Grpc --launch-profile http
```

In a second terminal:

```bash
dotnet run --project Library.API --launch-profile http
```

Swagger is available at:

```text
http://localhost:5217/swagger
```

The API is configured to call the gRPC service at `http://localhost:5138`.

## Run Tests

```bash
dotnet test RebtelAssignment.sln
```

Test coverage is split by intent:

- Unit tests: `tests/Library.UnitTests`
- Integration tests: `tests/Library.IntegrationTests`
- Functional tests: `tests/Library.FunctionalTests`
- System tests: `tests/Library.SystemTests`

## Notes

The assignment allows simulated database connections, so persistence is represented by `SqlLibraryRepository` with seeded data. Replacing it with a real SQL provider would only require a new `ILibraryRepository` implementation.
