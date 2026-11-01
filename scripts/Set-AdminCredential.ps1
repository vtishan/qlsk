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
$securePassword = Read-Host 'New administrator password (at least 12 characters, three character groups)' -AsSecureString
$credential = New-Object System.Management.Automation.PSCredential($Email, $securePassword)
$previousEmail = $env:QLSK_ADMIN_EMAIL
$previousPassword = $env:QLSK_ADMIN_PASSWORD
$previousId = $env:QLSK_ADMIN_ID
try {
    $env:QLSK_ADMIN_EMAIL = $Email
    $env:QLSK_ADMIN_PASSWORD = $credential.GetNetworkCredential().Password
    $env:QLSK_ADMIN_ID = [string]$AdminId
    $command = switch ($Operation) { 'Create' { 'create-admin' } 'Update' { 'update-admin' } 'AdoptLegacy' { 'adopt-legacy-data' } }
    $project = Join-Path $PSScriptRoot '../tools/QLSK.Setup/QLSK.Setup.csproj'
    & dotnet run --project $project --configuration Release -- $command
    if ($LASTEXITCODE -ne 0) { throw 'Administrator operation failed. No credentials were printed.' }
}
finally {
    $env:QLSK_ADMIN_EMAIL = $previousEmail
    $env:QLSK_ADMIN_PASSWORD = $previousPassword
    $env:QLSK_ADMIN_ID = $previousId
    $securePassword.Dispose()
    $credential = $null
}
