# ADR-0013: Local high-score table and initials entry

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

T-051 needs a persisted high-score table, initials entry after a qualifying run, and safe handling of malformed save files. The game already has a session score and explicit game-over flow, but no player identity, file storage or save-error handling.

## Decision

1. **Small JSON table:** `HighScores` stores at most five `HighScoreEntry` records (`Initials`, `Score`, `Loop`) in `%LOCALAPPDATA%/VibeBruceLee/highscores.json`, falling back to the app directory if LocalApplicationData is unavailable.
2. **Fail-safe loading:** a missing file is an empty table. `JsonException`, `IOException` and `UnauthorizedAccessException` also produce an empty table. Entries with non-positive scores are dropped; initials are uppercased, filtered to A-Z and padded/truncated to three letters.
3. **Safe saving:** writes go to a temporary file and then replace the live file. Write failures are swallowed; the in-memory table remains usable for the session.
4. **Qualification:** positive scores enter an empty/short table; a full table requires beating the fifth score. The table is sorted by score, loop and initials.
5. **Arcade initials flow:** `GameOverScreen` shows the table and prompts for initials when the score qualifies. `InitialsScreen` uses Up/Down for letters, Left/Right for position and Confirm to save. `HighScoreScreen` shows the updated table and returns to the title.
6. **Visible top score:** the title and chamber HUD show `TOP nnnnnn`; the title and high-score screens show the full table.
7. **Pause help:** `PauseScreen` remains an overlay and can open `ControlsScreen` above itself; the controls screen pops back to the pause overlay.

## Consequences

- Scores survive game restarts and bad save files cannot prevent play.
- The file is deliberately small and human-readable; no high-score migration or remote service is needed.
- Initials entry is keyboard/gamepad friendly without a text input widget.
- A failed save silently loses the entry on exit; a visible warning is future polish.
- The table is global to the installation, not per profile.

## Alternatives considered

- **Player name text input:** awkward on a gamepad and needs OS text input.
- **Binary save format:** smaller but opaque and harder to repair or inspect.
- **Save next to the executable:** may be read-only and mixes data with program files.
- **No initials:** simpler, but loses the arcade high-score feel and the task requirement.

## Related

- `Design/gameplay.md`
- `Design/architecture.md`
- ADR-0008 (chamber catalog and progress)
- `VibeBruceLee/Gameplay/HighScores.cs`
- `VibeBruceLee/Screens/InitialsScreen.cs`
