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
- Attack is contextual, as in the original: standing = chop, running = kick. Attacks only start while grounded.
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
| Kick (flying kick) | Attack while running | 1 | Longer reach, lower score |
| Land on enemy | Fall onto an enemy | — | 50 points and a knockdown; does not consume a hit point |

- Each attack has wind-up, active and recovery frames. Hitboxes are active only during the active frames.
- One attack can hit a given target at most once.
- Hits briefly freeze the action (four frames of hitstop) and flash the victim red for eight frames.
- Landings kick up a short dust burst.
- Attacks cannot be canceled into another attack before recovery ends. Taking a hit cancels the current attack.
- Attacks only start while grounded. Airborne attack presses are ignored.
- Landing on an enemy causes a knockdown but no hit-point damage.

### Attack frame data

All values are at the fixed 60 Hz step and live in `AttackDefinitions`.

| Attack | Startup | Active | Recovery | Total | Hitbox (w x h, y offset) | Knockback | Hitstun | Knockdown | Animation frames | Score |
| ------ | ------- | ------ | -------- | ----- | ------------------------- | --------- | ------- | --------- | ---------------- | ----- |
| Chop | 1 | 2 | 3 | 6 | 10x8, +6 | 50, -15 | 8 | No | 8 / 9 / 8 | 100 |
| Kick | 3 | 5 | 10 | 18 | 16x8, +10 | 90, -20 | 12 | No | 12 / 13 / 12 | 75 |

- Attack selection is contextual: moving faster than 20 px/s = kick, otherwise chop.
- The running kick is the move contemporary reviews call the flying kick. It carries Bruce forward at 100 px/s for the whole animation; the chop stops him.
- The hitbox extends forward from the body edge and mirrors with facing.
- These values are the tuning baseline and may be adjusted in T-055.

### Enemy durability and knockouts

| Enemy | Hit points | Knockout bonus |
| ----- | ---------- | -------------- |
| Ninja | 2 | 200 |
| Yamo | 3 | 450 |

- The final hit awards both the hit score (chop or kick) and the knockout bonus.
- A knocked-out enemy is removed for a short respawn delay in loop 1 and returns immediately from loop 2.
- Enemy attacks can hit other enemies. Body contact separates enemies without damage, and hazard damage arrives with T-042. The player earns no points for enemy-on-enemy or hazard knockouts.

### Taking damage

- Bruce has three blows per fall. Each blow causes hitstun, knockback and brief invulnerability; the third blow causes a fall.
- Enemy attacks and enemy body contact during an attack deal one blow.
- Environmental traps cost a whole fall immediately. This is a remake ruling; revisit in T-055 if it feels unfair.
- After a fall, Bruce respawns at the chamber entrance with full blows and brief invulnerability. The current chamber resets: lanterns and enemies return to their initial state. Hazards join the reset when the hazard framework lands in T-042. Score and remaining falls are kept.

## Falls, game over and extra falls

- The original calls lives "falls". The HUD shows falls in reserve.
- One-player runs start with five falls.
- Two-player runs use the larger reserve reported by CRASH (5-10 depending on mode); the exact split between players is defined in T-054.
- Every 30,000 points awards one extra fall.
- At zero falls and one more death, the run ends and the high-score flow starts.

## Lanterns and exits

- Lanterns hang from the ceiling and are collected by touching them. Each awards 125 points and disappears for the rest of the attempt. Required lanterns gate the exit; optional lanterns only add score.
- Every chamber defines required lanterns and may define optional ones. The exit opens only when the required set is collected.
- The exit is visible at a screen edge, floor hatch or ceiling opening. Its opening is signaled clearly: the edge flashes, the hatch animates, or an arrow appears. The HUD also shows required lantern progress and `EXIT OPEN`.
- Entering an open exit awards 2,000 points and starts the target chamber. The player keeps score and falls; the target chamber starts with fresh lanterns and enemies.
- The entrance hub has three chambers. All required lanterns in all three must be collected before the floor hatch in the middle chamber opens. Hub lantern state persists between the three rooms; dying in one room resets only that room's lanterns and enemies.
- The hub HUD shows pooled `HUB x/y` progress, and `HATCH OPEN` once the hatch is available. Side doors between the hub rooms are always open.
- Every other chamber is independent: leaving through an exit ends it.
- Dying on a non-final fall resets the current chamber's lanterns and enemies but keeps score and falls. The exit closes again. Hazards reset with the hazard framework in T-042.

## Chamber progression

- Chambers form a data-defined graph, matching the original fortress. The first three chambers form the entrance hub; the hatch in the middle chamber leads into the underworld.
- Each exit names its destination chamber. The CRASH reference map is the source for room numbering and connections.
- Chamber 20 is the Fire Wizard's lair. Avoiding his fireballs and pressing the destruction button awards 3,000 points and plays the ending.
- Completing chamber 20 starts the next loop at chamber 1.
- Loops increase difficulty: faster and more aggressive enemies, immediate enemy respawns, and removed safe spots in the electrified-comb chamber. Score and falls carry into the next loop.

## Enemies

- Both enemies share a state machine: idle, patrol, chase, attack, stunned, knocked down, respawning. They path along platforms and use vines.
- The Ninja is fast, attacks with a bokken, and jumps at Bruce. Two hits knock him out.
- The Yamo is slower and heavier; he runs Bruce down with punches and crushing kicks. Three hits knock him out.
- Neither can duck.
- Enemies pursue relentlessly but respect fairness rules: only one enemy can start an attack or leap at a time, a short pressure gate follows a hit on Bruce, and he gets a grace period after respawning.
- In two-player versus, the second player controls Yamo. If idle for a few seconds, the AI takes over until input resumes.

### Enemy interactions and pacing (T-033)

- `EnemyEncounter` grants a single attack token. If one enemy is attacking or leaping, the other keeps positioning but cannot start a new attack.
- When an enemy hits Bruce, new enemy attacks and leaps are blocked for 30 frames. This complements Bruce's 20-frame hit invulnerability and prevents two enemies from trading hits through it.
- Enemy attacks can hit the other enemy. Friendly fire is resolved before the player, awards no score, and can knock an enemy down; the victim recovers and respawns normally.
- Overlapping enemies are pushed apart horizontally, weighted by body width. The heavier Yamo yields less than the Ninja. Climbing, stunned and knocked-down enemies are left alone so vines and hit reactions are not disturbed.
- Enemies do not damage each other through body contact; body overlap only separates them. Hazard damage to enemies is implemented and tested with the hazard framework in T-042.

### Enemy framework (T-030)

- `EnemyDefinitions` holds the shared tuning. The state machine is identical for both types; only the values and attack frame data differ.
- `EnemyNavigator` plans routes over platform tops and vertical vines: walk to a vine, climb it, step onto the upper platform, or walk off a ledge to drop to a lower one. Routes are deterministic.
- A seeded xorshift generator (`SeededRandom`) chooses patrol direction and idle variation, so replays are stable.
- Baseline values before T-031/T-032 refinement:

| Enemy | Hits | Run / patrol | Sight | Attack range | Strike | Knockout score | Respawn |
| ----- | ---- | ------------ | ----- | ------------ | ------ | -------------- | ------- |
| Ninja | 2 | 70 / 35 px/s | 180 px | 22 px | 4 startup, 3 active, 8 recovery | 200 | 3 s |
| Yamo | 3 | 45 / 24 px/s | 150 px | 24 px punch, 38 px kick | Punch 6 / 3 / 8; kick 12 / 4 / 14 | 450 | 4 s |

- Player hits stun or knock down enemies, and the final hit awards the knockout bonus. Enemies ignore further hits while down.
- Enemies keep a stand-off: they stop just inside their attack range and back away if Bruce closes in, instead of walking into his position.
- The framework currently ignores horizontal vines for enemy pathing; the sample chambers use vertical vines. Revisit when a chamber needs it.
- Enemy type-specific moves and blocking are T-031/T-032; overlap and pacing are T-033; hazard damage is T-042.

### The Ninja (T-031)

| Move | Trigger | Startup | Active | Recovery | Hitbox | Effect |
| ---- | ------- | ------- | ------ | -------- | ------ | ------ |
| Bokken strike | Same surface within 22 px; 45-frame cooldown | 4 | 3 | 8 | 12x8, +6 | 10 hitstun, no knockdown |
| Leap strike | 23-80 px away at a similar height; 90-frame cooldown | 12 windup + 8 | 14 | 12 | 14x10, +4 | 14 hitstun, no knockdown |

- The leap is a committed arc: 300 px/s upward and 110 px/s toward the target. It covers about 73 px horizontally and stays airborne for about 40 frames.
- The 12-frame windup uses the idle pose as a tell. The arc and direction are fixed when the windup ends, so Bruce can move out of the landing zone; the Ninja does not home in mid-air.
- The airborne strike hitbox is active for 14 frames, starting 8 attack frames after launch.
- The Ninja leaps at Bruce on the same platform when the gap is between 23 and 80 px. It also leaps across a gap or onto a disconnected platform at a similar height when no walking or vine route exists.
- It will not attempt to leap onto a target more than 24 px above it; higher routes stay with the vines.
- Jump cooldown is 90 frames (1.5 s) after landing. Bokken cooldown is 45 frames (0.75 s) after recovery.
- The Ninja still takes two hits to defeat, stays down for 50 frames and returns after 180 frames (3 s). Immediate respawns in later loops are T-046.

### The Green Yamo (T-032)

| Move | Trigger | Startup | Active | Recovery | Hitbox | Effect |
| ---- | ------- | ------- | ------ | -------- | ------ | ------ |
| Punch | Same surface within 24 px; 45-frame cooldown | 6 | 3 | 8 | 12x8, +7 | 10 hitstun, no knockdown |
| Crush kick | 25-38 px; 100-frame cooldown | 12 | 4 | 14 | 18x10, +8 | 16 hitstun, knockdown |

- Move selection is distance-based: punch when Bruce is inside 24 px, crush kick from 25 to 38 px. While closing, the first attack is therefore usually the kick.
- The crush kick leans forward at 30 px/s and knocks Bruce down. Its 12-frame wind-up, 14-frame recovery and 100-frame cooldown are the punish window; whiffing it gives Bruce time to counter.
- Yamo holds a 20 px stand-off and punches from there. He takes three hits, stays down for 60 frames and returns after 240 frames (4 s).
- Yamo is a blocking presence: his body is solid to Bruce. Bruce cannot walk through him and is pushed out of any overlap. A jump can still clear him.
- A kick sets the attack timer to 100 frames and a punch to 45, so a whiffed kick cannot be chained into another heavy attack.

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
| Kick | 75 |
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
6. `EXIT OPEN` when the required lantern set is complete (modern addition).

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
