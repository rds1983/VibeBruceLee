# ADR-0009: Data-driven hub progress and hatch gating

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

Chambers 01-03 form the entrance hub. Their lanterns are pooled and must open a floor hatch in the middle room. Lantern state has to survive room changes, but a fall resets the current room's lanterns and enemies. The hatch rule is not just "current room lanterns complete": it depends on all three hub rooms. Hard-coding chamber ids inside the screen would make T-043+ fragile and would split the rule across JSON and code.

## Decision

1. **Hub membership is data:** `ChamberDefinition.Hub` marks the three hub rooms in JSON. No chamber id is hard-coded in gameplay.
2. **Shared sets, per-room reset:** `HubProgress` owns one collected-lantern set per hub chamber. `ChamberProgress` accepts an optional external set and uses it directly, so a revisited room sees its previous collection. `Reset()` clears only that room's set; a new run clears all sets.
3. **Pooled totals:** `HubProgress.RequiredCollected` and `RequiredTotal` sum required lanterns across every `Hub` chamber in the catalog. Optional lanterns do not count.
4. **Hatch exits are data:** `ExitDefinition.RequiresHub` marks the floor hatch. `ChamberProgress.IsExitOpen(exit, hubHatchOpen)` combines the per-room lantern gate, the `requiresLanterns` flag and the hub gate.
5. **Middle room owns the hatch:** chamber-02's hatch is a normal exit rectangle at the floor surface with `requiresLanterns: false, requiresHub: true`. `ChamberScreen` passes the current hub-complete flag into open-exit detection and drawing.
6. **HUD explains the pool:** hub rooms show `HUB x/y` or `HATCH OPEN`; non-hub rooms keep `EXIT OPEN`.

## Consequences

- The hub rule is testable headlessly with a catalog of synthetic chambers and does not depend on screen geometry.
- Revisiting a hub room preserves progress; dying in it drops only its contribution to the hatch.
- T-043+ can add hub rooms or hatch exits by data alone, provided their `Hub`/`RequiresHub` flags are set.
- The current collected sets live for the whole run; leaving the hub keeps them until a new run or a reset of that room.
- A malformed level that marks a hatch exit with `requiresHub` but no `Hub` rooms will leave it permanently locked; the catalog/validator can add a targeted warning later if needed.

## Alternatives considered

- **One shared set for all hub rooms:** simpler, but cannot reset a single room on death without clearing the other rooms.
- **Hard-coded chamber ids:** fast for five rooms, but hides the hub rule from the data and does not scale.
- **A special hatch screen/state:** would duplicate exit handling and complicate the general chamber loop.
- **Reset the whole hub on any death:** harsher than the "current chamber resets" rule and punishes exploration disproportionately.

## Related

- `Design/gameplay.md`
- `Design/chambers.md`
- `Design/architecture.md`
- ADR-0008 (chamber catalog and progress)
- `VibeBruceLee/Gameplay/HubProgress.cs`
