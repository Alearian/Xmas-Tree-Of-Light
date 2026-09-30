# Design Summary

Taken from the four PDFs. Where the PDFs are wrong or contradict each other, see [open-questions.md](open-questions.md).

## The tree

- An outdoor **5–6 m cone** made of **12 vertical 6 m LED strips**, strung between a top rigging ring and a base ground ring around a centre mast
- The strips hang on load-bearing backbones (steel aircraft wire, mesh or webbing), with UV zip-ties every 15–20 cm. The strips themselves carry **no** tension.
- The electronics sit in an IP66/67 **polycarbonate** box (not metal, which would block Wi-Fi), mounted vertically with cable exits on the bottom. It has an external high-gain antenna.

## Pixels

| | |
|---|---|
| Strip | **WS2815**, 12 V, 60 LEDs/m. Purchased: **HD 12V WS2815 60L B IP67**, working assumption **4 A per 6 m run, 48 A in total** (The builder's figure, still to be measured; see [strip-spec-ws2815.md](strip-spec-ws2815.md)) |
| Per strip | 360 px (6 m). Rolls are sold at 5 m, so each run is a 5 m roll plus a 1 m soldered extension. |
| Total | **4,320 px**. The bill of materials says 72 m (15 × 5 m rolls, including spares). |
| Wires | Red +12 V, White GND, Green DI (data), Blue BI (backup in) |
| Key rule | At the **start of each run**, tie BI to GND. At joints, connect all 4 pads straight through (BO→BI). Seal cut ends with neutral-cure silicone and adhesive-lined heat-shrink. |

## Controller

- QuinLED **Dig-Quad** (v3/v4) with an ESP32 running **WLED**. It has level shifters (3.3 V → 5 V data) and fused outputs.
- It runs as a **standalone access point** (no home internet). The planned SSID is `MatrixTree-WiFi`. Laptop and phone both connect to it directly at `4.3.2.1`.

### Target WLED LED outputs (Addendum D)

| Port | GPIO | Quadrant | Strips | Start | Count |
|---|---|---|---|---|---|
| LED1 | 16 | North | 1–3 | 0 | 1,080 |
| LED2 | 3 | East | 4–6 | 1,080 | 1,080 |
| LED3 | 1 | South | 7–9 | 2,160 | 1,080 |
| LED4 | 4 | West | 10–12 | 3,240 | 1,080 |

LED type: WS281x (WLED drives WS2815 with the standard WS281x 800 kHz setting).

Each port drives **3 strips joined end to end in a zig-zag**: up, down, up. The xLights mapping follows this (for example, string 1 top-to-bottom, string 2 bottom-to-top).

## Software

| Layer | Tool | Role |
|---|---|---|
| Design | **xLights** (laptop) | A 3D Tree model (12 strings × 360 nodes, 0 spiral wraps). Timeline effects. Output over **DDP** to controller `4.3.2.1`. |
| Controller | **WLED** | Receives DDP/E1.31, runs built-in effects, stores presets and playlists, provides the JSON API |
| Daily use | WLED app / web UI (phone) | On/off, brightness, speed, pick preset/playlist |

Two modes: **A: live stream** (xLights → DDP, used while designing) and **B: standalone** (presets on flash, phone control).
"Off" means one of two things: a **software blackout** (the PSUs stay live) or **cutting the mains** (for example with a smart plug).

## Power (as specified. ⚠️ Superseded: the Dig-Quad can't carry this current and the total draw is unresolved. See open-questions §1–2 and power-and-wiring.md)

- 0.015 A/px × 4,320 = **64.8 A @ 12 V, about 777 W**
- Recommended: **2 × Mean Well HLG-480H-12A** (480 W, IP67, fanless) with split loads. PSU 1 feeds channels 1–2 and PSU 2 feeds channels 3–4. The grounds are shared, and the V+ outputs must **never** be joined.
- Inject power at both ends: base feeds from the Dig-Quad, plus a 14 AWG bus up the trunk to a top ring. Fuse each branch at 5 A or 7.5 A.
- Wire gauges: PSU → Dig-Quad 10–12 AWG, top bus 14 AWG, base pigtails 18 AWG

## Build phases (from the technical doc)

1. **Bench validation**: flash WLED, set up the AP, test the antenna range, test one 6 m strip (including the backup-line routing and heat)
2. **Mechanical rigging**: mast, rings, backbones, strips, power buses, sealing
3. **Software & 3D mapping**: AP mode, xLights cone model, pixel order
4. **Sequencing & sealing**: effects, presets, enclosure glands, antenna placement
