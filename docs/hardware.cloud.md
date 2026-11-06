# Hardware — current state vs target

Read live from `http://4.3.2.1/json/info` and `/json/cfg` on 2026-09-30.

## Controller

| Item | Now | Target (from the docs) |
|---|---|---|
| Board | QuinLED **Dig-Quad V3** (ESP32), name `Dig-Quad-V3` | same |
| Firmware | WLED **0.14.0** (build 2310130) | WLED (upgrading is worth considering, see open-questions §7) |
| Wi-Fi | AP `WLED-AP`, **factory password**, channel 1, 4.3.2.1 | AP `MatrixTree-WiFi`, strong password, channel 1/6/11 |
| Client Wi-Fi | SSID `<home-wifi>` saved without a password (not joined) | not needed, because the design is AP-only |
| DDP / E1.31 input | not checked yet | enabled |
| MAC | <board-mac> | |
| Usermods | AudioReactive (off) | |

## LED outputs

> **Update 2026-10-10:** the design has moved to 5 m strips (300 LEDs each, 3,600 in total). `config/tree.json` is updated, but the board still holds the 360-per-strip layout until `treetool apply` is re-run, which changes the board config.

> **Update 2026-09-30 (afternoon):** TreeTool has applied the full-tree layout. The board now has 4 × 1,080 = 4,320 LEDs, 12 strip segments, limiter 40 A at 11 mA per LED, boot preset 10, and about 147 KB free memory. The "Now" column below shows the original factory state (backed up in `device-backups/`).

| Port | GPIO | Now (start/len, type) | Target |
|---|---|---|---|
| LED1 | 16 | 0 / 30, WS281x | 0 / 1,080 |
| LED2 | 3 | 30 / 30 | 1,080 / 1,080 |
| LED3 | 1 | 60 / 30 | 2,160 / 1,080 |
| LED4 | 4 | 90 / 30 | 3,240 / 1,080 |

The current-limit setting (ABL) is 1,250 mA, a placeholder. It needs setting to match the PSUs.

> GPIO 1 and 3 are the ESP32's UART pins. While they drive LEDs, USB serial output is garbled. Use USB only for flashing and recovery.

## USB

Windows shows **no ESP32 COM port**, only COM1. The cable may be power-only, or the CH340/CP210x driver may be missing. This needs fixing before we flash firmware or recover the board over USB.

## PC networking

The PC's Wi-Fi is on the WLED AP (4.3.2.2), and its Ethernet is on the home LAN (<pc-ip>), so the PC can reach both at once.
