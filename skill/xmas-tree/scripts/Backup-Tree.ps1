<#
.SYNOPSIS
  Takes a full read-only snapshot of the tree's WLED board into device-backups\.
.DESCRIPTION
  Saves cfg, state, info, eff, pal, fxdata, presets.json, and the app files tree.htm / tree.json to
  <Repo>\device-backups\<Name>-<yyyyMMdd-HHmmss>\, byte for byte as the board serves them.
  device-backups\ is git-ignored because cfg.json contains Wi-Fi passwords: never commit or share it.
.EXAMPLE
  .\Backup-Tree.ps1
  .\Backup-Tree.ps1 -Address 4.3.2.1 -Repo D:\Source\Xmas
#>
[CmdletBinding()]
param(
    [string]$Address = 'xmastree.local',
    [string]$Repo = 'D:\Source\Xmas',
    [string]$Name = 'Dig-Quad-A'
)

$ErrorActionPreference = 'Stop'
$b = "http://$Address"
try { Invoke-RestMethod "$b/json/info" -TimeoutSec 5 | Out-Null }
catch { Write-Host "Board not reachable at $b. Run Test-Tree.ps1 to find it." -ForegroundColor Red; exit 1 }

$dir = Join-Path $Repo ("device-backups\{0}-{1:yyyyMMdd-HHmmss}" -f $Name, (Get-Date))
New-Item -ItemType Directory -Path $dir -Force | Out-Null

$items = [ordered]@{
    'cfg.json'     = 'json/cfg'
    'state.json'   = 'json/state'
    'info.json'    = 'json/info'
    'eff.json'     = 'json/eff'
    'pal.json'     = 'json/pal'
    'fxdata.json'  = 'json/fxdata'
    'presets.json' = 'presets.json'
    'tree.htm'     = 'tree.htm'
    'tree.json'    = 'tree.json'
}
foreach ($k in $items.Keys) {
    try {
        Invoke-WebRequest "$b/$($items[$k])" -UseBasicParsing -TimeoutSec 15 -OutFile (Join-Path $dir $k)
        Write-Host ("  {0,-13} {1,8:N0} bytes" -f $k, (Get-Item (Join-Path $dir $k)).Length)
    } catch {
        Write-Host ("  {0,-13} not saved ({1})" -f $k, $_.Exception.Message) -ForegroundColor Yellow
    }
}

$p = Join-Path $dir 'presets.json'
if (Test-Path $p) {
    try { Get-Content $p -Raw | ConvertFrom-Json | Out-Null; Write-Host '  presets.json is valid JSON' -ForegroundColor Green }
    catch { Write-Host '  presets.json is NOT valid JSON - do not restore from this backup' -ForegroundColor Red }
}
Write-Host "Backed up to $dir"
$dir
