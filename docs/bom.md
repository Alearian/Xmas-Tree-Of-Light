# Bill of Materials: final tree (5 m strips, bottom-fed)

Written 2026-10-10, updated 2026-10-17 for the current design: **12 × 5 m BTF-LIGHTING WS2815 strips (3,600 LEDs), each fed at its bottom end only, power cables along the ground from the base box, PSUs and controller in one box at the base. No power goes up the mast.** See [power-and-wiring.md](power-and-wiring.md).

**Leaves out what you already have:** the QuinLED Dig-Quad and 2 good WS2815 strips (the 3 WS2811 strips are being returned). Progress is tracked in [bom-checklist-no-prices.md](bom-checklist-no-prices.md). Quantities include a few spares. Lengths assume a 5 m mast with the box about 2 m from it. Re-check them once the site is measured.

## 1. LED strips and sealing

| Item | Qty | Spec | Notes |
|---|---|---|---|
| BTF-LIGHTING **HD 12V WS2815 60L B IP67**, 5 m roll | **10** | Same as the ones you have | 12 in total, no spare: 2 in hand, 10 to buy |
| Silicone end caps for 12 mm IP67 tube | 30 | With and without a cable hole | Two per strip, plus spares |
| Neutral-cure (alkoxy) clear silicone | 2 tubes | Electronics-safe, **not** acetic/vinegar-smelling | Fills cut ends and caps |

## 2. Power

| Item | Qty | Spec | Notes |
|---|---|---|---|
| **Mean Well HLG-320H-12A** | **2** | 12 V, 22 A, 264 W, IP65, fanless | PSU 1: strips 1–5 (~16.5 A, 75 %) + the Dig-Quad. PSU 2: strips 6–9 (~13 A, 60 %). Leave the Vo/Io adjusters at factory settings. |
| 12 V 150 W supply (already bought) | 1 | ~12.5 A | PSU 3: strips 10–12 (~10 A, 80 %) |
| Blade fuse box with negative bus | 3 | 12 V, ATO/ATC blade, rated 30 A+ in total | One per PSU: two 6-way (have), plus a small 4-way for PSU 3 |
| ATO blade fuses, **5 A** | 1 pack (20+) | | One per strip feed, plus spares |
| Inline blade fuse holder + 2 A fuse | 1 | | Small feed from PSU 1 to the Dig-Quad |
| Red/black cable, **6 mm²** (10 AWG) | 2 m each colour | Flexible, tinned | PSU → fuse box, short runs |
| Earth/ground bus bar | 1 | 6–8 way, 12 V, 60 A+ | Common GND: all three PSUs, all fuse boxes and the Dig-Quad |
| Bootlace ferrules + crimper | 1 kit | 0.5–6 mm² | For every screw terminal |

## 3. Cabling to the tree

| Item | Qty | Spec | Notes |
|---|---|---|---|
| **H07RN-F 2-core 1.5 mm²** outdoor rubber cable | **50 m** | | One cable per strip, box → along the ground → bottom of strip (about 1.5–4.5 m each, about 40 m in total, plus spare) |
| 3-core 0.75 mm² outdoor cable | 15 m | H05RN-F or similar | Data + GND from the box to the 4 chain starts |
| 3-core 0.5 mm² outdoor cable | 10 m | | 8 data jumpers between strips (DO→DI, BO→BI, GND) |
| Glue-lined heat-shrink **butt connectors, blue** | 30 | 16–14 AWG (1.5–2.5 mm²), crimp + adhesive | Power feed to each strip's bottom leads: 12 × 2 wires + spares |
| Glue-lined heat-shrink **butt connectors, red** | 70 | 22–16 AWG (0.5–1.5 mm²), crimp + adhesive | Data: 16 jumper ends × 3 wires + 4 chain starts × 2 (BI and GND share one) + spares |
| UV-resistant cable ties, black | 2 × 100 | Assorted | Tying the power cables to the base ring and the data cables to the backbones |
| Outdoor cable protector or split conduit | ~5 m | Ground-level, UV-rated | Covers the 12 feeds and 4 data leads between the box and the base ring, so they can't be tripped over or mowed |
| Glue-lined heat-shrink end caps | 15 | Small | Seal the 12 unused power leads at the top of each strip |

**No soldering (decided 2026-10-17; joints chosen 2026-10-22):** every joint outside the box is a glue-lined heat-shrink crimp butt connector, made with a ratchet crimper and a heat gun. Ferrules are only for screw terminals inside the box.

## 4. Enclosure and mains

| Item | Qty | Spec | Notes |
|---|---|---|---|
| IP66/67 **polycarbonate** box, hinged lid | 1 | About 400 × 300 × 170 mm, with a mounting plate | Holds 2 PSUs, 2 fuse boxes and the Dig-Quad. Polycarbonate, not metal, so the Wi-Fi still works. |
| Cable glands, M20 IP68 | 1 pack (20) | Nylon, strain relief | Bottom face only: 12 power + 4 data + mains, plus spares |
| Mains lead, H07RN-F 3-core 1.5 mm² | 2 | With an IP44 plug | One per PSU (decided 2026-10-17). Reusing existing cables |
| SOMELINE IP68 waterproof junction box cable connector | 2 | Outdoor, 230 V, 3-core | One per PSU: joins its bare AC wires to its own mains lead. Replaces the WAGO junction (2026-10-17) |
| **3-way outdoor socket / extension with 30 mA RCD** | 1 | IP44+, 13 A | All three PSU plugs go behind the RCD. Required for outdoor mains |
| Outdoor smart plug or timer (optional) | 1 | IP44, 10 A | Cuts the mains to really turn the tree off |

Have the mains side checked by a qualified electrician.

## 5. Structure

| Item | Qty | Spec | Notes |
|---|---|---|---|
| Centre mast | 1 | About 5.5 m, e.g. a telescopic aluminium flagpole or scaffold tube | Carries the top ring (no cables) |
| Ground socket for the mast | 1 | Spike or concrete-in socket | |
| Top ring | 1 | Steel or aluminium hoop, about 0.5–1 m across, fixed to the mast top | 12 attachment points |
| Strip backbones | 12 | 2 mm stainless wire rope, about 5.5 m each, or UV-stable webbing | **The backbone takes the tension, never the strip** |
| Wire rope fittings | 1 set | 24 thimbles, 48 grips, 12 small turnbuckles | Tension each backbone |
| Ground pegs or base ring | 12 | Heavy-duty pegs | Lower anchor for each backbone |
| UV-resistant black zip ties | 500 | | Strip to backbone every 15 cm |
| Guy ropes + pegs for the mast | 3–4 | | Wind loading |

## 6. Tools (if you don't have them)

| Item | Notes |
|---|---|
| **DC clamp meter**, e.g. UNI-T UT210E | For the bench test (strip current and far-end voltage) |
| Ferrule/crimp tool | Included in the ferrule kit above |
| **Ratchet crimper for insulated terminals** | For the red/blue butt connectors (the ferrule crimper is the wrong shape) |
| **Heat gun** | Shrinks the butt connectors and end caps until the glue shows |

## Optional

- **External Wi-Fi antenna** (2.4 GHz, SMA): only if your Dig-Quad's ESP32 has an antenna connector (see the Antenna note in open-questions.md, still to check).
- **More controllers:** 3,600 LEDs is still over QuinLED's guidance of about 2,000 per ESP32 (open-questions §8). A second Dig-Quad (6 strips each) would follow that guidance, and it pairs with one PSU per half.
