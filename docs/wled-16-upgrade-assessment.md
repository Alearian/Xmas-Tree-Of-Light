# WLED 16 upgrade assessment (Dig-Quad A)

Checked 2026-10-24 against the WLED releases page, the 16.0.1 asset list, and today's backup
`device-backups/Dig-Quad-A-20261024-092524` (info.json, cfg.json).

## Verdict

**No-go for 16.x before Christmas. Go for 0.15.4 now (optional), and 16.0.1 in January once USB recovery is ready.**

The flash layout and the app are fine for 16.x. The problem is a known, unfixed issue where WLED 16 boot-loops
(brownout reset at Wi-Fi start) on some boards, and **QuinLED Dig-Quad v3r1 with QuinLED-ESP32 v1r3 is named in
the report**. Every affected user fixed it by going back to 0.15.x, which needs a USB flash if the board can no
longer join Wi-Fi. USB recovery isn't set up yet, and the board is the only controller for the tree.

## What the board has now (from the backup)

| Item | Value | What it means for the upgrade |
|---|---|---|
| Firmware | 0.14.0, build 2310130, IDF v3.3.6 core | 16.x is two platform jumps away (IDF 3.3 → 4.4) |
| Filesystem | 983 KB total, 139 KB used | Standard 4 MB layout. OTA to 0.15/16 works; no repartition needed |
| App slot | 1.5 MB (standard layout) | `WLED_16.0.1_ESP32.bin` is 1.24 MB, fits |
| LEDs | 3,600 on 4 buses × 900, type WS281x, GPIO 16 / 3 / 1 / 4 | Within limits on 0.15 and 16 |
| Free heap | ~140 KB | 16's per-segment buffers (layering) use roughly 15–30 KB more; should still be OK |
| Usermods | AudioReactive present | 0.15+ builds include it by default; 0.15.4 defaults to "no mic" |
| Files | cfg.json, presets.json, tree.htm, tree.json | Survive an OTA (OTA only writes the app slot) |

## Findings

1. **Brownout boot-loop on 16.x (the blocker).** [Issue #5570](https://github.com/wled/WLED/issues/5570) lists
   "QuinLED Dig-Quad (v3r1) with QuinLED-ESP32 (v1r3)" among boards that drop off Wi-Fi or boot-loop after 16.0.x,
   all fixed by reverting to 0.15.x. [#5595](https://github.com/wled/WLED/issues/5595) is the same on a plain
   ESP-WROOM-32 (4 MB). Maintainers say they won't revert the core; the fix is hardware (a large capacitor on 3V3,
   or powering the ESP32 module separately). Quindor's [forum thread](https://wled.discourse.group/t/16-x-problems-with-early-quinled-esp32s/15450)
   says there is no hardware fix from him yet (Sept 2026). Lowering Wi-Fi TX power did not reliably help.
   *Check:* the revision printed on your QuinLED-ESP32 module. v1r3 is the reported one; v2.x worked for one user.
   Not confirmed either way for a WROOM-32E module.
2. **OTA path from 0.14.0.** OTA straight to 0.15.x or 16.x works because the FS is 983 KB (the condition in
   [#4369](https://github.com/wled/WLED/issues/4369)). No intermediate version is required, but going via 0.15.4
   first is the safer route because it stays close to the old platform. 16.0 also offers a bootloader upgrade via
   OTA; skip that unless USB recovery is ready.
3. **Right binary.** Classic ESP32 4 MB: `WLED_16.0.1_ESP32.bin` (not `_Ethernet`, `_WROVER`, `_HUB75` or
   `_DEBUG`). For 0.15.4 the equivalent is `WLED_0.15.4_ESP32.bin`.
4. **Serial pins.** Outputs 2 and 3 use GPIO 3 and 1 (UART RX/TX). That works on 0.14 and 0.15. 16.x adds pin
   conflict checks and a pin info page; nothing in the notes says GPIO 1/3 break, but check strips 4–9 light after
   any upgrade.
5. **Config and presets.** 0.15 and 16 migrate cfg.json automatically. 0.15.4 fixes a long-standing preset
   corruption bug. 16.0 changed colour handling and gamma, so looks may shift slightly in colour, and 16.0.1
   changed boot-preset behaviour: confirm the board still boots into the right look.
6. **tree.htm.** Uses `/json/state`, `/json/info`, `/json/eff|pal|palx|fxdata`, `/presets.json` and the `/ws`
   live view, all still present. `/json/live` is turned off while WebSockets are on (since 0.15); tree.htm only
   uses it as a fallback, so the board copy keeps working through the WebSocket. Under `treetool serve` the
   fallback still works.
7. **TreeTool.** `/json`, `/json/state`, `/json/cfg`, `/presets.json` and `POST /upload` are unchanged in 16.
   0.15+ also has per-output current limits; the global `maxpwr` that TreeTool writes still applies when
   per-output limiting is off. Re-run `treetool info` and a layout upload after upgrading to confirm.
8. **Ledmaps / layout.** The project doesn't upload a WLED `ledmap.json`; `proto-tree.json` drives buses and
   segments through `/json/cfg` and presets, so 16.0.1's ledmap parser fix doesn't affect it.
9. **Other known 16.x issues.** Visual regressions in 16.0.0 (fixed in 16.0.1); xLights can't auto-discover 16.0
   (no ArtPoll reply), relevant only if music sync goes via xLights.

## Steps

### Now (optional): 0.14.0 → 0.15.4
1. Backup is done (2026-10-24). Also export presets/config from WLED Settings → Security & Updates.
2. Download `WLED_0.15.4_ESP32.bin` from the 0.15.4 release.
3. Power the board from the normal supply with the strips connected, then Settings → Security & Updates →
   Manual OTA update, upload the .bin and wait for the reboot.
4. Open `http://<board-ip>/tree.htm`; check all 4 outputs light, presets load, boot look is right,
   then `treetool info`.

### January: 0.15.4 → 16.0.1
1. Set up USB recovery first: USB-C cable to the QuinLED-ESP32, confirm it shows as a COM port, and test
   flashing from install.wled.me or `esptool`.
2. Fit the hardware mitigation (capacitor on 3V3/GND, as in the WLED wiring guide) if the module is v1r3.
3. OTA `WLED_16.0.1_ESP32.bin`. Don't take the bootloader upgrade yet.
4. If it boot-loops or drops off Wi-Fi: USB-flash 0.15.4 and restore cfg.json/presets.json from the backup.
