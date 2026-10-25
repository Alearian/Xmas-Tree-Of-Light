---
name: xmas-tree
description: Manage the WLED Xmas tree project (QuinLED Dig-Quad V3, WLED 0.14, 12 × WS2815 strips, D:\Source\Xmas) - control the board, check it is online, report its state, back up and save presets, run and change TreeTool (C#), update the phone app (tree.htm), design new looks/effects and shows, and avoid known pitfalls such as the presets.json size reboot. Use whenever the user mentions the tree, the lights, WLED, the Dig-Quad, xmastree.local, <board-ip>, TreeTool, treetool looks/apply/upload, tree.htm, looks.json, presets, shows, playlists, segments or effects, even if they don't say "skill".
---

# Xmas tree manager

An outdoor 5 m cone tree of 12 × 5 m WS2815 strips (300 LEDs each, 3,600 total) driven by one
QuinLED Dig-Quad V3 (ESP32) running **WLED 0.14.0**. The tree runs standalone: looks and shows live on
the board as WLED presets and playlists, and a single-file phone app served by the board is the remote.

Call the user **Mr Furniss**. Give **Windows PowerShell** commands only (no bash), unless he asks otherwise.

## Where things are

| What | Where |
|---|---|
| Repo (source of truth) | `D:\Source\Xmas` on Mr Furniss's PC (git, `main`). Cloud copies are older; stage from the PC before editing. |
| Board | `http://<board-ip>` (home Wi-Fi). `http://xmastree.local` works on the PC but **not on Android**. AP fallback / "always" AP: `http://4.3.2.1` on Wi-Fi `WLED-AP`. MAC `28:56:2f:55:35:8c`. |
| Phone app | `app\tree.htm` → served from the board at `/tree.htm`, version string `App version yyyy-MM-ddX` in the Config tab |
| TreeTool | `tools\TreeTool` (C#, .NET 10): `dotnet run --project tools\TreeTool -- <cmd> --layout config\proto-tree.json` |
| Layouts | `config\proto-tree.json` (**current**: 12 × 300, strips 1-3 attached), `config\tree.json` (final), `config\bench*.json` |
| Looks & shows | `config\looks.json` (installed by `treetool looks`) |
| Backups | `device-backups\Dig-Quad-A-<yyyyMMdd-HHmmss>\` (git-ignored: they hold Wi-Fi passwords) |
| Docs | `docs\` - start with `design-summary.md`, `software.md`, `open-questions.md`, `roadmap.md`; plans go in `docs\plans\` |
| Helper scripts | this skill's `scripts\*.ps1` - in the repo at `skill\xmas-tree\scripts\`; if the skill is installed elsewhere, use the copy next to this file |

Which reference to read:

| Task | Read |
|---|---|
| Change colours/effects/brightness, presets, playlists, timers, raw API | `references/board-control.md` |
| Is the board there? What state is it in? | run `scripts/Test-Tree.ps1`, `scripts/Get-TreeReport.ps1`; `references/board-control.md` |
| Run TreeTool, backups, saving presets, layouts, board memory, issue log | `references/treetool.md` |
| Change TreeTool's C# code | `references/treetool-dev.md` |
| Change or deploy the phone app | `references/app.md` |
| Design a new look, effect or show | `references/effects.md` |
| Anything goes wrong, or before any write to the board | `references/pitfalls.md` |

## Ground rules (why they matter)

1. **Reads are always safe** (`GET /json/info|state|cfg|eff|pal`, `/presets.json`). Do them freely to
   check facts before acting rather than trusting notes.
2. **State changes are fine while testing** (colours, on/off, effects via `POST /json/state` without
   `psave`). They vanish on reboot.
3. **Back up before any persistent write** - presets, config, files. `treetool backup` or
   `scripts/Backup-Tree.ps1`. A backup is the only undo; USB recovery is not set up.
4. **Ask before config writes** (`POST /json/cfg`): LED outputs, Wi-Fi, AP, current limiter. A bad
   Wi-Fi/AP config locks us out until someone reflashes over USB. Show the diff, get a "yes".
5. **Mr Furniss's PC blocks some board writes** relayed from Claude (auto-permission check). If a
   write is refused, show him the exact command and ask him to approve or run it himself.
6. **Firmware is frozen at 0.14.0 until after Christmas.** WLED 16.x boot-loops this board (brownout at
   Wi-Fi start, WLED issue #5570). 0.15.4 is optional. See `docs\wled-16-upgrade-assessment.md`.
7. **Electrical safety.** In the final build strip power never goes through the Dig-Quad; never join the
   V+ of two PSUs; always join grounds. Explain wiring as you go - Mr Furniss is learning it.

## Reaching the board

Cloud sessions and the Cowork device VM **cannot reach the home LAN**. Anything that talks to the
board must run on Mr Furniss's PC: either give him the PowerShell to run, or use a Remote Control
session in `D:\Source\Xmas`. Editing files in the repo works through folder access.

Always start a board task with the presence check:

```powershell
& .\skill\xmas-tree\scripts\Test-Tree.ps1          # tries <board-ip>, xmastree.local, 4.3.2.1, then ARP by MAC
```

It prints which address answered, WLED version, uptime and free heap. **A short uptime you didn't
expect means the board rebooted** - find out why (see pitfalls) before writing anything.

## Common jobs

**Report on the board** - `scripts/Get-TreeReport.ps1 [-Address <board-ip>] [-Markdown out.md]`
prints firmware, uptime, heap, filesystem, LED buses, limiter, segments, what's playing, presets.json
size vs the 56 KB limit, preset list (looks / shows / reserved), timers and warnings. Summarise it for
Mr Furniss in plain sentences; lead with anything abnormal.

**Back up** - `dotnet run --project tools\TreeTool -- backup --layout config\proto-tree.json` (cfg,
state, presets) or `scripts/Backup-Tree.ps1` (adds info, eff, pal, fxdata, tree.htm, tree.json).

**Install looks** - edit `config\looks.json`, then
`dotnet run --project tools\TreeTool -- looks --layout config\proto-tree.json`. It backs up, builds one
compact presets.json and uploads it in one go. Check the report afterwards (size, uptime).

**Update the app** - bump the version string, test with `treetool serve`, upload with
`treetool upload`, then have Mr Furniss check the version on the phone's Config tab. Details in
`references/app.md`.

**New effect/look** - work out the idea with Mr Furniss, try it live with a state POST (no save),
then add it to `looks.json` and install. Details in `references/effects.md`.

## The pitfalls you must know

Full list with fixes in `references/pitfalls.md`. The ones that have actually bitten:

- **presets.json near 64 KB reboots the board mid-save** and can lose or corrupt presets. Keep it
  **under 56 KB** (the app refuses "Save as look" above 57,344 B). Compact with
  `scripts/Compress-Presets.ps1` (validates, minifies, uploads with confirmation).
- **A timed-out save is the warning sign.** Don't retry. Check uptime and that `/presets.json` parses;
  restore from the latest good backup if not.
- **32 segments max** on ESP32/0.14 - 12 strips + overlays/height bands must fit.
- **Segment `bri` is reapplied per pixel**, so dim bands via the colour, not opacity.
- **Meteor-type effects need an explicit palette** (e.g. `* Color Gradient`) or WLED uses Party colours.
- **Reserved presets**: 10 = layout/boot, 249 = Fade down (show transitions), 250 = Lights off. Never
  offer, overwrite or delete them as looks.
- **Phones cache the old app** - check the version in the Config tab; reload or clear site data.
- **Android can't resolve `xmastree.local`** - use `<board-ip>` for phones and QR codes.

## Keeping the project memory straight

When you learn something that a later session would otherwise have to rediscover - a new failure, a
board state change, a decision by Mr Furniss - record it the same turn:

- an issue or decision → `docs\open-questions.md` (unresolved) or the relevant doc (settled);
- a pitfall → `references/pitfalls.md` in this skill (it's in the repo, so it's versioned);
- a board-state change (layout applied, app version deployed) → `docs\software.md` "State of play".

Plans go to `docs\plans\` (Mr Furniss's `C:\Users\Develepor\.claude\plans\Xmas` is a junction to it).
Commit with a short message when he asks; don't push without being asked.
