# The phone app (`app\tree.htm`)

One self-contained HTML file (~110 KB: HTML, CSS, JS and a built-in QR generator, no CDN - the tree's
AP has no internet). The board serves it at `/tree.htm`; it loads `/tree.json` (the layout) and
`/presets.json`, and talks to `/json/*` on the same origin.

Tabs: **Tree** (rotating 3D cone preview from `/json/live`, quick colours), **Effects** (A-Z list,
palette, 3 colours, speed, intensity, "Save as look…"), **Strips** (tap strips or N/E/S/W/odd/even
groups), **Shows** (start/pause/resume/stop, editor, daily timers, looks list with delete),
**Config** (controller info, both addresses with QR codes, version).

Key functions: `buildStrips(layout)` (must match `LayoutMath.Segments`), `api(ci, path, body)`,
`sendSegments(segFn, extra)` (sends per-strip segments and unfreezes them), `loadPresets()`,
`PRESETS_MAX = 56 * 1024`.

## Workflow for a change

1. **Edit** `app\tree.htm` on the PC copy (stage it first if working from the cloud).
2. **Bump the version** in `<p class="note" id="appVer">App version yyyy-MM-ddX</p>` - date plus a
   letter per change that day (2026-10-25a, b, …). This is how you tell which copy a phone has.
3. **Test locally** without touching the board's files:
   ```powershell
   dotnet run --project tools\TreeTool -- serve --layout config\proto-tree.json
   Start-Process http://localhost:8080/tree.htm
   ```
   The dev server proxies the API, so state changes do reach the tree. Ask before opening a browser
   pane for him.
4. **Check size**: `(Get-Item app\tree.htm).Length` - keep it well under ~150 KB; the board's heap is
   ~140 KB and several phones loading at once is the stress case.
5. **Upload** (writes `/tree.htm` and `/tree.json`):
   ```powershell
   dotnet run --project tools\TreeTool -- upload --layout config\proto-tree.json
   ```
6. **Verify on the board**: `(Invoke-WebRequest http://<board-ip>/tree.htm).Content -match 'App version 2026-10-25b'`.
7. **Verify on the phone**: open `http://<board-ip>/tree.htm`, Config tab, check the version. If it
   shows the old one, the phone cached it: pull to reload, or clear site data / remove and re-add the
   home-screen icon.
8. Record the deployed version in `docs\software.md` and commit when Mr Furniss asks.

## Rules of thumb

- No external resources (fonts, CDNs, images by URL). Inline everything.
- Phones use the **IP**, never `xmastree.local` (Android can't resolve `.local`). QR codes encode the IP.
- Every write the app makes to presets must check the size first and refuse above `PRESETS_MAX`.
- The app stores nothing of its own except per-phone conveniences in `localStorage` (e.g. paused show).
  The board's `presets.json` and timers are the single source of truth - no side files, no id ranges.
- Changing an effect from the app must unfreeze all strip segments and delete overlay/band segments
  (`sendSegments` does this) so "across" looks like Laser Search end cleanly.
- Keep the merge copies in `app\merge\` and `app\*.cloud.htm` out of uploads; only `app\tree.htm` goes
  to the board.
