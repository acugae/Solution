# ============================================================
# build.ps1 - Build NuGet e analisi warning per tutti i TFM
# ============================================================
# Utilizzo:
#   .\build.ps1                  -> build completo Release
#   .\build.ps1 -TFM net8.0     -> build solo per un TFM
#   .\build.ps1 -Log             -> salva output su build-log.txt
#   .\build.ps1 -WarningsOnly    -> mostra solo warning ed errori
# ============================================================

param(
	[string] $TFM          = "",
	[switch] $Log,
	[switch] $WarningsOnly,
	[switch] $Clean,
	[switch] $PackOnly
)

$Project   = "Solution.csproj"
$Config    = "Release"
$LogFile   = "build-log.txt"
$TFMs      = @("netstandard2.1","net5.0","net6.0","net7.0","net8.0","net9.0","net10.0")

function Invoke-Build {
	param([string] $Framework = "")

	$args = @("build", $Project, "-c", $Config, "-v", "normal")
	if ($Framework) { $args += @("-f", $Framework) }

	Write-Host ""
	if ($Framework) {
		Write-Host ">>> Build: $Framework" -ForegroundColor Cyan
	} else {
		Write-Host ">>> Build: tutti i TFM" -ForegroundColor Cyan
	}

	if ($Log) {
		dotnet @args 2>&1 | Tee-Object -FilePath $LogFile
		Write-Host "Log salvato in: $LogFile" -ForegroundColor DarkGray
	} elseif ($WarningsOnly) {
		dotnet @args 2>&1 | Select-String -Pattern ": warning |: error " | Sort-Object | Get-Unique
	} else {
		dotnet @args
	}
}

# ---- Clean ----
if ($Clean) {
	Write-Host ">>> Clean..." -ForegroundColor Yellow
	dotnet clean $Project -c $Config
}

# ---- Pack Only ----
if ($PackOnly) {
	Write-Host ">>> Pack (senza build)..." -ForegroundColor Magenta
	dotnet pack $Project -c $Config --no-build
	exit 0
}

# ---- Build ----
if ($TFM) {
	if ($TFM -notin $TFMs) {
		Write-Warning "TFM '$TFM' non riconosciuto. TFM validi: $($TFMs -join ', ')"
		exit 1
	}
	Invoke-Build -Framework $TFM
} else {
	Invoke-Build
}

# ---- Riepilogo warning significativi ----
if ($Log -and (Test-Path $LogFile)) {
	Write-Host ""
	Write-Host "=== Riepilogo warning significativi ===" -ForegroundColor Yellow

	$categories = @{
		"Nullable (CS86xx)"  = "CS86\d\d"
		"Obsolete (CS0618)"  = "CS0618|CS0619"
		"Deprecate .NET"     = "SYSLIB\d+"
		"NuGet compat"       = "NU1701"
		"XML doc (CS1591)"   = "CS1591"
	}

	foreach ($label in $categories.Keys) {
		$pattern = $categories[$label]
		$matches  = Select-String -Path $LogFile -Pattern $pattern
		$count    = ($matches | Measure-Object).Count
		if ($count -gt 0) {
			Write-Host "  [$count] $label" -ForegroundColor $(if ($count -gt 0) { "Red" } else { "Green" })
			$matches | Select-Object -ExpandProperty Line | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
		}
	}
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
