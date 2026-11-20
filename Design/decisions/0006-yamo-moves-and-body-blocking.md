# ADR-0006: Yamo move selection and body blocking

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

The Green Yamo needs to feel slower and heavier than the Ninja, with both a short punch and a punishing crushing kick. The reference also describes enemies hurting each other when one is in the other's way, and the wrestler's large body should read as a wall rather than another sprite the player can walk through. The shared enemy controller has one attack slot and the player has no collision response against bodies.

## Decision

1. **Two attacks on one controller:** `EnemyDefinition` gains an optional secondary attack, its range, its cooldown and a preferred stand-off range. It does not gain a Yamo-specific subclass.
2. **Distance-based selection:** inside 24 px the Yamo punches (6/3/8 frames, 45-frame cooldown); from 25 to 38 px he uses the crush kick (12/4/14 frames, 100-frame cooldown, knockdown). The chosen attack stores its own cooldown for when recovery ends.
3. **Telegraphed heavy move:** the kick leans forward slowly, has a long wind-up and recovery, and sets the shared timer to 100 frames. Whiffing it is the main punish window.
4. **Body blocking without dynamic bodies:** the Yamo is marked `BlocksMovement`. After the player moves, the chamber resolves overlap by pushing Bruce out of the Yamo's AABB to the nearer side and clearing his horizontal velocity. Yamo is not moved. This keeps `PhysicsWorld` deterministic and unchanged, and avoids coupling enemy AI to player physics.
5. **No explicit guard stance:** the reference does not describe a passive block, so "blocking presence" is implemented as physical obstruction. A future guard or armour mechanic can be added with its own state and art.

## Consequences

- The Yamo feels like a wall and cannot be walked through, while the Ninja remains a lighter, evasive pursuer.
- Punching at close range and kicking at medium range gives the player a readable distance game.
- Pushing Bruce out can briefly cancel his horizontal input when he walks into the Yamo; that is the intended block.
- A landing-on-enemy move and enemy-versus-enemy collision remain future work (T-033).
- The kick's 100-frame cooldown makes it punishing rather than spammy; T-055 can tune the exact values.

## Alternatives considered

- **Separate Yamo class:** duplicates the state machine and navigation for a handful of values.
- **Passive guard that negates attacks:** not supported by the reference and risks making the Yamo invulnerable for long stretches.
- **Dynamic body in `PhysicsWorld`:** more general, but adds moving-body resolution to a system built for static platforms and complicates determinism and tests.
- **Knockback on contact instead of blocking:** makes the wrestler feel like a hazard rather than a wall and can throw the player into other hazards unfairly.

## Related

- `Design/gameplay.md`
- `Design/architecture.md`
- ADR-0004 (enemy navigation and randomness)
- `VibeBruceLee/Gameplay/EnemyDefinition.cs`
- `VibeBruceLee/Gameplay/Player.cs`
