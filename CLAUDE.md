# Xmas Tree Lighting

An outdoor 5–6 m cone tree: 12 × 6 m **WS2815** 12 V strips (4,320 px) driven by a QuinLED **Dig-Quad V3** (ESP32) running **WLED 0.14.0**. Designs are made in **xLights** (streamed over DDP), and phones control it through the WLED app/API. The controller runs its own Wi-Fi AP. It doesn't use the home internet.

- Device: `http://4.3.2.1/`. WLED JSON API: `/json/info`, `/json/state`, `/json/cfg`, `/json/eff`, `/json/pal`.
- Docs: `docs/`. The source PDFs are `01`–`04`. Start with `docs/design-summary.md`, the current one-strip prototype wiring in `docs/prototype-one-strip.md`, the final 12-strip wiring in `docs/dig-quad-connections.md` (stored for later), then `docs/open-questions.md` (unresolved conflicts) and `docs/roadmap.md`.
- Project skill for talking to the device: `.claude/skills/wled/SKILL.md`.
- Software (`docs/software.md`): the layout lives in `config/tree.json` (shared by the tool and the app), the C# **TreeTool** is in `tools/TreeTool` (`dotnet run --project tools/TreeTool -- info|backup|plan|apply|upload|test|serve`), and the phone app is `app/tree.htm`, served from the board at `/tree.htm`. The app's strip mapping (`buildStrips`) must stay in step with `LayoutMath` in the tool.
- After a board reboot, Windows may not rejoin `WLED-AP` by itself. Reconnect with `netsh wlan connect name="WLED-AP"`.
- Don't open the browser pane without asking first.

## Rules
- Reading (`GET /json/*`) is always fine. Changing **state** (colours, on/off, effects) is fine during testing.
- **Ask before** changing config (`POST /json/cfg`): LED outputs, Wi-Fi/AP or the current limit. A bad AP config can lock us out, and USB recovery doesn't work yet. Save `/json/cfg` to `device-backups/` before any config change.
- The PDFs contain known errors (see `docs/open-questions.md`). Don't quote their power, FPS or "upload xLights shows to WLED" claims as settled.
- Strip: HD 12V WS2815 60L B IP67. **Working assumption (user's decision): the builder's 4 A per 6 m strip, 48 A in total, 2 × HLG-480H-12A with 6 strips each, 5 A fuse per feed.** One source says 7.2 A per strip, so remind the user to measure one strip before any PSUs are bought (`docs/open-questions.md` §1, roadmap Stage 1). In the **final 12-strip build**, strip power **never** goes through the Dig-Quad (30 A board limit, 10 A per shared fuse, one PSU only; see `docs/dig-quad-connections.md`). The **one-strip prototype** does use the board's 12 V passthrough, which is fine at 4–7 A. Never join the V+ of two PSUs. Always join the grounds.
- Explain the electronics as we go. The user is learning the wiring step by step.
