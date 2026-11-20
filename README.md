# VibeBruceLee

A from-scratch remake of the 1984 classic **Bruce Lee**, as released for the ZX Spectrum: one martial artist, twenty trap-filled chambers, two relentless enemies, and a wizard waiting at the top of the tower. Built with modern C# tooling and original, hand-made assets.

> Status: early development. Twenty playable chambers are in place, from the three-room entrance hub and the underworld chains to the Fire Wizard's chamber, with a title and controls screen, combat, lanterns, hazards, generated sound effects, polished procedural pixel art, falls, game over, an ending, looping difficulty and persistent high scores.

## Tech stack

| Area    | Choice                        |
| ------- | ----------------------------- |
| Language| C# / .NET 8+                  |
| Engine  | MonoGame (DesktopGL)          |
| Text    | FontStashSharp                |
| Assets  | XNAssets                      |
| Tests   | xUnit                         |

## Layout

```
Design/               Design and technical documentation (Markdown)
Tools/                Build-time tools, including the asset generator
VibeBruceLee/         Game source code and csproj
VibeBruceLee/Assets/  All game assets (art, audio, fonts, levels)
VibeBruceLee.Tests/   xUnit tests
VibeBruceLee.slnx     Solution file
README.md             This file
TASKS.md              Living roadmap
AGENTS.md             Agentic development workflow
```

## Build and run

Requires the .NET 10 SDK or newer (the solution uses `.slnx`).

```sh
dotnet build VibeBruceLee.slnx
dotnet run --project VibeBruceLee
dotnet test VibeBruceLee.slnx
```

## Documentation

- `Design/` - game design, architecture, chamber references, asset pipeline and decisions.
- `TASKS.md` - the roadmap. It is a living document, not a fixed contract.
- `AGENTS.md` - how work is selected, implemented, tested, reviewed and committed.

## Assets

Art and audio are generated from code by `Tools/VibeBruceLee.AssetGen`; fonts and licenses live alongside the generated files. Regenerate from the repository root:

```sh
dotnet run --project Tools/VibeBruceLee.AssetGen -- VibeBruceLee/Assets
```

All art, audio and fonts are created for this project or used under a compatible license. The original game is used only as a visual and behavioral reference; no original assets are redistributed. Third-party assets (for example fonts) keep their licenses in `VibeBruceLee/Assets/licenses/`.

## License

MIT - see [LICENSE](LICENSE).
