# Reference: Bruce Lee (1984)

Facts about the original game and the interpretation this remake follows. Sources were read on 2026-11-20. This is a summary, not a reproduction of the manual.

## Sources

| Source | Used for |
| ------ | -------- |
| [Wikipedia: Bruce Lee (video game)](https://en.wikipedia.org/wiki/Bruce_Lee_(video_game)) | Release history, 20 chambers, hazards, looping, two-player structure |
| [SpectrumComputing entry 718](https://spectrumcomputing.co.uk/entry/0000718/Bruce_Lee) and its [instructions text](https://spectrumcomputing.co.uk/pub/sinclair/games-info/b/BruceLee.txt) | Spectrum controls, rules, scoring, enemy durability, wizard |
| [CRASH issue 16 review](http://www.crashonline.org.uk/16/brucelee.htm) | Spectrum-specific progression, lives, extra life, modes, hazards |
| [CRASH issue 16 map and tips](https://archive.org/stream/crash-magazine-16/Crash_16_May_1985#page/n75/mode/1up) | Chamber numbering and connections, two-player fall bonus |
| [Speccy Screenshot Maps](http://maps.speccy.cz/map.php?id=BruceLee) | Chamber layout reference for M4 (not shipped) |

The instructions text is a Spectrum-adapted reprint of the Datasoft manual, whose gameplay text originated with the Apple II release. Where it differs from the CRASH review, this document prefers the Spectrum-specific review. Original assets, maps and music are copyrighted; they are reference material only.

## Release facts

| Fact | Value |
| ---- | ----- |
| Original release | 1984, Datasoft (Atari 8-bit) |
| Spectrum release | 1985, U.S. Gold; conversion by Ocean |
| Design | Ron J. Fortier |
| Graphics | Kelly Day |
| Music | John A. Fitzpatrick |
| Platform | ZX Spectrum 48K |
| Players | 1 or 2 |
| Display | 256x192, single screen per chamber, no scrolling |
| Price | £7.95 |

## Presentation

- Each chamber is one fixed 256x192 screen with platforms, vines, hazards and hanging lanterns.
- The status bar shows, left to right: whose turn it is, the current score, the session's top score, and the falls (lives) remaining.
- Title tune plus spot effects for actions and hits.
- The tone is a mix of platforming and one-on-one martial arts.

## Controls (Spectrum)

| Action | Keys |
| ------ | ---- |
| Up | `Q` |
| Down | `A` |
| Left | `O` |
| Right | `P` |
| Attack | `Z` to `M` (any of these keys) |
| Pause | `Enter` |
| Sound on/off | `Symbol Shift` |
| Restart game | `Shift` + `Break` |

Joysticks supported: Kempston, Protek, and Sinclair/Interface 2. The attack button is contextual: standing still performs a chop, running performs a kick. Player count and control options are chosen from the title menu; CRASH lists menu options A-D.

## Rules

- Bruce explores a wizard's fortress, collecting lanterns to open the way to adjoining chambers.
- Lanterns hang from the ceiling and are collected by jumping. When the required lanterns in a chamber are taken, an exit opens at the screen edge or floor; the player must watch the edges for the opening.
- The first three entrance chambers must be fully cleared. Their lanterns then open a hatch in the floor of the middle entrance chamber, leading into the underworld.
- The fortress has 20 chambers: the CRASH map numbers chambers 1-19 and shows a final chamber with the wizard.
- Bruce can take three blows before losing a fall. A one-player game starts with five falls; CRASH reports 5-10 lives depending on the number of players, and its two-player tips note that a second player grants more falls.
- An extra fall is awarded every 30,000 points.
- Defeated enemies return. From the second loop they are replaced immediately instead of after a delay, and some safe spots in the electrified-comb chamber are removed.
- Completing the game loops it at higher difficulty.

## Chamber progression

- Chambers 1-3 are the three-room entrance hub. Clearing their lanterns opens the floor hatch in the middle chamber.
- The rest of the fortress is a connected graph, not a simple 1-20 corridor: the CRASH map shows doors and arrows linking numbered chambers, with some rooms reachable from more than one neighbor.
- Chamber numbering in the CRASH map is the reference order for authoring. Exact connections are transcribed into `Design/chambers.md` and the chamber data during M4.
- The final chamber holds the Fire Wizard and ends the loop.

## Movement and combat

- Run left and right.
- Jump, including leaping up and diagonally to another ledge.
- Duck under attacks.
- Climb vines up, down, and sideways.
- Drop from ledges without injury.
- Chop (punch) while standing; kick while running. Reviewers call the running kick the "flying kick"; the manual documents no separate airborne attack.
- Landing on an enemy scores points and can knock them down.
- Enemies cannot duck. The Yamo can take three blows; the ninja can take two.
- Enemies can hurt each other when they collide or when one is in the other's way.

## Enemies

| Enemy | Behavior | Durability |
| ----- | -------- | ---------- |
| The Ninja | Fast pursuer armed with a bokken stick; jumps and strikes | Two blows |
| The Green Yamo | Large wrestler; runs down Bruce and delivers punches and crushing kicks | Three blows |

Both pursue Bruce across the chamber, use vines, and return after being knocked out. In two-player versus mode, the second player controls Yamo; if that player is idle, the computer takes over.

## Hazards

Later chambers add environmental dangers:

- electric charges arcing across gaps between ledges,
- exploding T'Sung-Lin bushes that appear from nowhere,
- streams of "PAN" lights across the floor,
- mines,
- moving walls,
- flying daggers or swords,
- killer bugs,
- fireballs from the wizard's eyes.

Enemies are vulnerable to these hazards too.

## Scoring

| Event | Points |
| ----- | ------ |
| Collect a lantern | 125 |
| Chop the ninja or Yamo | 100 |
| Kick the ninja or Yamo | 75 |
| Knock out the ninja | 200 |
| Knock out the Yamo | 450 |
| Land on the ninja or Yamo | 50 |
| Enter a new chamber | 2,000 |
| Destroy the wizard | 3,000 |
| Extra fall | Every 30,000 points |

The score table is from the Datasoft manual; CRASH confirms the 30,000-point extra life. The remake keeps these values unless playtesting shows a mismatch.

## Endgame

The Fire Wizard sits in the twentieth chamber. He shoots fireballs from his eyes. Bruce must avoid them and press the button that destroys the wizard, earning 3,000 points and ending the loop. The next loop is harder.

## Two-player modes

- One player as Bruce against the computer-controlled ninja and Yamo.
- Two players: one as Bruce, the other as Yamo, with the ninja still computer-controlled, and a larger fall reserve. CRASH lists menu options A-D for player count and controls.
- Two players alternating as Bruce, swapping after a lost fall, with the computer controlling both enemies.
- The computer takes over Yamo if the second player is inactive for a while.

## Uncertainties and follow-ups

These points are not fully documented and must be settled while building the chambers:

- Exact lantern count and required subset for each of the 20 chambers. The manual says "all the lanterns (or at least certain lanterns)", implying some lanterns may be optional.
- Exact chamber connections and hazard placement. The CRASH map provides the room numbering and link arrows; transcribe it during T-040 to T-045. Do not trace art from it.
- Whether environmental hazards cost a whole fall or a single blow. The remake currently rules that traps cost a fall; revisit in T-055.
- Two-player scoring details for the player controlling Yamo, and whether the larger two-player fall reserve is shared or per player.
- Exact frame timings, hitboxes and AI reaction times. These are remake decisions recorded in `Design/tuning.md`.
- The CRASH review calls the running kick a "flying kick"; the manual lists no airborne attack. The remake treats them as the same move. Revisit only if stronger evidence appears.
- The "waves of particles" mentioned in the manual: confirm whether they are a moving platform, a hazard, or both.

## Remake interpretation

- Twenty chambers, with the three-room entrance hub and the middle-floor hatch.
- Required lanterns are data per chamber; optional lanterns are allowed but never block an exit.
- Enemy blows feed a three-hit meter per fall; deadly traps cost a fall immediately.
- Enemies respawn indefinitely and can damage each other.
- Scoring keeps the original table; high-score persistence is a modern addition (T-051).
- Original key bindings remain available; modern keyboard and gamepad defaults are added in T-011.
- All shipped art, audio and fonts are original. Reference maps and screenshots are not committed.
