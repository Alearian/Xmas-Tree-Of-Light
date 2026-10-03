# Dig-Quad Connections (final build, 12 strips)

> **Status: stored for the final build.** The current prototype is one strip, wired differently: see [prototype-one-strip.md](prototype-one-strip.md).

How the QuinLED Dig-Quad, the two 12 V supplies and the 12 WS2815 strips connect. The drawn version is [dig-quad-wiring.html](dig-quad-wiring.html) (open it in a browser), also published at https://claude.ai/artifact/YTJVgmp966PwUT9kwGtskP.

This is the layout for **one Dig-Quad** (open-questions §8, option A). Background and the reasoning behind it: [power-and-wiring.md](power-and-wiring.md) and [quinled-dig-quad-guide.md](quinled-dig-quad-guide.md).

> ⚠️ **Before anything is bought or switched on**
> - This uses the working assumption of **4 A per 6 m strip** (The builder's figure). One source says 7.2 A. Measure one strip at full white first ([open-questions.md](open-questions.md) §1). Above about 4.5 A, change to 3 PSUs with 4 strips each.
> - Mains wiring to the PSUs, and any relay that switches mains, must be done by a qualified electrician.
> - Terminal positions are drawn schematically. Use the labels printed on the board and on the strip pads.

**The key rule:** the Dig-Quad's LED outputs carry **data only**. Each output uses its data terminal and its `−` (GND) terminal. The output **V+ terminals stay empty**. Strip power goes from each PSU through its own fuse block, never through the board.

## Why the board's own 12 V passthrough isn't used here

The Dig-Quad **is** designed to pass 12 V through itself. Power goes in at the input terminals, through the board's copper and fuses, and out of each output's `+` terminal ("Input voltage = output voltage for the LEDs", QuinLED v3.1 spec). That's how the [one-strip prototype](prototype-one-strip.md) is wired.

For the full tree, the numbers rule it out (checked against quinled.info on 2026-10-03):

| QuinLED limit | Full tree (4 A per strip) |
|---|---|
| 30 A continuous, whole board | 48 A |
| 10 A per fuse, and **LED1+LED2 share one fuse**, LED3+LED4 another | 6 strips = 24 A per fuse |
| One PSU per board ("Do not connect wires from multiple power supplies to a single QuinLED-Dig-Quad!") | Needs 2 PSUs |

Sending only the base feeds through the board doesn't work either. Each shared fuse would carry 12 A, and strips 7–12 would get base power from PSU 1 and top power from PSU 2, which joins the two PSUs' V+ through the strip's own copper.

Our board is the **pre-assembled** Dig-Quad V3 (confirmed by the user on 2026-10-03). It detects 5, 12 or 24 V by itself and has no voltage jumper.

## 1. Whole system

```
                     ┌──────── F1 15 A ── top bus A (12 AWG pair) ── top junction A (6 × 5 A) ──► tops of strips 1–6
 Mains ─┬─► PSU 1 ──►│ FUSE BLOCK A ── F2–F7 5 A ×6 (18 AWG) ───────────────────────────────────► bases of strips 1–6
        │   V−       └──────── F8 2 A ──► Dig-Quad +12 V in
        │   │
        │   │                 ┌── F1–F6 5 A ×6 (18 AWG) ─────────────────────────────────────────► bases of strips 7–12
        └─► PSU 2 ──────────►│ FUSE BLOCK B
            V−                └── F7 15 A ── top bus B (12 AWG pair) ── top junction B (6 × 5 A) ──► tops of strips 7–12
            │
 COMMON GND BAR ◄── PSU 1 −, PSU 2 −, Dig-Quad −, every strip GND (white), both top-bus returns

 Dig-Quad  LED1 (GPIO 16) ── data + GND pair ──► S1 base   (S1 ▲ S2 ▼ S3 ▲)     PSU 1 side
           LED2 (GPIO 3)  ── data + GND pair ──► S4 base   (S4 ▲ S5 ▼ S6 ▲)     PSU 1 side
           LED3 (GPIO 1)  ── data + GND pair ──► S7 base   (S7 ▲ S8 ▼ S9 ▲)     PSU 2 side
           LED4 (GPIO 4)  ── data + GND pair ──► S10 base  (S10 ▲ S11 ▼ S12 ▲)  PSU 2 side
```

LED3 and LED4 drive strips that are powered by PSU 2, while the Dig-Quad runs on PSU 1. That works because the grounds are joined, so the data signal has the same 0 V reference at both ends.

## 2. One output in detail (LED1 → strips 1, 2, 3)

All four outputs are wired the same way.

```
  TOP     +12V/GND from top junction A (5 A fuse per strip)
            │                  │                  │
            ▼                  ▼                  ▼
          ┌────┐  DO→DI   ┌────┐             ┌────┐   end of chain
          │ S1 │──BO→BI──►│ S2 │             │ S3 │   (DO, BO unused)
          │ ▲  │          │ ▼  │             │ ▲  │
          │0–  │          │360–│             │720–│
          │359 │          │719 │  DO→DI      │1079│
          └────┘          └────┘──BO→BI────►└────┘
            ▲                  ▲                  ▲
  BASE      │ +12V/GND from fuse block A (5 A fuse per strip)
            │
  Dig-Quad LED1 data ──► S1 DI
  Dig-Quad LED1 −    ──► S1 GND
  S1 BI ──► S1 GND   (chain start only)
```

- **Data zig-zags:** S1 runs up, S2 down, S3 up. Pixels 0–359, 360–719 and 720–1079 on that output.
- **Jumpers** carry both lines to the next strip: DO→DI **and** BO→BI. S1→S2 at the top, S2→S3 at the base.
- **BI → GND at the chain start only** (the base of S1, S4, S7 and S10).
- **Power goes into both ends of every strip**, each feed through its own 5 A fuse.
- A strip that points down is rotated, so its pad order is mirrored. Check the labels printed on the real strip, because the order varies between makers.

## 3. Connection list

| From | To | Wire | Fuse | Notes |
|---|---|---|---|---|
| PSU 1 V+ | Fuse block A input | 8–10 AWG, short | n/a | Sized for the PSU's full 40 A |
| PSU 2 V+ | Fuse block B input | 8–10 AWG, short | n/a | **Never** joined to PSU 1 V+ |
| PSU 1 V−, PSU 2 V− | Common GND bar | 8–10 AWG | none | Grounds are **always** joined |
| Block A, 2 A way | Dig-Quad `+` input | 18 AWG | 2 A | Board electronics only |
| Dig-Quad `−` input | Common GND bar | 18 AWG | none | Gives the data signal its 0 V reference |
| LED1 data (GPIO 16) | Strip 1 DI, at its base | data + GND pair, keep short | none | LED2 → S4, LED3 → S7, LED4 → S10 |
| LED1–4 `−` terminal | Same strip's GND pad | runs beside its data wire | none | Return path for the data signal |
| LED1–4 V+ terminal | **nothing** | n/a | n/a | Strip power never goes through the board |
| BI at S1, S4, S7, S10 base | GND at the same pad | short link | none | Chain start only. Leaving BI floating there can cause flicker. |
| DO / BO at each strip end | DI / BI of the next strip | jumper | none | S1→S2 at the top, S2→S3 at the base, and so on |
| Block A 5 A ways (×6) | Base `+` of strips 1–6 | 18 AWG pigtails | 5 A each | Block B does the same for strips 7–12 |
| Block A 15 A way | Top bus A → top junction A | 12 AWG pair (10 AWG better), UV-rated | 15 A | The junction has 6 × 5 A to the strip tops. The bus GND returns to the bar. |
| Every strip GND (white) | GND rail at that end | 18 AWG | none | Both ends of every strip |

The **15 A top-bus fuse** and the **2 A controller fuse** are our suggestions to protect those wires. They aren't in the source PDFs. Gauges are preliminary (see [power-and-wiring.md](power-and-wiring.md#wire-gauges-preliminary-recalculate-once-the-physical-layout-is-fixed)).

## Why it's wired this way

- **Data needs a shared 0 V.** The data line is a voltage measured against ground. If the strip's GND and the board's GND aren't connected, the strip can't read the signal. That's why every negative goes to one bar and why each data wire has a GND wire beside it.
- **Join grounds, never V+.** Two supplies never output exactly 12.00 V. Joining their V+ makes one push current into the other. Joining their grounds is safe and gives everything the same reference.
- **Power bypasses the board.** Each PSU feeds about 24 A. The Dig-Quad is rated 30 A for the whole board and 10 A per fuse, and takes only one supply, so the strips are fed from fuse blocks instead.
- **Feeding both ends.** A 6 m strip loses voltage along its thin copper. Feeding the top and the base halves the current in each half, which keeps the far LEDs from going dim and orange at full white.
- **A fuse protects the wire.** Each fuse is sized for the thinnest wire after it. A 5 A fuse on an 18 AWG pigtail blows before the wire overheats if a strip end shorts.
- **BI and the backup line.** WS2815 has a second data line. If one pixel dies, the next one reads BI instead and the rest of the strip keeps working. At the first pixel there's nothing before it, so BI is tied to GND.
