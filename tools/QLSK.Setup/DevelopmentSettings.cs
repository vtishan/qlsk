using System.Text.Json;

namespace QLSK.Setup;

public static class DevelopmentSettings
{
    // Only called for an interactive terminal; automation must specify its target explicitly.
    public static string? FindConnectionString(string thuMucBatDauTimKiem)
    {
        for (var thuMucLamViec = new DirectoryInfo(thuMucBatDauTimKiem); thuMucLamViec is not null; thuMucLamViec = thuMucLamViec.Parent)
        {
            var duongDanTepCauHinh = Path.Combine(thuMucLamViec.FullName, "appsettings.Development.json");
            if (!File.Exists(Path.Combine(thuMucLamViec.FullName, "QLSK.csproj")) || !File.Exists(duongDanTepCauHinh)) continue;
            using var taiLieuCauHinh = JsonDocument.Parse(File.ReadAllText(duongDanTepCauHinh));
            if (taiLieuCauHinh.RootElement.TryGetProperty("ConnectionStrings", out var nhomChuoiKetNoi) &&
                nhomChuoiKetNoi.TryGetProperty("Default", out var giaTriKetNoiJson) && giaTriKetNoiJson.ValueKind == JsonValueKind.String)
            {
                var giaTriCanXuLy = giaTriKetNoiJson.GetString();
                return string.IsNullOrWhiteSpace(giaTriCanXuLy) ? null : giaTriCanXuLy;
            }
            return null;
        }
        return null;
    }
}
