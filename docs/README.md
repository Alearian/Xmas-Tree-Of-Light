# Xmas Tree Lighting — Documentation

## Source documents (PDFs)

| File | What it covers |
|---|---|
| [01-single-page-explanation.pdf](01-single-page-explanation.pdf) | Plain-English overview. xLights (laptop) is "the director": it designs the 3D effects. WLED (ESP32 on the tree) is "the stage manager": it drives the pixels and runs standalone. Phones control it through the WLED app. |
| [02-technical-project-document.pdf](02-technical-project-document.pdf) | Full engineering spec: 5–6 m cone, 12 × 6 m WS2815 strips, 4,320 px, power, rigging, winterisation, 4 build phases. Addenda A–F cover IP ratings, WS2811 vs WS2815, strip wiring, the 4-channel layout, the PSU/enclosure and the bill of materials. |
| [03-software-controller-solution.pdf](03-software-controller-solution.pdf) | Software stack, how sequences reach the controller (live DDP stream vs saved presets), on/off, one-time commissioning (AP setup, xLights cone model). |
| [04-setup.pdf](04-setup.pdf) | Runbook for each day the tree is in use: pre-flight, power on, pick a preset or stream from xLights, operate from the phone, end of night. |

## Our notes

- [design-summary.md](design-summary.md): the design in one place (numbers, layout, target WLED config)
- [hardware.md](hardware.md): the controller as it is **right now**, compared with the target
- [strip-spec-ws2815.md](strip-spec-ws2815.md): the purchased strip (HD 12V WS2815 60L B IP67): 7.2 A per run, 86.4 A for the tree
- [quinled-dig-quad-guide.md](quinled-dig-quad-guide.md): the manufacturer's ratings, pinout, wiring steps and WLED settings (from Quindor's tutorial and quinled.info)
- [power-and-wiring.md](power-and-wiring.md): power and data wiring, reconciling the PDFs with the builder's email
- [open-questions.md](open-questions.md): **conflicts and errors found in the docs**, to settle with the builder
- [software.md](software.md): TreeTool (C#), the phone app and the shared layout, with how to use them
- [roadmap.md](roadmap.md): the build stages, including our own software

## People

