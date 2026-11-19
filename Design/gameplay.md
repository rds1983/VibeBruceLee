# Gameplay

The rules the remake implements. `reference.md` records what the original does; this document records what we build and where we intentionally differ. Numeric tuning values live in `Design/tuning.md` (T-055).

## Core loop

Clear a chamber's required lanterns, reach the opened exit, and move deeper into the fortress. Fight or avoid the ninja and Yamo, survive the traps, and reach the Fire Wizard in chamber 20. Defeat him to finish the loop, then play again at higher difficulty. Losing all falls ends the run.

## Controls

Default bindings. Remapping and the final table land in T-011.

| Action | Keyboard | Gamepad |
| ------ | -------- | ------- |
| Move | Arrow keys or `O`/`P`/`Q`/`A` | Left stick / D-pad |
| Jump | `X` or `Space` | A |
| Climb up | `Q` or `Up` | Stick up / D-pad up |
| Duck | `A` or `Down` | Stick down / D-pad down |
| Attack | `Z` or `M` | X |
| Pause | `Enter` | Start |
| Confirm | `Enter` | A |
| Cancel / quit | `Esc` | Back |
| Sound toggle | `F2` | Y |
| Fullscreen | `F11` | — |

- Movement keys mirror the Spectrum original; modern keys are aliases.
- Attack is contextual, as in the original: standing = chop, running = kick, airborne = flying kick.
- Up climbs vines; jumping is `X`/`Space`.
- Jump and attack presses are buffered for 120 ms so a slightly early input still lands.
- Fullscreen and integer scaling are presentation settings, not gameplay actions.

## Movement

- Bruce runs rather than walks. Acceleration and deceleration are short so direction changes feel immediate.
- Jumping is variable-height: releasing the jump button early produces a shorter hop. This is a deliberate modern-feel addition.
- A jump combined with a direction produces the original diagonal leap between ledges.
- Ducking lowers the hurtbox and stops horizontal movement.
- Vines can be climbed up and down; horizontal vines allow sideways movement. Bruce can drop off a vine at any time.
- Falling from any height is safe, matching the original. Screen edges clamp the player.
- No fall damage and no bottomless pits unless a chamber explicitly defines one.

## Combat

### Bruce

| Move | Input | Hits | Notes |
| ---- | ----- | ---- | ----- |
| Chop | Attack while standing (grounded) | 1 | High score value |
| Kick | Attack while running | 1 | Longer reach, lower score |
| Flying kick | Attack while airborne | 1 | Strongest knockback; scored as a kick |
| Land on enemy | Fall onto an enemy | — | 50 points and a knockdown; does not consume a hit point |

- Each attack has wind-up, active and recovery frames. Hitboxes are active only during the active frames.
- One attack can hit a given target at most once.
- Attacks cannot be canceled into another attack before recovery ends.
- Landing on an enemy causes a knockdown but no hit-point damage.

### Enemy durability and knockouts

| Enemy | Hit points | Knockout bonus |
| ----- | ---------- | -------------- |
| Ninja | 2 | 200 |
| Yamo | 3 | 450 |

- The final hit awards both the hit score (chop or kick) and the knockout bonus.
- A knocked-out enemy is removed for a short respawn delay in loop 1 and returns immediately from loop 2.
- Enemies can damage each other with attacks, collisions and hazards. The player earns no points when a hazard knocks an enemy out.

### Taking damage

- Bruce has three blows per fall. Each blow causes hitstun, knockback and brief invulnerability; the third blow causes a fall.
- Enemy attacks and enemy body contact during an attack deal one blow.
- Environmental traps cost a whole fall immediately. This is a remake ruling; revisit in T-055 if it feels unfair.
- After a fall, Bruce respawns at the chamber entrance with full blows and brief invulnerability. The current chamber resets: lanterns, enemies and hazards return to their initial state. Score and remaining falls are kept.

## Falls, game over and extra falls

- The original calls lives "falls". The HUD shows falls in reserve.
- One-player runs start with five falls.
- Two-player runs use the larger reserve reported by CRASH (5-10 depending on mode); the exact split between players is defined in T-054.
- Every 30,000 points awards one extra fall.
- At zero falls and one more death, the run ends and the high-score flow starts.

## Lanterns and exits

- Lanterns hang from the ceiling and are collected by jumping into them. Each awards 125 points.
- Every chamber defines required lanterns and may define optional ones. The exit opens only when the required set is collected.
- The exit is visible at a screen edge, floor hatch or ceiling opening. Its opening is signaled clearly: the edge flashes, the hatch animates, or an arrow appears.
- Entering an exit awards 2,000 points and starts the next chamber.
- The entrance hub has three chambers. All required lanterns in all three must be collected before a hatch opens in the floor of the middle chamber. Lantern state in the hub persists while the player moves between its chambers.
- Every other chamber is independent: leaving through an exit ends it.

## Chamber progression

- Chambers form a data-defined graph, matching the original fortress. The first three chambers form the entrance hub; the hatch in the middle chamber leads into the underworld.
- Each exit names its destination chamber. The CRASH reference map is the source for room numbering and connections.
- Chamber 20 is the Fire Wizard's lair. Avoiding his fireballs and pressing the destruction button awards 3,000 points and plays the ending.
- Completing chamber 20 starts the next loop at chamber 1.
- Loops increase difficulty: faster and more aggressive enemies, immediate enemy respawns, and removed safe spots in the electrified-comb chamber. Score and falls carry into the next loop.

## Enemies

- Both enemies share a state machine: idle, chase, attack, stunned, knocked down, respawning. They path along platforms and use vines.
- The Ninja is fast, attacks with a bokken, and jumps at Bruce. Two hits knock him out.
- The Yamo is slower and heavier; he runs Bruce down with punches and crushing kicks. Three hits knock him out.
- Neither can duck.
- Enemies pursue relentlessly but respect fairness rules: no stun-locking, no attacks from off-screen, and a short grace period after Bruce respawns.
- In two-player versus, the second player controls Yamo. If idle for a few seconds, the AI takes over until input resumes.

## Hazards

| Hazard | Behavior | Effect |
| ------ | -------- | ------ |
| Electric charge | Arc sweeps across a gap on a timer | Costs a fall |
| T'Sung-Lin bush | Appears, then explodes | Costs a fall |
| PAN lights | Beam streams across the floor | Costs a fall |
| Mine | Proximity or contact explosion | Costs a fall |
| Moving wall | Crusher sweeps a passage | Costs a fall |
| Dagger / sword | Flying projectile on a fixed path | Costs a fall |
| Killer bug | Small patrolling enemy | Costs a fall |
| Wizard fireball | Projectile aimed at Bruce | Costs a fall |

- All hazards are telegraphed and deterministic. No hazard may be unavoidable at its spawn location.
- Enemies are vulnerable to hazards and respawn afterward.
- The hazard framework is built in T-042; chambers introduce hazards in increasing combinations.

## Scoring

| Event | Points |
| ----- | ------ |
| Lantern | 125 |
| Chop | 100 |
| Kick or flying kick | 75 |
| Land on enemy | 50 |
| Knock out Ninja | 200 |
| Knock out Yamo | 450 |
| Enter a new chamber | 2,000 |
| Destroy the wizard | 3,000 |
| Extra fall | Every 30,000 points |

- The HUD score updates immediately.
- The session top score is shown during play; the all-time high score persists between runs (modern addition).
- Two-player versus scoring for the Yamo player is defined in T-054.

## HUD

Top bar, following the original order:

1. Player indicator (whose turn it is; hidden in one-player).
2. Current score.
3. Top score.
4. Falls remaining.
5. Lantern progress for the current chamber (modern addition).

## Game modes

- **One player:** Bruce versus the computer-controlled Ninja and Yamo.
- **Two players, Yamo:** player two controls Yamo; the Ninja stays computer-controlled. Scores are tracked separately, and the fall reserve is increased as in the original.
- **Two players, alternating Bruce:** players swap control after each lost fall; both enemies are computer-controlled, with the larger fall reserve.

Mode selection and the exact two-player scoring rules are implemented in T-054.

## Session flow

Title screen and mode select -> chamber intro card -> play -> chamber clear transition -> next chamber. On death, restart the chamber. On game over, enter initials, save the high score, return to the title. After the wizard, play the ending and start the next loop.

## Presentation and audio

- 256x192 logical resolution, integer-scaled, nearest-neighbor filtering.
- ZX-inspired palette and original pixel art, not traced from the original.
- Original chiptune-style title music and sound effects. The 1984 tune is not reproduced.
- Sound toggle available at any time; volume and mute belong in settings.

## Modern additions

These are intentional deviations, kept out of the core rules where possible:

- keyboard remapping and gamepad support,
- variable-height jumps,
- persisted high scores,
- pause menu with settings,
- lantern progress counter,
- configurable display scaling and fullscreen.

## Open questions

- Exact required lanterns and layouts per chamber (settled per chamber in M4).
- Whether all traps should cost a full fall; validate in T-055.
- Two-player Yamo scoring.
- The particle-wave platforms mentioned in the original manual; confirm their behavior from reference footage.
