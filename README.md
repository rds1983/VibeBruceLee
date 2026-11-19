# VibeBruceLee

A from-scratch remake of the 1984 ZX Spectrum classic **Bruce Lee**: one martial artist, twenty trap-filled chambers, two relentless enemies, and a wizard waiting at the top of the tower. Built with modern C# tooling and original, hand-made assets.

> Status: early development. The repository currently contains design and process documentation only.

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

All art, audio and fonts are created for this project. The original game is used only as a visual and behavioral reference; no original assets are redistributed. Third-party assets (for example fonts) keep their licenses in `VibeBruceLee/Assets/Licenses/`.

## License

MIT - see [LICENSE](LICENSE).
