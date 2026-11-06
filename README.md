# Xmas Tree of Light

An outdoor **5–6 m cone Christmas tree** made of 12 vertical LED strips (**3,600 addressable pixels**), run by a single ESP32 controller and controlled from a phone. It needs no laptop at the tree: looks and shows are stored on the controller and play by themselves, with a phone app as the remote.

![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![ESP32](https://img.shields.io/badge/ESP32-000000?style=flat-square&logo=espressif&logoColor=white)
![WLED](https://img.shields.io/badge/WLED-FFA000?style=flat-square&logo=lightning&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=flat-square&logo=javascript&logoColor=black)

## How it works

| Layer | What |
|---|---|
| **Pixels** | 12 × 5 m **WS2815** 12 V strips (60 LEDs/m, 300 px each), hung between a top ring and a base ring around a centre mast. Each strip is fed from the bottom. |
| **Controller** | QuinLED **Dig-Quad V3** (ESP32) running **WLED 0.14**. There are 4 outputs, each driving 3 strips joined in a zig-zag (up, down, up). |
| **Power** | 2 × Mean Well HLG-320H-12A supplies, 6 strips each, with a fused feed to every strip. The grounds are shared and the V+ outputs are never joined. |
| **Software** | A C# **TreeTool** that applies the configuration from version-controlled files, and a single-file **phone app** served from the controller itself. |

```
 config/tree.json ──► TreeTool ──► WLED /json/cfg    (LED outputs, current limiter, boot preset)
 config/looks.json             ──► WLED /json/state  (one segment per strip, looks, shows)
                               ──► WLED filesystem:  /tree.htm + /tree.json
 Phone ── Wi-Fi ──► http://xmastree.local/tree.htm ──► WLED JSON API
```

On the home Wi-Fi the controller answers at `xmastree.local`. If it can't find that network at boot, it falls back to its own access point, `WLED-AP`, at `http://4.3.2.1/`.

## Phone app

`app/tree.htm` is one self-contained HTML file, uploaded to and served from the controller, so it works without internet access.

- **Tree:** a rotating 3D cone preview drawn from WLED's live feed, plus quick whole-tree colours
- **Effects:** a thumb-friendly A–Z list of effects, with palette, colours, speed and intensity, applied to the whole tree or to selected strips
- **Strips:** a top-down view with 12 tappable strips and groups (North/East/South/West, odd, even)
- **Shows:** build a show from saved looks on the phone, with a time per look, fades between looks, a run limit, and a daily start and lights-off time, all stored as WLED playlists and timers
- **Share:** a full-screen QR code so someone else on the same Wi-Fi can open the app

On a phone, use "Add to Home Screen" to get an app icon.

## TreeTool

A .NET command-line tool in `tools/TreeTool`. Run it from the repo root:

```powershell
dotnet run --project tools/TreeTool -- <command> [--layout config/bench.json] [--yes]
```

| Command | What it does |
|---|---|
| `info` | Firmware, LED count, uptime and free memory for each controller |
| `backup` | Saves the controller's config, state and presets to `device-backups/` |
| `plan` | Shows the current configuration next to the one the layout asks for |
| `apply` | Backs up, shows the plan, asks, then writes the LED outputs and one segment per strip. Safe to re-run. |
| `upload` | Uploads the phone app and the layout to the controller |
| `looks` | Saves the cone looks from `config/looks.json` as presets and the shows as playlists |
| `test strips` / `chase` / `white` / `off` | Test patterns for checking wiring, direction and current draw |
| `serve` | Serves the app from disk and proxies the API, for editing the app without re-uploading it |

## Repository layout

| Path | Contents |
|---|---|
| `app/` | The phone app (`tree.htm`) |
| `config/` | Tree layouts (`tree.json` for the full tree, `bench*.json` for test rigs) and the look library (`looks.json`). See [config/README.md](config/README.md). |
| `tools/TreeTool/` | The C# configuration tool |
| `docs/` | Design, hardware, power and wiring notes, build guides and the roadmap. Start at [docs/README.md](docs/README.md). |
| `skill/xmas-tree/` | A Claude skill with scripts for checking, reporting on and backing up the controller |

The docs use placeholders for local network details: `<home-wifi>`, `<board-ip>` and `<pc-ip>`.

## Status

In progress for Christmas 2026. The controller is running the full 3,600-pixel layout, and the app, looks and shows are working on it with a test strip. The full 12-strip build, rigging and sealing are still to come. See [docs/roadmap.md](docs/roadmap.md).

Possible next steps include mapping the tree as a WLED 2D matrix (12 × 300) for effects that wrap around it, such as scrolling text, fire and rain.

## Credits

- [WLED](https://kno.wled.ge/) firmware
- [QuinLED](https://quinled.info/) Dig-Quad controller
- [qrcode-generator](https://github.com/kazuhikoarase/qrcode-generator) 1.4.4 (MIT), built into the app
