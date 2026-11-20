# ADR-0004: Enemy navigation and deterministic randomness

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

T-030 needs enemies that chase Bruce across the chamber, use vines, and behave identically for the same inputs. The game already uses a fixed 60 Hz step and deterministic physics, but enemy decisions need randomness for patrol direction and idle variation. `System.Random` is seeded but its algorithm is an implementation detail and may change between runtime versions, which would break replays and golden tests.

Pathing also has to stay small: chambers are single screens with one-way platforms, solid blocks and a few vines, not a large tile map.

## Decision

1. **Small xorshift32 generator:** `SeededRandom` provides `NextUInt`, `NextInt`, `NextFloat` and `NextBool` with a sequence fully defined in project code. Enemies receive one instance and use it only for cosmetic/decision variation, never for physics.
2. **Surface graph over platform tops:** each platform top is a navigation node. Nodes connect when they are on the same level and touch, when a vertical vine touches both surfaces, or when a higher surface can drop onto a lower one. Horizontal vines are excluded until a chamber needs them.
3. **Breadth-first search:** routes are the shortest number of graph edges, using insertion order as the deterministic tie-break. Returned waypoints are walk, climb and drop instructions; enemies do not run A* or physics-based pathfinding.
4. **Direct vine movement:** while climbing, an enemy moves along the vine axis with `ClimbSpeed` and snaps onto the destination platform. This avoids the physics rule that detaches a body once its center leaves the vine rectangle, which would otherwise abort a climb near the vine top.
5. **AI depends on a target position, not the player class:** `Enemy.Update(Vector2? targetFeet, ...)` keeps the state machine testable headless and reusable for two-player AI takeover in T-054.

## Consequences

- Enemy replays and tests are stable across .NET versions and operating systems.
- Routes are cheap to recompute and easy to debug, but cannot solve layouts that need mid-air jumps between disconnected platforms. The Ninja's jump attacks (T-031) can extend the graph later.
- Drop edges are one-way, so enemies can leave a high platform without a vine but cannot climb back without one.
- A chamber that relies on horizontal-vine pursuit needs a new navigation action; the current sample chambers use vertical vines only.

## Alternatives considered

- **`System.Random`:** simpler, but its output is not guaranteed stable across runtimes, so it cannot back deterministic tests.
- **A* over a tile grid:** more general, but heavier than needed for one-screen chambers and would duplicate the physics geometry.
- **Spline or jump-link navigation:** better for acrobatic pursuit, but it belongs with the Ninja's jump behaviour in T-031.
- **Steering without a graph:** cannot reliably decide which vine leads upward; enemies would pace under platforms instead of climbing.

## Related

- `Design/gameplay.md`
- `Design/architecture.md`
- `VibeBruceLee/Gameplay/Enemy.cs`
- `VibeBruceLee/Gameplay/EnemyNavigator.cs`
- `VibeBruceLee/Gameplay/SeededRandom.cs`
