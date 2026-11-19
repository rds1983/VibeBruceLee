# Architecture

## Solution layout

| Project | Purpose |
| ------- | ------- |
| `VibeBruceLee` | Game executable. MonoGame DesktopGL, `net8.0`, nullable enabled. |
| `VibeBruceLee.Tests` | xUnit tests. References the game project; tests must not create a graphics device. |

`VibeBruceLee.slnx` contains both projects. The game's `Assets/` folder is the runtime asset root and is copied to the output directory with `PreserveNewest`.

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

Architecture decision records live in `Design/decisions/`. ADR-0001 (engine and logical resolution) is added in T-003; ADR-0002 (asset pipeline) in T-004.
