# Xmas Tree Lighting

An outdoor 5–6 m cone tree: 12 × 5 m **WS2815** 12 V strips (3,600 px, fed at the bottom only) driven by a QuinLED **Dig-Quad V3** (ESP32) running **WLED 0.14.0**. Designs are made in **xLights** (streamed over DDP), and phones control it through the WLED app/API. The controller runs its own Wi-Fi AP. It doesn't use the home internet.

- Device: `http://4.3.2.1/`. WLED JSON API: `/json/info`, `/json/state`, `/json/cfg`, `/json/eff`, `/json/pal`.
- Docs: `docs/`. The source PDFs are `01`–`04`. Start with `docs/design-summary.md`, then `docs/open-questions.md` (unresolved conflicts) and `docs/roadmap.md`.
- Project skill for talking to the device: `.claude/skills/wled/SKILL.md`.
- Software (`docs/software.md`): the layout lives in `config/tree.json` (shared by the tool and the app), the C# **TreeTool** is in `tools/TreeTool` (`dotnet run --project tools/TreeTool -- info|backup|plan|apply|upload|test|serve`), and the phone app is `app/tree.htm`, served from the board at `/tree.htm`. The app's strip mapping (`buildStrips`) must stay in step with `LayoutMath` in the tool.
- After a board reboot, Windows may not rejoin `WLED-AP` by itself. Reconnect with `netsh wlan connect name="WLED-AP"`.
- Don't open the browser pane without asking first.

## Rules
- Reading (`GET /json/*`) is always fine. Changing **state** (colours, on/off, effects) is fine during testing.
- **Ask before** changing config (`POST /json/cfg`): LED outputs, Wi-Fi/AP or the current limit. A bad AP config can lock us out, and USB recovery doesn't work yet. Save `/json/cfg` to `device-backups/` before any config change.
- The PDFs contain known errors (see `docs/open-questions.md`). Don't quote their power, FPS or "upload xLights shows to WLED" claims as settled.
- Strip: HD 12V WS2815 60L B IP67. Made by BTF-LIGHTING (btf-lighting.com). **Working assumption (user's decisions): 5 m strips, about 3.3 A each (the builder's 11 mA per LED), 40 A in total, three PSUs (decided 2026-10-22): 2 × Mean Well HLG-320H-12A (22 A) for strips 1–5 and 6–9, plus a 150 W 12 V supply (~12.5 A) for strips 10–12, one 5 A fused feed per strip to its bottom end, cables along the ground (decided 2026-10-17, replacing the top feed up the mast), PSUs and controller in a box at the base** (`docs/power-and-wiring.md`). BTF rates it at 7.5 A per 5 m, so remind the user to measure one strip before any PSUs are bought (`docs/open-questions.md` §1, roadmap Stage 1). Strip power **never** goes through the Dig-Quad (30 A board limit, one PSU only; see `docs/quinled-dig-quad-guide.md`). Never join the V+ of two PSUs. Always join the grounds. **No soldering (user's decision 2026-10-17):** joints are glue-lined heat-shrink butt connectors; mains is one lead + SOMELINE IP68 connector per PSU into a 3-way RCD socket; the Dig-Quad is fed from PSU 1 via a 2 A inline fuse.
- Explain the electronics as we go. The user is learning the wiring step by step.
