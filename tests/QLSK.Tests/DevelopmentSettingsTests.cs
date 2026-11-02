using QLSK.Setup;
using Xunit;

namespace QLSK.Tests;

public sealed class DevelopmentSettingsTests : IDisposable
{
    private readonly DirectoryInfo thuMucLamViec = Directory.CreateTempSubdirectory("QLSK_Setup_Config_");

    [Fact]
    public void Finds_development_database_from_a_nested_tool_directory()
    {
        GhiTepCauHinhKiemThu("QLSK.csproj", "<Project />");
        GhiTepCauHinhKiemThu("appsettings.Development.json", """
            { "ConnectionStrings": { "Default": "Server=.;Database=QLSK_Dev_Setup;Trusted_Connection=True" } }
            """);
        var thuMucLongNhau = Directory.CreateDirectory(Path.Combine(thuMucLamViec.FullName, "tools", "QLSK.Setup"));

        Assert.Equal("Server=.;Database=QLSK_Dev_Setup;Trusted_Connection=True",
            DevelopmentSettings.FindConnectionString(thuMucLongNhau.FullName));
    }

    [Fact]
    public void Does_not_use_settings_outside_a_source_checkout()
    {
        GhiTepCauHinhKiemThu("appsettings.Development.json", """
            { "ConnectionStrings": { "Default": "Server=.;Database=QLSK_Dev_Setup" } }
            """);

        Assert.Null(DevelopmentSettings.FindConnectionString(thuMucLamViec.FullName));
    }

    [Fact]
    public void Does_not_use_production_settings_as_a_development_database()
    {
        GhiTepCauHinhKiemThu("QLSK.csproj", "<Project />");
        GhiTepCauHinhKiemThu("appsettings.Production.json", """
            { "ConnectionStrings": { "Default": "Server=.;Database=Production" } }
            """);

        Assert.Null(DevelopmentSettings.FindConnectionString(thuMucLamViec.FullName));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"ConnectionStrings\":{\"Default\":\" \"}}")]
    public void Missing_or_empty_default_does_not_select_a_database(string noiDungTepCauHinh)
    {
        GhiTepCauHinhKiemThu("QLSK.csproj", "<Project />");
        GhiTepCauHinhKiemThu("appsettings.Development.json", noiDungTepCauHinh);

        Assert.Null(DevelopmentSettings.FindConnectionString(thuMucLamViec.FullName));
    }

    private void GhiTepCauHinhKiemThu(string tenDoiTuong, string noiDungPhanHoi) => File.WriteAllText(Path.Combine(thuMucLamViec.FullName, tenDoiTuong), noiDungPhanHoi);

    public void Dispose() => thuMucLamViec.Delete(recursive: true);
}
