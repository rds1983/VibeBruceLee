# ADR-0010: Data-driven hazard families with central damage

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

T-042 adds eight hazard names to the chamber format but only a handful of behaviours: timed beams, one-shot explosions, a sweeping crusher and straight-line projectiles. The rules must be deterministic, testable without a window, reset with the chamber, and damage both Bruce and enemies. Each hazard type also needs distinct activation, warning and despawn timing.

## Decision

1. **One data entry, one hazard object:** `HazardSystem` maps each `hazards` entry to a `Hazard` subclass by `type`. The definition rectangle stays the authoritative placement for beams, bushes and mines.
2. **Behaviour families:** `TimedBeamHazard` handles electric and PAN lights, `BushHazard` and `MineHazard` are one-shot, `MovingWallHazard` sweeps a horizontal range, and `ProjectileHazard` covers dagger/sword/bug/fireball. Types differ only in timing, movement and visuals.
3. **Central damage resolution:** `HazardSystem.Update` checks active hazards against Bruce and every enemy. A player hit calls `Player.Kill` (whole fall, respects invulnerability); an enemy hit calls `Enemy.ApplyHazard` (unscored knockout then normal respawn). Non-active hazards receive `OnOverlap` so mines can arm.
4. **Data additions:** `directionX` is an optional projectile/wall direction with validation to 1 or -1. `speed` and `periodSeconds` keep their existing meanings; a moving wall's rectangle is its travel range and its body is eight pixels wide.
5. **Deterministic timings:** timed beams use a fixed 0.3 s warning and 0.4 s active phase; bushes and mines use fixed warning/fuse/explosion windows. No randomness.
6. **Reset by recreation:** `HazardSystem.Reset` discards consumed hazards and recreates the original set, matching chamber reset on a fall.
7. **Placeholder visuals:** hazards draw as colored rectangles until T-053; active, warning and inactive states use different colors.

## Consequences

- Every hazard family is testable with a synthetic `HazardDefinition`, a real `Player` and real `Enemy` objects.
- Hazard damage bypasses hit points for enemies and costs a whole fall for Bruce, matching the design ruling.
- Projectiles share implementation; sword and fireball behaviour can diverge later without changing the system.
- Large delta-time updates can skip a short active window; the game's fixed 60 Hz step makes this safe, and tests step deliberately.
- The moving-wall convention (rectangle = travel range) should be kept in mind when authoring chambers.

## Alternatives considered

- **One hazard class with a type switch:** less code, but timing and movement logic would become a large conditional.
- **Physics bodies for every hazard:** unnecessary for fixed paths and harder to keep deterministic.
- **Damage inside each hazard:** would duplicate player invulnerability and enemy scoring rules across types.
- **Permanent consumed hazards:** would make deaths in hazard-heavy rooms harsher and break the chamber reset rule.

## Related

- `Design/gameplay.md`
- `Design/chambers.md`
- `Design/architecture.md`
- ADR-0008 (chamber progress and reset)
- `VibeBruceLee/Gameplay/HazardSystem.cs`
