using Microsoft.Data.SqlClient;

namespace QLSK.LocalData;

public static class LocalSqlGuard
{
    public static void Validate(string connectionString, bool requireTestDatabase = false)
    {
        var connection = new SqlConnectionStringBuilder(connectionString);
        var server = connection.DataSource.Trim();
        if (server.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase) ||
            server.StartsWith("lpc:", StringComparison.OrdinalIgnoreCase)) server = server[4..];
        var host = server.Split('\\', ',')[0];
        string[] localNames = [".", "(local)", "(localdb)", "localhost", "127.0.0.1", "[::1]", Environment.MachineName];
        if (!localNames.Contains(host, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("This tool only accepts SQL Server instances on the local computer.");
        var database = connection.InitialCatalog.Trim();
        if (string.IsNullOrWhiteSpace(database) ||
            new[] { "master", "model", "msdb", "tempdb" }.Contains(database, StringComparer.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(connection.AttachDBFilename))
            throw new InvalidOperationException("A non-system local application database is required.");
        if (requireTestDatabase && !new[] { "QLSK_Test_", "QLSK_Dev_", "QLSK_Local" }
                .Any(prefix => database.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Population requires a separate QLSK_Test_*, QLSK_Dev_* or QLSK_Local* database.");
    }
}
