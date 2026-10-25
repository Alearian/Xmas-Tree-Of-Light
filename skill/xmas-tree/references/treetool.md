# Using TreeTool

Run from the repo root `D:\Source\Xmas`. Always pass the layout the board is actually running
(today `config\proto-tree.json`); the default is `config\tree.json`, which is the final build.

```powershell
Set-Location D:\Source\Xmas
$tt = 'dotnet run --project tools\TreeTool --'
dotnet run --project tools\TreeTool -- info --layout config\proto-tree.json
```

| Command | Effect | Writes to board? |
|---|---|---|
| `info` | firmware, LED count, uptime, free heap, filesystem used/total | no |
| `backup` | `cfg.json`, `state.json`, `presets.json` → `device-backups\<name>-<time>\` | no |
| `plan` | current vs desired outputs, limiter, segments | no |
| `apply [--yes]` | backup → plan → confirm → LED outputs (reboot) → one segment per strip → save as preset 10 (boot). Skips the cfg write and reboot when outputs already match. | **config** - ask Mr Furniss |
| `upload` | `app\tree.htm` → `/tree.htm`, the layout → `/tree.json` | files |
| `looks [--looks config\looks.json] [--force] [--yes]` | backup → build presets for every look, 249 Fade down, 250 Lights off and the shows → upload one compact `presets.json` → start the first show | presets |
| `test strips` / `chase` / `white` / `off` | colour per strip / direction check / full white for current measurement / off | state only |
| `serve [--port 8080]` | serves `app\tree.htm` at `http://localhost:8080/tree.htm`, proxies `/json` and `/presets.json` to the board | no |

`--yes` skips the confirmation prompt; only use it when Mr Furniss has already approved the change.

## Layouts (config)

Layout JSON fields: `ledsPerStrip`, `stripCount`, `ledType` (22 = WS281x), `colorOrder` (0 = GRB),
`abl.ledMa`/`abl.maxMa` (limiter estimate, **not** a safety device - the 5 A fuses are),
`layoutPreset` (10), `attached`, `network.apAlways`/`apAddress`, and `controllers[].outputs[]` (GPIO and
strips in chain order). See `config\README.md`.

- `proto-tree.json` - current: full 12 × 300 tree, strips 1-3 attached, limiter 38,000 mA (95 % of one
  HLG PSU). Raise to ~76,000 when PSU 2 arrives.
- Switching layout = `apply` then `upload` then `looks` (looks depend on strip count and length).
- `*.cloud.*` files in the repo are unmerged copies from the cloud folder; don't treat them as current.

## Backups

- Every `apply` and `looks` backs up first. Take a manual one before any hand-made write.
- `scripts\Backup-Tree.ps1` takes a fuller snapshot (adds `info`, `eff`, `pal`, `fxdata`, `tree.htm`,
  `tree.json`).
- `device-backups\` is git-ignored because `cfg.json` holds Wi-Fi passwords. Never commit or publish
  it, and never paste a cfg backup into an issue or a chat.
- Find the latest good presets backup:

```powershell
Get-ChildItem device-backups -Directory | Sort-Object Name -Descending | ForEach-Object {
  $p = Join-Path $_.FullName 'presets.json'
  if (Test-Path $p) {
    $ok = try { Get-Content $p -Raw | ConvertFrom-Json | Out-Null; $true } catch { $false }
    [pscustomobject]@{ Backup = $_.Name; Bytes = (Get-Item $p).Length; Valid = $ok }
  }
} | Select-Object -First 10 | Format-Table -AutoSize
```

- Restore presets from a backup: `scripts\Compress-Presets.ps1 -Source <backup>\presets.json -Upload`
  (validates, minifies, asks, uploads). Restore config by POSTing the relevant subtree of the backup's
  `cfg.json` to `/json/cfg` - ask first.
- Prune old backups only with Mr Furniss's OK; keep at least the last good one per day.

## Saves (presets)

- `treetool looks` is the safe way to save: one upload of a whole, minified file.
- After every save run `scripts\Get-TreeReport.ps1` and check: uptime didn't reset, presets.json
  parses, size is under 56 KB, the expected ids exist.
- If the save timed out (TreeTool's HttpClient has a 10 s timeout): **stop**. Check uptime and presets
  validity; restore from the backup TreeTool just made if the file is bad. Then shrink the look
  bodies or compact before trying again. See `pitfalls.md`.
- Phone-made shows live in the same file. `looks` keeps them unless `--force`.

## Board memory

| Resource | Size | Watch |
|---|---|---|
| Free heap | ~140 KB with 3,600 LEDs | a big drop from ~140 KB is worth investigating (more segments, big JSON bodies, several phones at once); WLED's web server gets unreliable when heap runs low |
| Filesystem | 983 KB total, ~140-200 KB used | app + presets + cfg; `info.fs` |
| presets.json | keep < 56 KB (57,344 B) | reboot near 64 KB during a save |
| Segments | 32 max | strips + overlays + bands |

## Issue log

When something new goes wrong or a decision is made, record it so the next session knows:
unresolved → `docs\open-questions.md`; a pitfall with a fix → `skill\xmas-tree\references\pitfalls.md`;
current board state → "State of play" in `docs\software.md`. Include the date, what was seen
(error text, uptime, file size), the cause if known, and the fix.
