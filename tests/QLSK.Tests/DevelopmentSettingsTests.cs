using QLSK.Setup;
using Xunit;

namespace QLSK.Tests;

public sealed class DevelopmentSettingsTests : IDisposable
{
    private readonly DirectoryInfo directory = Directory.CreateTempSubdirectory("QLSK_Setup_Config_");

    [Fact]
    public void Finds_development_database_from_a_nested_tool_directory()
    {
        Write("QLSK.csproj", "<Project />");
        Write("appsettings.Development.json", """
            { "ConnectionStrings": { "Default": "Server=.;Database=QLSK_Dev_Setup;Trusted_Connection=True" } }
            """);
        var nested = Directory.CreateDirectory(Path.Combine(directory.FullName, "tools", "QLSK.Setup"));

        Assert.Equal("Server=.;Database=QLSK_Dev_Setup;Trusted_Connection=True",
            DevelopmentSettings.FindConnectionString(nested.FullName));
    }

    [Fact]
    public void Does_not_use_settings_outside_a_source_checkout()
    {
        Write("appsettings.Development.json", """
            { "ConnectionStrings": { "Default": "Server=.;Database=QLSK_Dev_Setup" } }
            """);

        Assert.Null(DevelopmentSettings.FindConnectionString(directory.FullName));
    }

    [Fact]
    public void Does_not_use_production_settings_as_a_development_database()
    {
        Write("QLSK.csproj", "<Project />");
        Write("appsettings.Production.json", """
            { "ConnectionStrings": { "Default": "Server=.;Database=Production" } }
            """);

        Assert.Null(DevelopmentSettings.FindConnectionString(directory.FullName));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"ConnectionStrings\":{\"Default\":\" \"}}")]
    public void Missing_or_empty_default_does_not_select_a_database(string settings)
    {
        Write("QLSK.csproj", "<Project />");
        Write("appsettings.Development.json", settings);

        Assert.Null(DevelopmentSettings.FindConnectionString(directory.FullName));
    }

    private void Write(string name, string content) => File.WriteAllText(Path.Combine(directory.FullName, name), content);

    public void Dispose() => directory.Delete(recursive: true);
}
