# Open Questions & Doc Issues

Found while reading the PDFs on 2026-09-30. These need agreeing with the builder before anything is bought or wired.

## 1. Power budget — WORKING ASSUMPTION: the builder's figures (4 A per strip, 48 A total)

**Updated 2026-10-10:** the strips are now **5 m** (300 LEDs), about **3.3 A each, 40 A in total**, still on 2 × HLG-600H-12A. **Updated 2026-10-22:** now 3 PSUs: 2 × HLG-320H-12A (22 A) + one 150 W (~12.5 A), 56 A in total for strips 1–5 / 6–9 / 10–12. See [power-and-wiring.md](power-and-wiring.md).

**Decided 2026-09-30:** we're going ahead with **The builder's calculation of 4 A per 6 m strip, 48 A / ~576 W in total**. It matches the WS2815B table (7.92 W/m). The baseline PSU plan is **2 × Mean Well HLG-600H-12A** (80 A, 60 % load), 6 strips each. The builder's single 60 A PSU (80 % load) is still his call, but see the cautions below.

> ⚠️ **CHECK BEFORE BUYING PSUs:** measure one strip at full white on the bench (roadmap Stage 1). The part-number spec says 7.2 A per strip. If the measurement is above about 4.5 A, revisit this section and use the decision rule below.

### Background: the sources disagree

The strip is **HD 12V WS2815 60L B IP67**, but two sources disagree (see [strip-spec-ws2815.md](strip-spec-ws2815.md)):

| Source | Per 6 m run | Whole tree | PSUs needed (about 75 % load or less) |
|---|---|---|---|
| The builder's email | 4 A | 48 A | |
| **WS2815B table (7.92 W/m)** | **4.0 A** | **47.5 A / 570 W** | **2 × HLG-600H-12A** (80 A, 59 % load), 6 strips each |
| Technical doc (15 mA/LED) | 5.4 A | 64.8 A / 777 W | 2 × HLG-600H-12A (81 % load, tight) |
| **Part-number spec (14.4 W/m)** | **7.2 A** | **86.4 A / 1,040 W** | **3 × HLG-600H-12A** (120 A, 72 % load), 4 strips each |

The builder's 4 A matches the WS2815B table. Even at that figure, **one 60 A PSU** would run at 79 %, it would be a single ~720 W unit (probably with a fan, which the doc advises against outdoors), and all the strips would depend on it. **Two fanless supplies are better in every case.**

### Decision rule after measuring one strip at full white (current limiter off)
- **≤ 4.5 A per strip** → 2 × HLG-600H-12A, 6 strips each. This pairs naturally with **2 controllers**.
- **4.5–6.5 A** → 2 × HLG-600H-12A (50 A each), or 3 × HLG-600H.
- **> 6.5 A** → 3 × HLG-600H-12A, 4 strips each. This pairs naturally with **3 Dig-Quads** (§8, option C).

Whatever the result: each PSU has its **own fuse block**, the V+ outputs are never joined and **all grounds are joined**.

### Also
- Set WLED's current limit to match the PSUs as a second layer of protection. It is **not** a replacement for fuses.
- **Fuses:** one blade fuse per strip feed, **5 A** on the builder's 4 A figure (7.5 A if it measures about 7 A). Each strip is fed at both ends, so one feed can carry the whole strip if the other fails.
- The doc's PSU section was also wrong in itself: its "Option B" of 2 × LRS-350 gives 700 W, which is below even its own 777 W figure.

## 2. How much current can the Dig-Quad carry? — RESOLVED 2026-09-30

The doc routes strip power **through the Dig-Quad's fused outputs** and wires **two PSUs into the board**. QuinLED's own specs rule both out (see [quinled-dig-quad-guide.md](quinled-dig-quad-guide.md)):

- The board is rated **30 A continuous in total** (360 W at 12 V). The tree needs up to 65 A.
- **LED1+LED2 share one fuse and LED3+LED4 share another**, each with a 10 A maximum. Six strips on a 10 A fuse won't work.
- QuinLED says: **"Do not connect wires from multiple power supplies to a single QuinLED-Dig-Quad!"** The doc's Addendum E would join two PSUs' V+ through the board's shared input rail, which breaks its own "never join V+" rule.

**Decision:** strip power goes through a **separate fused distribution** (bus bars and a fuse block per PSU). The Dig-Quad carries **data only**, is powered from **one** PSU (or 5vEXT), and shares a common ground with everything.

## 3. Power injection points

- The doc injects at **both ends of each 3-strip chain**, which is the chain start at the base and the chain end at the top. Each chain is 18 m and 1,080 px, but the doc's own WS2815 table says to inject every **150–300 px**. The middle strip of each chain would sag.
- **Decided 2026-10-17:** with 5 m strips, each strip is fed at its **bottom end only**, by its own fused cable from the base box along the ground (replacing the 2026-10-10 top feed up the centre mast) ([power-and-wiring.md](power-and-wiring.md)). The text below is the earlier reasoning.
- **The builder's plan** is to feed power to **every strip**, bottom to top, which fits that guidance better. **Suggested approach:** feed every strip at both its top and bottom, all 12 of them. It needs 24 fused feeds, or 12 feeds with bus bars top and bottom.

## 4. Data routing (answers the builder's question)

- Use the **zig-zag** from the doc. Each port's data goes into strip A at the bottom and comes out at the top, then a short jumper takes it into strip B at the top. Strip B runs down, then a short jumper takes it into strip C at the bottom, which runs up. Adjacent strips in each quadrant are close together at the rings, so the jumpers stay short.
- Each jumper carries **all 4 wires** (+12 V, GND, DO→DI, BO→BI). Tie BI to GND **only** at the start of each chain.
- The doc disagrees with itself on long data runs. §5.2 says to add a booster if the board is more than 3 m from the first pixel. Addendum C says the level shifters reach the top of the tree without boosters. The zig-zag avoids the problem because data always enters at the base. **Keep the box within 2–3 m of the base ring.**

## 5. Frame rate claim

The doc says "42+ FPS". WS281x data takes about 30 µs per pixel, so **1,080 px per port gives at most about 30 FPS**. That's still smooth for Christmas effects, but don't expect 42+. The only fix is more ports with fewer pixels each.

## 6. Playing xLights shows standalone on WLED

The docs say to "export compiled xLights shows and upload them as presets" to WLED. **Standard WLED can't play xLights sequences (.fseq).** Its presets and playlists only use WLED's own built-in effects. The options are:

- **a.** Use WLED's built-in effects, presets and playlists for standalone running. This works now, and the effects can be mapped to the tree with WLED segments and 2D.
- **b.** Keep a small always-on player next to the tree, such as a Raspberry Pi running **Falcon Player (FPP)** or our own software, playing `.fseq` files to WLED over DDP.
- **c.** A WLED build with an SD-card/FSEQ usermod (non-standard, needs custom firmware).

This affects what our own software needs to do (see [roadmap.md](roadmap.md)).

## 8. How many controllers? (decision for the user and the builder)

QuinLED's guidance is about **2,000 LEDs per ESP32 running WLED** (about 3,000 with ESPixelStick) ([Dig-Octa brainboard specs](https://quinled.info/quinled-dig-octa-brainboard-32-8l-specifications/)). The Dig-Quad uses the same ESP32, so **one board for all 3,600 LEDs (12 × 300) is about 1.8 times the recommended load**. It's fine for bench tests, but not for the finished tree.

| Option | LEDs per ESP32 | Strips per output | Approx. max FPS* | Notes |
|---|---|---|---|---|
| **A.** 1 × Dig-Quad (what we have) | 3,600 | 3 | ~30 | Over the guidance. Bench and one-quadrant testing only. |
| **B.** 2 × Dig-Quad | 1,800 | 2/2/1/1 (uneven) | ~45 | Just over the guidance. The cheapest upgrade. |
| **C.** 3 × Dig-Quad | 1,200 | 1 | ~90 | Simplest wiring (one strip per output, no chaining). Three controllers to keep in step. |
| **D.** 2 × Dig-Octa brainboard | 2,160 | 1 (6 of 8 outputs used) | ~90 | Ethernet, **external antenna** (WROOM-32UE), SD card, 16 MB flash. Can run WLED **or ESPixelStick**. |

\* Limited by the WS281x signalling time (about 30 µs per LED on each output), before any ESP32 load.

**Whichever we choose, strip power still goes through separate fused distribution** (see §2). With 2 or more controllers we also need:
- **One network for everything:** ideally a small travel router or access point in the enclosure that the controllers and phones all join, with a static IP for each controller
- **Keeping them in step:** xLights adds each controller separately over DDP. For WLED's own effects, WLED's UDP sync keeps the boards together, and our phone app controls them as one tree.

### Dig-Octa details ([overview](https://quinled.info/quinled-dig-octa/))
- It's modular. The **brainboard** is the controller: 8 level-shifted outputs, ESP32-WROOM-32UE with an external antenna, LAN8720A Ethernet, microSD, 2 buttons and a relay output. It stacks with a separate **powerboard**. Note that LED8 shares its pin with I²C.
- Powerboards ([Power-7 specs](https://quinled.info/quinled-dig-octa-power-7-specifications/)): Power-7 is 50 A and Power-7HC is 100 A continuous. Both have 16 screw terminals, **each with its own fuse** (10 A maximum each), and take **one PSU per powerboard**. A Power-7HC could replace our DIY fuse block for 6 strips (12 feeds, about 72 A at the worst-case 10 A-roll figure). But that needs a single PSU of about 900 W, which usually means a fan, and the design doc advises against that outdoors.
- **ESPixelStick with SD card:** ESPixelStick can play xLights `.fseq` files from the SD card on its own. That's a possible answer to **§6** (running xLights shows without the laptop), at the cost of WLED's phone app and built-in effects on those boards. This needs checking before we rely on it.

**Recommendation for now:** bench test with the Dig-Quad we have (option A, one strip, then one quadrant) and measure the frame rate and stability. Choose B, C or D once the strip datasheet is in and we've decided how shows will play standalone (§6). If standalone xLights playback matters, option D with ESPixelStick deserves a serious look.

## 7. Other checks

- **ESP32 load:** QuinLED's guidance is about 2,000 LEDs per ESP32 with WLED, so 3,600 on one board is too many. See §8.
- **Security:** the controller is still on the factory AP password (`wled1234`). Change it during commissioning.
- **Antenna:** the doc specifies an ESP32 with an **external SMA antenna**. Check whether our Dig-Quad's ESP32 has one.
- **Strip model:** confirm it's WS2815 (The builder hasn't said) and that the wire colours match Addendum C before connecting anything.
