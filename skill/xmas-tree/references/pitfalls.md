# Known pitfalls, symptoms and fixes

Add to this file whenever something new bites (date, symptom, cause, fix).

## Contents
- Presets and saving
- Board and firmware
- Network and phones
- Effects and segments
- Tooling and permissions
- Hardware and power

## Presets and saving

| Symptom | Cause | Fix / prevention |
|---|---|---|
| `treetool looks` (or a `psave`) times out after 10 s; uptime resets; a preset is missing | presets.json near **64 KB**: WLED 0.14 reboots while writing it (seen 2026-10-10, 10-24 twice) | Stop - don't retry. Run the report: uptime, presets valid? Restore/compact from the backup with `scripts\Compress-Presets.ps1`. Keep the file **< 56 KB**. |
| presets.json grows although nothing was added | WLED blanks a replaced/deleted preset with whitespace and appends the new copy | Compact periodically (61 KB → 47 KB on 2026-10-24); save via `treetool looks` (one upload) rather than one-by-one |
| `/presets.json` doesn't parse | interrupted save | Restore the latest **valid** backup (see `treetool.md` "Backups") with `Compress-Presets.ps1 -Upload` |
| App "Save as look" refuses | file > 57,344 B (`PRESETS_MAX`) | Compact, or delete unused looks, then retry |
| A phone-made show vanished after `treetool looks` | `--force` used, or the show sat on an id looks.json uses | Run without `--force`; give looks.json entries ids that phones don't use |

## Board and firmware

| Symptom | Cause | Fix / prevention |
|---|---|---|
| Tempted to upgrade to WLED 16.x | 16.x boot-loops Dig-Quad v3r1 + QuinLED-ESP32 v1r3 (brownout at Wi-Fi start, WLED #5570) | **No 16.x before Christmas.** 0.15.4 by OTA is optional; 16.0.1 in January after USB recovery is set up and a 3V3 capacitor fitted. `docs\wled-16-upgrade-assessment.md` |
| Unexpected short uptime | reboot: oversized save, brownout, cfg write | Find the cause before writing again; check presets validity |
| Config change locked us out | bad `nw`/`ap` in `/json/cfg` | Prevention only: back up, ask first, send only the subtree. Recovery needs USB flash (not ready) |
| `{"ps": n}` does nothing | WLED 0.14 ignores loading the preset that's already current | Re-send segment 0's `fx` (how the app's Pause works), or load another preset first |
| No serial log | GPIO 1/3 are both UART and LED outputs | Expected; use the JSON API |

## Network and phones

| Symptom | Cause | Fix |
|---|---|---|
| Phone can't open `xmastree.local` | Android doesn't resolve mDNS `.local` names | Use `http://<board-ip>/tree.htm`; reserve that IP in the Virgin hub's DHCP |
| Board not on the home Wi-Fi | it couldn't join at boot → AP fallback | Join `WLED-AP`, use `http://4.3.2.1`. On Windows after a reboot: `netsh wlan connect name="WLED-AP"` |
| IP changed | DHCP | `Test-Tree.ps1` finds it by MAC `28:56:2f:55:35:8c` via `arp -a` |
| Phone shows an old app | browser cache | Check Config tab version; reload, clear site data, or re-add the home-screen icon |
| Claude can't reach the board | cloud and Cowork VM can't see the home LAN | Run the PowerShell on the PC, or use a Remote Control session in `D:\Source\Xmas` |
| AP open to neighbours | `apAlways` keeps WLED-AP up with the factory password `wled1234` | Ask Mr Furniss to change the AP password in WLED settings |

## Effects and segments

| Symptom | Cause | Fix |
|---|---|---|
| Meteor/snow look has a purple/rainbow background | no palette → WLED default Party | Set `pal` to `* Color Gradient` (or another explicit palette) |
| Dim bands look like solid lines or vanish | segment `bri` reapplied on every pixel write | Dim via colour, keep segment `bri` 255 |
| Extra segments refused / look half applied | 32-segment limit | 12 strips + ≤ 20 more; 19 bands max for `across` |
| Strips stay black after Laser Search | strip segments still frozen (`frz:true`) | Any normal look or app effect change unfreezes them; or send `frz:false` for ids 0-11 |
| Even-numbered strips run the wrong way | `rev` missing on down-running strips (2, 5, 8, 11) | `treetool apply` rebuilds segments; `test chase` to verify |
| Show swaps looks abruptly | WLED 0.14 changes effects instantly | Shows use preset 249 Fade down between looks (`fadeSeconds`) |

## Tooling and permissions

| Symptom | Cause | Fix |
|---|---|---|
| A board write from Claude is blocked on the PC | Mr Furniss's auto-permission check | Show him the command; he approves or runs it |
| `apply` times out after reboot (AP mode) | Windows didn't rejoin WLED-AP | Reconnect, re-run `apply` (safe to repeat) |
| Wrong layout applied | default `--layout` is `config\tree.json` | Always pass `--layout config\proto-tree.json` for now |
| `*.cloud.*` files | unmerged copies from the cloud folder | Mr Furniss decides merges; don't treat them as current |
| Can't write `C:\Users\Develepor\.claude\plans` from a cloud thread | protected folder | Write plans to `D:\Source\Xmas\docs\plans\` (junctioned there) |

## Hardware and power

- Strip: BTF-LIGHTING HD 12V WS2815 60L B IP67, GRB, ~3.3 A measured per 300-LED strip at full
  white (BTF rates 7.5 A). Measure before buying PSUs. Use `treetool test white` with the limiter
  raised, and a clamp meter.
- Final build: strip power never through the Dig-Quad (30 A board limit); each strip fed at the
  bottom through its own 5 A fuse; never join PSU V+; always join grounds.
- The WLED limiter (`abl.maxMa`) is an estimate, not protection. Fuses protect wiring.
- Double-check part numbers against the maker (an "HLG-480H-12A" that doesn't exist cost £27.80).
