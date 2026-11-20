# ADR-0008: Chamber catalog and per-attempt progress

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

T-040 needs chamber-01 to run as a complete slice: lanterns must be collectible, the exit must unlock, entering it must load the next chamber, and a fall must reset the chamber without resetting the run. The game previously loaded a single chamber into `ScreenContext` and `ChamberScreen` hard-coded its HUD title. Chamber definitions are immutable data, while lantern collection and death resets are per-attempt state that must be testable without a window.

## Decision

1. **Catalog, not a single chamber:** `BruceLeeGame` loads every chamber with `ChamberLoader.LoadAll`, wraps them in `ChamberCatalog`, and stores the catalog in `ScreenContext`. `First` starts a run; exit targets are resolved by id.
2. **Screen owns the attempt:** `ChamberScreen` creates one `ChamberProgress` per visit. Progress is not stored on the immutable `ChamberDefinition` and does not leak between chambers.
3. **Headless progress rules:** `ChamberProgress` owns lantern hitboxes, required/optional counts, exit gating and reset. It uses `Aabb` only, so tests do not need MonoGame graphics.
4. **Session owns persistent run state:** score increase, falls, extra-fall thresholds and game-over state stay in `GameSession`. A death resets the chamber but never the session score or falls.
5. **Chamber transition by replacement:** entering an open exit awards 2,000 points and replaces the current screen with a new `ChamberScreen` for the target. The title screen is not kept underneath an active run.
6. **HUD reflects progress:** required lantern count and `EXIT OPEN` come from `ChamberProgress`; the chamber title comes from the definition name.

## Consequences

- Lanterns and enemies reset on each fall while score is preserved, matching the reference's attempt structure.
- Re-entering a chamber starts a fresh attempt; hub-wide lantern persistence in T-041 will need an explicit hub state because the default is per-visit.
- The exit gate, optional lantern scoring and reset behavior are unit-tested without a window.
- Chamber-02 is reachable through the data graph but remains a placeholder layout until T-041.
- Death and exit transitions are simple screen-level conditionals; their building blocks are tested separately.

## Alternatives considered

- **Mutable chamber definitions:** simpler at first, but shared state would leak between screen instances and make tests order-dependent.
- **Progress stored in `GameSession`:** mixes per-chamber and per-run state; a death would need special cases to clear only part of it.
- **Single-screen reload:** cannot support the fortress graph and would make T-041 rewrite the flow again.
- **A separate chamber-run service:** viable, but the screen already owns the player, enemies and world; a small progress value object keeps the refactor contained.

## Related

- `Design/gameplay.md`
- `Design/chambers.md`
- `Design/architecture.md`
- `VibeBruceLee/Gameplay/ChamberProgress.cs`
- `VibeBruceLee/Levels/ChamberCatalog.cs`
