# Xmas Tree Lighting — Documentation

## Start here

- [user-guide.md](user-guide.md): **how to use the phone app**, every screen with screenshots
- [build.md](build.md): **how the tree is built**, with photos, and links to every wiring guide and parts list

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
- [bom.md](bom.md): shopping list for the final tree (5 m strips, bottom-fed)
- [bom-checklist-no-prices.md](bom-checklist-no-prices.md): the parts checklist: what was bought
- [tree-wiring-build-guide.md](tree-wiring-build-guide.md) / [.pdf](tree-wiring-build-guide.pdf): step-by-step wiring build instructions (2 supplies, 6 strips each)
- [tree-wiring-3d.html](tree-wiring-3d.html): 3D view of the strips, cables and box
- [power-and-wiring.md](power-and-wiring.md): power and data wiring, reconciling the PDFs with the builder's advice
- [prototype-one-strip.md](prototype-one-strip.md): the bench prototype: one strip powered through the Dig-Quad, with test steps. The drawn version is [prototype-one-strip.html](prototype-one-strip.html).
- [dig-quad-connections.md](dig-quad-connections.md): **the final build**: the 12-strip connection diagram: what goes to each Dig-Quad terminal, PSU, fuse block and strip, with a wire and fuse list. The drawn version is [dig-quad-wiring.html](dig-quad-wiring.html).
- [open-questions.md](open-questions.md): **conflicts and errors found in the docs**, to settle with the builder
- [software.md](software.md): TreeTool (C#), the phone app and the shared layout, with how to use them
- [roadmap.md](roadmap.md): the build stages, including our own software
- [shows-and-effects-plan.md](shows-and-effects-plan.md): the plan behind the show builder and the finger-friendly effect list (now built; see the user guide)
