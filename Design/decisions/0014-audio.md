# ADR-0014: Generated sound effects and failure-safe playback

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

T-052 needs original chiptune-style audio for every action and progression event, plus mute and volume controls. Audio devices can be absent in CI or disappear at runtime, and missing assets should not break gameplay. The existing asset pipeline already generates PCM WAVs from code.

## Decision

1. **Generate all effects:** `WavWriter` produces mono 16-bit PCM at 22,050 Hz from square sweeps, deterministic noise and short arpeggios. No third-party audio is shipped.
2. **One committed set:** jump, land, chop, kick, hit, knockdown, death, respawn, lantern pickup, chamber exit, wizard fireball and wizard defeat. `AssetGenerator` writes and verifies every file.
3. **Load through XNAssets:** `GameAudio` uses `AssetManager.LoadSoundEffect` and keeps a keyed dictionary. No custom audio service around the library.
4. **Failure-safe:** a broad guard around loading and playback turns missing assets, absent hardware and instance limits into silence. Gameplay never catches audio errors at call sites.
5. **Settings:** `AudioSettings` owns master volume (default 0.7, clamped) and mute. F2/Y toggles mute; `[`/`]` and gamepad shoulders adjust volume.
6. **Event wiring:** player sound events map to action/life sounds; enemy sound events map to attack/jump/hit/knockdown; the chamber screen adds pickup, exit, fireball and wizard-defeat sounds. The wizard raises a fireball event.
7. **Title music deferred:** it is optional in the task and not required for the audio baseline; a looping track can be added later without changing the event wiring.

## Consequences

- CI builds and tests do not depend on audio hardware.
- A missing WAV is silent rather than fatal; asset verification still catches missing generated files.
- Audio is generated deterministically, keeping the clean-checkout guarantee.
- Volume changes apply to future playback; currently playing sounds finish at their starting volume.
- No title music until a later polish task.

## Alternatives considered

- **MonoGame content pipeline:** adds content builds and a separate asset path; the project intentionally uses raw generated files.
- **A third-party audio library:** unnecessary for short effects and adds licensing/dependency risk.
- **Throw on missing audio:** would make a silent machine unplayable and complicate CI.

## Related

- `Design/gameplay.md`
- `Design/assets.md`
- `Design/architecture.md`
- ADR-0003 (code-generated assets)
- `VibeBruceLee/Audio/GameAudio.cs`
- `VibeBruceLee/Gameplay/AudioSettings.cs`
