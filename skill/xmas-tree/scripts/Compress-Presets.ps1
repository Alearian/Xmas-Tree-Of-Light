<#
.SYNOPSIS
  Validates and compacts the tree's presets.json, and optionally uploads the compact copy.
.DESCRIPTION
  WLED 0.14 leaves whitespace behind every time a preset is replaced or deleted, so presets.json grows,
  and near ~64 KB the board reboots while saving. This script removes whitespace OUTSIDE JSON strings
  only (names and values are untouched), checks the result still parses and holds the same presets,
  saves both copies under device-backups\, and with -Upload sends it to the board's /upload endpoint
  after asking, then reads it back to verify.

  Source: -Source <file> (e.g. a backup's presets.json, to restore it), otherwise the board's live file.
.EXAMPLE
  .\Compress-Presets.ps1                       # download, compact, save locally, report sizes
  .\Compress-Presets.ps1 -Upload               # ... and upload after confirmation
  .\Compress-Presets.ps1 -Source D:\Source\Xmas\device-backups\Dig-Quad-A-20261025-091553\presets.json -Upload
#>
[CmdletBinding()]
param(
    [string]$Address = 'xmastree.local',
    [string]$Source,
    [string]$Repo = 'D:\Source\Xmas',
    [switch]$Upload,
    [switch]$Yes
)

$ErrorActionPreference = 'Stop'
$b = "http://$Address"
$utf8 = New-Object Text.UTF8Encoding($false)
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$outDir = Join-Path $Repo 'device-backups'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

# 1. Read the source text exactly.
if ($Source) {
    $text = [IO.File]::ReadAllText((Resolve-Path $Source), $utf8)
    Write-Host "Source: $Source"
} else {
    $before = Join-Path $outDir "presets-$stamp-before-compact.json"
    Invoke-WebRequest "$b/presets.json" -UseBasicParsing -TimeoutSec 15 -OutFile $before
    $text = [IO.File]::ReadAllText($before, $utf8)
    Write-Host "Source: board $b (saved as $before)"
}

# 2. It must be valid JSON to start with.
try { $orig = $text | ConvertFrom-Json }
catch { Write-Host "Source is NOT valid JSON: $($_.Exception.Message). Pick an earlier backup with -Source." -ForegroundColor Red; exit 1 }

# 3. Strip whitespace outside strings.
$sb = New-Object Text.StringBuilder ($text.Length)
$inStr = $false; $esc = $false
foreach ($ch in $text.ToCharArray()) {
    if ($inStr) {
        [void]$sb.Append($ch)
        if ($esc) { $esc = $false }
        elseif ($ch -eq '\') { $esc = $true }
        elseif ($ch -eq '"') { $inStr = $false }
    } elseif ($ch -eq '"') {
        $inStr = $true; [void]$sb.Append($ch)
    } elseif (-not [char]::IsWhiteSpace($ch)) {
        [void]$sb.Append($ch)
    }
}
$compact = $sb.ToString()

# 4. Same presets after compaction?
$new = $compact | ConvertFrom-Json
$a = @($orig.PSObject.Properties.Name) -join ','
$c = @($new.PSObject.Properties.Name) -join ','
if ($a -ne $c) { Write-Host 'Preset ids differ after compaction - aborting.' -ForegroundColor Red; exit 1 }

$oldBytes = $utf8.GetByteCount($text); $newBytes = $utf8.GetByteCount($compact)
$compactFile = Join-Path $outDir "presets-$stamp-compact.json"
[IO.File]::WriteAllText($compactFile, $compact, $utf8)
Write-Host ("{0:N0} B -> {1:N0} B ({2} presets). Saved {3}" -f $oldBytes, $newBytes, @($new.PSObject.Properties).Count, $compactFile)
if ($newBytes -gt 57344) {
    Write-Host 'Still over 56 KB after compaction: remove unused looks or slim look bodies before uploading.' -ForegroundColor Yellow
}

if (-not $Upload) { exit 0 }

# 5. Upload (writes the board's presets) - confirm first.
if (-not $Yes) {
    $ans = Read-Host "Upload this presets.json to $b, replacing the board's copy? (y/N)"
    if ($ans -notmatch '^(y|yes)$') { Write-Host 'Not uploaded.'; exit 0 }
}
& curl.exe -s -S -F "data=@$compactFile;filename=/presets.json" "$b/upload" | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Host 'Upload failed.' -ForegroundColor Red; exit 1 }

# 6. Read back and verify.
Start-Sleep -Seconds 1
$back = (Invoke-WebRequest "$b/presets.json" -UseBasicParsing -TimeoutSec 15).Content
try { $back | ConvertFrom-Json | Out-Null } catch { Write-Host 'Board copy does not parse after upload!' -ForegroundColor Red; exit 1 }
$info = Invoke-RestMethod "$b/json/info" -TimeoutSec 5
Write-Host ("Uploaded. Board presets.json is {0:N0} B and valid; uptime {1} s." -f $utf8.GetByteCount($back), $info.uptime) -ForegroundColor Green
