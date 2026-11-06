# Tree Wiring Build Guide

Oct 23, 2026 · Mr Furniss

This guide takes the tree from bare parts to all 12 LED-strips lit, in the order to do the work. The design: 12 × 5 m WS2815 LED-strips (300 LEDs each) round a centre pole, each fed at its **bottom** by its own fused cable from one base box. Two supplies sit in the box, both Mean Well HLG-320H-12A (12 V, 22 A), with six LED-strips on each. The Dig-Quad only sends data. There is no soldering anywhere: outdoor joints are glue-lined heat-shrink butt connectors, and every wire inside the box gets a ferrule.

## 1. Safety rules and tools

Work with the mains unplugged until step 9, and have the mains side checked by a qualified electrician before the first power-up.

**Rules that never change**

- Never join the +12 V outputs of two supplies, anywhere.
- Always join every ground: both supplies, both fuse boxes and the Dig-Quad meet at the ground bar.
- LED-strip power never goes through the Dig-Quad. Its LED outputs carry data and ground only.
- No fuses go in until the step that says so.
- Every wire you don't use gets its own heat-shrink end cap. Never fold a spare wire back bare.

**Tools**

- Ratchet crimper for insulated terminals (red and blue butt connectors)
- Preciva square crimper and ferrules (wires going into screw terminals)
- Heat gun
- Wire strippers and side cutters
- Multimeter and the UNI-T clamp meter
- Small flat screwdriver for the fuse boxes, ground bar and Dig-Quad terminals
- Masking tape and a marker for labels

## 2. The plan

This guide uses two names. An **LED-strip** is one 5 m strip of 300 LEDs; there are 12. A **tree-strip** is three LED-strips joined end to end by their data wires (up, down, up) on one Dig-Quad output; there are 4. Data runs the length of a tree-strip, but every LED-strip is powered separately at its own bottom end. See [LED-strips and tree-strips](build.md#led-strips-and-tree-strips).

Number the LED-strips 1 to 12 clockwise round the pole, seen from above. Each Dig-Quad output drives three LED-strips in a zig-zag: the first runs up, the second down, the third up. Data always enters at the bottom of LED-strips 1, 4, 7 and 10. Power always enters at the bottom of every LED-strip.

| LED-strip | Data runs | Dig-Quad output | Data comes in from | Supply | Fuse |
| --- | --- | --- | --- | --- | --- |
| 1 | up | LED1 | box lead, bottom | PSU 1 | F1, 5 A |
| 2 | down | LED1 | top jumper from 1 | PSU 1 | F2, 5 A |
| 3 | up | LED1 | bottom jumper from 2 | PSU 1 | F3, 5 A |
| 4 | up | LED2 | box lead, bottom | PSU 1 | F4, 5 A |
| 5 | down | LED2 | top jumper from 4 | PSU 1 | F5, 5 A |
| 6 | up | LED2 | bottom jumper from 5 | PSU 1 | F6, 5 A |
| 7 | up | LED3 | box lead, bottom | PSU 2 | F7, 5 A |
| 8 | down | LED3 | top jumper from 7 | PSU 2 | F8, 5 A |
| 9 | up | LED3 | bottom jumper from 8 | PSU 2 | F9, 5 A |
| 10 | up | LED4 | box lead, bottom | PSU 2 | F10, 5 A |
| 11 | down | LED4 | top jumper from 10 | PSU 2 | F11, 5 A |
| 12 | up | LED4 | bottom jumper from 11 | PSU 2 | F12, 5 A |

PSU 1 also feeds the Dig-Quad through a 2 A inline fuse. Each supply drives two outputs: PSU 1 runs LED1 and LED2, PSU 2 runs LED3 and LED4. At about 3.3 A a LED-strip, six LED-strips draw about 20 A of each supply's 22 A at full white, so the WLED limits in section 9 keep each supply below that.

Each jumper carries three wires: data out to data in (DO to DI), backup out to backup in (BO to BI), and ground to ground. The four top jumpers join 1–2, 4–5, 7–8 and 10–11; the four bottom jumpers join 2–3, 5–6, 8–9 and 11–12.

## 3. Build the base box

Do this on the bench, indoors. Nothing is connected to mains yet and no fuses go in.

1. Lay out the plate: two supplies, two 6-way fuse boxes, the ground bar and the Dig-Quad. Leave a 2–3 cm gap round each supply so it can shed heat.
2. Drill M20 glands in the **bottom face only**, never the top or sides, so rain can't run in: one per mains lead (2), one per bundle of feed cables, and one for the four data leads.
3. Screw everything down.
4. Wire each supply to its own fuse box with short 6 mm² cable: red from the supply's V+ to the fuse box's positive input, black from the supply's V− to the ground bar. PSU 1 goes to fuse box A, PSU 2 to fuse box B.
5. Link each fuse box's negative bus to the ground bar with 6 mm² black.
6. Fit the 2 A inline fuse holder from fuse box A's spare way (or PSU 1's V+) to the Dig-Quad's power-in +. Run a black wire from the Dig-Quad's power-in − to the ground bar.
7. Crimp a ferrule on every wire end before it goes into a screw terminal.
8. Label the fuse ways F1 to F12 to match the table in section 2.
9. Check with the multimeter, on its continuity setting: every ground point beeps to every other, and no fuse box's positive input beeps to any ground or to another fuse box's positive input.

## 4. Mains side

Each supply has bare AC wires and gets its own plugged mains lead, joined in a SOMELINE IP68 connector. Both plugs go into the outdoor socket with the built-in 30 mA RCD.

1. Feed each mains lead in through its own bottom gland.
2. In each SOMELINE connector join: brown to the supply's L, blue to N, green/yellow to earth. Check the supply's own label for its wire colours first.
3. Tighten the SOMELINE's seals and the gland nut until the cable can't be pulled through.
4. Leave the plugs out until the electrician has checked both joints.

## 5. Cut and label the cables

Measure on site once the pole and rings are up, then cut. The lengths below assume a 3.2 m base ring, a 0.8 m top ring and the box about 2.2 m from the pole.

| Cable | Count | Cable type | Length each | Runs from → to |
| --- | --- | --- | --- | --- |
| Power feed | 12 | H07RN-F 2-core 1.5 mm² | about 1.5–4.5 m (measure each, add 30 cm) | Fuse box way → bottom of its LED-strip |
| Data lead | 4 | 3-core 0.75 mm² | about 2–4 m (measure, add 30 cm) | Dig-Quad LED1–4 → bottom of LED-strips 1, 4, 7, 10 |
| Top jumper | 4 | 3-core 0.5 mm² | about 45 cm | Top of 1→2, 4→5, 7→8, 10→11 |
| Bottom jumper | 4 | 3-core 0.5 mm² | about 105 cm | Bottom of 2→3, 5→6, 8→9, 11→12 |

- Label **both ends** of every cable with tape: S1 to S12 for feeds, LED1 to LED4 for data leads, and J1-2 and so on for jumpers.
- On every 2-core feed, decide which core is +12 V (for example brown) and which is ground (blue), and use the same colours everywhere.
- On every 3-core, fix one colour code and stick to it: for example brown = data, black = backup (BI), grey or blue = ground. Write it on the box lid.

## 6. Rig the LED-strips and route the cables

1. Put up the pole, the top ring and the base ring, with the guy ropes on.
2. Hang the 12 backbones from the top ring to the base ring.
3. Before fixing each LED-strip, look for the printed data arrows. On LED-strips 1, 3, 4, 6, 7, 9, 10 and 12 the arrows must point **up**. On LED-strips 2, 5, 8 and 11 they must point **down**.
4. Zip-tie each LED-strip to its backbone every 15 cm. Its bottom end should sit about 15 cm above the ground.
5. Lay the 12 feeds and 4 data leads from the box to the base ring inside the cable protector or split conduit.
6. Run each cable round the base ring to its LED-strip and tie it to the ring every 30–50 cm with UV ties.
7. Leave a drip loop where each cable leaves the ring for its LED-strip, and another below the box: the cable dips lower than the joint or gland, so water drips off rather than running in.
8. Nothing goes up the pole except the four top jumpers, which sit between neighbouring LED-strip tops at the ring.

## 7. Make the outdoor joints

Each end of a LED-strip has two leads: a 4-wire plug lead (+12 V, data, backup data, ground) and a pair of bare power wires (+12 V, ground). Check the labels printed by the pads before you cut anything; go by the labels, not the colours. Cut the plug off the 4-wire lead and keep the wires; never open the LED-strip's silicone tube or remove its end caps.

**How to make one joint**

1. Slide nothing on first: the butt connector is its own sleeve.
2. Strip 7 mm of insulation from each wire. If a wire is much thinner than the barrel, fold its bare end back on itself.
3. Push one wire into each end and crimp each end with the ratchet crimper until it releases.
4. Tug each wire firmly. If one moves, cut the joint out and redo it.
5. Heat with the heat gun, working from the middle out, until the sleeve is tight and a little clear glue shows at both ends.
6. Stagger the joints on a multi-core cable so they don't sit side by side in one lump.

**Which wires join where**

| Where | Joints | Connector | Wires joined |
| --- | --- | --- | --- |
| Bottom of every LED-strip (12) | 2 each | Blue | Feed +12 V → LED-strip's bare +12 V; feed ground → LED-strip's bare ground |
| Bottom of LED-strips 1, 4, 7, 10 (data in) | 2 each | Red | Data lead data → LED-strip data in (DI). Data lead ground → LED-strip ground **and** LED-strip backup in (BI), both LED-strip wires in one end of the same connector |
| Each jumper end (16) | 3 each | Red | Data out → data in (DO → DI), backup out → backup in (BO → BI), ground → ground |
| Every LED-strip end (24) | — | End cap | The plug lead's +12 V wire: not used, cap it |
| Top of every LED-strip (12) | — | End caps | The bare +12 V and ground pair: not used, cap each wire separately |

Tie BI to ground only at the four tree-strip starts. Everywhere else BI joins the BO of the LED-strip before it.

After each joint, tie it up onto the ring or backbone so it sits at least 10–15 cm above the grass, with its cable looping down below it.

## 8. Connect everything in the box

Still no fuses in and no mains plugged in.

1. Bring the feeds and data leads in through their bottom glands, with a drip loop below the box.
2. Ferrule every core.
3. Each feed's +12 V core goes to its fuse way (S1 to F1 and so on, as in section 2). Its ground core goes to that fuse box's negative bus.
4. Each data lead goes to its Dig-Quad output: data core to the LED output's data terminal, ground core to that output's ground terminal. LED1 = LED-strip 1, LED2 = LED-strip 4, LED3 = LED-strip 7, LED4 = LED-strip 10. Nothing goes in the outputs' +12 V terminals.
5. Tighten the glands, and tug each cable to check it can't move.
6. Check with the multimeter, still unpowered: each feed's +12 V core does not beep to ground; each data core does not beep to ground.

## 9. First power-up, one output at a time

1. Have the electrician check the mains side. Then plug both leads into the RCD socket, test the RCD with its own test button, and reset it.
2. With no fuses in, measure each fuse box's positive input to ground. Both should read about 12 V.
3. Put the 2 A fuse in the Dig-Quad's inline holder. The board should start and join the home Wi-Fi.
4. Set the WLED current limits before any LED-strip lights. Cap each of the four outputs at about 9,500 mA, so each supply carries at most about 19 A of its 22 A, and set the total to about 38,000 mA. Ask Claude in the project to apply the full-tree config from TreeTool; it waits for your go-ahead.
5. Insert F1–F3 (output LED1). LED-strips 1–3 should light, with effects running bottom to top on all three.
6. Set solid white at full brightness for a moment. Clamp each LED-strip's feed: expect about 3.3 A, never over 5 A. Measure the voltage at the **top** of each LED-strip: above about 10.5 V is fine.
7. Repeat for F4–F6 (LED2), F7–F9 (LED3) and F10–F12 (LED4).

**If something is wrong**

| What you see | Most likely cause |
| --- | --- |
| A whole output stays dark | Data lead or its tree-strip-start joint; BI not tied to ground there |
| The first LED-strip lights, the second doesn't | That output's top jumper, or the second LED-strip's fuse or power joint (an unpowered LED-strip passes no data) |
| The first two light, the third doesn't | That output's bottom jumper, or the third LED-strip's fuse or power joint |
| The first LED-strip of an output dark, all three dark | LED-strip 1, 4, 7 or 10's fuse or bottom power joint |
| Colours wrong or flickering | Data and backup swapped at a joint, or a loose ground |
| A fuse blows at once | +12 V touching ground: check that LED-strip's power joint and end caps |

## 10. Final weatherproofing check

Tick these off before leaving the tree out in the rain.

- [ ] Every outdoor joint is a glue-lined butt connector with glue showing at both ends: no tape, nothing bare
- [ ] Every unused wire has its own end cap (the plug lead's +12 V at every LED-strip end, the bare pair at every LED-strip top)
- [ ] No joint lies on the ground; each is tied up at least 10–15 cm, with a drip loop below it
- [ ] LED-strips keep their factory end caps; no tube has been cut or opened
- [ ] Cables between the box and the base ring are inside the cable protector
- [ ] All box glands are in the bottom face, tight, with a drip loop on every cable below the box
- [ ] SOMELINE connectors are closed and tight; both plugs are in the RCD socket, off the ground
- [ ] The box lid is closed and its seal is seated all the way round
- [ ] Spare 5 A and 2 A fuses are taped inside the lid
