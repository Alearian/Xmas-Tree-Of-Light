# QuinLED Dig-Quad — Wiring & WLED Setup Guide

Sources: Quindor's video [WLED tutorial 2022 – QuinLED-Dig-Quad setup from start to finish](https://www.youtube.com/watch?v=JIkoUCuXFa0), which the user found through [r/WLED](https://www.reddit.com/r/WLED/comments/s3wkx2/wled_tutorial_2022_quinleddigquad_setup_from/), and the written guides on quinled.info:
[Pre-assembled v3.1 specs](https://quinled.info/quinled-dig-quad-pre-assembled-v3-1-specifications/) ·
[Wiring guide](https://quinled.info/2020/10/03/quinled-dig-quad-wiring-guide/) ·
[Board dimensions (v3)](https://quinled.info/quinled-dig-quad-board-dimensions/) ·
[Pinout guide](https://quinled.info/quinled-dig-quad-pinout-guide/) ·
[WLED config for QuinLED](https://quinled.info/2021/03/21/wled-0-12-what-do-i-configure-for-quinled/)

> We couldn't fetch Reddit or watch the video, so this guide comes from the written quinled.info pages that cover the same material. Check it against the video when you watch it.

## Board ratings (pre-assembled v3.1)

| Spec | Value |
|---|---|
| Input voltage | 5 / 12 / 24 V DC, auto-detected (the outputs run at the same voltage as the input) |
| Input terminals | Double 9.5 mm terminals, both connected to **the same** + and − rails |
| **Continuous current, whole board** | **30 A**, which is **360 W at 12 V** (tested up to 50 A peak) |
| Output terminals | 7 × (+ / −) power terminals |
| Fuses | 5 ATO blade fuses. **LED1 and LED2 share one fuse; LED3 and LED4 share one fuse.** Terminal 5 has its own (often used for "edge" injection). |
| Max per fuse/terminal | **10 A** continuous |
| Level shifters | 5 × AHCT (3.3 V → 5 V), output only |
| Onboard extras | DS18B20 temperature sensor, button, audio input, I²C, Q1R relay output |

## Board dimensions (v3)

From [quinled.info/quinled-dig-quad-board-dimensions](https://quinled.info/quinled-dig-quad-board-dimensions/). The page has a full millimetre drawing. Take the length and hole spacing from that drawing before drilling an enclosure.

| | |
|---|---|
| Width | **100.3 mm** (slightly wider than v2, because v3 has larger power-input terminals) |
| Mounting | **M2.5** screw holes |
| Height to allow | about **3.5 cm** without Ethernet, about **6 cm** with the QuinLED-ESP32-ABE Ethernet add-on |
| Ventilation | A **vented enclosure** is "highly recommended" when running up to 30 A. A closed box needs reduced current. |

For us, the ventilation note matters only in the prototype, where strip power goes through the board. In the final build the board carries well under 1 A, so the sealed IP66/67 box is fine for it. The PSUs are what need the air gap.

## Pinout (v3 / v3.1)

| Function | GPIO |
|---|---|
| LED1 / LED2 / LED3 / LED4 data | **16 / 3 / 1 / 4** |
| Q1R relay trigger (pre-assembled, 5 V out) | 15 |
| Q1 / Q2 / Q3 / Q4 headers | 15 / 12 / 2 / 32 |
| Button | 0 |
| Temperature sensor (DS18B20) | 13 |
| Audio in (A0) | 36 |

GPIO 1 and 3 are the ESP32's serial TX/RX pins, so you lose serial logging while they drive LEDs. The LED and Q1R outputs are **output only**.

## Wiring steps

1. **Power in:** connect PSU + and − to the input terminals. For higher currents, use **several parallel wires of equal length** from the **same** PSU rather than one thick cable.
2. ⚠️ **One PSU per board.** QuinLED: *"Do not connect wires from multiple power supplies to a single QuinLED-Dig-Quad!"* The input terminals share one rail, so two supplies would have their V+ joined.
3. **Optional 5vEXT:** power the board's electronics from a small separate 5 V supply. The ESP32 then stays online while the LED PSU is switched off by a relay on Q1R.
4. **Each LED output** has three wires: V+ → strip +, GND → strip GND, data → the first pixel's DI. For WS2815, also tie **BI to GND** at the first pixel.
5. **Fuses:** size each fuse for the current actually flowing through its terminal. Common sizes are 5 / 10 / 15 A, but the per-terminal limit is 10 A.
6. **Power injection** (QuinLED's rule of thumb for 5 V): about 150 LEDs needs feeding at the start only, about 300 LEDs at the start and end, 600 or more needs mid-point feeds too. 12 V tolerates more voltage drop (1.2 V vs 0.5 V at 10 %).
7. **Wire gauge:** aim for at most **10 % voltage drop**. Use QuinLED's power charts, which assume 50 % white.

## WLED settings (Config → LED Preferences)

| Setting | Value for the tree |
|---|---|
| LED outputs | 4 outputs, GPIO 16 / 3 / 1 / 4, type **WS281x**, colour order GRB (check this against the real strip) |
| Starts / counts | 0 / 1080 / 2160 / 3240, each 1,080 (bench test: one output, 360) |
| Current limiter (ABL) | Set to the real PSU budget, **not** the 1,250 mA placeholder |
| Relay pin | **15** (Q1R), optional, for switching the LED PSU through a relay |
| Button | GPIO 0 |
| Temperature usermod | GPIO 13, which lets us monitor the enclosure temperature |

WLED treats all 4 outputs as one long strip. Use **segments** to split it back into strips or quadrants.

## Review of the wiring guide (2026-10-03)

We re-read the [wiring guide](https://quinled.info/2020/10/03/quinled-dig-quad-wiring-guide/) against these notes.

**It confirms:**
- The pre-assembled board "no longer has any jumpers it has auto voltage selection (5v-24v)". Ours is the pre-assembled board. Only the DIY version has the 5 V / 12 V / 5vEXT jumper.
- Positive terminals 1 and 2 share Fuse1, and 3 and 4 share Fuse2.
- "Do not connect wires from multiple power supplies to a single QuinLED-Dig-Quad!" The only exception is 5vEXT, which uses a small separate 5 V supply for the electronics only.
- At higher power, use several parallel input wires from the **same** PSU, kept the same length. There are 3 input terminals in a row.
- The rule-of-thumb diagrams assume 50 % full white.

**What's new or more precise:**
- **Terminal 5 and the 10 A fuse.** In the guide's 600-LED example, terminal 5's fuse is replaced with a 10 A fuse to feed the far end of a strip. That's an example, not the factory value, so check what's fitted.
- **Injection at 12 V is gentler than at 5 V.** The "300 LEDs needs front and end injection" rule is for **5 V**. For 12 V the guide says 10 m runs show no visible drop with single colours or effects, and "Even full white is ok in most situations!" So far-end injection stays **optional** for the one-strip prototype, and the measurement will tell us. The strip seller still recommends a feed every 5 m. The tree feeds both ends anyway, for headroom and redundancy.
- **WS2815 is "very inefficient"** (the guide's words). Each WS2815 pixel runs from 12 V and burns the excess voltage as heat, unlike WS2811, where 3 LEDs share each 12 V group. That explains the high power per pixel on our strip sheet (about 0.13 W, 11 mA at 12 V) and why the bench measurement matters.

**What it doesn't cover:** wire gauges, data wire length, a separate GND for the data line, the relay, and the temperature sensor. Where our docs give advice on those (for example 16–18 AWG pigtails, data under about 2 m, running GND beside the data wire), it's general LED practice, not QuinLED guidance.

## What this means for our tree

The tree draws **65 A worst case at 12 V (about 780 W)**. That's more than double the board's 30 A rating, and each output (3 strips at up to about 16 A) is over the 10 A per-fuse limit. **Strip power can't go through the Dig-Quad.** See [power-and-wiring.md](power-and-wiring.md) for the revised layout.
