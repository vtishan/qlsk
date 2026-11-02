param(
    [string]$ConnectionString
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $projectRoot 'QLSK.csproj'
$settings = Get-Content -LiteralPath (Join-Path $projectRoot 'appsettings.Development.json') -Raw -Encoding UTF8 | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = $settings.ConnectionStrings.Default
}
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw 'Set a development connection string with -ConnectionString or appsettings.Development.json.'
}

# Keep the JWT key outside the repository and command-line history.
$jwtKey = $env:Jwt__Key
if ([string]::IsNullOrWhiteSpace($jwtKey)) {
    [xml]$project = Get-Content -LiteralPath $projectPath -Raw
    $secretId = [string]$project.Project.PropertyGroup.UserSecretsId
    if ([string]::IsNullOrWhiteSpace($secretId)) {
        throw 'The project must define UserSecretsId.'
    }
    $secretPath = Join-Path $env:APPDATA "Microsoft/UserSecrets/$secretId/secrets.json"
    if (Test-Path -LiteralPath $secretPath) {
        $secrets = Get-Content -LiteralPath $secretPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $jwtKey = [string]$secrets.'Jwt:Key'
    }
    if ([string]::IsNullOrWhiteSpace($jwtKey)) {
        $bytes = New-Object byte[] 64
        $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
        $jwtKey = [Convert]::ToBase64String($bytes)
        @{ 'Jwt:Key' = $jwtKey } | ConvertTo-Json -Compress | & dotnet user-secrets set --project $projectPath
        if ($LASTEXITCODE -ne 0) { throw 'Could not save the development JWT key to User Secrets.' }
    }
}
if ([Text.Encoding]::UTF8.GetByteCount($jwtKey) -lt 32) {
    throw 'Jwt__Key or the Jwt:Key User Secret must contain at least 32 bytes.'
}

# These variables apply to this terminal and its child processes only.
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = $settings.Urls
$env:ConnectionStrings__Default = $ConnectionString
$env:Jwt__Key = $jwtKey

Write-Host 'Development environment is ready in this terminal.'
Write-Host 'Loaded: ASPNETCORE_ENVIRONMENT, DOTNET_ENVIRONMENT, ASPNETCORE_URLS, ConnectionStrings__Default, Jwt__Key.'
Write-Host "Run: dotnet run --project `"$projectPath`""
Write-Host "Frontend: $($settings.Urls)/ | Swagger: $($settings.Urls)/swagger"
Write-Host 'PayOS and SMTP require your provider credentials to enable payments and email.'
