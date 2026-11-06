# Wiring options: 15 m runs fed at both ends vs 5 m strips fed at one end

Written 2026-10-26 for Mr Furniss.

> **Decision (2026-10-26): Mr Furniss chose Option 2**, staying with 12 × 5 m strips, each fed at the bottom by its own 5 A fused cable. Option 1 (four 15 m runs fed at both ends) was rejected. The wiring docs already describe Option 2.

## What is being compared

| | Option 1 | Option 2 (current design) |
|---|---|---|
| Runs | **4 × 15 m**, each three 5 m rolls joined (2 runs per fuse box, 2 fuse boxes) | **12 × 5 m**, one roll each |
| Feeds | Both ends of each run: one at the bottom, one at the **top** (a 3-roll zig-zag up-down-up ends at the apex) | Bottom end of each strip only |
| LEDs | 3,600 (same) | 3,600 (same) |

Confirmed by Mr Furniss (2026-10-26): **4 × 15 m runs**, two per fuse box, one PSU per box, so the full 60 m.

Figures below use the measured **3.3 A per 5 m roll** at full white (BTF's own rating is 7.5 A, a worst-case ceiling). The strip's own copper resistance isn't published, so I've assumed **about 0.1 Ω per metre for the + and − rails together**, which is typical for 12 V 60/m strip. That is a guess; the bench far-end voltage test settles it.

## 1. Voltage drop (the deciding factor)

| | Option 1 | Option 2 |
|---|---|---|
| Current per run | 9.9 A (3 rolls) | 3.3 A |
| Worst point | Middle of the middle roll (S2, S5, S8, S11, halfway up the tree) | Top of each strip |
| Drop in the strip itself | **~1.9 V** (I·R·L/8 = 9.9 × 0.1 × 15 / 8) | **~0.8 V** (I·R·L/2 = 3.3 × 0.1 × 5 / 2) |
| Drop in the feed cable | Top feeds are ~8 m (box → base → up the mast) at ~5 A: ~1.0 V on 1.5 mm², ~0.6 V on 2.5 mm² | ≤4.5 m at 3.3 A on 1.5 mm²: ~0.35 V |
| Lowest voltage at full white | **~9.5 V** (on the WS2815's 9.5 V limit) | **~10.8 V** |
| At BTF's 7.5 A ceiling | ~4 V drop, well below 9.5 V | ~2.7 V drop, ~9.3 V |

Feeding both ends of a run three times as long only halves the effective length, so Option 1 still sags about **2.3 times more** than Option 2.

**How it looks:** in Option 2 every strip shades the same way (very slightly warmer at the top), which reads naturally on a tree. In Option 1 the outer rolls sit next to feeds and stay bright, while every **middle roll** (S2, S5, S8, S11) is dimmest halfway up. At full white that gives an alternating bright, dim, bright pattern around the tree. Coloured effects at normal brightness hide it in both options.

## 2. Cable runs

| | Option 1 | Option 2 |
|---|---|---|
| Bottom feeds | 4 (S1, S4, S7, S10), ~1.5–4.5 m each | 12, ~1.5–4.5 m each |
| Top feeds | **4 up the mast** (S3, S6, S9, S12 tops), ~8 m each, 2.5 mm² advised | none |
| Total feed cable | ~12 m + ~32 m ≈ **44 m**, partly heavier cable | **~40 m** of 1.5 mm² (50 m already bought) |
| Roll-to-roll joints | 8 **power** jumpers as well as the 8 data jumpers (4 at the apex, 4 at the base), 1.5 mm² | 8 data jumpers only, 0.5 mm² |
| Work at the top | 4 power joints + 4 data jumpers at the apex, cable tied up the mast | 4 data jumpers only |

Option 1 brings back the cables up the mast that you removed on 2026-10-17.

## 3. Fusing and faults

| | Option 1 | Option 2 |
|---|---|---|
| Fuse ways | 8 (2 per run) + Dig-Quad 2 A | 12 + Dig-Quad 2 A |
| Fuse size | ~7.5 A per end (each end carries ~5 A) | 5 A per strip |
| Fits a 6-way box? | Yes: 4 ways per box + Dig-Quad | Needs 6+6 (or 5/4/3 on three boxes) |
| One short | Blows both ends' fuses, **3 strips (a quarter of the tree) go dark** | Blows one fuse, 1 strip goes dark |
| One end's fuse blows | The other end pushes ~9.9 A through one roll's copper and its joints; a 7.5 A fuse may take minutes to blow at that load, and the far roll drops to a brown, dim tail | Not applicable |
| Lead rating | ~7.5 A fuse protecting the strip's factory power leads: check their gauge (often 18–20 AWG) | 5 A comfortably protects them |

**Two PSUs, two fuse boxes (confirmed by Mr Furniss 2026-10-26):** in Option 1, box 1 / PSU 1 feeds runs 1–2 and box 2 / PSU 2 feeds runs 3–4. **Both ends of a run must come from the same box.** If a run's bottom were fed from PSU 1 and its top from PSU 2, the strip's copper would join the two supplies' V+, which is never allowed. The GNDs still all meet at the ground bar, and the Dig-Quad's 2 A feed comes from one of the boxes.

So, to the direct question: **yes, a fuse box can carry two 15 m runs fed at both ends**, at ~20 A per box at measured current. That's the same total load as Option 2, so it's still within one Mean Well HLG-320H-12A (12 V, 22 A, 264 W, IP65, checked against distributor listings). The PSU sizing doesn't change between the options; only where the current flows does.

## 4. Pixel mapping, TreeTool and the app

**No change for either option.** A 15 m run is exactly what each Dig-Quad output already drives: three rolls zig-zagging up, down, up (`config/tree.json`: GPIO 16 → strips 1–3, GPIO 3 → 4–6, GPIO 1 → 7–9, GPIO 4 → 10–12). Option 1 only adds power joins where the data jumpers already are. Segments, `rev` on down-running strips, looks and the app all stay the same.

(If you meant two 15 m runs for the whole tree, `tree.json` would drop to 6 strips or 2 outputs, every look would need re-checking, and the tree would have half the pixels.)

## 5. Build effort

| | Option 1 | Option 2 |
|---|---|---|
| Fewer cables at the base | ✓ (4 instead of 12) | |
| Fewer fuse ways | ✓ | |
| No power up the mast | | ✓ |
| Matches docs, shopping list and bench wiring already done | | ✓ |
| Thin, low-current jumpers | | ✓ |
| Fault takes out only one strip | | ✓ |

## Recommendation

**Stay with Option 2 (12 × 5 m, bottom-fed).** It drops less than half the voltage, has no heavy cable up the mast, isolates faults to one strip, and is already what the docs, the 50 m of cable and your three bench strips are built for. Option 1's only gains are fewer cables at the base and fewer fuses.

If the bench test shows a strip top below about 10.5 V at full white, the fix is to **add a top feed to each 5 m strip** (the old 2026-10-10 plan), not to join rolls into 15 m runs.
