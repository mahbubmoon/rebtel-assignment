using System.Reflection;
using Microsoft.Data.Sqlite;

namespace Library.Infrastructure.Persistence.Sql;

public sealed class SqlLibraryDatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqlLibraryDatabaseInitializer(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        EnsureDatabaseDirectoryExists();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await ExecuteScriptAsync(connection, "001_create_schema.sql", cancellationToken);
        await ExecuteScriptAsync(connection, "002_seed_demo_data.sql", cancellationToken);
    }

    private void EnsureDatabaseDirectoryExists()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionFactory.ConnectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource) ||
            builder.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(builder.DataSource));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static async Task ExecuteScriptAsync(
        SqliteConnection connection,
        string scriptName,
        CancellationToken cancellationToken)
    {
        var sql = await ReadEmbeddedScriptAsync(scriptName, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string> ReadEmbeddedScriptAsync(string scriptName, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith(scriptName, StringComparison.Ordinal));

        await using var stream = assembly.GetManifestResourceStream(resourceName)
                                 ?? throw new InvalidOperationException($"SQL script '{scriptName}' was not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
