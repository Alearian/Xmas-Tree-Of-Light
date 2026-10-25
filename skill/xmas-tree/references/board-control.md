# Controlling the board (WLED 0.14 JSON API)

All examples are Windows PowerShell 5.1+ and run on Mr Furniss's PC. `$b` is the board address.

```powershell
$b = 'http://<board-ip>'          # or http://xmastree.local on the PC, http://4.3.2.1 on WLED-AP
```

## Contents
1. Read (safe)
2. Change live state
3. Presets and playlists
4. Timers and the clock
5. Config (ask first)
6. Files on the board
7. Segment map

## 1. Read (safe)

| Endpoint | What it gives |
|---|---|
| `/json/info` | `ver`, `vid` (build), `uptime` s, `freeheap`, `fs.u`/`fs.t` KB, `leds.count`, `leds.maxseg` (32), `wifi.rssi`, `ip`, `mac` |
| `/json/state` | `on`, `bri`, `ps` (current preset, -1 none), `pl` (playlist, -1 none, 0 = unsaved inline), `seg[]` |
| `/json/cfg` | full config: `hw.led.ins[]` (buses), `hw.led.maxpwr`/`ledma` (limiter), `def.ps` (boot preset), `timers`, `nw`, `ap` |
| `/json/eff`, `/json/pal` | effect and palette names; the index is the id |
| `/json/fxdata` | per-effect slider/option metadata (which effects have overlay, palette, colours) |
| `/presets.json` | every saved preset (looks, shows, reserved) |
| `/json/live` | live pixel sample (what the app's preview uses) |

```powershell
Invoke-RestMethod "$b/json/info" | Select-Object ver, uptime, freeheap, @{n='fs';e={"$($_.fs.u)/$($_.fs.t) KB"}}
(Invoke-RestMethod "$b/json/state").seg | Format-Table id, start, stop, fx, pal, on, bri, frz -AutoSize
```

## 2. Change live state (fine while testing, gone on reboot)

```powershell
function Send-Tree($body) {
  Invoke-RestMethod -Method Post -Uri "$b/json/state" -ContentType 'application/json' `
    -Body ($body | ConvertTo-Json -Depth 8 -Compress)
}
Send-Tree @{ on = $true; bri = 80 }                         # power + master brightness
Send-Tree @{ seg = @(@{ id = 0; fx = 0; col = @(@(255,0,0)) }) }   # strip 1 solid red
Send-Tree @{ ps = 27 }                                       # show a saved look
Send-Tree @{ ps = 250 }                                      # Lights off
```

Segment keys: `id`, `start`, `stop`, `fx`, `sx` speed, `ix` intensity, `pal`, `col` (up to 3 RGB
triples), `rev`, `on`, `bri`, `frz` (freeze), `grp`/`spc` (grouping/spacing), `o1`/`o2`/`o3` (effect
options, `o2` = overlay on Drip/Glitter/Sparkle). Add `"v": true` to get the new state back.

To touch all 12 strips, send 12 segment objects (ids 0-11). Segments 12-31 are overlays or height
bands; a normal look deletes them (`{"id":n,"stop":0}`).

**WLED 0.14 quirk:** `{"ps": n}` is ignored when n is already the current preset. To "pause" a
playlist, re-send segment 0's own `fx`; this unloads the playlist and leaves the look up.

## 3. Presets and playlists

- A preset with a `playlist` key is a **show**; anything else is a **look**. Ids 1-249 are free for
  use except the reserved ones: **10** layout/boot, **249** Fade down, **250** Lights off.
- Save one preset: add `"psave": id, "n": "Name", "o": true` (o = save only segment options, no
  on/bri) to a state body. **Avoid saving presets one by one** - WLED blanks the old copy with
  whitespace and appends the new one, so the file grows each time. TreeTool's `looks` builds the whole
  file and uploads it once instead.
- Delete: `{"pdel": id}` (also leaves whitespace).
- Playlist body: `{"playlist":{"ps":[20,27],"dur":[600,600],"transition":[20,20],"repeat":0,"end":250}}`
  (`dur`/`transition` in tenths of a second, `repeat` 0 = forever).
- Before any preset write, check the size: `(Invoke-WebRequest "$b/presets.json").RawContentLength`.
  Over 57,344 B → compact first (`scripts/Compress-Presets.ps1`).

## 4. Timers and the clock

Daily schedules are WLED timers in `cfg.timers.ins[]`: `{ "en":1, "hour":17, "min":0, "macro":40,
"dow":127 }`, each start timer followed by one with `macro` 250 for lights-off. The app writes only
`{"timers":{...}}` to `/json/cfg` with all 8 slots padded. NTP is on (GMT/BST); the app also sends the
phone's time (`{"time": <unix>}`) when it opens. A timers-only cfg POST doesn't reboot the board.

## 5. Config - ask Mr Furniss first

1. Back up (`treetool backup` or `scripts/Backup-Tree.ps1`).
2. Show him the change (old → new) and get a yes.
3. POST **only** the changed subtree to `/json/cfg`. LED-output changes reboot the board.
4. Never send `nw` (Wi-Fi) or `ap` unless he explicitly asks; layouts with
   `network.apAlways` make `treetool apply` send only `ap.behav = 2`.

For the LED outputs and limiter, prefer `treetool plan` / `treetool apply` over hand-written cfg.

## 6. Files on the board (960 KB LittleFS, shared with cfg/presets)

```powershell
curl.exe -s -F "data=@app\tree.htm;filename=/tree.htm" "$b/upload"        # upload / replace
Invoke-RestMethod "$b/edit?list=/"                                           # list files (0.14)
```

Files: `/cfg.json`, `/presets.json`, `/tree.htm` (~110 KB), `/tree.json` (the layout). Phones load
`/tree.htm`; large files served to several phones at once can exhaust heap - keep the app lean.

## 7. Segment map (proto-tree / tree layouts)

- 4 buses: GPIO 16, 3, 1, 4, each 900 LEDs, starts 0 / 900 / 1800 / 2700. GPIO 3 and 1 are the UART,
  so no serial log while they drive LEDs.
- Strip *n* (1-12) = segment *n-1* = LEDs `(n-1)*300 .. n*300-1`. Strips zig-zag within each output,
  so strips 2, 5, 8, 11 run top→bottom and their segments have `rev: true`. LED 0 of every segment is
  at the **bottom**.
- Strips are numbered clockwise from north; groups N/E/S/W are 3 strips each.
- `attached` in the layout lists the strips actually wired (currently 1-3). WLED still renders all 12.
