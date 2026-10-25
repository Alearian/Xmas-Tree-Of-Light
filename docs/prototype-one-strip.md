# Prototype: One Strip on the Dig-Quad

The bench prototype is **one WS2815 strip, powered through the Dig-Quad**. One 12 V supply feeds the board, and the board passes that 12 V straight through its fused outputs to the strip. This is the board's normal wiring. Our board is the pre-assembled version, which detects 12 V by itself (no jumper). Power goes in at the start end only. One strip (4–7 A) is within one output's 10 A fuse and the board's 30 A total.

The drawn version is [prototype-one-strip.html](prototype-one-strip.html) (open it in a browser), also published at https://claude.ai/artifact/3Lk67U7RF1XZuYfpbo9jQq.

The final 12-strip build is wired differently, with strip power going around the board: [dig-quad-connections.md](dig-quad-connections.md).

> ⚠️ **Before you connect power**
> - Use a 12 V supply rated **at least 10 A (120 W)**. One strip draws 4–7 A at full white (sources disagree), so a 10 A supply can't be overloaded even with the limiter off.
> - Use a supply with a moulded mains plug or IEC lead. An open-frame PSU with bare mains screw terminals needs a proper enclosure and someone qualified to wire it.
> - Check the strip's printed pad labels. The usual WS2815 colours are red +12 V, **green DI**, **blue BI** and black or white GND, but sellers vary. This strip's sheet says **black = GND**, while the design doc says white. A data/backup swap can still light normally, so check the pads rather than the lights.

## Diagram

```
 Mains ─► 12 V PSU (≥10 A)
            V+ ══14 AWG══► [+ in]  ┐
            V− ══14 AWG══► [− in]  │  QuinLED Dig-Quad
                                   │  12 V passes through the board copper
                                   │  ├─ fuse 1 (LED1+LED2) ─► LED1 +  ──16–18 AWG──► strip START bare red  (+12 V)
                                   │  ├─ GND ────────────────► LED1 −  ──16–18 AWG──► strip START bare black (GND)
                                   │  └─ ESP32 GPIO 16 ──────► LED1 D  ─────────────► strip START JST DI
                                   ┘
 Strip START JST:  DI (green) ← LED1 D  ·  BI (blue) + GND twisted together  ·  thin +12 V wire insulated, unused
 Strip FAR END: JST (DO, BO) and bare power leads all unused, insulate them

 [START, pixel 0] ════════════════ WS2815, 5 m = 300 px (6 m = 360) ════════════════ [FAR END]
```

## Connections

| From | To | Wire | Notes |
|---|---|---|---|
| PSU V+ | Dig-Quad `+` input | 14 AWG, short | One PSU only |
| PSU V− | Dig-Quad `−` input | 14 AWG, short | |
| LED1 `+` | Strip start, bare red lead | 16–18 AWG | 12 V through fuse 1 (shared by LED1 and LED2) |
| LED1 `−` | Strip start, bare GND lead | 16–18 AWG | Black on this strip. Check the label. |
| LED1 data (GPIO 16) | JST DI wire (usually green) | keep under about 2 m | Run it alongside the GND lead |
| JST BI wire (usually blue) | JST GND wire | twist, solder, heat-shrink | BI must not float at the first pixel |
| JST +12 V wire | **nothing** | insulate the end | Too thin for the strip's current. The bare red lead carries it. |
| Far-end JST (DO, BO) | **nothing** | cap or tape | End of the chain |
| Far-end bare red / GND leads | **nothing** | insulate each end separately | Not fed in the prototype. QuinLED says 12 V runs up to about 10 m are usually fine fed from one end. |

**Fuses:** the board's LED1/LED2 fuse should be a **7.5 A** blade (ATO) fuse. Check which fuses are fitted. Don't go above 10 A.

## Power-up order

1. Everything unplugged. Wire the start end only.
2. With a multimeter on continuity, check there's **no beep** between the Dig-Quad `+` and `−` inputs. A beep means a short.
3. Check the polarity at the PSU: V+ to `+`, V− to `−`.
4. Plug in the PSU. After 10–60 s the board joins the home Wi-Fi as `xmastree.local`. If it can't, it opens its own AP `WLED-AP` at `4.3.2.1` instead.
5. Set brightness low from the app, then try colours and effects.

## Measurements (roadmap Stage 1)

1. Solid white, brightness 255, current limiter off.
2. **Current:** clamp meter on the red lead, or a multimeter on its 10 A range in series (wire it in before powering on).
3. **Voltage at the far end** between the bare red and GND leads. Below about 10.8 V (a 10 % drop) shows up as dimmer, pinker white.
4. Over about 4.5 A per strip means the final build needs 3 PSUs instead of 2 ([open-questions.md](open-questions.md) §1).

## WLED settings for the prototype

The board still has the full-tree layout (LED1 = 900 LEDs). The strip shows the first 300 of those, so it works without a change. Two things are worth changing before long tests. Both are config changes, so back up `/json/cfg` to `device-backups/` first.

- **Current limiter:** it's set to 40 A for the full tree. For the prototype, set it to about 80 % of the PSU (8,000 mA on a 10 A supply). Turn it off only for the full-white measurement.
- **LED count (optional):** set LED1 to 300 so effects use the whole strip. TreeTool can put the full layout back afterwards (`apply`).
