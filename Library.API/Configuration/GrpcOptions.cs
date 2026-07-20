namespace Library.API.Configuration;

public sealed class GrpcOptions
{
    public const string SectionName = "Grpc";

    public string LibraryServiceUrl { get; init; } = "http://localhost:5138";
}
