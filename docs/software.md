# Software

Three pieces, all in this repo:

| Piece | Where | What it does |
|---|---|---|
| **Tree layout** | `config/tree.json` (full tree), `config/bench.json` (one strip) | The single source of truth for which strips are on which controller, output and position. See [config/README.md](../config/README.md). |
| **TreeTool** (C# / .NET 10) | `tools/TreeTool` | Backs up, plans and applies the WLED config, uploads the phone app, runs test patterns, and serves the app locally for development |
| **Phone app** | `app/tree.htm` | A single HTML file stored **on the controller**. Phones join the tree's Wi-Fi and open `http://4.3.2.1/tree.htm`. |

```
 config/tree.json ──► TreeTool ──► WLED /json/cfg   (LED outputs, limiter, boot preset)
                               ──► WLED /json/state (one segment per strip → preset 10)
                               ──► WLED filesystem: /tree.htm + /tree.json
 Phone ── tree Wi-Fi ──► http://4.3.2.1/tree.htm ──► WLED JSON API
```

## TreeTool

Run it from the repo root:

```powershell
dotnet run --project tools/TreeTool -- <command> [--layout config/bench.json] [--yes]
```

| Command | Effect | Changes the device? |
|---|---|---|
| `info` | Firmware, LED count, uptime, free memory and filesystem for each controller | no |
| `backup` | Saves `cfg.json`, `state.json` and `presets.json` to `device-backups/<name>-<time>/` | no |
| `plan` | Shows current vs desired outputs, limiter and segments | no |
| `apply` | Backs up, shows the plan, asks, writes the LED outputs (then reboots), creates one segment per strip and saves them as the boot preset. **Safe to re-run:** it skips the config step and the reboot if the outputs already match. | **yes** |
| `upload` | Uploads `app/tree.htm` and the layout (as `/tree.json`) | yes (files only) |
| `test strips` / `chase` / `white` / `off` | Test patterns: a colour per strip, a direction check, full white for measuring current, and off | state only |
| `serve [--port 8080]` | Serves the app from disk and proxies the API to the first controller, so you can edit the app without uploading it each time | no |

**Wi-Fi gotcha:** when `apply` reboots the board, Windows doesn't always rejoin `WLED-AP` by itself. If `apply` times out, reconnect the PC to `WLED-AP` and run `apply` again.

## Phone app

- **Header:** connection dot, power button and brightness (these affect every controller)
- **Tree:** a slowly rotating 3D cone preview made from WLED's live feed (about 15 samples per strip), plus quick whole-tree colours
- **Effects:** search the effect list, palette, 3 colours, speed and intensity. Applies to the selected strips (or the whole tree).
- **Strips:** a top-down tree with 12 tappable strips, plus groups (North/East/South/West, odd, even)
- **Presets:** tap to load, including playlists (▶), and save the current look as a new preset

It works with **no strips connected**. WLED still runs the effects and the preview shows what would be sent to the strips.

Tip: in the phone browser, use "Add to Home Screen" to get an app icon.

## State of play (2026-09-30)

- The board is running the full layout: 4,320 LEDs, 4 outputs × 1,080, 12 segments, boot preset 10 "Tree layout". Free memory is about 147 KB.
- The app is uploaded and served from the board. Its API calls have been checked from the PC (per-strip effects, live feed). **It hasn't been tried on a phone yet.**
- The original 120-LED config is in `device-backups/Dig-Quad-A-20260930-141045/`.

## Next

- Try it on a phone, then polish the UI
- Height bands (top/middle/bottom) need more segments per strip. WLED allows 32 in total, so 12 strips × 2 bands (24) would fit.
- Multiple controllers: the app and tool already handle a `controllers` list. Cross-controller requests from the phone may need CORS checking.
- **Standalone running (decided 2026-10-03):** no laptop at the tree. Effects live on the board as WLED presets and playlists, and the phone app is the remote. Next steps: a look library applied by TreeTool, a show playlist, phone time sync for schedules, and possibly WLED 2D mapping (roadmap Stage 4).
