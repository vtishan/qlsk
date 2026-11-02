param(
    [ValidateSet('Create', 'Update', 'AdoptLegacy')][string]$Operation = 'Update',
    [int]$AdminId,
    [string]$Email
)
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__Default)) {
    throw 'Set ConnectionStrings__Default for the target application database first.'
}
if ($Operation -eq 'Update' -and $AdminId -le 0) {
    throw 'Pass -AdminId with the existing administrator ID.'
}
if ([string]::IsNullOrWhiteSpace($Email)) { $Email = Read-Host 'Administrator email' }
$matKhauBaoMat = Read-Host 'New administrator password (at least 12 characters, three character groups)' -AsSecureString
$thongTinXacThucQuanTri = New-Object System.Management.Automation.PSCredential($Email, $matKhauBaoMat)
$emailQuanTriTruocDo = $env:QLSK_ADMIN_EMAIL
$matKhauQuanTriTruocDo = $env:QLSK_ADMIN_PASSWORD
$taiKhoanQuanTriTruocDoId = $env:QLSK_ADMIN_ID
try {
    $env:QLSK_ADMIN_EMAIL = $Email
    $env:QLSK_ADMIN_PASSWORD = $thongTinXacThucQuanTri.GetNetworkCredential().Password
    $env:QLSK_ADMIN_ID = [string]$AdminId
    $lenhThietLapQuanTri = switch ($Operation) { 'Create' { 'create-admin' } 'Update' { 'update-admin' } 'AdoptLegacy' { 'adopt-legacy-data' } }
    $duongDanProjectThietLap = Join-Path $PSScriptRoot '../tools/QLSK.Setup/QLSK.Setup.csproj'
    & dotnet run --project $duongDanProjectThietLap --configuration Release -- $lenhThietLapQuanTri
    if ($LASTEXITCODE -ne 0) { throw 'Administrator operation failed. No credentials were printed.' }
}
finally {
    $env:QLSK_ADMIN_EMAIL = $emailQuanTriTruocDo
    $env:QLSK_ADMIN_PASSWORD = $matKhauQuanTriTruocDo
    $env:QLSK_ADMIN_ID = $taiKhoanQuanTriTruocDoId
    $matKhauBaoMat.Dispose()
    $thongTinXacThucQuanTri = $null
}
