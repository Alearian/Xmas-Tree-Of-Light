---
name: wled
description: Talk to the Xmas tree's WLED controller (QuinLED Dig-Quad V3 at http://xmastree.local) — read status, set colours/effects/brightness, back up and change device config. Use whenever the task involves the tree lights, WLED, the Dig-Quad, segments, presets or the LED JSON API.
---

# WLED controller

Device base URL: `http://xmastree.local` (home Wi-Fi, DHCP, currently <board-ip>). If the board can't find the home Wi-Fi at boot, it falls back to its own AP `WLED-AP` at `http://4.3.2.1`. If the name stops resolving, find the board by its MAC <board-mac> with `arp -a`.

## Read (always safe)
```powershell
Invoke-RestMethod http://xmastree.local/json/info    # version, LED count, uptime
Invoke-RestMethod http://xmastree.local/json/state   # on, bri, segments
Invoke-RestMethod http://xmastree.local/json/cfg     # full config (hw.led.ins = outputs)
Invoke-RestMethod http://xmastree.local/json/eff     # effect names (index = fx id)
```

## Change state (fine while testing)
POST JSON to `/json/state`:
```powershell
$b = @{ on = $true; bri = 80; seg = @(@{ id = 0; fx = 0; col = @(@(255,0,0)) }) } | ConvertTo-Json -Depth 5
Invoke-RestMethod -Method Post -Uri http://xmastree.local/json/state -Body $b -ContentType 'application/json'
```
- `on`, `bri` 0–255, `transition` (in units of 100 ms)
- `seg[]`: `id`, `start`, `stop`, `fx`, `sx` (speed), `ix` (intensity), `pal`, `col` (up to 3 RGB triples), `rev`
- `ps` loads a preset; `{"v":true}` returns the new state

## Change config (ASK FIRST)
1. Back up first: save `/json/cfg` to `device-backups/cfg-<yyyyMMdd-HHmmss>.json`.
2. Show the user the diff and get a yes.
3. POST only the changed subtree to `/json/cfg`. LED-output changes reboot the board.
- Never overwrite `nw` (Wi-Fi) or `ap` without explicit approval. It can lock us out; recovery then needs USB.

## Prefer TreeTool
For layout, config and test work, use `dotnet run --project tools/TreeTool -- <cmd>` (see `docs/software.md`) rather than hand-written requests. It backs up first and is safe to re-run. The layout is preset **10** ("Tree layout", the boot preset). Preset 1 ("Solid") belongs to the user.

## Hardware facts
- 4 outputs: GPIO 16, 3, 1, 4. GPIO 1 and 3 are also the UART, so there's no serial logging while they drive LEDs.
- Target: 12 × WS2815 5 m strips of 300 px, 3 per port in a zig-zag, so 4 × 900 px (starts 0 / 900 / 1800 / 2700, 3,600 in total), LED type WS281x. Strip n (1–12) occupies pixels `(n-1)*300 .. n*300-1`. Strips 2, 5, 8 and 11 run top→bottom (they need `rev` for effects that assume bottom→top).
- Target AP: `MatrixTree-WiFi`. It's still the factory `WLED-AP` for now.
- xLights streams over DDP (UDP 4048). E1.31 is the alternative.
- Full details: `docs/design-summary.md`. Unresolved issues: `docs/open-questions.md`.
