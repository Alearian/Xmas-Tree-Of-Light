# Power & Wiring

The spec is in [design-summary.md](design-summary.md). Conflicts are listed in [open-questions.md](open-questions.md) §1–4.

## The builder's email

> 12V DC @ 4Amps each, so for 12 strips it will need 48Amps … looking at a 60Amp power supply … Will run power from the bottom of each set to the top, not sure how we are going to do the data.

## Power budget (from the strip spec, [strip-spec-ws2815.md](strip-spec-ws2815.md))

| | The builder / WS2815B table | Tech doc | Part-number spec |
|---|---|---|---|
| Per 6 m run | **4.0 A** (7.92 W/m) | 5.4 A | **7.2 A** (14.4 W/m) |
| Total | **47.5 A / 570 W** | 64.8 A / 777 W | **86.4 A / ~1,040 W** |
| PSUs | **2 × HLG-480H-12A** (59 % load) | 2 × HLG-480H-12A (81 %) | **3 × HLG-480H-12A** (72 %) |

**Working assumption: the builder's figures, 4 A per strip and 48 A in total, with 2 × HLG-480H-12A (6 strips each).** ⚠️ Confirm by measuring one strip at full white before buying the PSUs ([open-questions.md](open-questions.md) §1). If it measures above about 4.5 A, switch to 3 PSUs with 4 strips each.

### Split-load rule
- PSU 1 → strips 1–6, PSU 2 → strips 7–12. Each PSU has its own fuse block.
- **Never join the V+ of two supplies.** **Always join their GNDs** (a shared ground block, also connected to the controller or controllers).

## Suggested wiring (combines the doc and the builder's plan)

One quadrant (LED1 → strips 1–3):

```
  TOP RING     ┌──── jumper ────┐                 ◄── fused +12V/GND feed to top of every strip
               │                │
             S1 ▲             S2 ▼              S3 ▲
               │                │                │
               │                └──── jumper ────┘
  BASE RING    │                                    ◄── fused +12V/GND feed to bottom of every strip
     LED1 data ┘  (BI tied to GND here only)
```

- **Data:** each port runs in a zig-zag (S1 up, S2 down, S3 up). Jumpers carry DO→DI **and** BO→BI. Tie BI to GND only at the chain start.
- **Power:** feed **every strip at both ends** (The builder's bottom→top idea plus the doc's top ring). Each feed is fused.
- **Heavy current does not go through the Dig-Quad.** The board's limits are 30 A in total and 10 A per shared fuse, and it takes only one PSU ([open-questions.md](open-questions.md) §2).

## Revised power layout

```
 Mains ─┬─► PSU 1 (12 V, 40 A) ─► fuse block A ─► strips 1–6    (top + bottom feed per strip, 5 A fuse each)
        └─► PSU 2 (12 V, 40 A) ─► fuse block B ─► strips 7–12

 Common GND bar ◄── PSU1 −, PSU2 −, every strip GND, every controller GND

 Controller(s): each powered from ONE PSU through a small fused feed (or 5vEXT).
                LED outputs carry DATA only. Their V+ output terminals are NOT used for the strips.
```

- Load per PSU: 6 × 4 A = **24 A of 40 A (60 %)**.
- The PSUs' V+ outputs are **never** joined. Their grounds **are** joined.
- With 2 Dig-Quads (open-questions §8, option B), each half of the tree is one PSU, one controller and 6 strips.
- Optional: a relay on Q1R (GPIO 15) switches the PSUs' mains, so the WLED on/off button really cuts power ([quinled-dig-quad-guide.md](quinled-dig-quad-guide.md)). That needs a proper mains-rated relay or contactor, installed by someone qualified.

## Wire gauges (preliminary. Recalculate once the physical layout is fixed.)

| Run | Current | Gauge | Why |
|---|---|---|---|
| PSU → its fuse block | 24 A (40 A PSU max) | **8–10 AWG**, short | Must survive the PSU's full output |
| Top-ring bus per PSU group (about 6 m up) | ~12 A normal (the top halves of 6 strips) | **12 AWG** minimum, 10 AWG better, outdoor, UV-rated | Over the 12 m round trip: 14 AWG drops ~1.2 V (10 %, borderline), 12 AWG ~0.75 V (6 %), 10 AWG ~0.5 V (4 %) |
| Fuse block → each strip end | ~2 A normal, 5 A fuse | **18 AWG** | Short pigtails |

These are based on 4 A per strip. If the bench measurement is higher, recalculate.

## Weatherproofing essentials

- Neutral-cure silicone in every cut sleeve end, with dual-wall adhesive heat-shrink over every joint
- IP68 glands on the bottom face of the box only, with drip loops
- 2–3 cm air gap between the PSUs and the Dig-Quad for cooling
