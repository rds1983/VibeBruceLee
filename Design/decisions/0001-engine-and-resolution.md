# ADR-0001: MonoGame, a fixed 256x192 canvas, and 60 Hz simulation

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

We are rebuilding a 1984 single-screen arcade-platformer with the requested stack: C#, MonoGame, FontStashSharp for text, and XNAssets for assets. The original ran at 256x192 on a ZX Spectrum, with no scrolling per chamber. The remake must be cross-platform, pixel-perfect, testable without a graphics device, and easy to package.

We need decisions on the engine variant, logical resolution and scaling, timestep, and asset loading.

## Decision

1. **Engine:** MonoGame DesktopGL, targeting `net8.0`. The solution is built with .NET SDK 10 (pinned in `global.json`) because `.slnx` requires SDK 9 or newer.
2. **Logical resolution:** 256x192, matching the Spectrum. All gameplay and art use this coordinate space.
3. **Presentation:** render to a 256x192 back buffer and scale with nearest-neighbor filtering by the largest integer factor that fits the window, letterboxing the remainder. Default window is 1280x960 (5x).
4. **Simulation:** fixed 60 Hz timestep. Rendering may interpolate later; gameplay updates are deterministic and independent of frame rate.
5. **Assets:** XNAssets loads raw textures, fonts and level data from a copied `Assets` folder. The MonoGame content pipeline is not used.
6. **Text:** FontStashSharp renders all UI and text.
7. **Tests:** xUnit tests target pure logic. Physics, AI, chamber parsing and scoring must not depend on MonoGame graphics types.

## Consequences

- Pixel art snaps to whole pixels at every integer scale; no shimmering from fractional scaling.
- The 256x192 constraint limits on-screen detail but guarantees fidelity to the original look.
- No content pipeline means assets are copied as-is and loaded by path. Hot reload and asset validation are our responsibility (T-004).
- Deterministic fixed-step logic enables headless simulation tests and replay tests (T-060).
- Targeting `net8.0` keeps MonoGame compatibility while the SDK stays current. Retargeting later is a contained change.
- Window resizing and fullscreen must be handled by the presentation layer (T-010), not by changing the logical canvas.

## Alternatives considered

- **MonoGame WindowsDX:** Windows-only, so rejected for cross-platform parity.
- **FNA / SDL:** viable, but not the requested stack and adds integration work.
- **Native 1280x960 sprites:** wastes memory and risks inconsistent scaling.
- **Variable timestep:** simpler to start, but makes physics, AI and tests non-deterministic.

## Related

- `Design/architecture.md`
- `Design/gameplay.md`
- ADR-0002 (asset pipeline, T-004)
