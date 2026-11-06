# Power & Wiring

> An **LED-strip** is one 5 m strip (12 in all); a **tree-strip** is the three LED-strips on one Dig-Quad output, joined by their data wires (4 in all). See [LED-strips and tree-strips](build.md#led-strips-and-tree-strips). The finished tree has **two separate fuse boxes**, one per supply.

The spec is in [design-summary.md](design-summary.md). Conflicts are listed in [open-questions.md](open-questions.md) §1–4.

## The builder's email

> 12V DC @ 4Amps each, so for 12 LED-strips it will need 48Amps … looking at a 60Amp power supply … Will run power from the bottom of each set to the top, not sure how we are going to do the data.

## Decision (2026-10-17): 5 m LED-strips, fed at the bottom only

Mr Furniss chose: **12 × 5 m LED-strips (300 LEDs each, 3,600 in total), one whole roll per LED-strip with no soldered extensions. Each LED-strip is fed at its bottom end only, by its own fused cable run along the ground from the base box. The PSUs and the controller sit in one box at the base, and no power goes up the mast.** This replaces the 2026-10-10 top-feed plan (cables up the centre mast), which itself replaced the earlier 6 m plan with feeds at both ends.

Why one end is enough: 5 m is the LED-strip's own injection interval (the seller says to feed every 5 m), and WS2815 runs down to about 9.5 V. At full white the **top (apex end)** of each LED-strip may look slightly warmer or dimmer than the bottom. Coloured effects won't show it.

Why the bottom: the feed cables are much shorter (about 1.5–4.5 m instead of about 8 m), so they drop less voltage and need about half the cable, nothing heavy climbs the mast, and every power joint can be reached from the ground.

⚠️ **Bench check:** feed one LED-strip from one end, set solid white at 255 with the limiter off, and measure the voltage at the far end. **Above about 10.5 V is fine.** Below that, add a second feed at the top of each LED-strip (up the mast, as in the old plan).

## Power budget (5 m LED-strips)

| | The builder / measured | BTF-LIGHTING rating (ceiling) |
|---|---|---|
| Per 5 m LED-strip | **~3.3 A** (11 mA × 300) | 7.5 A (0.3 W × 300) |
| Total, 12 LED-strips | **~40 A / 480 W** | 90 A / 1,080 W |
| PSUs | **PSU 1 HLG-320H-12A: LED-strips 1–6 + Dig-Quad, ~20 A of 22 A (92 %). PSU 2 HLG-320H-12A: LED-strips 7–12, ~20 A of 22 A (90 %)** | Far beyond 44 A. The limiter must cap it |

**Decision (2026-10-26): two supplies, two fuse boxes**, replacing the 2026-10-22 three-supply plan. Both are **HLG-320H-12A** (12 V, 22 A, 264 W, IP65), with six LED-strips on each: PSU 1 drives outputs LED1–LED2 (LED-strips 1–6) and the Dig-Quad, PSU 2 drives LED3–LED4 (LED-strips 7–12). That's 44 A for a working load of about 40 A, so there is little margin at full white: **cap each of the four outputs at about 9,500 mA in WLED, total about 38,000 mA**, which keeps each supply at about 19 A. If a LED-strip measures **above about 3.5 A** on the bench, lower those caps to match. (The HLG-480H-12A in an earlier plan doesn't exist; the 12 V, 480 W model is the HLG-600H-12A.)

**HLG-320H-12A adjusters:** the A version is IP65 and has two small adjustment screws under rubber seal caps: Vo (output voltage, factory-set to 12 V) and Io (current limit, factory-set to maximum). Leave both alone and make sure the seal caps are pushed fully home. IP65 is fine inside the base box.

**The 150 W supply** bought on 2026-10-22 is not in the finished tree. It stays as the bench supply (and powers the three bench LED-strips until the HLGs arrive).

### Split-load rule
- PSU 1 → LED-strips 1–6 (outputs LED1–LED2) and the Dig-Quad via its 2 A fuse; PSU 2 → LED-strips 7–12 (outputs LED3–LED4). Each PSU has its own fuse box in the base box.
- No tree-strip crosses a PSU boundary, so each output's three LED-strips are all on one supply. Jumpers still carry GND as well as data, and all grounds meet at the bar.
- **Never join the V+ of two supplies.** **Always join their GNDs** (a shared ground bar, also connected to the controller).

## Wiring

```
                 TOP RING
     S1 top                   S2 top                   S3 top
       ▲ data      └─ jumper (DO/BO → DI/BI) ─┘  ▼ data            ▲ data
       │                                         │                 │
     S1 (up)                                   S2 (down)         S3 (up)
       │                                         └──── jumper ─────┘
     LED1 data ──┘  (BI tied to GND here only)
     S1 bottom ◄── feed 1     S2 bottom ◄── feed 2     S3 bottom ◄── feed 3
                 BASE RING
                                 ║ 12 two-core cables along the ground
     ┌───────────── BASE BOX ────╨───────────────────────┐
     │ PSU 1 → fuse box A (6 × 5 A) → feeds 1–6          │
     │       └ inline 2 A fuse → Dig-Quad power in       │
     │ PSU 2 → fuse box B (6 × 5 A) → feeds 7–12         │
     │ Common GND bar · Dig-Quad (data + GND out)        │
     └───────────────────────────────────────────────────┘
```

- **Power:** each LED-strip gets its own fused feed from the base box, run along the ground (in cable protection) and around the base ring to the LED-strip's **bottom** power leads. The fuses stay in the base box. Nothing electrical needs servicing at the top except the 4 data jumpers.
- **Data:** each port zig-zags (S1 up, S2 down, S3 up). The tree-strip **starts** at the base (bottom of S1), the S1→S2 jumper is at the **top**, and the S2→S3 jumper is at the **bottom**. Jumpers carry DO→DI, BO→BI **and** GND. Tie BI to GND only at the tree-strip start.
- **Ground for data:** the controller's data lead includes a GND wire. Connect it to the GND pad at the tree-strip start (the bottom of S1), so the data signal has a ground reference right where it enters the LED-strip.
- **Heavy current does not go through the Dig-Quad.** The board is powered from **PSU 1 through an inline 2 A blade fuse** (decided 2026-10-17, so board and LED-strips switch on together), and its LED outputs carry **data + GND only**. The bench supply is for bench testing, not the finished tree.
- Optional: a relay on Q1R (GPIO 15) switches the PSUs' mains, so the WLED on/off button really cuts power. That needs a proper mains-rated relay or contactor, installed by someone qualified.

## Cable sizes (preliminary. Recalculate once the mast height is known.)

| Run | Current | Size | Why |
|---|---|---|---|
| PSU → its fuse block | ~20 A (40 A PSU max) | **8–10 AWG (6–10 mm²)**, short | Must survive the PSU's full output |
| Base box → bottom of each LED-strip (about 1.5–4.5 m each way, depending on which side of the tree) | ~3.3 A normal, 5 A fuse | **1.5 mm² two-core** outdoor rubber cable (H07RN-F), one per LED-strip | Longest run drops ~0.35 V (3 %) at 3.3 A, and ~0.8 V even at BTF's 7.5 A ceiling |
| Data, box → tree-strip start | signal | 3-core (data, GND, spare) | Keep the box within 2–3 m of the base ring |

The alternative is one thick trunk per PSU (6 mm²) to a fused junction box on the ground at the far side of the ring. It uses a little less cable, but adds a second weatherproof box with fuses in it. Not worth it at these lengths.

## Mains side (decided 2026-10-17)

- Each PSU has bare AC flying leads. Each one gets **its own mains lead with a plug** (reused existing H07RN-F 3-core cables), joined to the PSU's AC wires with a **SOMELINE IP68 junction connector** (L to brown, N to blue, earth to green/yellow). No shared mains junction box.
- Both plugs go into an **outdoor socket / extension (2-way or more) with a built-in 30 mA RCD**, so every supply is RCD-protected. An optional outdoor smart plug or timer upstream really turns the tree off.
- Have the mains side checked by a qualified electrician before first power-up.

## Fuse distribution

- **Two 6-way blade fuse boxes** (both already bought), one per PSU, each with a negative bus. All six ways in each box carry a LED-strip feed, so the Dig-Quad's 2 A fuse goes in an inline holder from PSU 1's V+ (or a 7-way box if you prefer). A single 12-way box is only OK if it is **split-bus** (two independent positive bars with their own inputs, ≥30 A per side). A common single-bar box would join the PSUs' V+, which is never allowed.
- 5 A ATO blade per LED-strip feed. 2 A ATO blade in the Dig-Quad's inline holder (car-type holders with 16 AWG leads are fine).
- PSU → fuse box with short 6 mm² red/black. Every GND (both PSUs, all fuse-box negative buses, the Dig-Quad) goes to the common ground bar.

## Joints (no soldering, decided 2026-10-17)

There is **no soldering anywhere**. Whole 5 m rolls are used as they come, so there are no cut LED-strip ends to seal. Wires only meet at the LED-strip's factory leads (a 4-pin JST SM plug plus bare power leads at each end, see [strip-spec-ws2815.md](strip-spec-ws2815.md)).

| Where | Count | Wires joined |
|---|---|---|
| Bottom of every LED-strip | 12 | 1.5 mm² feed cable → LED-strip's bottom power leads (+12 V, GND) |
| Chain starts (bottom of S1, S4, S7, S10) | 4 | 0.75 mm² data cable from the box → DI and GND; BI joined to the same GND |
| Jumper ends (8 jumpers, both ends) | 16 | 0.5 mm² 3-core → DO→DI, BO→BI, GND→GND |
| Unused top power leads | 12 | Not joined. Cut the bare ends short and seal each one with a glue-lined heat-shrink end cap, so the +12 V lead can't touch GND or get wet |

- **Joint method (decided 2026-10-22): adhesive-lined heat-shrink butt connectors.** Red (22–16 AWG / 0.5–1.5 mm²) for data, jumpers and the LED-strip's own leads; blue (16–14 AWG / 1.5–2.5 mm²) for the 1.5 mm² power feed, with the thinner LED-strip lead folded back to fill the barrel. Avoid "solder seal" connectors, which melt solder. Each is crimped with a ratchet crimper for insulated terminals, then shrunk with a heat gun until the glue shows at both ends. Tug-test every crimp. If a LED-strip lead is thinner than 22 AWG, fold the bare end back on itself before crimping.
- **Optional take-down plugs:** to unplug the tree each year, put a small pre-wired IP68 plug-and-socket pair (2-pin for power, 3-pin for data) at each joint and butt-splice its pigtails on. Still no soldering. *Not chosen for now: splices only (2026-10-22).*
- Screw-terminal IP68 inline boxes also work but were judged too bulky (2026-10-17).
- **Inside the base box**, every wire goes into a screw terminal with a crimped **bootlace ferrule** (Preciva square crimper). No butt splices in the box.
- **Check wire colours on the real LED-strip** before connecting: the PDFs say white = GND and blue = BI; this LED-strip's spec says black = GND. Go by the pad labels.

## Build order

1. **Bench:** one LED-strip on the bench PSU with the Dig-Quad; measure current and far-end voltage at full white (roadmap Stage 1). If a LED-strip draws over about 3.5 A, lower the WLED per-output caps (about 9,500 mA each) to keep each HLG-320H-12A under 22 A.
2. **Base box:** mount the PSUs, fuse boxes, ground bar and Dig-Quad on the plate, with a 2–3 cm gap around the PSUs. Fit M20 glands in the **bottom face only**. Wire PSU → fuse boxes (6 mm²), all grounds → ground bar, PSU 1 → 2 A inline fuse → Dig-Quad. Ferrule every wire end. Leave the fuses out.
3. **Mains:** fit the SOMELINE connectors to each PSU's AC leads and mains lead. Have it checked. Plug into the RCD socket, switch on, and check 12 V at each fuse-box output with no LED-strips connected.
4. **Feed cables:** measure each run from the box to its LED-strip's bottom, add 30 cm, and cut 12 lengths of 1.5 mm² two-core (about 1.5–4.5 m each, about 40 m in total). Label both ends (S1–S12). Run them along the ground in cable protector or split conduit, then around the base ring, tying them to it with UV ties. Leave a drip loop at the box and at each LED-strip.
5. **Data cables:** 4 × 0.75 mm² 3-core from the box to the bottom of S1, S4, S7 and S10, plus 8 × 0.5 mm² jumpers (4 at the top, 4 at the bottom).
6. **Rig the LED-strips:** hang each backbone, then zip-tie the LED-strip to it every 15 cm. Orient each LED-strip so its data arrow runs up for S1/S4/S7/S10 and S3/S6/S9/S12, and down for S2/S5/S8/S11.
7. **Make the joints** (table above), sealing each one as you go. The bottom joints (power and data) sit just above the ground: tie each one up onto the base ring or backbone, at least 10–15 cm clear of the ground and grass, with the cable looping down below it so water drips off rather than running into the joint.
8. **Terminate at the box:** feeds into fuse boxes (positive) and the negative bus; data leads into the Dig-Quad LED1–4 (data + GND). Bring one quadrant up at a time: insert its 3 fuses, power on, check it lights correctly, then the next.
9. **Seal:** close the lid, check the glands are tight, and add drip loops on every cable below the box.

## Weatherproofing essentials

- Every joint outside the box is a glue-lined heat-shrink butt connector (or an IP68 plug pair spliced on the same way). Nothing exposed, no tape.
- Whole rolls keep their factory end caps; don't cut or open the sleeves.
- No joint ever lies on the ground or in grass, where it would sit in water. Ground runs go in a cable protector or conduit so they can't be tripped over, mowed or chewed.
- IP68 glands on the bottom face of the box only, with drip loops on every cable
- 2–3 cm air gap between the PSUs and the Dig-Quad for cooling
