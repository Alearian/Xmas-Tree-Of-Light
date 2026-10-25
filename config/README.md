# Tree layouts

A layout file describes how the physical strips map onto controllers and outputs. Both **TreeTool** (C#) and the **phone app** (`app/tree.htm`, which loads `/tree.json` from the controller) read the same file.

| File | Use |
|---|---|
| `tree.json` | The full tree: 12 strips, one Dig-Quad, 3 strips per output |
| `bench.json` | Bench test: one strip on LED1 (GPIO 16), with the current limiter capped at 5 A |
| `proto-tree.json` | **Current (2026-10-20):** the whole tree at 12 × 300 LEDs, with strips 1–3 wired up end to end on LED1 (`attached`), powered straight from the HLG through the fuse box (see `docs/three-strips-direct-power.md`). WLED calculates every strip, so the app previews all 12. |
| `looks.json` | The cone look library and shows, installed as presets by `treetool looks` (not a layout) |

## Fields

| Field | Meaning |
|---|---|
| `ledsPerStrip` | LEDs in each 5 m strip (60/m × 5 = 300) |
| `stripCount` | Total number of strips across all controllers |
| `ledType` | WLED bus type. `22` = WS281x (drives WS2815) |
| `colorOrder` | WLED colour order. `0` = GRB. **Check this on the bench**: if red shows as green, try `1` (RGB). |
| `abl.ledMa` | WLED's estimate of mA per LED at full white. `11` follows the builder's 4 A per 360 LEDs (a 6 m strip), which is also about 3.3 A per 300-LED strip. |
| `abl.maxMa` | WLED's current-limiter budget per controller, in mA. This is **not** a safety device. Fuses are. |
| `layoutPreset` | The preset slot where the strip segments are saved; it also becomes the boot preset |
| `controllers[].outputs[]` | The GPIO and the strips it drives, **in chain order** |
| `network` (optional) | `{ "apAlways": true, "apAddress": "4.3.2.1" }`. With `apAlways`, `apply` sets WLED-AP to "Always" (WLED `ap.behav` 2), so the board's own Wi-Fi stays up alongside the home Wi-Fi. Only that setting is sent; the AP name and password are left as they are. The app uses `apAddress` for its "Switch to tree's Wi-Fi" link and QR code. |
| `attached` (optional) | Strips that are physically wired up. The app shows them as live and the rest as what they would display. Leave it out when every strip is attached. |

## Conventions

- Strips are numbered 1…N clockwise from north. Strip *n* stands at an angle of `(n-1) × 360 / stripCount` degrees.
- Within each output the chain **zig-zags**: the 1st strip runs up, the 2nd runs down, the 3rd runs up, and so on.
- TreeTool creates **one WLED segment per strip**, with `rev` set on strips that run down. That way LED 0 of every segment is at the **bottom** of the tree, and per-strip effects all run bottom → top.
- To move to 2 controllers, add a second entry to `controllers` and move strips to it.

## Current limiter with unattached strips

WLED 0.14 has one current limiter for the whole board, and it counts **every** LED it calculates, including strips that aren't wired up. Since 2026-10-20 the strips are fed from HLG-480H-12A #1 (40 A) through the fuse box, so `proto-tree.json` sets `abl.maxMa` to **38,000** (95 % of that one PSU). At 11 mA per LED WLED estimates the whole 12-strip tree at 39.6 A, so full white is trimmed by only about 4 %, and the 3 real strips draw about 10 A. The 5 A fuse on each strip feed is what protects the wiring; the limiter is not a safety device. Raise it to about 76,000 when PSU 2 is added.

## Looks (`looks.json`)

Each look is one WLED effect applied to every strip, with cone-aware tweaks:

| Field | Meaning |
|---|---|
| `id`, `name` | Preset number (1–249) and name. There are no ranges: a preset with a playlist is a show, anything else is a look. Only 10 (the layout/boot preset) and 250 ("Lights off") are reserved. |
| `fx`, `pal` | Effect and palette **by name**, as in WLED's lists. TreeTool looks up the numbers on the board. |
| `sx`, `ix` | Speed and intensity (0–255) |
| `colours` | Up to 3 colours as `#rrggbb` |
| `twist` | LEDs of offset added per strip going round the tree. 0 makes rings. A non-zero value turns bands into a **spiral**. |
| `flip` | `odd`, `even` or `all`: reverse the direction on those strips (for example up on odd, down on even) |
| `groups` | Overrides for `odd`, `even`, `N`, `E`, `S`, `W` or a strip number, for example a different colour per side |
| `overlay` | A second effect drawn on top of every strip (`fx`, `pal`, `sx`, `ix`, `colours`, `twist`, `flip`, `groups`), using the effect's Overlay option, so only effects that have one work (Drip, Glitter, Sparkle). It uses extra segments after the strip segments. Looks without an overlay, and effect changes in the app, remove them. |
| `across` | Run the effect **across** the strips instead of up them: `from`, `to` (strip numbers, next to each other on the board), `bands` (odd; up to 32 segments minus the strips, so 19 with 12 strips; default 7) and `endBrightness` (% of the first colour at the top and bottom of each strip; default 100). Each strip becomes one pixel, so Scanner sweeps a lit strip round the tree (preset 33 **Laser Search**). Each height band is its own segment (ids after the strip segments, up to WLED's 32), dimmer towards the ends; the strip segments are frozen black while it runs. Every other look, and any effect change in the app, unfreezes them. `twist`, `flip`, `groups` and `overlay` don't apply. |

A **show** is a WLED playlist: `looks` (preset ids), `seconds` per look and `fadeSeconds` between looks. Shows end on preset **250 "Lights off"**, which `treetool looks` creates if it is missing.

Shows can also be made, changed and scheduled in the phone app (Shows tab), straight into the board's `presets.json`. So that those aren't wiped, `treetool looks` only writes a show from this file when its preset number is free on the board, and skips a look whose number a phone-made show has taken. Add `--force` to replace them anyway.

Install with:

```powershell
dotnet run --project tools/TreeTool -- looks --layout config/proto-tree.json
```

Re-run it after changing `looks.json` or applying a different layout, because the offsets depend on the strip count and length.
