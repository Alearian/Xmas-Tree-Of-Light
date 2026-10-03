# Tree layouts

A layout file describes how the physical strips map onto controllers and outputs. Both **TreeTool** (C#) and the **phone app** (`app/tree.htm`, which loads `/tree.json` from the controller) read the same file.

| File | Use |
|---|---|
| `tree.json` | The full tree: 12 strips, one Dig-Quad, 3 strips per output |
| `bench.json` | Bench test: one strip on LED1 (GPIO 16), with the current limiter capped at 5 A |
| `proto-tree.json` | **Current (2026-10-03):** the whole tree at 12 × 300 LEDs, with only strip 1 wired up (`attached`). WLED calculates every strip, so the app previews all 12. |
| `looks.json` | The cone look library and shows, installed as presets by `treetool looks` (not a layout) |

## Fields

| Field | Meaning |
|---|---|
| `ledsPerStrip` | LEDs in each 6 m strip (60/m × 6 = 360) |
| `stripCount` | Total number of strips across all controllers |
| `ledType` | WLED bus type. `22` = WS281x (drives WS2815) |
| `colorOrder` | WLED colour order. `0` = GRB. **Check this on the bench**: if red shows as green, try `1` (RGB). |
| `abl.ledMa` | WLED's estimate of mA per LED at full white. `11` follows the builder's 4 A per 360 LEDs. |
| `abl.maxMa` | WLED's current-limiter budget per controller, in mA. This is **not** a safety device. Fuses are. |
| `layoutPreset` | The preset slot where the strip segments are saved; it also becomes the boot preset |
| `controllers[].outputs[]` | The GPIO and the strips it drives, **in chain order** |
| `attached` (optional) | Strips that are physically wired up. The app shows them as live and the rest as what they would display. Leave it out when every strip is attached. |

## Conventions

- Strips are numbered 1…N clockwise from north. Strip *n* stands at an angle of `(n-1) × 360 / stripCount` degrees.
- Within each output the chain **zig-zags**: the 1st strip runs up, the 2nd runs down, the 3rd runs up, and so on.
- TreeTool creates **one WLED segment per strip**, with `rev` set on strips that run down. That way LED 0 of every segment is at the **bottom** of the tree, and per-strip effects all run bottom → top.
- To move to 2 controllers, add a second entry to `controllers` and move strips to it.

## Current limiter with unattached strips

WLED 0.14 has one current limiter for the whole board, and it counts **every** LED it calculates, including strips that aren't wired up. `proto-tree.json` therefore sets `abl.maxMa` to 60,000 (5 A × 12 strips), so the real strip isn't dimmed because of strips that don't exist. At 11 mA per LED, WLED estimates one 300-LED strip at 3.3 A at most, so the limiter never acts on a single strip anyway. The PSU and the 7.5 A fuse protect the real strip.

## Looks (`looks.json`)

Each look is one WLED effect applied to every strip, with cone-aware tweaks:

| Field | Meaning |
|---|---|
| `id`, `name` | Preset number and name. Looks use 20–39, shows use 40+. Preset 10 is the layout/boot preset. |
| `fx`, `pal` | Effect and palette **by name**, as in WLED's lists. TreeTool looks up the numbers on the board. |
| `sx`, `ix` | Speed and intensity (0–255) |
| `colours` | Up to 3 colours as `#rrggbb` |
| `twist` | LEDs of offset added per strip going round the tree. 0 makes rings. A non-zero value turns bands into a **spiral**. |
| `flip` | `odd`, `even` or `all`: reverse the direction on those strips (for example up on odd, down on even) |
| `groups` | Overrides for `odd`, `even`, `N`, `E`, `S`, `W` or a strip number, for example a different colour per side |

A **show** is a WLED playlist: `looks` (preset ids), `seconds` per look and `fadeSeconds` between looks.

Install with:

```powershell
dotnet run --project tools/TreeTool -- looks --layout config/proto-tree.json
```

Re-run it after changing `looks.json` or applying a different layout, because the offsets depend on the strip count and length.
