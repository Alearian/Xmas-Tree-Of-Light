# Hardware — current state vs target

Read live from `http://4.3.2.1/json/info` and `/json/cfg` on 2026-09-30.

## Controller

| Item | Now | Target (from the docs) |
|---|---|---|
| Board | QuinLED **Dig-Quad V3** (ESP32), name `Dig-Quad-V3` | same |
| Version | **Pre-assembled**, with automatic 5/12/24 V input detection and no voltage jumper (confirmed by the user on 2026-10-03) | same |
| Firmware | WLED **0.14.0** (build 2310130) | WLED (upgrading is worth considering, see open-questions §7) |
| Wi-Fi | **Home Wi-Fi `<home-wifi>` since 2026-10-03:** `xmastree.local`, DHCP <board-ip>, channel 11, about -65 dBm (70 %) at the bench. AP `WLED-AP` (4.3.2.1) is the fallback: it opens only if home Wi-Fi isn't found at boot. AP password still to be checked/changed. |
| Clock | **Internet time (NTP) on since 2026-10-03**, `0.wled.pool.ntp.org`, time zone GMT/BST. The clock resets on power loss and re-syncs once the board is back on home Wi-Fi. In AP fallback mode there's no internet, so no clock. | same | AP `MatrixTree-WiFi`, strong password, channel 1/6/11 |
| Client Wi-Fi | SSID `<home-wifi>` saved without a password (not joined) | not needed, because the design is AP-only |
| DDP / E1.31 input | not checked yet | enabled |
| MAC | <board-mac> | |
| Usermods | AudioReactive (off) | |

## LED outputs

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
