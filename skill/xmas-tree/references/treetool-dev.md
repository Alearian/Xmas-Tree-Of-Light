# Changing TreeTool (C#, .NET 10)

`tools\TreeTool\` - a console app, assembly `treetool`, no NuGet packages.

| File | Responsibility |
|---|---|
| `Program.cs` | argument parsing, the `Usage` text, one `async Task<int>` per command (`Info`, `BackupAll`, `Plan`, `Apply`, `Upload`, `InstallLooks`, `Test`), `Presets()`, `EffectId()`, `Fail()` |
| `WledClient.cs` | thin HTTP wrapper: `GetJsonAsync`, `GetStringAsync`, `PostJsonAsync`, `UploadAsync` (multipart to `/upload`), `IsOnlineAsync`, `WaitOnlineAsync`. 10 s timeout. |
| `Layout.cs` | records `TreeLayout`, `Controller`, `Output`, `Abl`, `Network`; `LayoutMath.Buses` and `LayoutMath.Segments` (strip → LED range, `RunsDown`) |
| `Planner.cs` | desired cfg (`hw.led`, limiter, boot preset, `ap.behav`), `CfgMatches`, desired segments state, `PrintPlan` |
| `Looks.cs` | looks.json → WLED state bodies: `State` (twist, flip, groups, overlay), `AcrossState` (height bands), `LightsOffPreset` (250), `FadeDownPreset` (249), `ShowPreset`, `AddFades`, `Playlist`, `MaxSegments = 32` |
| `DevServer.cs` | `serve`: HttpListener on localhost, serves app + layout, proxies the rest |

## Build and check

```powershell
Set-Location D:\Source\Xmas
dotnet build tools\TreeTool
dotnet run --project tools\TreeTool -- --help
dotnet run --project tools\TreeTool -- plan --layout config\proto-tree.json     # read-only smoke test
```

There are no unit tests. For a change to how presets are built, add a dry path that writes the
generated presets.json to disk without uploading (e.g. a `--dry-run` that saves to
`device-backups\dry-<time>\presets.json`) and inspect its size and content before a real run.

## Invariants - keep these when you edit

1. **Strip mapping lives in two places**: `LayoutMath.Segments` in `Layout.cs` and `buildStrips` in
   `app\tree.htm`. Change both together or the app's preview and strip taps go to the wrong LEDs.
2. **Back up before writing.** Every command that writes presets or config calls `Backup` first.
3. **One upload for presets.** Build the whole file in memory, serialise compact (no indentation), and
   upload once via `UploadAsync(temp, "presets.json")`. Never loop `psave` over many looks.
4. **Keep preset bodies minimal.** Strip segments only carry what differs (`id`, `fx`, `pal`, `col`,
   `sx`, `ix`, `on`, `bri`, `frz:false`); `grp`/`spc`/`bri` resets only on overlay/band segments. Every
   extra key × 12 strips × ~15 looks costs KB.
5. **Stay under 56 KB** (57,344 B, the app's `PRESETS_MAX`). As of 2026-10-25 `InstallLooks` only
   prints the size after uploading; adding a check that refuses (or warns and asks) before
   `UploadAsync` when `json.Length > 57344` is a worthwhile first improvement. Don't let a code change
   grow the file silently.
6. **Reserved ids**: 10 (layout), 249 (Fade down), 250 (Lights off). Shows are only written to free ids
   unless `--force`, so phone-made shows survive.
7. **32 segments.** Strips + overlay/band segments ≤ `Looks.MaxSegments`. Looks that don't use the
   extra segments must delete ids up to 31 (`stop: 0`).
8. **Dim with colour, not `bri`** on effect segments (WLED reapplies segment opacity per pixel).
9. **Effects and palettes by name** - looked up on the board (`/json/eff`, `/json/pal`), so ids can
   change between firmware versions without breaking looks.json.
10. **Match the surrounding style**: file-scoped namespace `TreeTool`, records, `JsonNode`/`JsonObject`,
    top-level statements in Program.cs, short `///` summaries explaining *why*.

## Adding a command

1. Add a line to `Usage`.
2. Add a `"name" => await Name(layout)` arm to the switch.
3. Write `async Task<int> Name(TreeLayout layout)` looping `layout.Controllers` with a `WledClient`.
4. Return 0 on success, `Fail("...")` with an actionable message otherwise.
5. Update `docs\software.md` (command table) and `references/treetool.md` in this skill.
