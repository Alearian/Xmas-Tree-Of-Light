<#
.SYNOPSIS
  Reads the tree's WLED board and reports its state, parameters and anything that looks wrong.
.DESCRIPTION
  Read-only. Fetches /json/info, /json/state, /json/cfg, /json/eff, /json/pal and /presets.json, then
  prints: firmware, uptime, heap, filesystem, LED buses, current limiter, boot preset, what's playing,
  the segments, the presets (looks / shows / reserved) with presets.json size and validity, the
  timers, and a list of warnings. With -Markdown, also writes the report to a .md file.
.EXAMPLE
  .\Get-TreeReport.ps1
  .\Get-TreeReport.ps1 -Address xmastree.local -Markdown D:\Source\Xmas\docs\board-report.md
#>
[CmdletBinding()]
param(
    [string]$Address = 'xmastree.local',
    [string]$Markdown
)

$ErrorActionPreference = 'Stop'
$b = "http://$Address"
$PresetsMax = 57344      # the app's PRESETS_MAX (56 KB)
$PresetsDanger = 61440   # board has rebooted mid-save near 64 KB
$Reserved = @{ 10 = 'Layout / boot'; 249 = 'Fade down'; 250 = 'Lights off' }

try {
    $info = Invoke-RestMethod "$b/json/info" -TimeoutSec 5
} catch {
    Write-Host "Board not reachable at $b. Run Test-Tree.ps1 to find it." -ForegroundColor Red
    exit 1
}
$state = Invoke-RestMethod "$b/json/state" -TimeoutSec 10
$cfg   = Invoke-RestMethod "$b/json/cfg"   -TimeoutSec 10
$eff   = Invoke-RestMethod "$b/json/eff"   -TimeoutSec 10
$pal   = Invoke-RestMethod "$b/json/pal"   -TimeoutSec 10

$warn = New-Object System.Collections.Generic.List[string]
$out  = New-Object System.Collections.Generic.List[string]
function Add([string]$s = '') { $out.Add($s) }

# --- Controller ---------------------------------------------------------------------------------
$up = [TimeSpan]::FromSeconds([double]$info.uptime)
Add "# Tree board report - $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
Add ''
Add '## Controller'
Add "| Item | Value |"
Add "|---|---|"
Add "| Address | $b ($($info.name), MAC $($info.mac)) |"
Add "| Firmware | WLED $($info.ver) build $($info.vid) |"
Add ("| Uptime | {0}d {1:hh\:mm\:ss} |" -f $up.Days, $up)
Add "| Free heap | $([math]::Round($info.freeheap / 1024)) KB |"
Add "| Filesystem | $($info.fs.u) / $($info.fs.t) KB used |"
Add "| Wi-Fi | RSSI $($info.wifi.rssi) dBm, signal $($info.wifi.signal) %, channel $($info.wifi.channel) |"
Add "| LEDs | $($info.leds.count) (max segments $($info.leds.maxseg)) |"
Add "| Current limiter | $($cfg.hw.led.maxpwr) mA budget at $($cfg.hw.led.ledma) mA/LED |"
Add "| Boot preset | $($cfg.def.ps) |"

if ([int]$info.uptime -lt 600) { $warn.Add("Uptime is $([int]$info.uptime) s: the board restarted recently. Find out why before saving.") }
if ($info.ver -ne '0.14.0') { $warn.Add("Firmware is $($info.ver), expected 0.14.0 (no 16.x before Christmas).") }
if ($info.fs.t -gt 0 -and ($info.fs.u / $info.fs.t) -gt 0.8) { $warn.Add("Filesystem is over 80 % full.") }
if ($info.freeheap -lt 61440) { $warn.Add("Free heap is low ($([math]::Round($info.freeheap / 1024)) KB).") }
if ($cfg.def.ps -ne 10) { $warn.Add("Boot preset is $($cfg.def.ps), expected 10 (Tree layout).") }

# --- LED outputs --------------------------------------------------------------------------------
Add ''
Add '## LED outputs'
Add '| # | GPIO | Start | Length | Type | Order |'
Add '|---|---|---|---|---|---|'
$i = 0
foreach ($bus in $cfg.hw.led.ins) {
    $i++
    Add "| $i | $($bus.pin -join ',') | $($bus.start) | $($bus.len) | $($bus.type) | $($bus.order) |"
}

# --- What's playing ------------------------------------------------------------------------------
Add ''
Add '## Now'
Add "Power **$(if ($state.on) {'on'} else {'off'})**, brightness $($state.bri), preset $($state.ps), playlist $($state.pl)."
Add ''
Add '| Seg | LEDs | Effect | Palette | Speed | Int. | Colour 1 | rev | on | bri | frz |'
Add '|---|---|---|---|---|---|---|---|---|---|---|'
foreach ($s in $state.seg) {
    $c1 = if ($s.col -and $s.col[0]) { '#' + (($s.col[0] | Select-Object -First 3 | ForEach-Object { '{0:x2}' -f [int]$_ }) -join '') } else { '' }
    Add "| $($s.id) | $($s.start)-$($s.stop) | $($eff[[int]$s.fx]) | $($pal[[int]$s.pal]) | $($s.sx) | $($s.ix) | $c1 | $($s.rev) | $($s.on) | $($s.bri) | $($s.frz) |"
}
$segCount = @($state.seg).Count
if ($segCount -gt 32) { $warn.Add("$segCount segments - over WLED's 32.") }
if (@($state.seg | Where-Object { $_.frz }).Count -gt 0 -and $state.ps -ne 33) {
    $warn.Add('Some segments are frozen (frz) but Laser Search is not showing - strips may stay black.')
}

# --- Presets ------------------------------------------------------------------------------------
Add ''
Add '## Presets'
$raw = $null; $presets = $null; $bytes = 0
try {
    $resp = Invoke-WebRequest "$b/presets.json" -UseBasicParsing -TimeoutSec 10
    $raw = $resp.Content
    $bytes = [Text.Encoding]::UTF8.GetByteCount($raw)
    $presets = $raw | ConvertFrom-Json
} catch {
    $warn.Add("presets.json could not be read or is not valid JSON: $($_.Exception.Message). Restore from a backup.")
}
if ($presets) {
    $trimmed = [regex]::Replace($raw, '\s', '').Length
    Add "presets.json: **$bytes bytes** ($([math]::Round($bytes / 1024, 1)) KB) of the 56 KB budget; valid JSON; about $($bytes - $trimmed) bytes are whitespace."
    if ($bytes -gt $PresetsDanger) { $warn.Add("presets.json is $bytes B - in the reboot zone (~64 KB). Compact it now (Compress-Presets.ps1) before any save.") }
    elseif ($bytes -gt $PresetsMax) { $warn.Add("presets.json is $bytes B - over the 56 KB limit; the app will refuse to save. Compact it.") }
    Add ''
    Add '| Id | Name | Kind |'
    Add '|---|---|---|'
    $ids = @()
    $sorted = $presets.PSObject.Properties | Where-Object { $_.Name -match '^\d+$' } | Sort-Object { [int]$_.Name }
    foreach ($p in $sorted) {
        $id = [int]$p.Name
        if ($id -eq 0) { continue }
        $ids += $id
        $kind = if ($Reserved.ContainsKey($id)) { "reserved: $($Reserved[$id])" }
                elseif ($p.Value.playlist) { "show ($(@($p.Value.playlist.ps).Count) looks)" }
                else { 'look' }
        Add "| $id | $($p.Value.n) | $kind |"
    }
    foreach ($r in $Reserved.Keys) { if ($ids -notcontains $r) { $warn.Add("Reserved preset $r ($($Reserved[$r])) is missing. 'treetool looks' recreates 249/250; 'treetool apply' recreates 10.") } }
}

# --- Timers -------------------------------------------------------------------------------------
Add ''
Add '## Timers'
$timers = @($cfg.timers.ins | Where-Object { $_.en -eq 1 -or $_.en -eq $true })
if ($timers.Count -eq 0) { Add 'No timers enabled.' }
else {
    Add '| Time | Preset | Days mask |'
    Add '|---|---|---|'
    foreach ($t in $timers) { Add ("| {0:00}:{1:00} | {2} | {3} |" -f [int]$t.hour, [int]$t.min, $t.macro, $t.dow) }
}

# --- Warnings -----------------------------------------------------------------------------------
Add ''
Add '## Warnings'
if ($warn.Count -eq 0) { Add 'None.' } else { foreach ($w in $warn) { Add "- $w" } }

$text = $out -join [Environment]::NewLine
Write-Output $text
if ($Markdown) {
    [IO.File]::WriteAllText($Markdown, $text, (New-Object Text.UTF8Encoding($false)))
    Write-Host "Report written to $Markdown"
}
if ($warn.Count -gt 0) { exit 2 } else { exit 0 }
