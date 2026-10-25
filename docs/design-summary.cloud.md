# Design Summary

Taken from the four PDFs. Where the PDFs are wrong or contradict each other, see [open-questions.md](open-questions.md).

## The tree

- An outdoor **5–6 m cone** made of **12 vertical 5 m LED strips** (changed from 6 m on 2026-10-10), strung between a top rigging ring and a base ground ring around a centre mast
- The strips hang on load-bearing backbones (steel aircraft wire, mesh or webbing), with UV zip-ties every 15–20 cm. The strips themselves carry **no** tension.
- The electronics sit in an IP66/67 **polycarbonate** box (not metal, which would block Wi-Fi), mounted vertically with cable exits on the bottom. It has an external high-gain antenna.

## Pixels

| | |
|---|---|
| Strip | **WS2815**, 12 V, 60 LEDs/m. Purchased: **HD 12V WS2815 60L B IP67**, working assumption **about 3.3 A per 5 m strip, 40 A in total** (The builder's figure, still to be measured; see [strip-spec-ws2815.md](strip-spec-ws2815.md)) |
| Per strip | 300 px (5 m), one whole roll per strip with no extensions. |
| Total | **3,600 px**, 60 m (12 × 5 m rolls, plus spares). |
| Wires | Red +12 V, White GND, Green DI (data), Blue BI (backup in) |
| Key rule | At the **start of each run**, tie BI to GND. At joints, connect all 4 pads straight through (BO→BI). No soldering: whole rolls are used uncut, and every joint is a glue-lined heat-shrink butt connector (see [power-and-wiring.md](power-and-wiring.md#joints-no-soldering-decided-2026-10-17)). |

## Controller

- QuinLED **Dig-Quad** (v3/v4) with an ESP32 running **WLED**. It has level shifters (3.3 V → 5 V data) and fused outputs.
- It runs as a **standalone access point** (no home internet). The planned SSID is `MatrixTree-WiFi`. Laptop and phone both connect to it directly at `4.3.2.1`.

### Target WLED LED outputs (Addendum D)

| Port | GPIO | Quadrant | Strips | Start | Count |
|---|---|---|---|---|---|
| LED1 | 16 | North | 1–3 | 0 | 900 |
| LED2 | 3 | East | 4–6 | 900 | 900 |
| LED3 | 1 | South | 7–9 | 1,800 | 900 |
| LED4 | 4 | West | 10–12 | 2,700 | 900 |

LED type: WS281x (WLED drives WS2815 with the standard WS281x 800 kHz setting).

Each port drives **3 strips joined end to end in a zig-zag**: up, down, up. The xLights mapping follows this (for example, string 1 top-to-bottom, string 2 bottom-to-top).

## Software

| Layer | Tool | Role |
|---|---|---|
| Design | **xLights** (laptop) | A 3D Tree model (12 strings × 300 nodes, 0 spiral wraps). Timeline effects. Output over **DDP** to controller `4.3.2.1`. |
| Controller | **WLED** | Receives DDP/E1.31, runs built-in effects, stores presets and playlists, provides the JSON API |
| Daily use | WLED app / web UI (phone) | On/off, brightness, speed, pick preset/playlist |

Two modes: **A: live stream** (xLights → DDP, used while designing) and **B: standalone** (presets on flash, phone control).
"Off" means one of two things: a **software blackout** (the PSUs stay live) or **cutting the mains** (for example with a smart plug).

## Power (as specified. ⚠️ Superseded: the Dig-Quad can't carry this current and the total draw is unresolved. See open-questions §1–2 and power-and-wiring.md)

- 0.015 A/px × 4,320 = **64.8 A @ 12 V, about 777 W**
- **Superseded 2026-10-22:** 3 PSUs (2 × HLG-320H-12A + one 150 W supply), see power-and-wiring.md. Original spec: **2 × Mean Well HLG-600H-12A** (480 W, IP67, fanless) with split loads. PSU 1 feeds channels 1–2 and PSU 2 feeds channels 3–4. The grounds are shared, and the V+ outputs must **never** be joined.
- **Superseded 2026-10-17:** each strip is fed at its **bottom end only**, by its own fused 1.5 mm² cable from the base box along the ground (this replaced the 2026-10-10 top feed up the centre mast). See [power-and-wiring.md](power-and-wiring.md).
- Wire gauges: PSU → Dig-Quad 10–12 AWG, top bus 14 AWG, base pigtails 18 AWG

## Build phases (from the technical doc)

1. **Bench validation**: flash WLED, set up the AP, test the antenna range, test one 5 m strip (including the backup-line routing and heat)
2. **Mechanical rigging**: mast, rings, backbones, strips, power buses, sealing
3. **Software & 3D mapping**: AP mode, xLights cone model, pixel order
4. **Sequencing & sealing**: effects, presets, enclosure glands, antenna placement
