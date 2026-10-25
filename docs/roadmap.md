# Roadmap

Follows the four phases in the technical doc, plus our own software work.

## Stage 0 — Docs & setup
- [x] Collect the PDFs into `docs/` and summarise them
- [ ] Settle the [open questions](open-questions.md) with the builder (PSU size, Dig-Quad current limits, injection points, strip model)
- [ ] Get the ESP32 USB COM port working (cable or driver)

## Stage 1 — Bench validation (doc Phase 1)
- [ ] Back up the current WLED config
- [ ] Commission the AP: SSID `MatrixTree-WiFi`, a new password, channel 1/6/11, DDP/E1.31 enabled
- [ ] Wire **one** WS2815 strip to LED1, powered through the board ([prototype-one-strip.md](prototype-one-strip.md)) (BI tied to GND) on a bench PSU with a shared ground. Set WLED to 300 px.
- [ ] Test colours, effects, heat, and bypassing a dead pixel
- [ ] ⚠️ **Power check (before buying PSUs):** with the current limiter off, set the strip to solid white at 255 brightness. Record the current and the voltage at the far end. We're working on the builder's **4 A per strip** (a 5 m strip should be about 3.3 A). If it's above about 4.5 A, revisit [open-questions.md](open-questions.md) §1 (it may need 3 PSUs instead of 2).
- [ ] Check the wire colours and pad labels on the real strip (the spec says black = GND, the doc says white = GND)
- [ ] Test the antenna range and the phone connection

## Stage 2 — One full quadrant, then all four
- [ ] Wire a zig-zag chain of 3 strips on one port (1,080 px) and measure FPS and ESP32 memory
- [ ] Set the target output config (4 × 1,080) and create one WLED segment per strip
- [ ] Set the current limit to match the real PSUs

## Stage 3 — Mapping & design
> **Optional since 2026-10-03.** The tree runs standalone on WLED presets ([open-questions.md](open-questions.md) §6), so xLights is only for experiments while a laptop is available.

- [ ] xLights: controller `xmastree.local` over DDP, Tree model with 12 strings × 300 nodes and the zig-zag order
- [ ] Live-stream tests from xLights

## Stage 4 — Our software (see [software.md](software.md))
- [x] Shared layout file (`config/tree.json`) and C# TreeTool (backup/plan/apply/upload/test/serve)
- [x] Full-tree layout applied to the board. The app has been uploaded to `http://4.3.2.1/tree.htm`.
- [ ] Try the app on a phone and polish it
- [x] **Networking (done 2026-10-03, option 1):** the board is on home Wi-Fi `<home-wifi>` as `xmastree.local` (DHCP <board-ip>), with `WLED-AP` as the boot fallback. Both layouts now use `xmastree.local`.
  1. ~~The board joins home Wi-Fi `<home-wifi>`, with `WLED-AP` as a fallback.~~ **Chosen.** The user types the password at `http://4.3.2.1/settings/wifi`. Afterwards: find its new IP, update `config/tree.json`, re-upload, and consider a fixed IP.
  2. PC bridge: `treetool serve` listening on the LAN at <pc-ip>:8080 (needs a firewall rule).
  3. A travel router in the enclosure (later, for multiple controllers).
- [ ] **Phone controller**: a mobile-first web app (PWA) using the WLED JSON API. It gives on/off, brightness, speed, presets and playlists, plus tree-aware controls (per strip or quadrant, bands by height).
- [ ] **Config tool**: applies a version-controlled WLED config (outputs, segments, presets, AP settings) and always backs up the current config first
- [x] **Decision (2026-10-03):** the tree runs standalone on WLED built-in effects, presets and playlists, controlled from the phone app. No laptop is needed ([open-questions.md](open-questions.md) §6).
- [x] **Look library (first version, 2026-10-03):** `config/looks.json`, installed by `treetool looks`: 9 cone looks (presets 20–28) and the "Tree show" playlist (40). Next: tune them on the real strip.
- [ ] **Show builder and controls in the app (built 2026-10-24, to check on the tree):** build shows from looks on the phone, with times per look, limits, pause/resume/stop, daily start/end and lights off at the end, all on WLED playlists and timers. Also a finger-friendly, sorted effect list. See [shows-and-effects-plan.md](shows-and-effects-plan.md).
- [ ] **Custom cone effects in firmware (later):** a WLED add-on in C++ for effects that presets can't do, such as a rotating beam, true snowfall and a star burst. Needs USB recovery working first.
- [x] **Laser Search look (built and seen on the tree 2026-10-24):** preset 33, a red strip sweeping round the tree like a Larson scanner, 4 s one way, fading to 30 % at the top and bottom. Started as "Cylon". WLED's Scanner run across the strips with grouped segments. See [plans/cylon-effect-plan.md](plans/cylon-effect-plan.md).
- [ ] ~~Look library~~ (original note): a version-controlled set of tree presets (whole-tree and per-strip/quadrant looks) and a "show" playlist that cycles through them, applied by TreeTool so the board can be rebuilt from the repo
- [x] **App time sync (2026-10-24):** the app sends the phone's time when it opens, so WLED timers work even without internet time (on home Wi-Fi the board also has NTP).
- [ ] **Consider WLED 2D mapping:** the zig-zag strips form a 12 × 300 serpentine matrix, so 2D effects (scrolling text, fire, plasma, rain) could wrap around the tree. **Planned 2026-10-25, on hold until after 28 November 2026:** [plans/2d-matrix-plan.md](plans/2d-matrix-plan.md) (four vertical serpentine panels, one per output; trial on the board first)

## Stage 5 — Rig, seal, run (doc Phases 2 & 4)
- [ ] Mechanical rigging, power buses, sealing and the enclosure
- [ ] Load the show library and follow [04-setup.pdf](04-setup.pdf) each day the tree is in use
