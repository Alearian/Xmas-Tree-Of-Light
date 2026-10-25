# Software

Three pieces, all in this repo:

| Piece | Where | What it does |
|---|---|---|
| **Tree layout** | `config/tree.json` (full tree), `config/bench.json` (one strip) | The single source of truth for which strips are on which controller, output and position. See [config/README.md](../config/README.md). |
| **TreeTool** (C# / .NET 10) | `tools/TreeTool` | Backs up, plans and applies the WLED config, uploads the phone app, runs test patterns, and serves the app locally for development |
| **Phone app** | `app/tree.htm` | A single HTML file stored **on the controller**. Phones on the home Wi-Fi open `http://xmastree.local/tree.htm` (or `http://4.3.2.1/tree.htm` when the board is in AP fallback mode). |

```
 config/tree.json ──► TreeTool ──► WLED /json/cfg   (LED outputs, limiter, boot preset)
                               ──► WLED /json/state (one segment per strip → preset 10)
                               ──► WLED filesystem: /tree.htm + /tree.json
 Phone ── home Wi-Fi ──► http://xmastree.local/tree.htm ──► WLED JSON API
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
| `looks [--looks config/looks.json] [--force]` | Backs up, saves the cone looks as presets, preset 250 "Lights off" and the shows as playlists (only to free numbers unless `--force`), then starts the first show. See [config/README.md](../config/README.md#looks-looksjson). | yes (presets) |
| `test strips` / `chase` / `white` / `off` | Test patterns: a colour per strip, a direction check, full white for measuring current, and off | state only |
| `serve [--port 8080]` | Serves the app from disk and proxies the API to the first controller, so you can edit the app without uploading it each time | no |

**Host:** both layouts use `xmastree.local`. If the board is in AP fallback mode, use `4.3.2.1` instead.

**Both networks (2026-10-25):** layouts with `"network": { "apAlways": true }` make `apply` set WLED-AP to "Always", so the board answers on the home Wi-Fi *and* on its own `WLED-AP` (`http://4.3.2.1/tree.htm`) at the same time. The app's Config tab lists both addresses with a QR code each and a "Switch to…" link. A home-screen icon opens one address, so add one icon per network. While the board is on the home Wi-Fi, its AP runs on the router's channel; it is open to anyone in range who knows its password, so change the factory `wled1234`.

**Pause (fixed 2026-10-25):** WLED 0.14 ignores `{"ps": n}` when `n` is the look already showing, so the old Pause did nothing. The app now re-sends segment 0's own effect, which unloads the playlist and leaves the look up. WLED then reports `pl` -1 and `ps` -1, so the phone remembers (in `localStorage`) which show and look were paused. Resume starts an unsaved copy of the show at that look, which WLED reports as `pl` 0. Checked on the board.

**Fades between looks (2026-10-25):** WLED 0.14 swaps effects instantly; a playlist's transition only blends colours, so shows didn't visibly fade. A show's playlist now has a hidden step after each look, preset **249 "Fade down"**, which sets every segment's brightness to 0. The show sets `transition` to half the fade, so the look fades down over half the fade, and the next look (which switches its segments back on at brightness 255) fades up over the other half. The fade-down step lasts 0.2 s longer than its fade (`FADE_GAP` / `Looks.FadeGap`), because WLED 0.14 won't start a transition while one is running, and a look that arrived exactly as the fade-down ended skipped its fade-up. Measured on the board from the WebSocket live feed. Every look must set `on` and `bri` on its segments (TreeTool looks and app-saved looks do). `treetool looks` converts existing shows to this format and keeps their looks and times. With fades, a show can have up to 50 looks (WLED plays 100 playlist steps).

**presets.json size:** when WLED re-saves a preset that has grown, it blanks the old copy with spaces and appends the new one, so the file swells. The board restarted at 66 KB on 2026-10-25. `treetool looks` now builds the whole file and uploads it compacted in one go (about 51 KB).

**Wi-Fi gotcha (AP mode only):** when `apply` reboots the board, Windows doesn't always rejoin `WLED-AP` by itself. If `apply` times out, reconnect the PC to `WLED-AP` and run `apply` again.

## Phone app

- **Header:** connection dot, share button, power button and brightness (these affect every controller)
- **Share:** the QR-code button (also "Show QR code" on the Config tab) opens a full-screen QR code of `http://<board IP>/tree.htm`, so someone else can scan it and get the app. It uses the IP rather than `xmastree.local`, because many Android phones can't resolve `.local` names. The phone still has to be on the same Wi-Fi. The QR generator ([qrcode-generator](https://github.com/kazuhikoarase/qrcode-generator) 1.4.4, MIT) is built into `tree.htm`, so it works offline.
- **Tree:** a slowly rotating 3D cone preview made from WLED's live feed (about 15 samples per strip), plus quick whole-tree colours. Strips listed in the layout's `attached` glow and are tagged LIVE. The others are drawn faded with dashed guides, showing what they would display.
- **Effects:** an A–Z effect list with big rows for scrolling with a thumb, plus search, palette, 3 colours, speed and intensity. Applies to the selected strips (or the whole tree).
- **Strips:** a top-down tree with 12 tappable strips, plus groups (North/East/South/West, odd, even)
- **Shows:** each show (a WLED playlist preset) has Start, Pause, Resume and Stop, and an editor: looks in order, a time in seconds per look, a fade, a limit (forever, 1–24 hours or 1–100 times) and an optional daily start and lights-off time (WLED timers). Shows end on preset 250 "Lights off". Below them, the **looks**: tap to show one or delete one with its bin (new looks are saved with "Save as look…" on the Effects tab) (it warns if a show uses it; the layout preset and 250 are never offered). Nothing is stored except the board's presets and timers. See [shows-and-effects-plan.md](shows-and-effects-plan.md).
- **Clock:** when it opens, the app sends the phone's time to the board (as WLED's own page does), so timers work even without internet time.
- **Across looks:** a look in `config/looks.json` can run its effect across the strips instead of up them (`across`), for example **Laser Search** (preset 33), a red strip sweeping round the tree. It picks up from the Looks list like any other. See [plans/cylon-effect-plan.md](plans/cylon-effect-plan.md). Changing an effect in the app ends it and unfreezes the strips.

It works with **no strips connected**. WLED still runs the effects and the preview shows what would be sent to the strips.

Tip: in the phone browser, use "Add to Home Screen" to get an app icon.

## State of play (2026-09-30)

- The board is running the full layout: 3,600 LEDs, 4 outputs × 900, 12 segments, boot preset 10 "Tree layout". Free memory is about 147 KB.
- The app is uploaded and served from the board. Its API calls have been checked from the PC (per-strip effects, live feed). **It hasn't been tried on a phone yet.**
- The original 120-LED config is in `device-backups/Dig-Quad-A-20260930-141045/`.

## Next

- Try it on a phone, then polish the UI
- Height bands (top/middle/bottom) need more segments per strip. WLED allows 32 in total, so 12 strips × 2 bands (24) would fit.
- Multiple controllers: the app and tool already handle a `controllers` list. Cross-controller requests from the phone may need CORS checking.
- **Standalone running (decided 2026-10-03):** no laptop at the tree. Effects live on the board as WLED presets and playlists, and the phone app is the remote. Next steps: a look library applied by TreeTool, a show playlist, phone time sync for schedules, and possibly WLED 2D mapping (roadmap Stage 4).
