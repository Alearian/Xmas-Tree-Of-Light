<#
.SYNOPSIS
  Finds the Xmas tree's WLED board and says whether it is online.
.DESCRIPTION
  Tries each address in turn (home Wi-Fi IP, mDNS name, WLED-AP), then falls back to looking up the
  board's MAC address in the ARP table. Prints the address that answered with firmware, uptime and
  free heap (with -Quiet it returns them as an object instead). Exit code 0 = found, 1 = not found.
.EXAMPLE
  .\Test-Tree.ps1
  .\Test-Tree.ps1 -Address xmastree.local -Quiet
#>
[CmdletBinding()]
param(
    [string[]]$Address = @('xmastree.local', '4.3.2.1'),
    [string]$Mac = '',   # board MAC as aa-bb-cc-dd-ee-ff (arp -a format); enables the ARP fallback
    [int]$TimeoutSec = 3,
    [switch]$Quiet
)

function Get-Info([string]$a) {
    try { Invoke-RestMethod -Uri "http://$a/json/info" -TimeoutSec $TimeoutSec -ErrorAction Stop }
    catch { $null }
}

$found = $null
foreach ($a in $Address) {
    if (-not $Quiet) { Write-Host "Trying $a ..." -NoNewline }
    $info = Get-Info $a
    if ($info) { if (-not $Quiet) { Write-Host ' answered' -ForegroundColor Green }; $found = $a; break }
    if (-not $Quiet) { Write-Host ' no answer' -ForegroundColor DarkGray }
}

if (-not $found -and $Mac -and (Get-Command arp -ErrorAction SilentlyContinue)) {
    # The IP may have changed (DHCP). Look the board up by MAC in the ARP table; it only appears there
    # if this PC has talked to it recently, so this is a best-effort fallback.
    $line = arp -a | Select-String -Pattern $Mac -SimpleMatch | Select-Object -First 1
    if ($line) {
        $ip = ($line.ToString().Trim() -split '\s+')[0]
        if (-not $Quiet) { Write-Host "MAC $Mac is at $ip, trying it ..." -NoNewline }
        $info = Get-Info $ip
        if ($info) { if (-not $Quiet) { Write-Host ' answered' -ForegroundColor Green }; $found = $ip }
        elseif (-not $Quiet) { Write-Host ' no answer' -ForegroundColor DarkGray }
    }
}

if (-not $found) {
    Write-Host ''
    Write-Host 'Tree board NOT found.' -ForegroundColor Red
    Write-Host 'Check: is the board powered (LED on the Dig-Quad)? Is this PC on the home Wi-Fi?'
    Write-Host 'If the board fell back to its own AP, join Wi-Fi "WLED-AP" (netsh wlan connect name="WLED-AP") and use 4.3.2.1.'
    exit 1
}

$up = [TimeSpan]::FromSeconds([double]$info.uptime)
$result = [pscustomobject]@{
    Address  = $found
    Name     = $info.name
    Version  = $info.ver
    Build    = $info.vid
    Uptime   = '{0}d {1:hh\:mm\:ss}' -f $up.Days, $up
    UptimeS  = [int]$info.uptime
    FreeHeap = $info.freeheap
    LEDs     = $info.leds.count
    FS       = "$($info.fs.u)/$($info.fs.t) KB"
    RSSI     = $info.wifi.rssi
    IP       = $info.ip
}
if (-not $Quiet) {
    Write-Host ($result | Format-List | Out-String).TrimEnd()
    if ($result.UptimeS -lt 600) {
        Write-Host "Warning: uptime is under 10 minutes - the board restarted recently. Find out why before saving anything." -ForegroundColor Yellow
    }
    if ($info.ver -ne '0.14.0') {
        Write-Host "Note: firmware is $($info.ver), not the expected 0.14.0." -ForegroundColor Yellow
    }
}
else { $result }   # -Quiet: return the object for scripts
exit 0
