# ADR-0012: Loop progression and scaled difficulty

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

The original loops after the wizard, carrying score and falls into a harder pass. The remake needs the same without duplicating enemy or hazard definitions. Loop state must survive chamber transitions and be testable headlessly.

## Decision

1. **Loop lives in the session:** `GameSession.Loop` starts at 1, `AdvanceLoop` increments it and clears hub progress, and `StartNewRun` resets it. Score, falls and extra-fall progress carry across loops.
2. **Definition copies, not mutable tuning:** `EnemyScaling.Apply` returns a `with` copy of the base `EnemyDefinition`. It raises run/patrol/climb speed by 15% per loop (cap 2x), lowers attack/jump/idle delays by 10% per loop (floor 0.5x), raises sight by 10% (cap 1.5x) and sets `RespawnFrames = 0`.
3. **Immediate respawns:** `Enemy.BeginRespawn` calls `Respawn` directly when the definition has no respawn delay. The scaled definition is what makes loop-2 enemies return immediately after knockdown.
4. **Loop hazards are chamber data:** `ChamberDefinition.LoopHazards` uses the same schema as `Hazards`. `HazardSystem` appends them from loop 2 and recreates them on reset. Chamber-16 (the electrified comb) fills its ground-lane gaps, leaving the platform route as the reliable path.
5. **Ending continues the loop:** `EndingScreen` shows the cleared loop, and Confirm calls `AdvanceLoop` then starts chamber-01 through the catalog. The title is not forced; a new run starts only from the title after game over or a fresh launch.
6. **HUD visibility:** from loop 2 the chamber HUD prints `LOOP n`.

## Consequences

- Every loop uses the same data; only the transient enemy definitions and hazard list change.
- Scaling is deterministic and easy to test without graphics.
- Immediate respawns are a single `RespawnFrames = 0` rule rather than a separate enemy mode.
- The hub resets each loop so its lanterns must be collected again.
- The wizard's fireball cadence is not loop-scaled yet; the boss remains a fixed challenge while normal enemies accelerate.
- Difficulty caps prevent unbounded speed growth in very high loops; later loops can add new modifiers if needed.

## Alternatives considered

- **Global static multipliers:** simple, but hidden coupling and hard to keep deterministic in tests.
- **Separate loop-specific enemy definitions:** duplicates data and drifts from the base tuning.
- **Persistent hub progress across loops:** would let players skip the entrance hub after the first loop; the reference resets the fortress.
- **Loop hazards hard-coded by chamber id:** would put level knowledge in code; data lists keep it authorable.

## Related

- `Design/gameplay.md`
- `Design/chambers.md`
- `Design/architecture.md`
- ADR-0008 (chamber catalog and progress)
- ADR-0009 (hub progress)
- ADR-0010 (hazard framework)
- `VibeBruceLee/Gameplay/EnemyScaling.cs`
- `VibeBruceLee/Gameplay/GameSession.cs`
