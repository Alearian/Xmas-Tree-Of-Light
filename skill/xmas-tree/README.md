# xmas-tree - a Claude skill for running a WLED Christmas tree

This is a [Claude skill](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/overview) that
lets Claude manage a home-built, phone-controlled outdoor Christmas tree: control the lights, check
the controller is online, report its state, back up and save presets, run and extend the C# tool
that configures it, update the phone app, design new light effects, and steer clear of the problems
we have already hit.

## The project

An outdoor cone-shaped tree about 5 m tall, made of **12 vertical LED strips** arranged round a
central mast.

| Part | What we use |
|---|---|
| LEDs | 12 × 5 m BTF-LIGHTING WS2815 12 V strips, 60 LEDs/m, IP67 - 300 LEDs each, **3,600** in total |
| Controller | [QuinLED Dig-Quad V3](https://quinled.info/pre-assembled-quinled-dig-quad/) (ESP32), 4 outputs × 3 strips in a zig-zag |
| Firmware | [WLED](https://kno.wled.ge/) **0.14.0** |
| Power | Mean Well HLG 12 V supplies, each strip fed at the bottom through its own 5 A fuse |
| Remote | a single-file phone web app (`tree.htm`) served by the controller itself |
| Configuration | **TreeTool**, a small C# (.NET 10) command-line tool |

The tree runs **standalone**: no PC is needed at the tree. Every "look" (an animated effect) and
every "show" (a timed sequence of looks) is stored on the controller as a WLED preset or playlist.
Phones on the home Wi-Fi open `http://<board-ip>/tree.htm` to pick looks, run and schedule shows,
set colours per strip or per side of the tree, and see a live 3D preview of all 12 strips.

```
 config/*.json ──► TreeTool ──► WLED /json/cfg   (LED outputs, current limiter, boot preset)
                            ──► WLED presets.json (looks, shows, Lights off)
                            ──► WLED files: /tree.htm (app) + /tree.json (layout)
 Phone ── home Wi-Fi ──► http://<board-ip>/tree.htm ──► WLED JSON API
```

### Repository layout (the project this skill manages)

```
app/tree.htm              phone app (single file, no external resources)
config/proto-tree.json    current layout: 12 strips, strips 1-3 wired
config/tree.json          final 12-strip layout
config/looks.json         the look library and shows
tools/TreeTool/           C# tool: info, backup, plan, apply, upload, looks, test, serve
device-backups/           snapshots of the board (git-ignored: they contain Wi-Fi passwords)
docs/                     design, wiring, power, roadmap, open questions, plans
skill/xmas-tree/          this skill
```

## What the skill covers

| Area | How |
|---|---|
| **Control the board** | WLED JSON API recipes in PowerShell: power, brightness, per-strip effects, presets, playlists, timers |
| **Detect the board** | `scripts/Test-Tree.ps1` tries the home IP, `xmastree.local`, the WLED-AP fallback, then ARP by MAC |
| **Report state and parameters** | `scripts/Get-TreeReport.ps1`: firmware, uptime, heap, filesystem, LED buses, limiter, segments, presets (with size budget), timers, warnings - optionally as Markdown |
| **Backups and saves** | `scripts/Backup-Tree.ps1` (full snapshot), `scripts/Compress-Presets.ps1` (validate, compact, restore, upload with confirmation), plus TreeTool's own backup-before-write |
| **Use TreeTool** | when to run `info`, `plan`, `apply`, `upload`, `looks`, `test`, `serve`, with which layout, and what to check afterwards |
| **Modify TreeTool** | code map, invariants (strip mapping shared with the app, one-shot preset upload, size budget, 32 segments) |
| **Update the app** | version bump, local testing via `treetool serve`, upload, verifying on the phone, cache issues |
| **Create new effects** | a workflow from idea → live test (no save) → `looks.json` → install, with the schema (twist, flip, groups, overlay, across) and effects that suit vertical strips |
| **Pitfalls** | a symptom → cause → fix table for everything that has gone wrong so far |
| **Project memory** | where to record new issues, decisions and board state so the next session knows |

### Hard-won pitfalls baked in

- **presets.json near 64 KB reboots WLED 0.14 mid-save** and can lose presets. Keep it under 56 KB;
  WLED leaves whitespace behind on every replace/delete, so compact it now and then.
- A **timed-out save** is the warning sign: check uptime and file validity, don't retry blindly.
- **WLED 16.x boot-loops this board** (brownout at Wi-Fi start, WLED issue #5570) - stay on 0.14/0.15
  until USB recovery is ready.
- **32 segments** maximum; **segment brightness is reapplied per pixel**, so dim with colour.
- **Meteor effects need an explicit palette**, or WLED paints a Party-colour background.
- **Android can't resolve `.local`** - phones and QR codes use the IP.
- **Phones cache the app** - check the version on the Config tab.
- Reserved presets: 10 (layout/boot), 249 (Fade down), 250 (Lights off).

## Install

Copy the folder into a skills directory Claude Code reads:

```powershell
# personal (all projects)
Copy-Item -Recurse .\skill\xmas-tree "$env:USERPROFILE\.claude\skills\xmas-tree"
# or project-only: keep it in the repo under .claude\skills\
Copy-Item -Recurse .\skill\xmas-tree .\.claude\skills\xmas-tree
```

On claude.ai, zip the `xmas-tree` folder and upload it as a custom skill in Settings.

Then just talk about the tree: *"is the tree online?"*, *"give me a report on the board"*, *"make a
look with gold sparkles falling down the north side"*, *"the looks save timed out"*, *"update the app
version and upload it"*.

## Requirements

- Windows with PowerShell 5.1 or 7 (scripts use `Invoke-RestMethod` and the built-in `curl.exe`)
- .NET 10 SDK for TreeTool
- The PC on the same network as the controller. Claude running in the cloud cannot reach a home LAN;
  it hands you the commands or works through a session on your PC.

## Adapting it to your own tree

The board address, MAC, layout file name and reserved preset ids are at the top of `SKILL.md` and as
parameters of each script (`-Address`, `-Repo`, `-Mac`). Change them to match your setup; the
pitfalls apply to any WLED 0.14 build on an ESP32.

## Safety

The skill reads freely, changes live colours freely, but **backs up before every persistent write and
asks before any config change** (LED outputs, Wi-Fi, AP, current limiter), because a bad Wi-Fi config
can lock you out of the board. The WLED current limiter is an estimate, not protection: fuse every
strip feed, never join the positive outputs of two power supplies, and always join their grounds.

## Licence

MIT - see [LICENSE](../../LICENSE). The phone app embeds [qrcode-generator](https://github.com/kazuhikoarase/qrcode-generator)
(MIT).
