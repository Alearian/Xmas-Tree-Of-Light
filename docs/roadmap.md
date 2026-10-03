# Roadmap

Follows the four phases in the technical doc, plus our own software work.

## Stage 0 — Docs & setup
- [x] Collect the PDFs into `docs/` and summarise them
- [ ] Settle the [open questions](open-questions.md) with the builder (PSU size, Dig-Quad current limits, injection points, strip model)
- [ ] Get the ESP32 USB COM port working (cable or driver)

## Stage 1 — Bench validation (doc Phase 1)
- [ ] Back up the current WLED config
- [ ] Commission the AP: SSID `MatrixTree-WiFi`, a new password, channel 1/6/11, DDP/E1.31 enabled
- [ ] Wire **one** WS2815 strip to LED1, powered through the board ([prototype-one-strip.md](prototype-one-strip.md)) (BI tied to GND) on a bench PSU with a shared ground. Set WLED to 360 px.
- [ ] Test colours, effects, heat, and bypassing a dead pixel
- [ ] ⚠️ **Power check (before buying PSUs):** with the current limiter off, set the strip to solid white at 255 brightness. Record the current and the voltage at the far end. We're working on the builder's **4 A per 6 m strip**. If it's above about 4.5 A, revisit [open-questions.md](open-questions.md) §1 (it may need 3 PSUs instead of 2).
- [ ] Check the wire colours and pad labels on the real strip (the spec says black = GND, the doc says white = GND)
- [ ] Test the antenna range and the phone connection

## Stage 2 — One full quadrant, then all four
- [ ] Wire a zig-zag chain of 3 strips on one port (1,080 px) and measure FPS and ESP32 memory
- [ ] Set the target output config (4 × 1,080) and create one WLED segment per strip
- [ ] Set the current limit to match the real PSUs

## Stage 3 — Mapping & design
- [ ] xLights: controller `4.3.2.1` over DDP, Tree model with 12 strings × 360 nodes and the zig-zag order
- [ ] Live-stream tests from xLights

## Stage 4 — Our software (see [software.md](software.md))
- [x] Shared layout file (`config/tree.json`) and C# TreeTool (backup/plan/apply/upload/test/serve)
- [x] Full-tree layout applied to the board. The app has been uploaded to `http://4.3.2.1/tree.htm`.
- [ ] Try the app on a phone and polish it
- [ ] **Paused here (2026-09-30):** choose the networking option.
  1. The board joins home Wi-Fi `<home-wifi>`, with `WLED-AP` as a fallback. **Recommended.** The user types the password at `http://4.3.2.1/settings/wifi`. Afterwards: find its new IP, update `config/tree.json`, re-upload, and consider a fixed IP.
  2. PC bridge: `treetool serve` listening on the LAN at <pc-ip>:8080 (needs a firewall rule).
  3. A travel router in the enclosure (later, for multiple controllers).
- [ ] **Phone controller**: a mobile-first web app (PWA) using the WLED JSON API. It gives on/off, brightness, speed, presets and playlists, plus tree-aware controls (per strip or quadrant, bands by height).
- [ ] **Config tool**: applies a version-controlled WLED config (outputs, segments, presets, AP settings) and always backs up the current config first
- [ ] **Decision:** how to run xLights shows standalone ([open-questions.md](open-questions.md) §6). The choices are WLED built-in effects only, an FPP/Pi player, or a custom FSEQ player.

## Stage 5 — Rig, seal, run (doc Phases 2 & 4)
- [ ] Mechanical rigging, power buses, sealing and the enclosure
- [ ] Load the show library and follow [04-setup.pdf](04-setup.pdf) each day the tree is in use
