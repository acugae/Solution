<#
.SYNOPSIS
    Censisce l'utilizzo della libreria NuGet 'Solution' e delle sue API critiche/deprecate
    su uno o piu repository consumer (.NET).

.EXAMPLE
    .\scripts\audit-consumers.ps1 -RootPath "C:\Users\gacunzo\Progetti"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$RootPath
)

$ErrorActionPreference = "Stop"

Write-Host "=== Censimento Consumer della libreria Solution ===" -ForegroundColor Cyan
Write-Host "Directory scansionata: $RootPath`n"

# 1. Trova tutti i .csproj che referenziano Solution
$csprojFiles = Get-ChildItem -Path $RootPath -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.git)\\' -and $_.Name -ne 'Solution.csproj' }

$consumers = @()
foreach ($proj in $csprojFiles) {
    $text = Get-Content -Path $proj.FullName -Raw -ErrorAction SilentlyContinue
    if ($text -match 'Include="Solution"' -or $text -match 'Solution\.csproj') {
        $version = if ($text -match 'Include="Solution"\s+Version="([^"]+)"') { $Matches[1] } else { "ProjectReference/CPM" }
        $consumers += [PSCustomObject]@{
            Project = $proj.Name
            Version = $version
            Path    = $proj.DirectoryName
        }
    }
}

if ($consumers.Count -eq 0) {
    Write-Host "Nessun progetto consumer trovato in $RootPath." -ForegroundColor Yellow
    exit 0
}

Write-Host "Progetti consumer trovati: $($consumers.Count)" -ForegroundColor Green
$consumers | Format-Table -AutoSize

# 2. Analizza moduli e chiamate ad alto rischio per ciascun progetto
$patterns = [ordered]@{
    "JWT.Read (legacy)"                  = 'JWT\.Read\s*\('
    "SymmetricCrypt.Encrypt (legacy)"    = 'SymmetricCryptAlgorithm|(?<!Secure)\.Encrypt\s*\('
    "AsymmetricCrypt (default DSA)"      = 'new\s+AsymmetricCryptAlgorithm\s*\(\s*\)'
    "CRUDFilter / CRUD.Find"             = 'CRUDFilter|CRUDFind|\.Find\s*\('
    "QueryBuilder (.From)"               = '\.From\s*\('
    "Persistence (Commander/Table)"      = 'Solution\.Persistence|new\s+Commander|new\s+Table\s*\('
    "DbOperations (Bulk)"                = 'BulkInsert|BulkUpdate|joinSql|otherSqlWhereCondition'
    "SolutionMapper"                     = 'Solution\.SolutionMapper|SolutionMapper'
    "Email / SMTP"                       = 'Solution\.Communication|new\s+Email\s*\('
    "FTP / SFTP"                         = 'new\s+FTP\s*\(|new\s+SFTP\s*\('
    "ZIP / FileManager"                  = 'new\s+ZIP\s*\(|FileManager\.'
    "Cache<T> / CacheManager"            = 'new\s+Cache<|CacheManager'
    "AddSolutionDB (Singleton)"          = 'AddSolutionDB.*Singleton'
}

Write-Host "`n=== Mappa Utilizzo Moduli & API Sensibili ===" -ForegroundColor Cyan
foreach ($consumer in $consumers) {
    Write-Host "`n[$($consumer.Project)] ($($consumer.Version))" -ForegroundColor Yellow
    $csFiles = Get-ChildItem -Path $consumer.Path -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '\\(bin|obj|\.git)\\' }

    $anyHit = $false
    foreach ($label in $patterns.Keys) {
        $hits = $csFiles | Select-String -Pattern $patterns[$label] -ErrorAction SilentlyContinue
        if ($hits) {
            $anyHit = $true
            Write-Host ("  - {0,-34}: {1} occorrenze" -f $label, $hits.Count)
        }
    }
    if (-not $anyHit) {
        Write-Host "  (nessun utilizzo diretto rilevato nei .cs)" -ForegroundColor DarkGray
    }
}
