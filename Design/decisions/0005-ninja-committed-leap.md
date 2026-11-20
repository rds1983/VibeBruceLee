# ADR-0005: Ninja leaps use a committed arc

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

T-030 gave enemies deterministic platform/vine pathing, but the Ninja must also jump at Bruce and cross gaps. A jump has to remain readable and dodgeable at a fixed 60 Hz, and it must not turn chasing into an unfair homing attack. Chambers are one screen wide, so a small, predictable leap is preferable to general airborne pathfinding.

## Decision

1. **Fixed arc:** a leap launches with 300 px/s upward and 110 px/s toward the target, producing roughly a 73 px horizontal reach and 40 airborne frames under the shared 900 px/s² gravity.
2. **Telegraph and commitment:** a 12-frame ground windup precedes launch. The direction and target distance are fixed at launch; the Ninja does not steer or track Bruce in the air.
3. **Limited vertical use:** the Ninja only leaps when the target is at most 24 px above it. Higher routes remain the job of vines, so a leap cannot phase through a solid platform or climb arbitrary walls.
4. **Decision window:** a leap is allowed at 23-80 px. On the same surface it is an aggressive medium-range option; on another surface it is used only when `EnemyNavigator` finds no walking, vine or drop route.
5. **Airborne strike:** `NinjaJumpAttack` is active for the middle of the arc (8 startup, 14 active, 12 recovery frames) and hits once. The leap cooldown is 90 frames after landing; the bokken cooldown remains 45 frames.
6. **First-pass art:** the 12-frame windup uses the idle frame and the arc uses run/attack frames until T-053 supplies dedicated jump art.

## Consequences

- The leap is visible, deterministic and dodgeable by moving out of the landing zone.
- It traverses the sample chamber's 64 px platform gaps and can jump at Bruce on open ground.
- A fixed arc can miss a moving target and cannot climb high ledges; that is intentional for this enemy.
- Later difficulty loops can shorten the windup or cooldown (T-046) without changing the state machine.
- A future airborne art pass should keep the same frame timing so tests and gameplay stay aligned.

## Alternatives considered

- **Homing leap:** tracks Bruce in the air, but feels unfair and removes the dodge window.
- **Physics AI with air control:** more flexible, but makes attack range and difficulty hard to reason about and test.
- **Scripted jump-link graph:** useful for guaranteed platform-to-platform routes, but overkill for one screen and can be added later if chambers demand it.
- **Full tile A*:** duplicates the platform graph and does not solve the attack-timing problem.

## Related

- `Design/gameplay.md`
- `Design/architecture.md`
- ADR-0004 (enemy navigation and randomness)
- `VibeBruceLee/Gameplay/Enemy.cs`
- `VibeBruceLee/Gameplay/EnemyDefinition.cs`
