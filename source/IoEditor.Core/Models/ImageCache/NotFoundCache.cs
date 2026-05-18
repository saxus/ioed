using Microsoft.Data.Sqlite;

namespace IoEditor.Models.ImageCache
{
    internal sealed class NotFoundCache
    {
        private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);

        private readonly string _dbPath;

        public NotFoundCache(string dbPath)
        {
            _dbPath = dbPath;
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

            using var conn = OpenConnection();
            await conn.OpenAsync();

            using var createCmd = conn.CreateCommand();
            createCmd.CommandText = """
                CREATE TABLE IF NOT EXISTS NotFoundCache (
                    Key       TEXT    PRIMARY KEY,
                    Timestamp INTEGER NOT NULL
                )
                """;
            await createCmd.ExecuteNonQueryAsync();

            var cutoff = DateTimeOffset.UtcNow.Subtract(RetentionPeriod).ToUnixTimeSeconds();
            using var cleanCmd = conn.CreateCommand();
            cleanCmd.CommandText = "DELETE FROM NotFoundCache WHERE Timestamp < $cutoff";
            cleanCmd.Parameters.AddWithValue("$cutoff", cutoff);
            await cleanCmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsNotFoundAsync(string key)
        {
            using var conn = OpenConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM NotFoundCache WHERE Key = $key LIMIT 1";
            cmd.Parameters.AddWithValue("$key", key);

            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }

        public async Task AddNotFoundAsync(string key)
        {
            using var conn = OpenConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO NotFoundCache (Key, Timestamp)
                VALUES ($key, $ts)
                ON CONFLICT(Key) DO UPDATE SET Timestamp = excluded.Timestamp
                """;
            cmd.Parameters.AddWithValue("$key", key);
            cmd.Parameters.AddWithValue("$ts", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ClearAsync()
        {
            using var conn = OpenConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM NotFoundCache";
            await cmd.ExecuteNonQueryAsync();
        }

        private SqliteConnection OpenConnection()
            => new SqliteConnection($"Data Source={_dbPath}");
    }
}
