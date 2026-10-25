# Plan: Shows built on WLED playlists, and a finger-friendly effect list

Status: **built 2026-10-24, not yet checked on the tree** (app version 2026-10-24). See "Implementation notes" at the end. Tracked in [roadmap.md](roadmap.md) Stage 4.

## Context
The Shows tab today is one flat grid of presets, with shows and looks mixed together. Shows can only be made by `treetool looks`, with one time for every look. The user wants to build and run shows from the phone. The Effects tab list (`<select size="6">`) is too small to scroll with a finger and isn't sorted.

**User's direction:** WLED presets and playlists already do most of this, so use them as they are and write as little extra code and storage as possible. There's no `shows.json` and no state file of our own. The board's `presets.json` is the single source of truth.

The device may move off WLED 0.14.0: see [wled-16-upgrade-assessment.md](wled-16-upgrade-assessment.md), which advises 0.15.4 now and 16.x in January. It was backed up first to `device-backups/Dig-Quad-A-20261024-092331`. Build against whichever firmware is on the board when the work starts.

## How each requirement maps to WLED itself
| Requirement | WLED feature used | App code needed |
|---|---|---|
| Look (effect, palette, speed, intensity, colours) | A **preset** (any free id 1–249) | none new |
| Show = ordered looks | A **playlist preset** (any free id 1–249): `ps[]` | editor: add / remove / reorder |
| Time per look | `dur[]` (tenths of a second) | a time stepper per entry |
| Fade between looks | `transition[]` | one setting for the whole show |
| Run once / n times (≤100) / forever | `repeat` (0 = forever) | a selector |
| Run 1–24 hours | turned into `repeat = ceil(hours·3600 / cycle seconds)` on save; shown afterwards as "n times (≈ h hours)" | the sum |
| Lights off when a show ends | `end:` preset **250 "Lights off"** (`{"on":false}`); the app creates it if it's missing | a few lines |
| Start (and restart) | `{ps: showId}` | a button |
| Stop (reset) | `{ps: 250}`, so lights off; the next Start begins at look 1 | a button |
| Pause | apply the current look on its own (`{ps: state.ps}`): it stays up, no advancing | a button |
| Resume | send an inline `playlist` (not saved), rotated to start at the paused look, with the saved `repeat`/`end` | ~15 lines |
| Daily start / end time (overrides limits) | WLED **time-controlled presets** (`timers.ins`): start → show preset, end → preset 250 | a small editor that writes only `timers` to `/json/cfg` |

**Accepted trade-offs of not storing anything extra:**
- An hours limit comes back as a number of passes when you edit the show later.
- Resume restarts the paused look from its beginning, and starts its pass count afresh.
- Pause state isn't shared: any phone sees the paused look as "now showing" and can press Resume.

The timer writes save the existing `timers` first and leave alone entries whose macro isn't a show or 250. They never touch LEDs, Wi-Fi or limits.

## UI — `app/tree.htm`
**Shows tab** (markup ~line 139; `loadShows` ~line 578; `saveShow` ~line 594):
1. **Shows** section at the top. Each playlist preset gets a card with ▶ Start / ⏸ Pause / ⏵ Resume / ⏹ Stop. The status comes from `state.pl` and `state.ps` (showing / which look / paused). The section also has **New show** and **Edit**.
2. **Show editor**, as an overlay like `#share`:
   - name and fade
   - entries, each with a look and a time in seconds, plus ▲▼, a bin to remove it, and Add look
   - limit: Forever / 1–24 h / 1–100 times
   - optional daily start/end and days
   - Save makes one `/json/state` call (`psave`, `n`, `o:true`, `playlist{…}`), plus a `timers` write only if the schedule changed.
   - Delete uses `{pdel: id}`.
3. **Looks** section below, with the existing buttons. New looks are saved from the Effects tab with "Save as look…" under Apply (moved there 2026-10-24; it won't save if presets.json is over 56 KB).
   - **No id ranges.** A preset is a Show if it has a `playlist`; otherwise it's a Look.
   - New presets take the first free id from 1 to 249. Only 10 (the layout) and 250 (Lights off) are reserved.
   - Drop the "looks 20–39, shows 40+" convention from `config/README.md`.
4. Reuse `api`, `sendAll`, `loadPresets`, `el`, `toast` and the existing styles.

**Effects tab** (~line 145; `renderEffects` ~line 452):
- Replace `select#fxSel` with `div#fxList` (role `listbox`) of 52 px rows.
  - `max-height: min(50dvh, 480px)`, `overflow-y: auto`. No `overscroll-behavior: contain`: on a short phone screen it hid the last rows behind the nav (fixed 2026-10-24).
  - wide scrollbar (`::-webkit-scrollbar { width: 16px }`)
  - the selected row in the accent colour and scrolled into view
- Sort by name with `localeCompare`. Keep the search box.
- Keep the selection in `S.fx`, and update the places that read `$("fxSel")` (~454, 465, 478, 512–513, 520).

Bump the "App version" note (~181).

## TreeTool
- `Looks.cs` / `looks`: also create preset 250 "Lights off", and set `end: 250` on the shows it seeds. To avoid wiping shows made on the phone, it only writes a show from `looks.json` when that preset id is free, unless `--force` is given.
- Follow-up, not this change: in 16.x the current limiter is set per output, which affects `Planner.cs` (`hw.led.maxpwr/ledma`).

## Verify the firmware before coding
On the installed version, check:
- the playlist fields (`ps/dur/transition/repeat/end`) and the limits on `dur` and `repeat`
- whether `state` now reports the playlist position, or there's a native pause (use it if so)
- the timers format and how many there are
- that `tree.htm`, `tree.json` and the presets survived (re-run `treetool upload` if not)

## End-to-end check
1. `treetool upload`, then open `http://xmastree.local/tree.htm` on a phone (ask before opening the browser pane).
2. Effects: sorted A–Z, scrolls easily with a thumb, search works, and Apply still works.
3. Make a 3-look show (20 s, 10 s, 15 s), 2 times. Check the order and times on the tree, that it turns off at the end, and that `presets.json` holds the playlist.
4. Pause holds the look. Resume carries on from it. Stop turns the lights off and the next Start begins at look 1.
5. Start 2 min ahead and end 4 min ahead: it runs by itself with the phone closed, and any other timers are untouched.

## Implementation notes (2026-10-24)
**Firmware:** built for **WLED 0.14.0**, which stays on the board until after Christmas. Checked against the backup `device-backups/Dig-Quad-A-20261024-092524`: playlists hold `ps/dur/transition/repeat/end/r`; `state` reports only `pl` (the show) and `ps` (the look), so there's no position and no native pause; `timers.ins` is empty; NTP is on (`tz` 1, UK) and the clock was right.

**Decisions made while building:**
- The lights-off time is optional. With one set, the show is saved as "forever" and the timer ends it. Without one, the limit applies.
- An hours limit is capped at 100 passes. The editor says so when the looks are too short for the hours asked.
- The per-look "× n" count was dropped on 2026-10-24 at the user's request. Each look now has only its time in seconds.
- One look's time can be at most 65,000 tenths of a second (about 108 min), which is WLED's 16-bit limit.
- A show's schedule is its start timer plus the "Lights off" timer straight after it. WLED 0.14 has 8 clock timers, so up to 4 shows can have both times. A lights-off time earlier than the start falls on the next day.
- A timer write sends all 8 clock timers (unused ones as blanks, because WLED leaves slots it isn't sent unchanged), keeps sunrise/sunset as they are, reads them back and retries once. The previous list is kept in the phone's browser storage as `timersBackup0`.
- When it opens, the app sends the phone's time (`{"time": …}`), as WLED's own page does.
- TreeTool also keeps a look from `looks.json` from overwriting a phone-made show that took its number (unless `--force`).

**Assumed, to check on the board (0.14 code read from memory, not tested):**
- `repeat: n` plays n passes and then goes to the `end` preset.
- A resumed (unsaved) playlist reports `pl: 0`.
- `POST /json/cfg` with only `timers` doesn't restart the board, and `GET /json/cfg` leaves blank timers out.
- Saving a preset with `o: true` doesn't change what the tree shows.

