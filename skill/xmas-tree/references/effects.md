# Designing new looks, effects and shows

The tree's "effects" are **looks**: one WLED effect applied to all 12 strip segments with cone-aware
tweaks, saved as a preset. A **show** is a playlist of looks. Both are defined in `config\looks.json`
and installed with `treetool looks`, or made on the phone (Effects → "Save as look…", Shows tab).
New native effects (C++ in WLED) need a custom firmware build - not before Christmas.

## Workflow with Mr Furniss

1. **Pin down the idea** in his words: colours, movement (up, down, round, spiral, sparkle), speed,
   which strips (all, N/E/S/W, odd/even), and whether it belongs in a show.
2. **Pick the WLED effect** that does the movement along one strip (list below, or `/json/eff`), then
   decide how strips relate: `twist` for spirals, `flip` for up/down alternation, `groups` for
   per-side colours, `overlay` for a second layer, `across` to run round the tree instead of up it.
3. **Try it live** with a state POST - no save, so nothing can corrupt presets:
   ```powershell
   $b = 'http://<board-ip>'
   $segs = 0..11 | ForEach-Object { @{ id = $_; fx = 76; pal = 2; sx = 10; ix = 40; col = @(@(208,232,255),@(0,0,0),@(0,0,0)) } }
   Invoke-RestMethod -Method Post "$b/json/state" -ContentType 'application/json' -Body (@{ on = $true; seg = $segs } | ConvertTo-Json -Depth 6 -Compress)
   ```
   (Look up fx/pal ids by name: `$e = Invoke-RestMethod "$b/json/eff"; [array]::IndexOf($e, 'Meteor')`.)
   Strips 1-3 are the only ones wired; the app's Tree tab previews all 12.
4. **Tune** speed (`sx`) and intensity (`ix`) with him watching; adjust colours.
5. **Write the look** into `config\looks.json` with a free id (check the report's preset list; never 10,
   249, 250; skip ids used by phone-made shows).
6. **Install**: check presets.json size first, then
   `dotnet run --project tools\TreeTool -- looks --layout config\proto-tree.json`, then run the report.
7. Optionally add it to a show (`shows[].looks`).

## looks.json schema

```json
{
  "looks": [
    { "id": 34, "name": "Candy cane spiral", "fx": "Running", "pal": "Default", "sx": 70, "ix": 40,
      "colours": ["#ff1010", "#ffffff", "#000000"], "twist": 12 }
  ],
  "shows": [
    { "id": 40, "name": "Tree show", "looks": [20, 27, 21], "seconds": 60, "fadeSeconds": 2 }
  ]
}
```

| Field | Meaning |
|---|---|
| `id`, `name` | preset number (1-249, not 10/249/250) and name |
| `fx`, `pal` | effect and palette **by name** as WLED lists them (`/json/eff`, `/json/pal`) |
| `sx`, `ix` | speed, intensity 0-255 |
| `colours` | up to 3 `#rrggbb` |
| `twist` | LED offset added per strip going round: 0 = rings, non-zero = spiral (12-25 gentle, 137 scatters) |
| `flip` | `odd`, `even` or `all` - reverse direction on those strips |
| `groups` | overrides keyed `odd`, `even`, `N`, `E`, `S`, `W` or a strip number: `{ "E": { "colours": [...] } }` |
| `overlay` | second effect on top (`fx`, `pal`, `sx`, `ix`, `colours`, `twist`, `flip`, `groups`); only effects with an Overlay option work (Drip, Glitter, Sparkle). Uses segments 12-23. |
| `across` | run across the strips: `from`, `to`, `bands` (odd, ≤ 19 with 12 strips), `endBrightness` %. Each strip becomes one pixel; strip segments are frozen black. Example: preset 33 Laser Search. |

Shows: `looks` (ids in order), `seconds` per look, `fadeSeconds` (uses hidden preset 249 Fade down
between looks - WLED 0.14 swaps effects instantly otherwise). Shows end on 250 Lights off.

## Effects that work well on a vertical strip

| Effect | Good for | Notes |
|---|---|---|
| Running, Wipe | rising bands, candy cane with `twist` | 2 colours |
| Meteor, Meteor Smooth | falling snow, sci-fi streaks | **set `pal` to `* Color Gradient`** or it uses Party colours |
| Fire 2012 | fire tree | palette `Fire` |
| Twinklefox | fairy lights | palette `C9` for classic bulbs |
| Rainbow | rainbow helix with `twist` | |
| Solid Glitter, Drip, Sparkle | sparkle, snow overlay | have overlay option |
| Scanner | Laser Search with `across` | `* Color 1`, high `ix` = short trail |

## Limits to design within

- **32 segments**: 12 strips + up to 12 overlay segments, or + 19 height bands.
- **Dim by colour**: to make a band fainter, scale its colour (e.g. `#4d0000` for 30 % red), not `bri`;
  segment opacity is reapplied per pixel and makes trailing effects vanish.
- **presets.json < 56 KB**: each look costs ~1-3 KB (more with groups/overlay/across). Check the size
  before adding several.
- **Power**: full white on all strips is the worst case; the limiter trims to `abl.maxMa`. Prefer
  colourful looks with black backgrounds.
- **Beyond WLED**: per-pixel designs (images, text, 3D sweeps) need xLights streaming over DDP
  (UDP 4048) from a PC, or a 2D matrix config after Christmas.
