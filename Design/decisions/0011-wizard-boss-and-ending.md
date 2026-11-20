# ADR-0011: Wizard boss, aimed fireballs and the ending

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

T-045 needs a final chamber that is different from the lantern/exit loop: a static wizard, fireballs from his eyes, a destruction button, a defeat sequence and an ending. The existing hazard framework only creates hazards from chamber data and the projectile hazard moves on a fixed axis. The chamber schema also requires an exit, which a boss room should not need.

## Decision

1. **Boss data:** `ChamberDefinition` gains `Boss` and a `WizardDefinition` containing the wizard and button rectangles plus the fireball period. Boss rooms may have no exits; normal chambers still require one.
2. **Separate boss controller:** `WizardBoss` owns the fire cycle, warning window, alternating eyes, button check and defeat timer. It is not an `Enemy` because it does not move, chase, take hit points or respawn.
3. **Dynamic aimed fireballs:** `AimedFireballHazard` extends the hazard family with a velocity vector. `WizardBoss` adds new instances to the chamber's `HazardSystem`, so player damage, despawn and drawing use the same path as every other hazard.
4. **Telegraph and fairness:** 1.4 s cycle, 0.3 s warning flash, 90 px/s aimed at the player's centre at spawn. The player can see the eyes flash and move clear. Fireballs are cleared when the button is pressed.
5. **Button interaction:** overlap plus `Attack`. It awards 3,000 points, stops fireballs and starts a two-second defeat timer. It is not a hit-point fight.
6. **Ending flow:** after the timer, `EndingScreen` displays the result and returns to `TitleScreen` on Confirm. T-046 will insert loop handling before the title return.

## Consequences

- The final chamber is data-defined and the boss/button rectangles are validated with the rest of the chamber.
- Fireballs reuse hazard damage and despawn rules instead of duplicating collision code.
- The boss cannot be damaged by attacks; the button is the only win condition.
- Placeholder rectangles represent the wizard until T-053; timing and behaviour are final enough for tests.
- The ending currently returns to the title rather than starting loop 2; that belongs to T-046.

## Alternatives considered

- **Make the wizard an `Enemy`:** would pull in navigation, health, hitstun and respawn code that the stationary boss does not use.
- **Static fireball hazards only:** cannot aim at Bruce and cannot react to his position.
- **Attack the wizard until health reaches zero:** simpler, but loses the destruction-button fantasy from the reference.
- **Require an exit from the boss room:** would leave a redundant exit or force a fake target.

## Related

- `Design/gameplay.md`
- `Design/chambers.md`
- `Design/architecture.md`
- ADR-0010 (hazard framework)
- `VibeBruceLee/Gameplay/WizardBoss.cs`
- `VibeBruceLee/Gameplay/AimedFireballHazard.cs`
