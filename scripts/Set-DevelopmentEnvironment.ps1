param(
    [string]$ConnectionString
)

$ErrorActionPreference = 'Stop'
$thuMucDuAn = Split-Path -Parent $PSScriptRoot
$duongDanProject = Join-Path $thuMucDuAn 'QLSK.csproj'
$cauHinhPhatTrien = Get-Content -LiteralPath (Join-Path $thuMucDuAn 'appsettings.Development.json') -Raw -Encoding UTF8 | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = $cauHinhPhatTrien.ConnectionStrings.Default
}
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw 'Set a development connection string with -ConnectionString or appsettings.Development.json.'
}

# Keep the JWT key outside the repository and command-line history.
$khoaKyTokenDangNhap = $env:Jwt__Key
if ([string]::IsNullOrWhiteSpace($khoaKyTokenDangNhap)) {
    [xml]$taiLieuProjectXml = Get-Content -LiteralPath $duongDanProject -Raw
    $maKhoBiMat = [string]$taiLieuProjectXml.Project.PropertyGroup.UserSecretsId
    if ([string]::IsNullOrWhiteSpace($maKhoBiMat)) {
        throw 'The project must define UserSecretsId.'
    }
    $duongDanTepBiMat = Join-Path $env:APPDATA "Microsoft/UserSecrets/$maKhoBiMat/secrets.json"
    if (Test-Path -LiteralPath $duongDanTepBiMat) {
        $cacGiaTriBiMat = Get-Content -LiteralPath $duongDanTepBiMat -Raw -Encoding UTF8 | ConvertFrom-Json
        $khoaKyTokenDangNhap = [string]$cacGiaTriBiMat.'Jwt:Key'
    }
    if ([string]::IsNullOrWhiteSpace($khoaKyTokenDangNhap)) {
        $cacByteKhoaKy = New-Object byte[] 64
        $boSinhSoNgauNhien = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try { $boSinhSoNgauNhien.GetBytes($cacByteKhoaKy) } finally { $boSinhSoNgauNhien.Dispose() }
        $khoaKyTokenDangNhap = [Convert]::ToBase64String($cacByteKhoaKy)
        @{ 'Jwt:Key' = $khoaKyTokenDangNhap } | ConvertTo-Json -Compress | & dotnet user-secrets set --project $duongDanProject
        if ($LASTEXITCODE -ne 0) { throw 'Could not save the development JWT key to User Secrets.' }
    }
}
if ([Text.Encoding]::UTF8.GetByteCount($khoaKyTokenDangNhap) -lt 32) {
    throw 'Jwt__Key or the Jwt:Key User Secret must contain at least 32 bytes.'
}

# These variables apply to this terminal and its child processes only.
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = $cauHinhPhatTrien.Urls
$env:ConnectionStrings__Default = $ConnectionString
$env:Jwt__Key = $khoaKyTokenDangNhap

Write-Host 'Development environment is ready in this terminal.'
Write-Host 'Loaded: ASPNETCORE_ENVIRONMENT, DOTNET_ENVIRONMENT, ASPNETCORE_URLS, ConnectionStrings__Default, Jwt__Key.'
Write-Host "Run: dotnet run --project `"$duongDanProject`""
Write-Host "Frontend: $($cauHinhPhatTrien.Urls)/ | Swagger: $($cauHinhPhatTrien.Urls)/swagger"
Write-Host 'PayOS and SMTP require your provider credentials to enable payments and email.'
