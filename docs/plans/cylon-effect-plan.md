# Plan: "Cylon" effect, saved as "Laser Search" (Larson scanner across the strips)

Status: **built and seen on the tree 2026-10-24; saved as preset 33 "Laser Search"** (app version 2026-10-24k). See "Implementation notes" at the end. Firmware stays on WLED 0.14.0 (see [wled-16-upgrade-assessment.md](../wled-16-upgrade-assessment.md)). Builds on the app version 2026-10-24 (Shows tab), which is not yet checked on the tree.

## What we want (agreed 2026-10-24)
- A red vertical line, **one whole strip**, sweeps sideways across the strips and back, like a Cylon eye or a Larson scanner.
- **Whole tree**: it sweeps 1 → 12 → 1, right round the cone. Starts at strip 1; the centre is 6½, which is fine.
- **The eye is a single strip** with little or no trail ("for now").
- **Brightness along the strip**: 100 % in the middle of the strip's height, fading to **30 % at the top and bottom**.
- **Speed**: **4 seconds one way** (strip 1 → 12; changed from 2 s after the first look), so 8 s there and back.
- **Preset 33 "Cylon"**, a look on its own: **not** added to any show.

## The idea in one line
Make WLED treat each strip as one "pixel", run WLED's own **Scanner** effect over those 12 pixels, and do the top/bottom fade by splitting the strip's height into **bands**, each band its own Scanner segment at a different brightness. There's no new firmware and no config change, nothing needs the PC or phone while it runs, and it's saved as an ordinary look/preset, so it can go into shows.

### Grouping and spacing
WLED segments have two options that make this work:
- **Grouping** (`grp`): paint *g* LEDs with each virtual pixel.
- **Spacing** (`spc`): then skip *s* LEDs before the next virtual pixel.

The strips are numbered in LED order (1 = LEDs 0–299, 2 = 300–599, … 12 = 3,300–3,599), so a segment that starts at LED *offset*, with `grp` = band length and `spc` = 300 − band length, has exactly **12 virtual pixels, one band per strip**, at the same height on every strip.

The strips zig-zag (1 up, 2 down, 3 up …), so LED offset 0 is the bottom of strip 1 but the top of strip 2. Because the brightness profile is **symmetric** (30 % at both ends), that doesn't matter: the top band and the bottom band get the same brightness, so "offset 0–39" is always an end band whichever way the strip runs.

### The bands (7 bands, symmetric)
| Band (LED offset within each strip) | Height | Brightness (segment opacity) | Segments |
|---|---|---|---|
| 0–39 and 260–299 | ends, 0.67 m each | 30 % (77) | 2 |
| 40–79 and 220–259 | | 55 % (140) | 2 |
| 80–119 and 180–219 | | 80 % (204) | 2 |
| 120–179 | middle 1 m | 100 % (255) | 1 |

That's **7 segments**, ids 12–18. The app and TreeTool already treat ids 12–23 as "overlay" segments and delete them when another look or effect is picked, so it fits without new clean-up code. Every band runs the same Scanner with the same speed and colour; WLED's Scanner works from the clock, so the bands move together (to be confirmed on the bench). The fade comes in steps of about 0.67 m; more, thinner bands (up to 11 in the free ids) would make it smoother.

### Keeping the strip segments
The app and TreeTool depend on the **12 strip segments (ids 0–11)** staying in place: the app's `sendSegments` only sends effect settings, never start/stop. So Cylon keeps them, set to Solid black and **frozen** (`frz: true`).

Why frozen: in 0.14 every segment draws into one shared pixel buffer and the last one drawn wins. Scanner also makes its fade by dimming what's already there. If the strip segments kept drawing black underneath every frame, they'd fight the scanner. Frozen segments are skipped, so only the bands paint.

WLED keeps `frz` until told otherwise, so everything that sets a look must send `frz: false`:
- TreeTool `Looks.Segment`: add `["frz"] = false` (re-run `treetool looks` so existing presets get it).
- App `sendSegments` (app/tree.htm ~line 362): add `frz: false` to each strip segment.
- Looks saved with "Save current look" on the phone store the current state, which is unfrozen, so they're fine.

**Fallback** if the freeze trick misbehaves on the bench: let Cylon replace the segment layout outright, and make the app's `sendSegments` and TreeTool send `start`/`stop` with every strip segment so the layout comes back afterwards (more code, same result).

## Options considered
| Option | Verdict |
|---|---|
| **A. Banded grouped segments + built-in Scanner** (recommended) | On the board alone, no config change, fits looks and shows. Moves a whole strip at a time (12 positions), which suits a Cylon eye. |
| B. WLED 2D matrix (12 × 300) | A **config change** that changes how every other look and the app's strip segments work, and it still has no built-in vertical fade. January at the earliest. |
| C. App-driven animation from the phone | Only runs while a phone has the app open, jerky over Wi-Fi, can't be scheduled. No. |
| D. xLights over DDP | Easy to draw with any fade, but needs the PC running. Good for one-offs only. |
| E. Custom WLED effect (usermod) | Needs our own firmware build and USB recovery. No. |
| F. A playlist of 22 presets, one per eye position | Works, but uses 22 preset slots, reads flash 10 times a second and can't sit inside a show (0.14 can't nest playlists). No. |

## Build steps (when you say go)
1. **Bench test by hand** (state only, allowed during testing). First check the effect number: `(Invoke-RestMethod http://<board-ip>/json/eff).IndexOf('Scanner')`. Then, with that number in `$fx`:
   ```powershell
   $fx  = 40          # from the line above
   $red = @(@(255,0,0), @(0,0,0), @(0,0,0))
   $seg = @(0..11 | ForEach-Object { @{ id = $_; fx = 0; col = @(@(0,0,0)); frz = $true } })
   # band: first LED offset, length, opacity
   $bands = @(@(0,40,77), @(260,40,77), @(40,40,140), @(220,40,140), @(80,40,204), @(180,40,204), @(120,60,255))
   $id = 12
   foreach ($b in $bands) {
     $seg += @{ id = $id; start = $b[0]; stop = $b[0] + 11*300 + $b[1]; grp = $b[1]; spc = 300 - $b[1];
                bri = $b[2]; fx = $fx; sx = 128; ix = 30; col = $red; on = $true; frz = $false }
     $id++
   }
   $body = @{ on = $true; seg = $seg } | ConvertTo-Json -Depth 6
   Invoke-RestMethod -Uri http://<board-ip>/json/state -Method Post -Body $body -ContentType application/json
   ```
   Check on strips 1–3 and in the app preview:
   - the eye is one strip wide, brightest in the middle, about 30 % at top and bottom
   - all 7 bands move together and stay together over a few minutes
   - the eye crosses the GPIO change between strips 3 and 4 cleanly (watch the preview)
   - with a stopwatch, set `sx` so one pass takes 2 s; set `ix` low enough that the trail is barely visible
   - picking any effect in the app afterwards brings the tree back to normal
2. **TreeTool**: a new look shape in `looks.json`, for example
   ```json
   { "id": 33, "name": "Cylon", "fx": "Scanner", "pal": "Default", "sx": 128, "ix": 30,
     "colours": ["#ff0000", "#000000", "#000000"],
     "across": { "from": 1, "to": 12, "endBrightness": 30, "bands": 7 } }
   ```
   `Looks.State` sees `across` and writes: the strip segments as frozen Solid black, then the bands from id `stripCount` up, worked out from `ledsPerStrip` (so it still works if the strip length changes), with the brightness stepping evenly from `endBrightness` to 100 %. Normal looks get `frz: false`. About 40 lines in `Looks.cs`, plus the looks table in `config/README.md`. Put the `sx`/`ix` found on the bench into the entry.
3. **App**: add `frz: false` in `sendSegments`. Cylon then appears in the Looks list and can go into shows from the Show editor. The live preview reads the real LED colours, so it shows the eye with no extra work.
4. **Install**: back up `presets.json`, then `dotnet run --project tools/TreeTool -- looks --layout config/proto-tree.json`; check it on the tree and tune.
5. **Docs**: `config/README.md`, `docs/software.md`, `docs/roadmap.md` (Stage 4).

Nothing here changes `/json/cfg` (LED outputs, Wi-Fi or limits).

## Things to know
- **Only strips 1–3 are wired**, so today the eye appears on 3 strips and goes "behind" for the other 9; the app preview shows the full sweep. With 2 s per pass it's dark on the real strips for about 1.5 s of each pass until 4–12 go in.
- **Power is tiny**: one strip of red at partial brightness, well under 2 A.
- **It goes round the tree**, so from any one spot you see the eye cross the near side, go behind, and come back. That's the "whole tree" choice; a front-arc-only version is a small change later (`from`/`to`), as long as the arc doesn't cross the strip 12/1 join at north.
- **Steps, not glides**: the eye jumps a strip at a time. A smooth glide would need option B or D.
- With two controllers in the future each board would have its own half of the sweep and they'd drift apart. Fine for one Dig-Quad.

## Still open
- **Trail**: none at all, or a faint one-strip tail behind the eye? Scanner always leaves a little; its Intensity sets how much. Decide on the bench.

## Implementation notes (2026-10-24)
- `config/looks.json`: preset **33 "Cylon"**: Scanner, palette "* Color 1" (so it uses the red, not a rainbow), `sx` 100, `ix` 220, `across` { from 1, to 12, bands 7, endBrightness 30 }. Not in any show.
- `Looks.cs`: `State` hands `across` looks to a new `AcrossState`. 7 bands on 300 LEDs come out as six end bands of 42 LEDs (30 %, 53 %, 76 %) and a 48-LED middle band at 100 %, segment ids 12–18; 19–23 are deleted. Normal looks now also send `grp` 1, `spc` 0, `bri` 255 and `frz` false, so nothing is left over from Cylon (a band at id 12 would otherwise keep its grouping when a look with an overlay reuses id 12).
- `Planner.cs` (`apply`): the strip segments get the same reset.
- App `sendSegments`: sends `frz: false` to every strip on the board, including ones not selected, so picking an effect for a few strips doesn't leave the rest frozen.
- **Speed maths** (WLED 0.14 Scanner): one pass takes 65,536 / (`sx`/4 + 8) ms, so `sx` 100 gives ≈ 1.99 s. Higher `sx` is faster.
- **Trail**: for Scanner, higher `ix` fades faster, so a *higher* Intensity means a *shorter* trail. 220 should leave about one strip of glow; 255 is nearly none.
- **Start position**: Scanner works from the board's clock, so the eye starts wherever the clock puts it, not always at strip 1. It then sweeps 1 ↔ 12 as planned.
- Not compiled in the cloud (no .NET there): build it on the PC.
- **2026-10-24, first look on the tree:** 7 bands were too clunky. Raised to **19 bands**, the most WLED 0.14 allows (32 segments on an ESP32, minus the 12 strip segments, made odd). That's 18 end bands of 15 LEDs (0.25 m) stepping 30 → 92 % in about 8 % steps, plus a 30-LED middle band at 100 %, ids 12–30. The app and TreeTool now delete every id after the strips up to 31, not just 12–23. Smoother than this needs xLights over DDP, or a 2D/firmware route after Christmas.
- **2026-10-24, second look:** too fast, and the fade didn't show ("mostly solid lines"). Two causes, found in WLED 0.14's source:
  - **Opacity is the wrong dimmer.** `Segment::setPixelColor` scales by the segment's opacity on every write, and Scanner's `fade_out` reads each pixel back and writes it again every frame, so a band at 30 % opacity is dimmed by 30 % again each frame and vanishes almost at once while the middle band keeps its trail. Bands are now dimmed through their **colour** (red scaled to 30–100 %) with opacity 255. WLED's colour gamma then makes the steps even to the eye, so 30 % may look darker than expected; raise `endBrightness` if so.
  - **Trail and speed:** `ix` 220 left a trail 2–4 strips long. Now `ix` 245 (fades in about 0.15 s) and `sx` 34, which is 65,536 / (8 + 8) ≈ **4.1 s per pass**, as asked.
- **2026-10-24, outcome:** Mr Furniss felt it looks more like a laser than a Cylon eye and renamed it **"Laser Search"** (preset 33, still not in any show). A truer Cylon could come back later as its own look.
