$ErrorActionPreference = 'Stop'
$taskName = 'QLSK_REPORT_' + [Guid]::NewGuid().ToString('N').Substring(0,12)
$taskRoot = Join-Path $env:TEMP $taskName
New-Item -ItemType Directory -Path $taskRoot | Out-Null
$saved = @{}
$names = @('ConnectionStrings__QLSK','ASPNETCORE_ENVIRONMENT','QLSK__PrivateFiles','QLSK__Demo__Password','QLSK__BootstrapAdmin__Email','Logging__LogLevel__Default','Logging__LogLevel__Microsoft')
foreach ($name in $names) { $saved[$name] = [Environment]::GetEnvironmentVariable($name) }
function Query($database, $sql) {
    $connection = New-Object System.Data.SqlClient.SqlConnection("Server=(localdb)\MSSQLLocalDB;Database=$database;Integrated Security=True;TrustServerCertificate=True")
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $table = New-Object System.Data.DataTable
        $table.Load($command.ExecuteReader())
        return ,$table
    } finally { $connection.Dispose() }
}
function Snapshot {
    $rows = Query $taskName "SELECT t.name AS TableName, SUM(p.rows) AS [RowCount] FROM sys.tables t JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN (0,1) WHERE t.is_ms_shipped=0 GROUP BY t.name ORDER BY t.name"
    $counts = [ordered]@{}
    foreach ($row in $rows.Rows) { $counts[$row.TableName] = [long]$row.RowCount }
    $versions = Query $taskName "SELECT name, CONVERT(int,value) AS Version FROM sys.extended_properties WHERE class=0 AND name IN ('QLSK.SchemaVersion','QLSK.DemoSeedVersion')"
    $versionMap = @{}
    foreach ($row in $versions.Rows) { $versionMap[$row.name] = [int]$row.Version }
    return @{tables=$counts; versions=$versionMap; table_count=$counts.Count; total_rows=($counts.Values | Measure-Object -Sum).Sum}
}
$created = $false
try {
    $exists = Query master "SELECT COUNT(*) AS n FROM sys.databases WHERE name='$taskName'"
    if ($exists.Rows[0].n -ne 0) { throw 'Generated DB already exists; refusing to continue.' }
    $env:ConnectionStrings__QLSK = "Server=(localdb)\MSSQLLocalDB;Database=$taskName;Integrated Security=True;TrustServerCertificate=True"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:QLSK__PrivateFiles = Join-Path $taskRoot 'private'
    $env:QLSK__Demo__Password = [Guid]::NewGuid().ToString('N') + 'aA1!'
    $env:QLSK__BootstrapAdmin__Email = ''
    $env:Logging__LogLevel__Default = 'Warning'
    $env:Logging__LogLevel__Microsoft = 'Warning'
    $created = $true
    $log1 = & dotnet obj/bao-cao-build/QLSK.dll --qlsk-seed-demo 2>&1
    if ($LASTEXITCODE -ne 0) { throw 'First seed failed; logs retained only in process memory.' }
    $first = Snapshot
    $filesFirst = @(Get-ChildItem -LiteralPath $taskRoot -File -Recurse).Count
    $log2 = & dotnet obj/bao-cao-build/QLSK.dll --qlsk-seed-demo 2>&1
    if ($LASTEXITCODE -ne 0) { throw 'Second seed failed.' }
    $second = Snapshot
    $same = ($first.tables | ConvertTo-Json -Compress) -eq ($second.tables | ConvertTo-Json -Compress)
    if (!$same) { throw 'Seed was not idempotent by per-table row counts.' }
    $report = [ordered]@{executed_at=[DateTimeOffset]::Now.ToString('o'); database=$taskName; isolated=$true; source='obj/bao-cao-build/QLSK.dll'; command='dotnet QLSK.dll --qlsk-seed-demo'; first_run=$first; second_run=$second; unchanged_per_table_counts=$same; private_file_count_first=$filesFirst; private_file_count_second=@(Get-ChildItem -LiteralPath $taskRoot -File -Recurse).Count; cleanup='pending'}
} finally {
    if ($created -and $taskName -match '^QLSK_REPORT_[a-f0-9]{12}$') {
        [System.Data.SqlClient.SqlConnection]::ClearAllPools()
        $null = Query master "IF DB_ID('$taskName') IS NOT NULL BEGIN ALTER DATABASE [$taskName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$taskName]; END"
        if ($report) { $report.cleanup='own disposable database dropped; private fixture files retained in task temporary directory' }
    }
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $saved[$name]) }
}
$report | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 docs/bao-cao/bang-chung/isolated-demo-seed.json
Write-Output ('Tables: ' + $first.table_count + '; rows: ' + $first.total_rows + '; identical after rerun: ' + $same)

