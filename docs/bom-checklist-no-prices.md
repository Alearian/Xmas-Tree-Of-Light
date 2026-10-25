# Tree build shopping list (no prices)

Mirror of the live checklist page: https://claude.ai/artifact/Ry8bn8QRNgF9VCDwNjqKLN
Copy without prices, for sharing. Generated from [bom-checklist-no-prices.md](bom-checklist-no-prices.md).

Last synced: 2026-10-23 16:29 UTC. Based on [bom.md](bom.md) and the 12 × 5 m bottom-fed, no-soldering design.

**Summary:** 13 complete, 3 part-way, 7 ordered, 19 to buy, of 39 parts.


## Already have

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [x] Got it | QuinLED Dig-Quad controller (ESP32, running WLED) | 1 / 1 | 1 | Yes | On the bench |
| [x] Got it | BTF-LIGHTING HD 12V WS2815 60L B IP67, 5 m (Strips in hand) | 2 / 2 | 2 | Yes | The 3 WS2811 strips are being returned |
| [x] Got it | 12 V bench power supply (5 A) | 1 / 1 | 1 | Yes | Bench testing only |

## LED strips and sealing

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [x] Got it | BTF-LIGHTING HD 12V WS2815 60L B IP67, 5 m roll (Same as the one you have) | 10 / 10 | 10 | Yes | 12 strips in total, no spare: 2 in hand, 10 to buy. 4 on order so far |
| [x] Got it | Silicone end caps for 12 mm IP67 tube (With and without a cable hole) | 40 / 30 | 30 | Yes | Two per strip, plus spares |
| [x] Got it | Neutral-cure (alkoxy) clear silicone (Electronics-safe, not acetic) | 2 / 2 | 2 tubes | Yes | Fills cut ends and caps |

## Power

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [~] Ordered | Mean Well HLG-320H-12A (PSU 1 and PSU 2) (12 V, 22 A, 264 W, IP65, fanless) | 0 / 2 | 2 | Yes | PSU 1 feeds strips 1–5 (~16.5 A) and the Dig-Quad; PSU 2 feeds strips 6–9 (~13 A). Leave the Vo/Io adjusters at factory settings |
| [x] Got it | 12 V 150 W supply (PSU 3) (12 V, ~12.5 A (confirm model on label)) | 1 / 1 | 1 |  | Bought 2026-10-22. Feeds strips 10–12 (~10 A, 80 %) |
| [ ] To buy | 6-way blade fuse box with negative bus (ATO/ATC, 30 A+ total) | 2 / 3 | 3 | Yes | One per PSU. Have two 6-way; the third can be a small 4-way (PSU 3 only feeds 3 strips) |
| [x] Got it | ATO blade fuses, 5 A | 1 / 1 | 1 pack (20+) | Yes | One per strip feed, plus spares |
| [x] Got it | Inline blade fuse holder + 2 A fuse | 1 / 1 | 1 | Yes | Small feed from PSU 1 to the Dig-Quad |
| [x] Got it | Red/black cable, 6 mm² (10 AWG) (Flexible, tinned) | 2 / 2 | 2 m each colour | Yes | PSU to fuse box |
| [x] Got it | Earth/ground bus bar (6–8 way, 60 A+) | 1 / 1 | 1 | Yes | Common GND for both PSUs, fuse boxes and Dig-Quad |
| [x] Got it | Bootlace ferrules + crimper (0.5–6 mm²) | 1 / 1 | 1 kit |  | For every screw terminal |

## Cabling to the tree

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [ ] To buy | H07RN-F 2-core 1.5 mm² outdoor rubber cable | 0 / 50 | 50 m | Yes | One per strip, box along the ground to the bottom of the strip (~1.5–4.5 m each, ~40 m total plus spare). Bottom-fed since 2026-10-17 |
| [~] Ordered | 3-core 0.75 mm² outdoor cable (H05RN-F or similar) | 15 / 15 | 15 m | Yes | Data + GND, box to the 4 chain starts |
| [~] Ordered | 3-core 0.5 mm² outdoor cable | 0 / 10 | 10 m | Yes | 8 data jumpers between strips |
| [~] Ordered | Glue-lined heat-shrink butt connectors, BLUE (16–14 AWG (1.5–2.5 mm²), crimp + adhesive, no solder) | 0 / 30 | 30 | Yes | Power feed to each strip's bottom leads: 12 strips × 2 wires = 24, plus spares. Fold the thinner strip lead back to fill the barrel |
| [~] Ordered | Glue-lined heat-shrink butt connectors, RED (22–16 AWG (0.5–1.5 mm²), crimp + adhesive, no solder) | 0 / 70 | 70 | Yes | Data joints: 16 jumper ends × 3 wires = 48, 4 chain starts × 2 = 8 (BI and GND share one), plus spares. Stagger them on each 3-core |
| [ ] To buy | UV-resistant cable ties, black (Assorted) | 0 / 200 | 2 × 100 | Yes | Tying the power cables to the base ring and data cables to the backbones |
| [ ] To buy | Outdoor cable protector or split conduit (Ground-level, UV-rated) | 0 / 5 | ~5 m | Yes | Covers the 12 feeds and 4 data leads on the ground between box and base ring |
| [ ] To buy | Glue-lined heat-shrink end caps (Small) | 0 / 50 | 50 | Yes | Every unused wire: the bare +12 V and ground pair at each strip top (24) and the plug lead's +12 V at every strip end (24), plus spares |

## Enclosure and mains

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [x] Got it | IP66/67 polycarbonate box, hinged lid (~400 × 300 × 170 mm, mounting plate) | 1 / 1 | 1 | Yes | Polycarbonate so Wi-Fi works |
| [ ] To buy | Cable glands, M20 IP68 (Nylon, strain relief) | 0 / 1 | 1 pack (20) | Yes | Bottom face only |
| [ ] To buy | Mains lead, H07RN-F 3-core 1.5 mm² (IP44 plug) | 0 / 3 | 3 |  | Reusing existing cables, one per PSU (3 PSUs), each with its own plug |
| [~] Ordered | SOMELINE IP68 waterproof junction box cable connector (Outdoor, 230 V, 3-core) | 2 / 6 | 6 | Yes | One per PSU (3 PSUs) joins each PSU's bare AC wires to its own mains lead, plus 3 spares |
| [ ] To buy | 3-way outdoor socket / extension with 30 mA RCD (IP44+, 13 A total, RCD built in) | 0 / 1 | 1 | Yes | All three PSU mains plugs go in here so they all sit behind the RCD |
| [ ] To buy | Outdoor smart plug or timer (IP44, 10 A) | 0 / 1 | 1 | Yes | Optional: really turns the tree off |

## Structure

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [ ] To buy | Centre mast (~5.5 m, flagpole or scaffold tube) | 0 / 1 | 1 |  | Carries the top ring only (no power cables since the bottom-feed change) |
| [ ] To buy | Ground socket for the mast (Spike or concrete-in) | 0 / 1 | 1 |  |  |
| [ ] To buy | Top ring (0.5–1 m hoop, 12 fixing points) | 0 / 1 | 1 |  |  |
| [ ] To buy | Strip backbones (2 mm stainless wire rope, ~5.5 m each) | 0 / 12 | 12 |  | The backbone takes the tension, never the strip |
| [ ] To buy | Wire rope fittings (24 thimbles, 48 grips, 12 turnbuckles) | 0 / 1 | 1 set |  |  |
| [ ] To buy | Ground pegs or base ring (Heavy-duty) | 0 / 12 | 12 |  | Lower anchor for each backbone |
| [ ] To buy | UV-resistant black zip ties | 0 / 500 | 500 |  | Strip to backbone every 15 cm |
| [ ] To buy | Guy ropes + pegs for the mast | 0 / 4 | 3–4 |  | Wind loading |

## Tools

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [x] Got it | DC clamp meter (e.g. UNI-T UT210E) | 1 / 1 | 1 |  | Bench test: strip current and far-end voltage |
| [x] Got it | Ratchet crimper for insulated terminals (Red/blue/yellow jaws, ratchet release) | 1 / 1 | 1 |  | For the red/blue butt connectors (the Preciva is for ferrules only) |
| [~] Ordered | Heat gun (Any hobby/DIY heat gun) | 0 / 1 | 1 |  | Shrinks the butt connectors and end caps until glue shows at both ends |
| [x] Got it | Preciva 0.25-10mm² Crimping Tool Square with 2000 Ferrules | 1 / 1 | 1 |  |  |

## Optional extras

| Status | Part | Got / needed | Qty as listed | The builder | Notes |
|---|---|---|---|---|---|
| [ ] To buy | External Wi-Fi antenna, 2.4 GHz SMA | 0 / 1 | 1 |  | Only if the ESP32 has an antenna connector |
| [ ] To buy | Second QuinLED Dig-Quad | 0 / 1 | 1 |  | Follows QuinLED's ~2,000 LEDs per ESP32 guidance |
