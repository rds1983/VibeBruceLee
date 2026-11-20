# ADR-0007: Enemy encounter pacing and interactions

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

Two enemies pursue Bruce at once. With independent attack timers they could start attacks on the same frame, trade hits through his invulnerability window, and stack on the same spot. The reference also allows enemies to hurt each other when one is in the other's way. Those rules need to be deterministic, testable without a window, and shared by every chamber instead of living in `ChamberScreen`.

## Decision

1. **One controller:** a headless `EnemyEncounter` owns the player/enemy update order, hit resolution, friendly fire, pacing and separation. `ChamberScreen` only draws, handles falls and runs the HUD.
2. **Single attack token:** any enemy in `Attack` or `Jumping` holds the token. Other enemies may still chase and reposition but cannot start an attack or leap. An attacker starting during its update takes the token immediately for the rest of that frame.
3. **Pressure gate:** after an enemy hits Bruce, a 30-frame counter blocks new enemy attacks and leaps. The hit still uses Bruce's existing 20-frame invulnerability. This prevents two enemies from alternating hits through the window.
4. **Friendly fire before the player:** an enemy hitbox checks other enemies first. A friendly hit awards no score and can stun or knock down the victim; it consumes the attack's single hit.
5. **Validity gate:** `Enemy.CanBeHit` rejects respawning, knocked-down and invulnerable enemies before a hitbox is tested, so an attack is not wasted and score is not awarded for a hit that `ApplyHit` would ignore.
6. **Weighted separation:** overlapping grounded enemies are pushed apart by two passes, weighted by body width, so the wider Yamo yields less than the Ninja. Enemies that are climbing, stunned, knocked down or airborne are skipped to avoid breaking vines and hit reactions.
7. **No contact damage:** body overlap only separates. Hazard damage to enemies is part of T-042.

## Consequences

- Fights stay readable and deterministic; a second enemy cannot attack while the first is committed.
- The 30-frame pressure gate gives Bruce a short breathing room after every hit, at the cost of slightly slower enemy pressure.
- Friendly fire can knock out an enemy without awarding points, matching the reference's "in the way" behavior.
- Separation is positional, not physical: it can push an enemy toward a ledge or leave a partial overlap when both are clamped at a screen edge. T-042 hazards and later tuning can revisit this.
- The coordinator adds one allocation-free object per chamber and no new rendering work.

## Alternatives considered

- **Independent enemies with longer cooldowns:** simple, but simultaneous attacks and stun-locks remain possible.
- **Randomized attack delays per enemy:** less readable and harder to test deterministically.
- **Dynamic bodies in `PhysicsWorld`:** would solve separation and contact damage generally, but adds moving-body resolution to the deterministic platform physics.
- **No friendly fire:** simpler, but loses a reference behavior and makes an enemy standing in the way harmless.

## Related

- `Design/gameplay.md`
- `Design/architecture.md`
- ADR-0004 (enemy navigation and randomness)
- ADR-0006 (Yamo moves and body blocking)
- `VibeBruceLee/Gameplay/EnemyEncounter.cs`
