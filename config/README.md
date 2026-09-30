# Tree layouts

A layout file describes how the physical strips map onto controllers and outputs. Both **TreeTool** (C#) and the **phone app** (`app/tree.htm`, which loads `/tree.json` from the controller) read the same file.

| File | Use |
|---|---|
| `tree.json` | The full tree: 12 strips, one Dig-Quad, 3 strips per output |
| `bench.json` | Bench test: one strip on LED1 (GPIO 16), with the current limiter capped at 5 A |

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

## Conventions

- Strips are numbered 1…N clockwise from north. Strip *n* stands at an angle of `(n-1) × 360 / stripCount` degrees.
- Within each output the chain **zig-zags**: the 1st strip runs up, the 2nd runs down, the 3rd runs up, and so on.
- TreeTool creates **one WLED segment per strip**, with `rev` set on strips that run down. That way LED 0 of every segment is at the **bottom** of the tree, and per-strip effects all run bottom → top.
- To move to 2 controllers, add a second entry to `controllers` and move strips to it.
