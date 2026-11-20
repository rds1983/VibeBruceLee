# Debug tooling

Debug features exist only in `DEBUG` builds. Release builds do not create the debug controller or overlay, so the keys have no effect. Start a debug build with the overlay visible:

```sh
dotnet run --project VibeBruceLee --configuration Debug -- --debug-overlay
```

## Keys

| Key | Action |
| --- | ------ |
| `F1` | Toggle overlay |
| `F3` | Toggle slow motion (simulation runs at 1/4 speed) |
| `F4` | Pause/resume simulation |
| `F5` | Pause and advance exactly one 1/60 s frame |
| `F6` | Toggle invincibility flag |
| `F7` | Request chamber skip |
| `F2` | Sound toggle (normal binding) |
| `F11` | Fullscreen (normal binding) |

## Overlay

The overlay draws on the 256x192 canvas, so it scales with the game. It shows:

- smoothed FPS and frame count,
- the current screen type,
- run state (`RUN`, `SLOW 1/4`, `PAUSED`), invincibility and rectangle count,
- the debug controls.

Debug rectangles use color coding:

| Color | Meaning |
| ----- | ------- |
| Red | Hitbox |
| Green | Hurtbox |
| Blue | Solid geometry |

Screens register rectangles through `ScreenContext.Debug` while drawing. The chamber screen registers platform solids, Bruce's hurtbox and attack box, and every visible enemy's hurtbox and attack box.

## Pause and slow motion

- Pausing stops screen updates but keeps drawing and reading debug input.
- A frame step runs one simulation update with a fixed 1/60 s delta.
- Slow motion scales the `GameTime` passed to screens to 1/4. Input edge detection still runs every real frame.
- The game still presents every frame while paused.

## Cheat flags

`DebugState` is always compiled so gameplay can read it, but only debug builds can change it:

- `Invincible` - set by F6; gameplay checks it when applying damage (T-022).
- `SkipChamberRequested` - set by F7; chambers consume it with `ConsumeSkipChamber` (T-040).

Release builds leave both flags false.

## Extending

Add debug-only keys through `InputAction`/`InputBindings` and handle them in `DebugController`. Add new box categories to `DebugState`; never ship debug-only text or cheats in the normal release flow.
