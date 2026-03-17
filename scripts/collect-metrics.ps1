<#
.SYNOPSIS
    Sammelt objektive Metriken aller CustomerTariffSwitch-Varianten und gibt Markdown-Tabellen aus.
.DESCRIPTION
    Erkennt automatisch alle Varianten (Unterordner mit .sln/.slnx).
    Generiert Abschnitt 1.1–1.7 der Vergleichsanalyse.
.EXAMPLE
    .\scripts\collect-metrics.ps1
    .\scripts\collect-metrics.ps1 -OutputFile "Vergleich-Metrics.md"
    .\scripts\collect-metrics.ps1 -SkipBuild -SkipRun
#>
param(
    [string]$OutputFile = "",
    [switch]$SkipBuild,
    [switch]$SkipRun
)

$ErrorActionPreference = "Continue"
$RepoRoot = Split-Path $PSScriptRoot -Parent

# --- Discovery ---

function Find-Variants {
    $dirs = Get-ChildItem -Path $RepoRoot -Directory |
        Where-Object { $_.Name -notmatch '^(scripts|Input Files|\.git|\.vs|\.claude)$' } |
        Where-Object {
            (Get-ChildItem -Path $_.FullName -Filter "*.sln" -Recurse -Depth 0).Count -gt 0 -or
            (Get-ChildItem -Path $_.FullName -Filter "*.slnx" -Recurse -Depth 0).Count -gt 0
        } |
        Sort-Object Name
    return $dirs
}

function Get-SolutionFile($variantDir) {
    $sln = Get-ChildItem -Path $variantDir -Filter "*.slnx" -Depth 0 | Select-Object -First 1
    if (-not $sln) {
        $sln = Get-ChildItem -Path $variantDir -Filter "*.sln" -Depth 0 | Select-Object -First 1
    }
    return $sln
}

function Get-MainProject($variantDir) {
    # Match on file NAME only, not full path (avoids false matches when variant folder contains "Test")
    $csprojs = Get-ChildItem -Path $variantDir -Filter "*.csproj" -Recurse
    $main = $csprojs | Where-Object { $_.Name -notmatch '(?i)test' } | Select-Object -First 1
    return $main
}

function Get-TestProject($variantDir) {
    $csprojs = Get-ChildItem -Path $variantDir -Filter "*.csproj" -Recurse
    return $csprojs | Where-Object { $_.Name -match '(?i)test' } | Select-Object -First 1
}

# --- File Classification ---
# Uses the actual test csproj location instead of fragile regex heuristics.
# Previous regex broke when the variant folder name contained "Test" (e.g. CustomerTariffSwitch-Claude-Tests).

function Get-TestFiles($variantDir) {
    $testProj = Get-TestProject $variantDir
    if (-not $testProj) { return @() }
    $testProjDir = $testProj.DirectoryName
    Get-ChildItem -Path $testProjDir -Filter "*.cs" -Recurse |
        Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' -and $_.Name -ne "GlobalUsings.cs" }
}

function Get-SourceFiles($variantDir) {
    $testProj = Get-TestProject $variantDir
    $testProjDir = if ($testProj) { $testProj.DirectoryName } else { $null }
    Get-ChildItem -Path $variantDir -Filter "*.cs" -Recurse |
        Where-Object {
            $_.FullName -notmatch '\\obj\\|\\bin\\' -and
            $_.Name -ne "GlobalUsings.cs" -and
            (-not $testProjDir -or -not $_.FullName.StartsWith($testProjDir))
        }
}

# --- LOC Counting ---

function Measure-Loc($files) {
    $total = 0; $code = 0; $comments = 0; $blank = 0
    $inBlock = $false

    foreach ($f in $files) {
        $lines = Get-Content -Path $f.FullName -ErrorAction SilentlyContinue
        if (-not $lines) { continue }

        foreach ($line in $lines) {
            $total++
            $trimmed = $line.Trim()

            if ([string]::IsNullOrWhiteSpace($trimmed)) {
                $blank++
                continue
            }

            # Block comment tracking
            if ($inBlock) {
                $comments++
                if ($trimmed -match '\*/') { $inBlock = $false }
                continue
            }

            if ($trimmed -match '^\s*/\*') {
                $comments++
                if ($trimmed -notmatch '\*/') { $inBlock = $false; $inBlock = $true }
                continue
            }

            if ($trimmed.StartsWith("//")) {
                $comments++
                continue
            }

            $code++
        }
    }

    return @{
        Total    = $total
        Code     = $code
        Comments = $comments
        Blank    = $blank
    }
}

# --- Structural Complexity ---

function Measure-Structure($files) {
    $classes = 0; $enums = 0; $records = 0; $methods = 0

    foreach ($f in $files) {
        $content = Get-Content -Path $f.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        $classes  += ([regex]::Matches($content, '\b(class|sealed class|abstract class|static class|partial class)\s+\w+')).Count
        $enums    += ([regex]::Matches($content, '\benum\s+\w+')).Count
        $records  += ([regex]::Matches($content, '\brecord\s+\w+')).Count

        # Count methods: lines with access modifier + return type + name + opening paren
        # Exclude constructors (name == class name is ok, they're methods too in a sense)
        $lines = $content -split "`n"
        foreach ($l in $lines) {
            $t = $l.Trim()
            if ($t -match '^\s*(public|private|protected|internal|static|override|async|sealed)\s+' -and
                $t -match '\w+\s*\(' -and
                $t -notmatch '\b(class|enum|record|interface|new\s+\w+)\b' -and
                $t -notmatch '^\s*//' -and
                $t -notmatch '\bif\b|\bwhile\b|\bfor\b|\bforeach\b|\bcatch\b') {
                $methods++
            }
        }
    }

    return @{
        Classes = $classes
        Enums   = $enums
        Records = $records
        Methods = $methods
    }
}

# --- Build ---

function Invoke-Build($variantDir) {
    if ($SkipBuild) { return @{ Warnings = "-"; Errors = "-" } }

    $sln = Get-SolutionFile $variantDir
    if (-not $sln) { return @{ Warnings = "?"; Errors = "?" } }

    $output = & dotnet build $sln.FullName --nologo -v q 2>&1 | Out-String
    $warnings = ([regex]::Matches($output, ': warning ')).Count
    $errors   = ([regex]::Matches($output, ': error ')).Count

    return @{
        Warnings = $warnings
        Errors   = $errors
    }
}

# --- Tests ---

function Invoke-Tests($variantDir) {
    $testProj = Get-TestProject $variantDir
    if (-not $testProj) { return @{ Total = 0; Passed = 0; Failed = 0; TestFiles = 0 } }

    $testFiles = Get-TestFiles $variantDir
    $testFileCount = ($testFiles | Measure-Object).Count

    $output = & dotnet test $testProj.FullName --nologo -v n 2>&1 | Out-String

    $passed = 0; $failed = 0; $total = 0
    if ($output -match 'Passed:\s*(\d+)') { $passed = [int]$Matches[1] }
    if ($output -match 'Failed:\s*(\d+)') { $failed = [int]$Matches[1] }
    if ($output -match 'Total:\s*(\d+)')  { $total  = [int]$Matches[1] }

    # Fallback: if Total is 0 but Passed > 0
    if ($total -eq 0 -and $passed -gt 0) { $total = $passed + $failed }

    return @{
        Total     = $total
        Passed    = $passed
        Failed    = $failed
        TestFiles = $testFileCount
    }
}

# --- Runtime ---

function Measure-Runtime($variantDir) {
    if ($SkipRun) { return @{ Run1 = "-"; Run2 = "-" } }

    $mainProj = Get-MainProject $variantDir
    if (-not $mainProj) { return @{ Run1 = "?"; Run2 = "?" } }

    # Build first so --no-build works
    & dotnet build $mainProj.FullName --nologo -v q 2>&1 | Out-Null

    $times = @()
    for ($i = 0; $i -lt 2; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        & dotnet run --project $mainProj.FullName --no-build 2>&1 | Out-Null
        $sw.Stop()
        $times += $sw.ElapsedMilliseconds
    }

    return @{
        Run1 = "$($times[0]) ms"
        Run2 = "$($times[1]) ms"
    }
}

# --- Output Formats ---

function Get-OutputFormats($variantDir) {
    $outputDir = Join-Path $variantDir "Output"
    if (-not (Test-Path $outputDir)) { return @{ Json = $false; Csv = ""; Txt = "" } }

    $json = Test-Path (Join-Path $outputDir "decisions.json")
    $csvFiles  = Get-ChildItem -Path $outputDir -Filter "*.csv" -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }
    $txtFiles  = Get-ChildItem -Path $outputDir -Filter "*.txt" -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }

    return @{
        Json = $json
        Csv  = ($csvFiles -join ", ")
        Txt  = ($txtFiles -join ", ")
    }
}

# --- Markdown Generation ---

function Format-Table($headers, $rows) {
    # $headers = @("Metrik", "P1", "P2", ...)
    # $rows = @( @("Zeile1Col1", "V1", "V2", ...), ... )
    $sb = [System.Text.StringBuilder]::new()

    # Header row
    $sb.AppendLine("| $($headers -join ' | ') |") | Out-Null
    $sep = ($headers | ForEach-Object {
        if ($_ -eq $headers[0]) { "--------" } else { ":--:" }
    })
    $sb.AppendLine("| $($sep -join ' | ') |") | Out-Null

    # Data rows
    foreach ($row in $rows) {
        $sb.AppendLine("| $($row -join ' | ') |") | Out-Null
    }

    return $sb.ToString()
}

# ===================
# MAIN
# ===================

Write-Host "Searching for variants in $RepoRoot ..." -ForegroundColor Cyan
$variants = Find-Variants

if ($variants.Count -eq 0) {
    Write-Host "No variants found." -ForegroundColor Red
    exit 1
}

Write-Host "Found $($variants.Count) variants:" -ForegroundColor Green
$variants | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host ""

# Collect all metrics
$metrics = @{}
$index = 0

foreach ($v in $variants) {
    $index++
    $label = "P$index"
    Write-Host "[$index/$($variants.Count)] Collecting metrics for $($v.Name) ..." -ForegroundColor Yellow

    $sln = Get-SolutionFile $v.FullName
    $csprojs = Get-ChildItem -Path $v.FullName -Filter "*.csproj" -Recurse |
        Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' }
    $sourceFiles = Get-SourceFiles $v.FullName
    $testFiles   = Get-TestFiles $v.FullName

    $sourceLoc = Measure-Loc $sourceFiles
    $testLoc   = Measure-Loc $testFiles
    $structure = Measure-Structure $sourceFiles

    Write-Host "  LOC: $($sourceLoc.Total) source, $($testLoc.Total) test" -ForegroundColor DarkGray
    Write-Host "  Structure: $($structure.Classes) classes, $($structure.Enums) enums, $($structure.Records) records, $($structure.Methods) methods" -ForegroundColor DarkGray

    Write-Host "  Building ..." -ForegroundColor DarkGray
    $build = Invoke-Build $v.FullName

    Write-Host "  Testing ..." -ForegroundColor DarkGray
    $tests = Invoke-Tests $v.FullName
    Write-Host "  Tests: $($tests.Passed)/$($tests.Total) passed" -ForegroundColor DarkGray

    Write-Host "  Measuring runtime ..." -ForegroundColor DarkGray
    $runtime = Measure-Runtime $v.FullName

    $outputs = Get-OutputFormats $v.FullName

    $metrics[$label] = @{
        Name        = $v.Name
        SlnType     = if ($sln.Extension -eq ".slnx") { ".slnx" } else { ".sln" }
        Assemblies  = $csprojs.Count
        SourceFiles = ($sourceFiles | Measure-Object).Count
        TestFileCount = ($testFiles | Measure-Object).Count
        SourceLoc   = $sourceLoc
        TestLoc     = $testLoc
        Structure   = $structure
        Build       = $build
        Tests       = $tests
        Runtime     = $runtime
        Outputs     = $outputs
    }
}

# --- Generate Markdown ---

$labels = $metrics.Keys | Sort-Object
$headerRow = @("Metrik") + ($labels | ForEach-Object { "**$_ ($($metrics[$_].Name -replace 'CustomerTariffSwitch-?', '' | ForEach-Object { if($_) {$_} else {'Mensch'} }))**" })

$sb = [System.Text.StringBuilder]::new()
$sb.AppendLine("# Objektive Metriken (automatisch generiert)") | Out-Null
$sb.AppendLine("") | Out-Null
$sb.AppendLine("> **Generiert am:** $(Get-Date -Format 'dd.MM.yyyy HH:mm')") | Out-Null
$sb.AppendLine("> **Skript:** ``scripts/collect-metrics.ps1``") | Out-Null
$sb.AppendLine("") | Out-Null

# 1.1 Projektstruktur
$sb.AppendLine("## 1.1 Projektstruktur") | Out-Null
$sb.AppendLine("") | Out-Null
$rows = @(
    (@("**Assemblies/csproj**") + ($labels | ForEach-Object { $metrics[$_].Assemblies })),
    (@("**Source-Dateien**")    + ($labels | ForEach-Object { $metrics[$_].SourceFiles })),
    (@("**Test-Dateien**")      + ($labels | ForEach-Object { $metrics[$_].TestFileCount })),
    (@("**Solution-Typ**")      + ($labels | ForEach-Object { $metrics[$_].SlnType }))
)
$sb.AppendLine((Format-Table $headerRow $rows)) | Out-Null

# 1.2 LOC
$sb.AppendLine("## 1.2 Lines of Code (LOC)") | Out-Null
$sb.AppendLine("") | Out-Null
$rows = @(
    (@("**Source: Gesamtzeilen**") + ($labels | ForEach-Object { $metrics[$_].SourceLoc.Total })),
    (@("**Source: Code-Zeilen**")  + ($labels | ForEach-Object { $metrics[$_].SourceLoc.Code })),
    (@("**Source: Kommentare**")   + ($labels | ForEach-Object { $metrics[$_].SourceLoc.Comments })),
    (@("**Source: Leerzeilen**")   + ($labels | ForEach-Object { $metrics[$_].SourceLoc.Blank })),
    (@("**Test: Gesamtzeilen**")   + ($labels | ForEach-Object { $metrics[$_].TestLoc.Total })),
    (@("**Test: Code-Zeilen**")    + ($labels | ForEach-Object { $metrics[$_].TestLoc.Code }))
)
$sb.AppendLine((Format-Table $headerRow $rows)) | Out-Null

# 1.3 Strukturelle Komplexitaet
$sb.AppendLine("## 1.3 Strukturelle Komplexität") | Out-Null
$sb.AppendLine("") | Out-Null
$rows = @(
    (@("**Klassen**")      + ($labels | ForEach-Object { $metrics[$_].Structure.Classes })),
    (@("**Enums**")        + ($labels | ForEach-Object { $metrics[$_].Structure.Enums })),
    (@("**Records**")      + ($labels | ForEach-Object { $metrics[$_].Structure.Records })),
    (@("**Methoden (src)**") + ($labels | ForEach-Object { $metrics[$_].Structure.Methods }))
)
$sb.AppendLine((Format-Table $headerRow $rows)) | Out-Null

# 1.4 Tests
$sb.AppendLine("## 1.4 Tests") | Out-Null
$sb.AppendLine("") | Out-Null
$passRates = @{}
foreach ($l in $labels) {
    $t = $metrics[$l].Tests
    if ($t.Total -gt 0) { $passRates[$l] = "$([math]::Round($t.Passed / $t.Total * 100))%" }
    else { $passRates[$l] = "-" }
}
$rows = @(
    (@("**Testanzahl**")    + ($labels | ForEach-Object { $metrics[$_].Tests.Total })),
    (@("**Bestanden**")     + ($labels | ForEach-Object { $metrics[$_].Tests.Passed })),
    (@("**Fehlgeschlagen**") + ($labels | ForEach-Object { $metrics[$_].Tests.Failed })),
    (@("**Pass Rate**")     + ($labels | ForEach-Object { $passRates[$_] })),
    (@("**Test-Dateien**")  + ($labels | ForEach-Object { $metrics[$_].TestFileCount })),
    (@("**Test-LOC**")      + ($labels | ForEach-Object { $metrics[$_].TestLoc.Code }))
)
$sb.AppendLine((Format-Table $headerRow $rows)) | Out-Null

# 1.5 Laufzeit
$sb.AppendLine("## 1.5 Laufzeit") | Out-Null
$sb.AppendLine("") | Out-Null
$rows = @(
    (@("**Run 1 (Cold)**")         + ($labels | ForEach-Object { $metrics[$_].Runtime.Run1 })),
    (@("**Run 2 (Warm/Idempotent)**") + ($labels | ForEach-Object { $metrics[$_].Runtime.Run2 }))
)
$sb.AppendLine((Format-Table $headerRow $rows)) | Out-Null

# 1.6 Build-Qualitaet
$sb.AppendLine("## 1.6 Build-Qualität") | Out-Null
$sb.AppendLine("") | Out-Null
$rows = @(
    (@("**Compiler Warnings**") + ($labels | ForEach-Object { $metrics[$_].Build.Warnings })),
    (@("**Compiler Errors**")   + ($labels | ForEach-Object { $metrics[$_].Build.Errors }))
)
$sb.AppendLine((Format-Table $headerRow $rows)) | Out-Null

# 1.7 Output-Formate
$sb.AppendLine("## 1.7 Output-Formate") | Out-Null
$sb.AppendLine("") | Out-Null
$outHeaders = @("Projekt") + ($labels | ForEach-Object { $_ })
$outRows = @(
    (@("**decisions.json**") + ($labels | ForEach-Object { if ($metrics[$_].Outputs.Json) { "yes" } else { "no" } })),
    (@("**CSV Report**")     + ($labels | ForEach-Object { $o = $metrics[$_].Outputs.Csv; if ($o) { $o } else { "-" } })),
    (@("**Text Report**")    + ($labels | ForEach-Object { $o = $metrics[$_].Outputs.Txt; if ($o) { $o } else { "-" } }))
)
$sb.AppendLine((Format-Table $outHeaders $outRows)) | Out-Null

# --- Output ---

$result = $sb.ToString()

if ($OutputFile) {
    $outPath = if ([System.IO.Path]::IsPathRooted($OutputFile)) { $OutputFile } else { Join-Path $RepoRoot $OutputFile }
    Set-Content -Path $outPath -Value $result -Encoding UTF8
    Write-Host "`nMetrics written to: $outPath" -ForegroundColor Green
} else {
    $defaultPath = Join-Path $RepoRoot "Vergleich-Metrics.md"
    Set-Content -Path $defaultPath -Value $result -Encoding UTF8
    Write-Host "`nMetrics written to: $defaultPath" -ForegroundColor Green
}

Write-Host "Done." -ForegroundColor Cyan
