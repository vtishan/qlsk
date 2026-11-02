using Microsoft.Data.SqlClient;

namespace QLSK.LocalData;

public static class LocalSqlGuard
{
    public static void KiemTraKetNoiSqlLocal(string chuoiKetNoiCoSoDuLieu, bool requireTestDatabase = false)
    {
        var boPhanTichChuoiKetNoi = new SqlConnectionStringBuilder(chuoiKetNoiCoSoDuLieu);
        var tenMayChuSql = boPhanTichChuoiKetNoi.DataSource.Trim();
        if (tenMayChuSql.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase) ||
            tenMayChuSql.StartsWith("lpc:", StringComparison.OrdinalIgnoreCase)) tenMayChuSql = tenMayChuSql[4..];
        var tenMayChuKhongCoInstance = tenMayChuSql.Split('\\', ',')[0];
        string[] danhSachTenDuLieuLocal = [".", "(local)", "(localdb)", "localhost", "127.0.0.1", "[::1]", Environment.MachineName];
        if (!danhSachTenDuLieuLocal.Contains(tenMayChuKhongCoInstance, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("This tool only accepts SQL Server instances on the local computer.");
        var tenCoSoDuLieu = boPhanTichChuoiKetNoi.InitialCatalog.Trim();
        if (string.IsNullOrWhiteSpace(tenCoSoDuLieu) ||
            new[] { "master", "model", "msdb", "tempdb" }.Contains(tenCoSoDuLieu, StringComparer.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(boPhanTichChuoiKetNoi.AttachDBFilename))
            throw new InvalidOperationException("A non-system local application database is required.");
        if (requireTestDatabase && !new[] { "QLSK_Test_", "QLSK_Dev_", "QLSK_Local" }
                .Any(tienToDuLieu => tenCoSoDuLieu.StartsWith(tienToDuLieu, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Population requires a separate QLSK_Test_*, QLSK_Dev_* or QLSK_Local* database.");
    }
}
