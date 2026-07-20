using Microsoft.Data.Sqlite;

namespace Library.Infrastructure.Persistence.Sql;

public sealed class SqliteConnectionFactory
{
    public SqliteConnectionFactory(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public SqliteConnection CreateConnection() => new(ConnectionString);
}
