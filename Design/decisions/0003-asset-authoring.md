# ADR-0003: Code-generated art and audio

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

All shipped assets must be original and reproducible from a clean checkout. The project is developed largely by an agent, so the authoring workflow must be scriptable, diffable and free of manual GUI steps. The developer can install software when needed, but avoiding an editor dependency keeps CI and contributors simple.

The options considered were a fully scripted generator, a free pixel editor with export scripts, Aseprite, or a hybrid.

## Decision

1. **Code-driven generation:** sprites, tiles and audio are produced by a small C# console project at `Tools/VibeBruceLee.AssetGen` using StbImageNext.
2. **StbImageNext encoding:** the tool builds RGBA pixel buffers and encodes them with `StbImageNext.Encoders.PngEncoder`. It does not implement its own image codec.
3. **PCM WAV:** audio is generated as 16-bit mono PCM at 22,050 Hz with integer math.
4. **Source of truth:** PNG and WAV files are build artifacts committed for the game to load. Never hand-edit them; edit the generator.
5. **Fonts:** FontStashSharp loads a licensed TTF. The bootstrap font is Press Start 2P (SIL OFL 1.1); the license ships with the game.
6. **Reproducibility gate:** CI regenerates the assets and fails if `git diff` finds changes.
7. **No editor dependency:** no Aseprite, LibreSprite or Piskel is required. An editor can still be introduced later for specific hand-drawn assets if this workflow becomes a bottleneck; that change needs a new ADR.

## Consequences

- A clean checkout can rebuild every generated asset with one documented command.
- Generated art is diff-friendly at the source level and easy for an agent to iterate on.
- Complex animation requires writing pixel-row definitions or small drawing helpers; this is slower than a dedicated pixel editor.
- StbImageNext handles PNG details (filters, compression, CRCs), so the tool stays small.
- Generated binaries are committed; CI regenerates them on Windows and Linux and fails on any difference.
- The tool is not shipped with the game; it is a development dependency only.

## Alternatives considered

- **Aseprite:** excellent for animation, but paid, GUI-driven, and requires an install and license.
- **LibreSprite/Piskel:** free editors, but still GUI-driven; export automation and agent use are awkward.
- **Hybrid:** good long-term, but unnecessary until the scripted workflow proves insufficient.
- **A hand-written PNG encoder:** rejected; StbImageNext already provides a tested encoder and keeps the tool focused on art definitions.

## Related

- `Design/assets.md`
- ADR-0002 (asset pipeline)
- `Tools/VibeBruceLee.AssetGen`
