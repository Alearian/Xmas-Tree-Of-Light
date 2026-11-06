# Three strips powered straight from the HLG

**Update 2026-10-22:** the HLG in this guide was never bought (the model didn't exist). Until the two HLG-320H-12As arrive, use the **150 W supply** in its place: three strips draw about 10 A of its 12.5 A. Wire it exactly as shown. When PSU 1 (HLG-320H-12A) arrives, move strips 1–3 to it; the 150 W supply then goes back to bench use (decided 2026-10-26: two supplies in the tree).

**Goal:** strips 1–3 get their 12 V from the PSU through the fuse box, not through the Dig-Quad. The board only sends **data** (plus a data ground) and runs on a small 2 A feed. This is the final-build wiring ([dig-quad-connections.md](dig-quad-connections.md)) cut down to one PSU and one output, and it is bottom-fed as decided on 2026-10-17.

Written 2026-10-20.

## Why change it

Right now all 12 V for three strips goes through the board's **LED1/LED2 fuse (7.5 A)**. Three strips at full white draw about 10 A (3.3 A each measured by other users, up to 22 A by BTF's rating), so a bright white scene can blow that fuse or overheat the board's copper. **Until you rewire, keep brightness below about half and avoid full white.**

## Parts you need

| Part | For | Status on the shopping list |
|---|---|---|
| 6-way blade fuse box with negative bus | One fused way per strip, plus the board | Have |
| 5 A ATO blade fuses ×3 | One per strip feed | Have |
| 2 A fuse (inline holder, or a 2 A blade in a spare way of the fuse box) | Dig-Quad feed | Have |
| Ground bus bar | Common 0 V | Have |
| Red/black 6 mm² cable | HLG → fuse box and ground bar | Have |
| 1.5 mm² two-core (H07RN-F later; any 1.5 mm² or 16–18 AWG pair on the bench) | Fuse box → bottom of each strip | Need (any spare 1.5 mm² pair will do for now) |
| Ferrules + crimper | Every screw terminal | Have |

**Don't wire the strips to the HLG without the fuses.** The HLG can deliver 40 A, which is enough to melt a strip lead or start a fire if something shorts.

## Target wiring

```
 Mains ─► 150 W PSU (later HLG-320H-12A PSU 1)
            V+ (red)   ══6 mm²══► FUSE BOX input (+)
            V− (black) ══6 mm²══► GROUND BAR

 FUSE BOX   way 1  5 A ──1.5 mm²──► S1 bottom +12 V (red)
            way 2  5 A ──1.5 mm²──► S2 bottom +12 V (red)      S2 runs DOWN, so its bottom is its data-OUT end
            way 3  5 A ──1.5 mm²──► S3 bottom +12 V (red)
            way 6  2 A ──1.5 mm²──► Dig-Quad + input
            negative bus ══6 mm²══► GROUND BAR

 GROUND BAR ◄── HLG V−, fuse-box negative bus, Dig-Quad − input, S1/S2/S3 bottom GND (black on this strip)

 Dig-Quad   LED1 data (GPIO 16) ──► S1 bottom JST DI (green)
            LED1 −              ──► S1 bottom JST GND   (data ground, runs beside the data wire)
            LED1 +              ──► NOTHING (empty from now on)

 Data chain (unchanged):  S1 ▲ bottom→top,  top jumper DO→DI + BO→BI into S2,
                          S2 ▼ top→bottom,  bottom jumper DO→DI + BO→BI into S3,  S3 ▲
 S1 bottom: BI tied to GND (chain start only).  S3 top: DO/BO capped.
 Thin +12 V wires in the JST plugs: insulate them; they never carry strip power.
 Top-end bare power leads of all three strips: insulate each one separately (bottom-fed only).
```

**Why it's safe:** each strip has its own 5 A fuse, so a short on one strip blows only that fuse. The board's fuses no longer see strip current. All grounds meet at one bar, so the data signal (a voltage measured against ground) means the same thing at the board and at the strip.

## Step by step

1. **Unplug the HLG at the wall** and wait a minute for its capacitors to drain. Unplug the board's USB if it's connected.
2. **Take the strip power off the board.** Remove the strip's red lead from **LED1 +** and leave that terminal empty. Leave **LED1 data** and **LED1 −** connected to S1's JST plug (DI and GND).
3. **Take the HLG off the board.** Remove the HLG red and black from the Dig-Quad's `+` and `−` input terminals.
4. **HLG to fuse box and ground bar.** Crimp ferrules on the 6 mm² leads. HLG red (V+) → fuse box `+` input. HLG black (V−) → ground bar. Fuse box negative bus → ground bar with a short 6 mm² black.
5. **Board feed.** Put a **2 A** fuse in fuse-box way 6 (or use the inline holder from the way). Way 6 → Dig-Quad `+` input. Dig-Quad `−` input → ground bar.
6. **Strip feeds.** For each strip, run a 1.5 mm² pair from the fuse box to the strip's **bottom** bare leads: red to a **5 A** way (S1 way 1, S2 way 2, S3 way 3), black to the ground bar or the fuse box's negative bus. Remember S2 runs down, so its bottom bare leads are at its **data-out** end.
7. **Tidy the unused ends.** Insulate each top-end bare power lead separately, and the thin red wire in every JST plug. Don't join strips' power through the JST jumpers; each strip is fed only by its own fuse.
8. **Check before power** (multimeter on continuity, everything unplugged):
   - Fuse box `+` input to ground bar: **no beep**.
   - Each strip's bottom red to ground bar: **no beep**.
   - Dig-Quad `+` input to `−` input: **no beep**.
   - Dig-Quad LED1 `+` terminal: **nothing** attached.
   - Ground bar to S1's JST GND: **beep** (the data ground is connected).
9. **Power up.** Plug in the HLG. After 10–60 s the board is back at http://<board-ip>/. Set brightness low in the app and check all three strips light and run the right way (in PowerShell from `D:\Source\Xmas`: `dotnet run --project tools/TreeTool -- test strips --layout config/proto-tree.json`, then the same with `test chase`).
10. **Measure** (roadmap Stage 1) with the clamp meter at brightness 255, solid white:
    - The current on each strip's red feed. Expect about 3.3 A. **Over about 4 A means the 150 W supply can't carry three strips at full white; keep the limiter on.**
    - The voltage between red and black at the **top** (far) end of each strip. Below about 10.5 V shows as pinker white at the top.
11. **Tell Claude it's done**, and say "go ahead" to update the board (next section).

## Board update (after the rewire, needs your go-ahead)

`config/proto-tree.json` now lists strips 1–3 as attached and sets the current limiter to **38,000 mA at 11 mA per LED**: 95 % of one HLG. WLED counts all 12 strips in its estimate (39.6 A at full white), so the three real strips are capped at about a quarter of that, about 9.5 A, which suits the 150 W supply (12.5 A). The fuses protect the wiring; the limiter is not a safety device. With all 12 strips on the two HLGs, cap each output at about 9,500 mA and set about 38,000 mA total.

In PowerShell on the PC:

```powershell
Set-Location D:\Source\Xmas
# See what will change (read-only)
dotnet run --project tools/TreeTool -- plan --layout config/proto-tree.json
# Back up, then set the outputs and limiter (asks to confirm; the board reboots)
dotnet run --project tools/TreeTool -- apply --layout config/proto-tree.json
# Send the app its new layout so strip 3 shows as live
dotnet run --project tools/TreeTool -- upload --layout config/proto-tree.json
```

`apply` takes a backup in `device-backups/` before it writes anything.
