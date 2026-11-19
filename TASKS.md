# VibeBruceLee - Task Roadmap

This is a living document. The agent owns it and may split, reorder, rewrite, or drop tasks as understanding of the game improves. IDs stay stable once a task has been referenced in a commit. See `AGENTS.md` for the workflow that governs this file.

**Legend:** `[ ]` todo - `[~]` in progress - `[x]` done - `[!]` blocked

**Current milestone:** M0 - Foundation

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

- [ ] **T-002 Engineering baseline**
  Add `Directory.Build.props` (nullable, implicit usings, analyzers), `.editorconfig`, and a GitHub Actions workflow that builds and tests on Windows and Linux. Verify the README commands.
  *Done when:* CI is green and a fresh clone builds without warnings.

- [ ] **T-003 Reference and design docs**
  Research the ZX Spectrum release: controls, scoring, lantern counts, enemy behavior, hazards, and chamber layouts. Record findings in `Design/reference.md`, `Design/overview.md`, and `Design/gameplay.md`. Add ADR-0001 covering engine choice and the 256x192 logical canvas.
  *Done when:* docs are enough to implement without re-researching the original.

- [ ] **T-004 Asset pipeline**
  Integrate XNAssets behind a small asset service and FontStashSharp behind a text renderer service. Define the asset root, naming rules, missing-asset diagnostics, hot reload if cheap, and the license folder. Add ADR-0002.
  *Done when:* a test scene draws an XNAssets texture and FontStashSharp text; a unit test verifies asset resolution.

- [ ] **T-005 Asset authoring toolchain**
  Decide and document how sprites, tiles, fonts, and audio are produced: scripted generation versus an external editor. Ask the developer before installing any software. Commit the reproducible pipeline and a sample sprite plus sample sound.
  *Done when:* regenerating sample assets from scratch requires no undocumented steps.

---

## M1 - Engine core

- [ ] **T-010 Game shell and screen flow**
  Implement the MonoGame host, fixed 60 Hz timestep, 256x192 virtual canvas with integer scaling and letterboxing, and a screen stack (title, chamber, pause, game over).
  *Done when:* resize and fullscreen keep the image undistorted; screen transitions are tested.

- [ ] **T-011 Input system**
  Map keyboard and gamepad to abstract actions (left, right, up, down, jump, punch, kick, pause). Add short input buffering for jump and attacks. Handle focus loss and gamepad hotplug.
  *Done when:* buffer behavior is unit-tested and remapping lives in one place.

- [ ] **T-012 Rendering and animation**
  Implement sprite sheets, animation clips, frame timing, horizontal flipping, layer ordering, and deterministic animation updates on top of XNAssets.
  *Done when:* animation state transitions are unit-tested and sprites render pixel-perfect at every integer scale.

- [ ] **T-013 Chamber data model**
  Define a chamber format (platforms, ladders, ropes, lanterns, spawn points, hazards, exit, decor) in JSON under `VibeBruceLee/Assets/Levels`. Add schema validation with actionable errors and a sample chamber.
  *Done when:* loader and validator are tested, including malformed input.

- [ ] **T-014 Physics and collision**
  Implement AABB collision, gravity, one-way platforms, landing, ceilings, screen bounds, and ladder/rope climbing in a fixed-step, deterministic world.
  *Done when:* edge cases such as tunneling at max speed and landing on thin platforms are tested.

- [ ] **T-015 Debug tooling**
  Add an F1 overlay showing FPS, entity states, hitboxes, and collision boxes. Add slow motion, frame advance, invincibility, and chamber skip. Document it in `Design/debug.md`.
  *Done when:* the overlay is usable during normal play and cheats are disabled outside debug builds.

- [ ] **T-016 Initial asset set**
  Produce a coherent first-pass asset set: Bruce (idle, run, jump, crouch, punch, kick, climb, hit, death), Ninja, Yamo, tiles, lantern, HUD elements, and a readable pixel font. Include source files and generation notes.
  *Done when:* all entities and the HUD render from the asset pipeline with no placeholders.

---

## M2 - Player character

- [ ] **T-020 Locomotion**
  Implement run acceleration, variable-height jump, crouch, climbing ladders and ropes, and drop-through where appropriate, with matching animations.
  *Done when:* all movement states are unit-tested and a manual feel check passes.

- [ ] **T-021 Combat moves**
  Implement punch, kick, and jump kick with wind-up, active frames, recovery, hitboxes, hitstun, knockdown, and animation cancels. Cover both hitting and being hit.
  *Done when:* frame data is centralized, tested, and documented in `Design/gameplay.md`.

- [ ] **T-022 Health, lives, and respawn**
  Handle enemy contact, hazards, death animation, three lives, respawn with brief invulnerability, and game over.
  *Done when:* life-loss transitions and invulnerability windows are tested.

- [ ] **T-023 Player animation and feel pass**
  Refine timings, add hitstop, hit flash, and landing dust, and wire sound hooks. Record final values later in `Design/tuning.md`.
  *Done when:* the character feels responsive and every action has clear visual feedback.

---

## M3 - Enemies

- [ ] **T-030 Enemy framework**
  Build a shared enemy state machine (idle, patrol, chase, attack, stunned, knocked down, respawn), platform-aware pathing, and a seeded random source for determinism.
  *Done when:* the state machine is tested with a fake world and enemies navigate ladders correctly.

- [ ] **T-031 The Ninja**
  Implement the fast pursuer: chase behavior, bokken strike, jump attacks, and return after being defeated. Keep him threatening but dodgeable.
  *Done when:* attack patterns are documented and balanced, with tests for decisions and cooldowns.

- [ ] **T-032 The Green Yamo**
  Implement the slow, powerful wrestler: blocking presence, punches, crushing kicks, and return after defeat.
  *Done when:* he pressures the player without feeling unfair, and his timings are tested.

- [ ] **T-033 Enemy interactions and pacing**
  Let enemies take damage from hazards, fall, get knocked into mines, and recover. Tune aggression so two enemies create pressure without stun-locking the player.
  *Done when:* difficulty values are documented and a scripted encounter test passes repeatedly.

---

## M4 - Chambers

- [ ] **T-040 Chamber 1 vertical slice**
  Recreate the first chamber from reference. Implement lantern collection, the exit opening only after all lanterns are taken, scoring, death and restart, and a complete playable loop.
  *Done when:* the chamber is beatable and losing all lives returns to game over.

- [ ] **T-041 Chambers 2-5**
  Add four chambers that extend platforming and ladder movement, with data-driven layouts and per-chamber spawns.
  *Done when:* each chamber validates, loads, and can be completed.

- [ ] **T-042 Hazards framework**
  Implement mines, moving walls, the electric comb, and projectiles as reusable, testable hazards. Enemies must be vulnerable to them too.
  *Done when:* each hazard has tests for activation, damage, and despawn.

- [ ] **T-043 Chambers 6-12**
  Build the mid-game chambers, introducing and combining hazards and tighter enemy placement.
  *Done when:* all seven chambers are beatable and recorded in `Design/chambers.md`.

- [ ] **T-044 Chambers 13-19**
  Build the late-game chambers with the hardest layouts, hazard combinations, and enemy pressure.
  *Done when:* all seven chambers are beatable and no layout requires frame-perfect input.

- [ ] **T-045 Chamber 20 - the Fire Wizard**
  Implement the final chamber and boss: fireball patterns, phases, defeat sequence, and ending screen.
  *Done when:* the boss is beatable, testable, and the ending returns to the title cleanly.

- [ ] **T-046 Loop and difficulty scaling**
  After chamber 20, loop the game with faster enemies, immediate respawns, and removed safe spots, matching the original's escalation.
  *Done when:* loop state is persisted across chambers and covered by tests.

---

## M5 - Presentation

- [ ] **T-050 Title screen and controls**
  Add a title screen with logo, attract prompt, story intro, and a clear controls screen. Support keyboard and gamepad.
  *Done when:* a new player can start a game without reading the docs.

- [ ] **T-051 HUD, pause, game over, and high scores**
  Show score, lives, and lanterns. Add pause, game over, initials entry, and a persisted high-score table.
  *Done when:* scores survive restarts and malformed save files fail safely.

- [ ] **T-052 Audio**
  Create original chiptune-style sound effects for punches, kicks, jumps, hits, knockdowns, lantern pickups, death, exits, and the wizard. Add optional title music, volume control, and mute.
  *Done when:* every gameplay event is wired, audio failures never crash the game, and generated audio sources are committed.

- [ ] **T-053 Art polish**
  Replace first-pass art with final tiles, backdrops, character animation, portraits, and the wizard. Document the palette and pixel grid in `Design/assets.md`.
  *Done when:* visuals are consistent across all chambers and every asset has a source file.

- [ ] **T-054 Local two-player mode**
  Let a second player control Yamo, or alternate control of Bruce after death, with AI takeover after inactivity.
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

- _(empty)_

---

## Done log

| Task | Date | Summary |
| ---- | ---- | ------- |
| Docs bootstrap | 2026-11-19 | Added README.md, this roadmap, and AGENTS.md workflow. |
| T-001 | 2026-11-19 | Solution skeleton: MonoGame host, xUnit project, asset copy rule, architecture doc and .gitignore. |
