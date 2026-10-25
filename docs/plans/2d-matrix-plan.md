# Plan: the tree as a WLED 2D matrix (12 × 300)

Status: **plan only (2026-10-25), on hold until after 28 November 2026 (the user's decision). The board config has not been changed.** WLED 0.14.0 stays. Details below come from the WLED 0.14.0 source (`cfg.cpp`, `FX_2Dfcn.cpp`, `FX_fcn.cpp`, `json.cpp`, `ws.cpp`) and the board's `/json/fxdata`.

## Why
36 of WLED's 179 effects only draw on a 2D matrix (flag `2` without `1` in `/json/fxdata`): Scrolling Text, Matrix, Game Of Life, Black Hole, DNA, DNA Spiral, Julia, Polar Lights, Firenoise, Plasma Ball, Metaballs, Octopus, Soap, Swirl and others. The board has the tree as 12 separate strips, so they draw nothing, and the app (2026-10-25f) hides them. As a matrix the tree is **12 columns (one per strip, round the cone) × 300 rows (up the strip)**.

## What to expect (honestly)
- **Good fits** for a tall, narrow wrap-around grid: Matrix (digital rain falling down the tree), Firenoise (2D fire rising), DNA / DNA Spiral (a helix up the tree), Polar Lights, Noise2D, Soap, Waving Cell, Drift, Plasma Ball, Game Of Life, Colored Bursts.
- **Odd fits:** effects drawn round a centre (Black Hole, Octopus, Julia, Lissajous) assume a square panel. On 12 × 300 they'll be squashed across and stretched up.
- **Scrolling Text will not be readable.** A letter is about 6 columns wide, and only about 6 columns face you, so you'd see one letter at a time going round the tree. The "Rotate" option turns the text sideways up the tree, where it spans about ⅔ of the way round the cone. Worth a look, but don't count on words.
- **Speed:** frame rate is about 20 fps for 3,600 LEDs today. 2D effects cost more, so measure (`/json/info` `leds.fps`).

## How WLED does it (0.14)
### Board config: four panels, one per output
`hw.led.matrix` in `/json/cfg`. If it's there, WLED becomes a matrix, and `setUpMatrix()` builds a pixel map on the next start.

```json
"matrix": { "mpc": 4, "panels": [
  { "b": true, "r": false, "v": true, "s": true, "x": 0, "y": 0, "w": 3, "h": 300 },
  { "b": true, "r": false, "v": true, "s": true, "x": 3, "y": 0, "w": 3, "h": 300 },
  { "b": true, "r": false, "v": true, "s": true, "x": 6, "y": 0, "w": 3, "h": 300 },
  { "b": true, "r": false, "v": true, "s": true, "x": 9, "y": 0, "w": 3, "h": 300 } ] }
```

Each panel is one output: 3 strips wide, vertical (`v`), serpentine (`s`), first LED at the bottom (`b`). That matches the wiring exactly:
- strip 1 runs up, strip 2 down, strip 3 up;
- the next output starts at the bottom again, at column 3.

Panel *p* takes physical LEDs 900·*p* … 900·*p* + 899, in output order. Row 0 is the **top** of the tree.

Electronics note: nothing is rewired. The matrix is only a lookup table in the ESP32 (7 KB of its ~135 KB free memory), from grid cell to the LED it really is.

### Segments become rectangles
- In 2D, `start`/`stop` are **columns** and `startY`/`stopY` are **rows**.
- A strip is the 1-column segment `start = n−1, stop = n, startY = 0, stopY = 300`.
- The current per-strip segments (start 0, 300, 600 …) are meaningless on a 12-wide grid, so they must be rebuilt (`treetool apply`) right after the switch.
- New segment options: `rY` (reverse rows), `mY` (mirror rows), `tp` (transpose) and `m12`, which sets how a **1D** effect is laid out on a 2D segment:
  - `m12` 0 **Pixels**: the effect runs row by row. On a 1-column strip segment that is simply down the strip, top first. **`rY: true` makes LED 0 the bottom**, the same for every strip, because the zig-zag is now handled by the panel map (no more `rev` on strips 2, 5, 8 and 11).
  - `m12` 1 **Bar**: on a full-width segment, each pixel of the effect fills a whole row, drawn **from the bottom up**. That gives "rings" on every strip in step.
  - **Bar with `tp`** (transpose): each pixel fills a whole **column**, so the effect sweeps round the tree. That replaces the Laser Search grouping/spacing trick.

### Live view, `/json/live` and DDP change order
- **WebSocket live view and `/json/live`:** with a matrix they read pixels in **grid order** (row by row, top-left first), not wiring order. For 3,600 LEDs the WebSocket frame is a version-2 frame (4-byte header with width and height). It sends every 4th pixel, skips every other row, and reports a halved size.
- **xLights over DDP:** DDP data also goes through the map, so DDP pixel *i* becomes grid cell (*i* mod 12, *i* ÷ 12). An xLights model built in wiring order would come out scrambled. In 2D mode, xLights would need a 12 × 300 matrix model, row by row from the top-left. xLights is optional now (standalone decision 2026-10-03), but the docs must say so. **Check this in the trial.**

## Unknowns to settle in the trial
1. **Does the per-strip offset (`of`) still work on a 2D segment?** It makes the spiral looks (Candy cane spiral, Rainbow helix, Snowfall, Spiral fill/light). In 0.14 the 2D branch of `setPixelColor` may skip it. If it does, those looks need another way: a 2D effect, or per-strip segments with `startY` shifts (no wrap-around).
2. Do fades still work: the Fade down step (segment brightness 0, then back up) on 2D segments?
3. The real frame rate with 1D looks and with 2D effects.
4. The WebSocket frame layout (which grid cells are sent) and the DDP order, both read from the board.
5. **How to switch back.** Sending `/json/cfg` without `matrix` doesn't clear it (WLED only sets it when the key is there). The options:
   - `"matrix": {"mpc": 0}`, to be tested;
   - WLED's 2D settings page ("1D strip");
   - uploading the backed-up `cfg.json`.

## Steps

### Step 1: trial on the board (a config change: back up first, ask before doing it)
- Add a layout `config/proto-tree-2d.json`: a copy of proto-tree with `"matrix": true`.
- Extend TreeTool's `plan`/`apply` so they can send the panel config above and rebuild the 12 column segments (`rY: true`). Nothing else changes yet.
- `treetool plan`, then `apply` (backs up `/json/cfg`; the board restarts).
- Check `/json/info` shows `matrix {w: 12, h: 300}`.
- Then, from WLED's own UI or the API:
  - run `test chase`: every strip should run bottom to top;
  - try 6–8 of the 2D effects on one full-grid segment;
  - read the frame rate;
  - test `of` on column segments;
  - run one show with fades;
  - capture one WebSocket frame to confirm its layout.
- Switch back, and write down which method worked. **Decide go / no-go with the results.** The phone app isn't 2D-aware yet, so its preview and strip controls will be wrong during the trial. Use WLED's UI.

### Step 2: TreeTool (only if go)
- `Layout.cs`: an optional `"matrix": true`. `LayoutMath.Panels()` derives the panels from the outputs (one per output, `w` = strips on it). `LayoutMath.Segments()` gives column bounds in 2D.
- `Planner.cs`:
  - `DesiredCfg` adds `hw.led.matrix`, and `CfgMatches`/`PrintPlan` compare it;
  - `DesiredSegmentsState` sends `start`/`stop` as a column, `startY`/`stopY`, `rY: true`, `rev: false`, `m12: 0`.
- `Looks.cs`:
  - `Segment()`: 2D bounds, and `rY = !flip` instead of `rev = RunsDown ^ flip`. The twist depends on unknown 1.
  - `AcrossState()` (Laser Search): bands become full-width row ranges (`start 0, stop 12, startY a, stopY b`) with `tp: true, m12: 1` (Bar), so there's no grouping/spacing.
  - Overlays become columns too.
  - A new look option, `"grid": true`, for 2D effects: one segment over the whole grid, the other ids deleted.
- `Program.cs`: the `test strips`/`chase` modes use the new segments. `looks` **converts app-saved looks** in presets.json (1 "Solid", 2 "Ocean Meteor", and any made on a phone):
  - a 300-LED linear segment at 300·*k* becomes column *k*;
  - `rY` = (old `rev` == strip runs down);
  - this happens in the same single compact upload as the fades conversion.
- `test 2d` mode: Matrix on the whole grid, as a quick check.

### Step 3: the app (`app/tree.htm`, only if go)
The layout's `matrix` flag in `/tree.json` switches it, so 1D keeps working (`buildStrips` still mirrors `LayoutMath`).
- `buildStrips`: strips get a column (`x`) instead of a linear start/stop.
- `buildLedMaps`: map **grid order** to strip and height. Cell *i* is column *i* mod 12 (strip *x* + 1), and the height from the bottom is 299 − ⌊*i*/12⌋.
- WebSocket preview: decode version-2 frames by repeating WLED's own sampling loop (every *n*-th cell, skipping rows), and decode `/json/live` the same way.
- `sendSegments`:
  - always sends each strip's bounds (`start/stop/startY/stopY`, `rY`), so a whole-grid 2D look is replaced by the 12 strips again;
  - a 2D-only effect goes on one segment over the whole tree, or over the selected strips if they're next to each other; otherwise it says it needs neighbouring strips.
- The Effects list shows the 36 2D effects automatically (`S.matrix`). Mark them "2D", and say on the Effects tab that they cover the whole tree.
- Pause/fades/now-showing: no change expected beyond the bounds. Check them.

### Step 4: looks, shows and docs
- `treetool looks` regenerates every look in 2D form and converts the app-saved looks, with one compact presets.json upload.
- Add 3–4 2D looks to `config/looks.json`, for example "Digital rain" (Matrix), "Fire up the tree" (Firenoise), "Helix" (DNA Spiral) and "Northern lights" (Polar Lights). Maybe add a "2D show".
- Docs:
  - `docs/software.md`: the matrix, the order change for live view and DDP, and how to switch back;
  - `config/README.md`: the `matrix` field;
  - `docs/roadmap.md`: tick the Stage 4 item;
  - the xLights note in `CLAUDE.md`/design docs: in 2D, the DDP order is a 12 × 300 grid, row by row from the top.

## Risks
| Risk | Effect | Mitigation |
|---|---|---|
| `of` ignored in 2D | spiral looks lose their twist | settled in the trial; redesign those looks or stay 1D |
| Slower frames | 2D effects stutter | measure in the trial; keep heavy ones out of shows |
| Can't switch back cleanly | stuck in 2D | backup `cfg.json`; test `mpc: 0` and the settings page in the trial |
| xLights order | scrambled DDP shows | 12 × 300 matrix model in xLights, or stay 1D while sequencing |
| Old presets | looks drawn wrong after the switch | TreeTool converts them in one upload; backup first |

## Done when
- Matrix, Firenoise and DNA Spiral play on the tree from the app.
- Every existing look still runs bottom to top, and shows still fade.
- The app's preview matches the tree.
- `treetool apply` can switch the board between `proto-tree.json` (1D) and `proto-tree-2d.json` (2D) and back.
