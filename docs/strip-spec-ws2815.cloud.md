# LED Strip Spec — HD 12V WS2815 60L B IP67

Supplied by the user on 2026-09-30. The part number breaks down as HD (series) · 12 V · WS2815 · 60 LEDs/m · B (black PCB) · IP67.

> ⚠️ This sheet is a **spec derived from the part number**, not the manufacturer's datasheet. The figures are typical for WS2815 60/m strip. **Check the current at the bench** (one strip at full white, measured with a clamp meter or the PSU's display). It also has one internal error: "12 mm spacing" is wrong, because 60 LEDs/m gives **16.7 mm**.

## Manufacturer: BTF-LIGHTING (added 2026-10-10)

The strips are made by **BTF-LIGHTING** ([btf-lighting.com](https://www.btf-lighting.com/products/ws2815-rgb-addressable-led-strip-dc12v)). From their product page and listings:

| Item | BTF-LIGHTING figure |
|---|---|
| Chip / voltage | WS2815, DC 12 V |
| Colour order | **GRB** (matches WLED colour order 0 in `config/*.json`) |
| Size in tube | 12 mm wide × 3.85 mm high, no adhesive tape on IP67 |
| Operating temperature | **-20 °C to +40 °C** |
| Rated power | **0.3 W per LED = 90 W per 5 m roll** (1.5 A/m, so **7.5 A per 5 m strip** at full white) |

A WLED user measured a BTF-LIGHTING WS2815 300-LED roll at full white: **3.3 A** (about 11 mA per LED), which is the same as **3.3 A per 5 m strip** (the tree now uses 5 m strips) ([wled/WLED#2013](https://github.com/wled/WLED/issues/2013)). BTF's 90 W rating is a ceiling, and real strips draw much less. The bench measurement still decides.

## Electrical

| Parameter | Value |
|---|---|
| Voltage | 12 V DC (works from 9.5 to 13.5 V) |
| Density | 60 LEDs/m, 300 per 5 m roll, 1 IC per LED (every LED individually addressable) |
| Power | **max 14.4 W/m** at full white, which is **1.2 A/m**, 72 W / 6 A per 5 m roll, about 0.24 W (20 mA) per LED |
| LED | 5050 RGB, 24-bit colour, 120° beam |
| Protocol | WS2815 / GS8208 compatible, 800 kbps. PWM 2 kHz. |
| Backup data | DI plus BI. If one pixel fails the rest keep working. Two *adjacent* dead pixels break the chain. |

## Physical

| Parameter | Value |
|---|---|
| Waterproofing | IP67, silicone tube (outdoor use, not submersible) |
| PCB | Black, 10 mm wide (about 12 mm inside the tube) |
| Cut points | At every LED (16.7 mm) |

## Connections

A 4-pin JST SM connector plus bare power leads at **both ends**:

1. +12 V (red)
2. DI: data in
3. BI: backup in. **Tie it to GND at the first pixel** (the Dig-Quad only has 3 wires per output).
4. GND (black)

> Wire colours differ between sellers. The design doc says White = GND and Blue = BI, but this strip says **black = GND**. **Check the pad labels on the actual strip** before connecting anything.

The seller recommends feeding power in every 5 m for even colour at 100 % white.

## Second source: the WS2815B range table (supplied by the user)

| Density | Power |
|---|---|
| 30 LEDs/m | 3.96 W/m |
| **60 LEDs/m** | **7.92 W/m** |
| 72 LEDs/m | 9.5 W/m |
| 96 LEDs/m | 12.67 W/m |
| 144 LEDs/m | 19 W/m |

This works out at a consistent 0.132 W (about 11 mA at 12 V) per LED, with the same 5050 RGB chip, 120° beam and cut at every LED. It also says "Control protocol: SPI", which is wrong. WS2815 uses a single-wire 800 kHz signal, not SPI, so treat the rest of that sheet as marketing copy too.

## The two sources disagree by nearly 2×

| Source | W/m | Per 6 m run | Whole tree (12 runs) | Matches |
|---|---|---|---|---|
| Spec from the part number | 14.4 | 7.2 A / 86 W | **86.4 A / ~1,040 W** | |
| WS2815B range table | 7.92 | **4.0 A** / 47.5 W | **47.5 A / ~570 W** | The builder's "4 A per strip" |
| (Design doc) | 10.8 | 5.4 A | 64.8 A / 777 W | |

**Deciding step: measure it.** Power one 5 or 6 m strip from a bench PSU, set WLED to solid white at 255 brightness with the current limiter **off**, and read the current. Also measure the voltage at the far end. Size the PSUs from that reading.
