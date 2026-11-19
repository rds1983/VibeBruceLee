# Overview

VibeBruceLee is a from-scratch remake of the 1984 platform-fighting game **Bruce Lee**. The player guides Bruce through twenty single-screen chambers of a wizard's fortress, collecting lanterns to open exits, fighting a relentless ninja and the Green Yamo, avoiding deadly traps, and finally confronting the Fire Wizard. Completing the tower loops the game with harder rules.

The remake keeps the original's compact, readable rules and fast, punchy action, while adding modern input support, deterministic logic, automated tests, and a reproducible asset pipeline. All art, audio and fonts are original; the 1984 game is a behavioral and visual reference only.

## Goals

- Faithful core: twenty chambers, lantern-gated exits, the same enemies, hazards, scoring and loop.
- Immediate, responsive controls with keyboard and gamepad support.
- Local two-player modes: control Yamo, or alternate control of Bruce.
- Pixel-perfect presentation on a 256x192 logical canvas at a fixed 60 Hz.
- Testable game logic that runs headless, separate from rendering.
- Clean, warning-free cross-platform build (Windows and Linux).

## Non-goals

- Online multiplayer, mobile, or console ports.
- New story content, characters, or chamber themes beyond the original structure.
- Ripped or traced original assets. Layouts and behavior may be studied; art and audio are recreated.
- Modding tools or a level editor in v1.0 (possible stretch goals).

## Pillars

1. **Fidelity first.** When the original and a modern convention disagree, the original wins unless it is unplayable.
2. **Readable action.** Every attack, hazard and exit is telegraphed; deaths feel earned.
3. **Fair pressure.** Two enemies should create tension, not stun-locks or unavoidable damage.
4. **Small and testable.** Chambers are data; physics, AI and scoring are deterministic and unit-tested.

## Target

| Area | Target |
| ---- | ------ |
| Platforms | Windows and Linux desktop |
| Resolution | 256x192 logical, integer-scaled, letterboxed |
| Frame rate | Fixed 60 Hz simulation and render |
| Players | 1 player, or 2 players locally |
| Session | 20 chambers + Fire Wizard, then looping difficulty |
| Language | C# on .NET, MonoGame DesktopGL |

## Success criteria for v1.0

- All 20 chambers are playable, beatable, and visually distinct.
- The wizard can be defeated, the ending plays, and the game loops with increased difficulty.
- One-player and both two-player variants work.
- Scoring, extra lives, high scores, pause, sound toggle and game over behave correctly.
- The full test suite and CI are green, and a clean machine can build and play.
