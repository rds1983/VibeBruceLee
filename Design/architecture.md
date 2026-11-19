# Architecture

## Solution layout

| Project | Purpose |
| ------- | ------- |
| `VibeBruceLee` | Game executable. MonoGame DesktopGL, `net8.0`, nullable enabled. |
| `VibeBruceLee.Tests` | xUnit tests. References the game project; tests must not create a graphics device. |

`VibeBruceLee.slnx` contains both projects. The game's `Assets/` folder is the runtime asset root and is copied to the output directory with `PreserveNewest`.

## Toolchain

- The SDK is pinned through `global.json` to .NET 10 (`latestFeature` roll-forward). The `.slnx` solution format requires SDK 9 or newer, and the README asks for SDK 10.
- The game targets `net8.0` for MonoGame compatibility; SDK 10 builds and runs it.
- `Directory.Build.props` enables nullable reference types, implicit usings, .NET analyzers at the recommended level, and treats warnings as errors.
- Formatting and style defaults live in `.editorconfig`: C# uses tab indentation; XML, JSON and YAML use two spaces.
- CI runs restore, Release build, and tests on Windows and Linux (`.github/workflows/ci.yml`).

## Host

`BruceLeeGame` is the MonoGame `Game` host:

- fixed timestep at 60 Hz (`TargetElapsedTime = 1/60`),
- back buffer of 1280x960, which is exactly 5x the 256x192 logical canvas,
- vertical sync enabled,
- Escape or gamepad Back exits.

Rendering is intentionally minimal in this task: the device is cleared to a dark color. The virtual canvas, integer scaling, letterboxing and the screen stack arrive in T-010.

## Assets

The project does not use the MonoGame content pipeline. Raw assets are copied from `VibeBruceLee/Assets` to `Assets` next to the executable and will be loaded through XNAssets (T-004). Fonts are rendered through FontStashSharp.

## Testing

- `dotnet build VibeBruceLee.slnx`
- `dotnet test VibeBruceLee.slnx`
- `dotnet run --project VibeBruceLee`

Unit tests target pure logic and type-level smoke checks. Anything requiring a window is verified manually and, later, through headless simulation (T-060).

## Decisions

Architecture decision records live in `Design/decisions/`. ADR-0001 records the engine, logical resolution and timestep; ADR-0002 (asset pipeline) is added in T-004.
