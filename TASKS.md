# VibeBruceLee - Task Roadmap

This is a living document. The agent owns it and may split, reorder, rewrite, or drop tasks as understanding of the game improves. IDs stay stable once a task has been referenced in a commit. See `AGENTS.md` for the workflow that governs this file.

**Legend:** `[ ]` todo - `[~]` in progress - `[x]` done - `[!]` blocked

**Current milestone:** M5 - Presentation

## Guiding principles

- Fidelity to the 1984 game first, modern conveniences second.
- Fixed logical resolution of 256x192 (ZX Spectrum), integer-scaled and pixel-perfect.
- Chambers are data, not code. Game logic is deterministic and testable headless.
- Every task ships with tests, updated docs, and a manual playtest where it affects gameplay.
- Assets are original: the original game is reference only.

---

## M0 - Foundation

- [x] **T-001 Solution skeleton**
  Create `VibeBruceLee.slnx`, `VibeBruceLee` (MonoGame DesktopGL, `net8.0`), `VibeBruceLee.Tests` (xUnit), and the asset copy setup.
  *Done when:* `dotnet build` and `dotnet test` pass, the game opens a window, and a trivial test executes.

- [x] **T-002 Engineering baseline**
  Add `Directory.Build.props` (nullable, implicit usings, analyzers), `.editorconfig`, and a GitHub Actions workflow that builds and tests on Windows and Linux. Verify the README commands.
  *Done when:* the Release build and tests pass locally and in a clean copy with warnings as errors, and the two-OS CI workflow is green on push.

- [x] **T-003 Reference and design docs**
  Research the ZX Spectrum release: controls, scoring, lantern counts, enemy behavior, hazards, and chamber layouts. Record findings in `Design/reference.md`, `Design/overview.md`, and `Design/gameplay.md`. Add ADR-0001 covering engine choice and the 256x192 logical canvas.
  *Done when:* docs are enough to implement without re-researching the original.

- [x] **T-004 Asset pipeline**
  Integrate XNAssets and FontStashSharp directly. Define the asset root, key conventions, missing-asset diagnostics, hot reload stance, and the license folder. Add ADR-0002.
  *Done when:* a test scene draws an XNAssets texture and FontStashSharp text, and a missing asset fails with XNAssets' resolved-path diagnostics.

- [x] **T-005 Asset authoring toolchain**
  Decide and document how sprites, tiles, fonts, and audio are produced: scripted generation versus an external editor. Ask the developer before installing any software. Commit the reproducible pipeline and a sample sprite plus sample sound.
  *Done when:* regenerating sample assets from scratch requires no undocumented steps.

---

## M1 - Engine core

- [x] **T-010 Game shell and screen flow**
  Implement the MonoGame host, fixed 60 Hz timestep, 256x192 virtual canvas with integer scaling and letterboxing, and a screen stack (title, chamber, pause, game over).
  *Done when:* resize and fullscreen keep the image undistorted; screen transitions are tested.

- [x] **T-011 Input system**
  Map keyboard and gamepad to abstract actions (left, right, up, down, contextual attack, jump, pause, sound toggle). Add short input buffering for jump and attacks. Handle focus loss and gamepad hotplug.
  *Done when:* buffer behavior is unit-tested and remapping lives in one place.

- [x] **T-012 Rendering and animation**
  Implement sprite sheets, animation clips, frame timing, horizontal flipping, layer ordering, and deterministic animation updates on top of XNAssets.
  *Done when:* animation state transitions are unit-tested and sprites render pixel-perfect at every integer scale.

- [x] **T-013 Chamber data model**
  Define a chamber format (platforms, vines, required and optional lanterns, spawn points, hazards, exit connections, decor) in JSON under `VibeBruceLee/Assets/levels`. Add schema validation with actionable errors and a sample chamber.
  *Done when:* loader and validator are tested, including malformed input.

- [x] **T-014 Physics and collision**
  Implement AABB collision, gravity, one-way platforms, landing, ceilings, screen bounds, and vine climbing (up, down and sideways) in a fixed-step, deterministic world.
  *Done when:* edge cases such as tunneling at max speed and landing on thin platforms are tested.

- [x] **T-015 Debug tooling**
  Add an F1 overlay showing FPS, entity states, hitboxes, and collision boxes. Add slow motion, frame advance, invincibility, and chamber skip. Document it in `Design/debug.md`.
  *Done when:* the overlay is usable during normal play and cheats are disabled outside debug builds.

- [x] **T-016 Initial asset set**
  Produce a coherent first-pass asset set: Bruce (idle, run, jump, duck, chop, kick, climb, hit, death), Ninja, Yamo, tiles, lantern, HUD elements, and a readable pixel font. Include source files and generation notes.
  *Done when:* all entities and the HUD render from the asset pipeline with no placeholders.

---

## M2 - Player character

- [x] **T-020 Locomotion**
  Implement run acceleration, variable-height jump, ducking, climbing vines up, down and sideways, and safe drops, with matching animations.
  *Done when:* all movement states are unit-tested and a manual feel check passes.

- [x] **T-021 Combat moves**
  Implement the context-sensitive attack: chop while standing and kick while running (the move reviewers call the flying kick), with wind-up, active frames, recovery, hitboxes, hitstun, knockdown, and animation cancels. Cover both hitting and being hit.
  *Done when:* frame data is centralized, tested, and documented in `Design/gameplay.md`.
  *Plan:* add centralized attack frame data, player attack/hitstun/knockdown states, active hitboxes with one-hit-per-attack, contextual attack selection, and tests for timing, facing, hit application and recovery.

- [x] **T-022 Health, lives, and respawn**
  Handle enemy blows (three per fall), deadly hazards, death animation, five starting falls, an extra fall every 30,000 points, respawn with brief invulnerability, and game over.
  *Done when:* life-loss transitions and invulnerability windows are tested.

- [x] **T-023 Player animation and feel pass**
  Refine timings, add hitstop, hit flash, and landing dust, and wire sound hooks. Record final values later in `Design/tuning.md`.
  *Done when:* the character feels responsive and every action has clear visual feedback.

---

## M3 - Enemies

- [x] **T-030 Enemy framework**
  Build a shared enemy state machine (idle, patrol, chase, attack, stunned, knocked down, respawn), platform-aware pathing, and a seeded random source for determinism.
  *Done when:* the state machine is tested with a fake world and enemies navigate vines correctly.

- [x] **T-031 The Ninja**
  Implement the fast pursuer: chase behavior, bokken strike, jump attacks, and return after being defeated. Keep him threatening but dodgeable.
  *Done when:* attack patterns are documented and balanced, with tests for decisions and cooldowns.

- [x] **T-032 The Green Yamo**
  Implement the slow, powerful wrestler: blocking presence, punches, crushing kicks, and return after defeat.
  *Done when:* he pressures the player without feeling unfair, and his timings are tested.

- [x] **T-033 Enemy interactions and pacing**
  Let enemy attacks hit other enemies, keep enemies from stacking, and add a pressure gate so two enemies cannot attack simultaneously or chain hits. Hazard damage to enemies stays with T-042.
  *Done when:* difficulty values are documented and a scripted encounter test passes repeatedly.

---

## M4 - Chambers

- [x] **T-040 Chamber 1 vertical slice**
  Recreate the first entrance chamber from reference. Implement lantern collection, the exit opening only after the required lanterns are taken, scoring, death and restart, and a complete playable loop.
  *Done when:* the chamber is beatable and losing all falls returns to game over.

- [x] **T-041 Chambers 2-5 and the entrance hub**
  Add four chambers that extend platforming and vine movement, plus the three-room entrance hub rule: clearing chambers 1-3 opens the floor hatch in the middle chamber.
  *Done when:* each chamber validates, loads, and can be completed, and the hatch gating is tested.

- [x] **T-042 Hazards framework**
  Implement electric charges, exploding T'Sung-Lin bushes, PAN floor lights, mines, moving walls, and projectile hazards (daggers, swords, bugs, fireballs) as reusable, testable hazards. Enemies must be vulnerable to them too.
  *Done when:* each hazard has tests for activation, damage, and despawn.

- [x] **T-043 Chambers 6-12**
  Build the mid-game chambers, introducing and combining hazards and tighter enemy placement.
  *Done when:* all seven chambers are beatable and recorded in `Design/chambers.md`.

- [x] **T-044 Chambers 13-19**
  Build the late-game chambers with the hardest layouts, hazard combinations, and enemy pressure.
  *Done when:* all seven chambers are beatable and no layout requires frame-perfect input.

- [x] **T-045 Chamber 20 - the Fire Wizard**
  Implement the final chamber: fireballs from the wizard's eyes, the destruction button, defeat sequence, and ending screen.
  *Done when:* the boss is beatable, testable, and the ending returns to the title cleanly.

- [x] **T-046 Loop and difficulty scaling**
  After chamber 20, loop the game with faster enemies, immediate respawns, and removed safe spots, matching the original's escalation.
  *Done when:* loop state is persisted across chambers and covered by tests.

---

## M5 - Presentation

- [x] **T-050 Title screen and controls**
  Add a title screen with logo, attract prompt, story intro, and a clear controls screen. Support keyboard and gamepad.
  *Done when:* a new player can start a game without reading the docs.

- [ ] **T-051 HUD, pause, game over, and high scores**
  Show score, falls, and lantern progress. Add pause, game over, initials entry, and a persisted high-score table.
  *Done when:* scores survive restarts and malformed save files fail safely.

- [ ] **T-052 Audio**
  Create original chiptune-style sound effects for chops, kicks, jumps, hits, knockdowns, lantern pickups, death, exits, and the wizard. Add optional title music, volume control, sound toggle, and mute.
  *Done when:* every gameplay event is wired, audio failures never crash the game, and generated audio sources are committed.

- [ ] **T-053 Art polish**
  Replace first-pass art with final tiles, backdrops, character animation, portraits, and the wizard. Give the Ninja a dedicated leap wind-up and airborne pose, and the Yamo distinct punch and kick poses. Document the palette and pixel grid in `Design/assets.md`.
  *Done when:* visuals are consistent across all chambers and every asset has a source file.

- [ ] **T-054 Local two-player mode**
  Let a second player control Yamo, or alternate control of Bruce after a lost fall, with AI takeover after inactivity. Track scores for both players and implement the larger two-player fall reserve.
  *Done when:* control handover is tested and documented.

- [ ] **T-055 Tuning and game feel**
  Tune movement, attack ranges, invulnerability, enemy aggression, and hazard timing. Run a full playtest and record values in `Design/tuning.md`.
  *Done when:* the developer signs off on feel and difficulty.

---

## M6 - Ship

- [ ] **T-060 Test hardening**
  Add headless simulation tests (complete a chamber without rendering), golden replay tests, an asset-load smoke test, and coverage reporting for core logic. Keep CI on Windows and Linux.
  *Done when:* core logic has meaningful coverage and regressions are caught by tests, not by hand.

- [ ] **T-061 Performance and compatibility**
  Verify a stable 60 FPS at high resolutions, avoid hot-path allocations, support resizable/fullscreen windows, gamepad hotplug, and missing audio devices.
  *Done when:* profiling results are documented and no frame exceeds the budget.

- [ ] **T-062 Packaging and release**
  Produce self-contained `win-x64` and `linux-x64` builds, zip artifacts, add a tag-triggered release workflow, and show the version in-game.
  *Done when:* a clean machine can download, unpack, and play.

- [ ] **T-063 Final playtest and v1.0**
  Complete two full loops, smoke-test two-player mode, review all docs, and record known issues.
  *Done when:* the checklist is signed off and `v1.0` is tagged.

---

## Stretch goals

- [ ] **S-01 CRT-style presentation** - optional scanline/curvature shader with integer scaling preserved.
- [ ] **S-02 Speedrun mode** - timer, split per chamber, and local best times.
- [ ] **S-03 Remix chamber editor** - in-game chamber editor with JSON export.
- [ ] **S-04 Alternate soundtracks** - switch between beeper-style and richer arrangements.

---

## Backlog

New discoveries and follow-ups land here, then get promoted into a milestone.

- Gate `DebugGameOver` (`G`) behind `DEBUG`; it is currently an accessible cheat in release builds.
- Investigate occasional missing glyphs in HUD text (seen once each in `HP` and `FALLS`); likely a FontStashSharp atlas/batching issue.

---

## Done log

| Task | Date | Summary |
| ---- | ---- | ------- |
| Docs bootstrap | 2026-11-19 | Added README.md, this roadmap, and AGENTS.md workflow. |
| T-001 | 2026-11-19 | Solution skeleton: MonoGame host, xUnit project, asset copy rule, architecture doc and .gitignore. |
| T-002 | 2026-11-19 | Engineering baseline: shared build props, SDK pin, editorconfig with tab indentation, and Windows/Linux CI. |
| T-003 | 2026-11-20 | Reference research, overview, gameplay rules, design index and ADR-0001; roadmap corrected for vines, falls, scoring and chamber graph. |
| T-004 | 2026-11-20 | XNAssets + FontStashSharp asset pipeline: relative keys, test scene, test card and OFL font, asset docs and ADR-0002. |
| T-005 | 2026-11-20 | Code-driven asset generator using StbImageNext and PCM WAV output; sample lantern and pickup sound; CI reproducibility check; ADR-0003. |
| T-010 | 2026-11-20 | 256x192 virtual canvas with integer letterboxing, resizable/fullscreen window, tested screen stack and placeholder screens. |
| T-011 | 2026-11-20 | Action-based input system with central bindings, 120 ms jump/attack buffers, focus-loss reset and gamepad reconnection handling. |
| T-012 | 2026-11-20 | Sprite sheets, deterministic animation player, four-frame lantern sheet, layer ordering and overlay-aware screen drawing. |
| T-013 | 2026-11-20 | Strict JSON chamber model, loader/validator with field-path errors, two sample chambers and format documentation. |
| T-014 | 2026-11-20 | Deterministic AABB physics with sub-pixel collision, gravity, one-way platforms, vines and world clamps; 12 tests. |
| T-015 | 2026-11-20 | Debug-only overlay/controller with FPS, boxes, slow motion, frame step, invincibility and skip flags; documented. |
| T-016 | 2026-11-20 | First-pass Bruce/Ninja/Yamo sheets, tile and HUD atlases, animation frame maps, and title/chamber showcases. |
| T-020 | 2026-11-20 | Player locomotion: run, variable jump, duck, vine climbing/detach, state animations, wired into the sample chamber. |
| T-021 | 2026-11-20 | Grounded chop/running-kick combat with centralized frame data, one-hit hitboxes, hitstun/knockdown and corrected source research. |
| T-022 | 2026-11-20 | Health/falls/respawn: three blows, hit i-frames, death, five falls, extra falls per 30k, HUD/game over; longer momentum-carrying kick. |
| T-023 | 2026-11-20 | Hitstop, flash, landing dust, sound hooks; vine-top platform transition and feet-anchored sprite alignment. |
| T-030 | 2026-11-20 | Shared enemy state machine, deterministic platform/vine navigation and xorshift RNG; Ninja/Yamo baseline stats, hit exchange, knockout scores, respawns and debug boxes. |
| T-031 | 2026-11-20 | Ninja leap attack with a telegraphed fixed arc, gap crossing, air strike and jump cooldown; attack decisions documented and enemy stand-off added so pursuers no longer overlap Bruce. |
| T-032 | 2026-11-20 | Yamo punch and knockdown crush kick with distance selection and per-attack cooldowns; solid body blocking, stand-off attacks, timings documented and tested. |
| T-033 | 2026-11-20 | Headless enemy encounter with one attack token, 30-frame post-hit pressure gate, friendly fire, score validity gating and weighted enemy separation; deterministic scripted encounter tests. |
| T-040 | 2026-11-20 | Chamber catalog and per-attempt progress; playable entrance hall with lantern collection, exit gating, 125/2,000 scoring, chamber transition, death reset and HUD progress. |
| T-041 | 2026-11-20 | Three-room entrance hub with persistent pooled lanterns, per-room death resets, data-driven `requiresHub` floor hatch, and chambers 03-05 with multi-vine layouts. |
| T-042 | 2026-11-20 | Hazard families for electric/PAN beams, bushes, mines, moving walls and projectiles; central player/enemy damage, reset, debug boxes and example placements in chambers 01-05. |
| T-043 | 2026-11-20 | Seven-chamber mid-game chain 06-12 with back/forward doors, escalating hazard combinations, two enemies per room and the 12-chamber catalog/chain tests. |
| T-044 | 2026-11-20 | Seven-chamber late-game chain 13-19 with beam lanes, projectiles, crushers, mines and combined trap rooms; safe spawn/wait gaps and the 19-chamber catalog/chain tests. |
| T-045 | 2026-11-20 | Fire Wizard boss with aimed eye fireballs, warning flash, destruction button, 3,000-point win, defeat timer, ending screen and boss/wizard validation. |
| T-046 | 2026-11-20 | Persistent loops with carried score/falls, enemy speed/cooldown/sight scaling, immediate respawns, chamber-16 loop hazards, `LOOP n` HUD and loop-continue ending. |
| T-050 | 2026-11-20 | Title logo/story/blinking start prompt, dedicated keyboard+gamepad controls screen bound to C/B, and controls input test. |
