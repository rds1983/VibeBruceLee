# Chambers

Each chamber is a single 256x192 screen defined by a JSON file in `VibeBruceLee/Assets/levels`. `ChamberLoader` reads the set, `ChamberValidator` checks geometry and references, and gameplay code consumes the resulting `ChamberDefinition`.

## Coordinates

- Origin is the top-left corner; X grows right, Y grows down.
- All coordinates and sizes are integer logical pixels in the 256x192 canvas.
- Rectangles are `x`, `y`, `width`, `height`.
- Points are `x`, `y`.

## File shape

```json
{
  "id": "chamber-01",
  "name": "Entrance Hall",
  "width": 256,
  "height": 192,
  "hub": true,
  "playerSpawn": { "x": 32, "y": 144 },
  "platforms": [
    { "x": 0, "y": 176, "width": 256, "height": 16 }
  ],
  "vines": [
    { "x": 120, "y": 64, "width": 8, "height": 112, "orientation": "vertical" }
  ],
  "lanterns": [
    { "id": "lantern-1", "x": 40, "y": 100, "required": true }
  ],
  "enemySpawns": [
    { "type": "ninja", "x": 220, "y": 144 }
  ],
  "hazards": [
    { "type": "electric", "x": 96, "y": 168, "width": 64, "height": 8, "periodSeconds": 2.0 }
  ],
  "exits": [
    { "x": 248, "y": 152, "width": 8, "height": 24, "target": "chamber-02", "requiresLanterns": false }
  ],
  "decor": [
    { "sprite": "art/lantern.png", "x": 8, "y": 8, "scale": 1 }
  ]
}
```

## Fields

| Field | Rules |
| ----- | ----- |
| `id` | Required, unique across the chamber set. |
| `name` | Required display name. |
| `width`, `height` | Must be 256x192. Defaults to those values. |
| `playerSpawn` | Required point inside the chamber. |
| `hub` | Optional boolean; true for the three entrance-hub chambers. Hub lantern state persists between rooms. |
| `boss` | Optional boolean; true for the final boss chamber. Boss rooms may have no exits. |
| `wizard` | Boss-only object: `x`, `y`, `width`, `height`, `buttonX`, `buttonY`, `buttonWidth`, `buttonHeight`, `fireballPeriodSeconds`. |
| `platforms` | Rectangles. `oneWay` is optional and defaults to `false`. |
| `vines` | Rectangles with `orientation` `vertical` or `horizontal`. |
| `lanterns` | `id` unique per chamber; point position; `required` defaults to `true`. |
| `enemySpawns` | `type` is `ninja` or `yamo`; point position. |
| `hazards` | `type` from the allowed list; rectangle; optional `periodSeconds` (> 0), `speed` (>= 0) and `directionX` (1 or -1). |
| `loopHazards` | Same shape as `hazards`; added to the chamber from loop 2 onward. |
| `exits` | Rectangle; `target` chamber id; `requiresLanterns` defaults to `true`; `requiresHub` defaults to `false`. |
| `decor` | `sprite` asset key; point position; integer `scale` >= 1. |

Allowed hazard types: `electric`, `mine`, `moving-wall`, `floor-light`, `dagger`, `bug`, `bush`, `fireball`.

## Validation

- JSON is strict: unknown fields are rejected. Comments and trailing commas are allowed.
- Every rectangle must have positive size and fit inside the chamber.
- Every point must be inside the chamber.
- At least one exit is required unless `boss` is true.
- A boss chamber requires a `wizard` object; wizard and button rectangles must fit inside the chamber.
- A non-boss chamber must not define a wizard.
- Lantern ids are unique per chamber.
- Chamber ids are unique across the set.
- Every exit target must exist in the set.
- Errors are aggregated with field paths such as `platforms[0]` and thrown as `InvalidDataException` by `ChamberLoader`.

## Loading

```csharp
var levelsDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "levels");
var chambers = ChamberLoader.LoadAll(levelsDirectory);
```

`LoadAll` returns chambers sorted by file name. `ChamberCatalog` keeps that order, exposes `First`, and resolves exit targets by id. Twenty chambers are committed: the three-room entrance hub (01-03), the first two underworld rooms (04-05), the mid-game chain (06-12), the late-game chain (13-19), and the Fire Wizard's chamber (20).

## Entrance hub and hatch (T-041)

- Chambers 01, 02 and 03 are marked `"hub": true`.
- Their required lanterns are pooled: `HubProgress` keeps a collected set per hub chamber, so lanterns stay collected when moving between hub rooms.
- Dying in a hub room resets only that room's lantern set and enemies; the other hub rooms keep their progress. Score and falls stay.
- Chamber-02 is the middle room. Its floor hatch is an exit with `"requiresLanterns": false, "requiresHub": true`; it stays locked until every required lantern in chambers 01-03 is collected.
- Side doors between 01-03 use `"requiresLanterns": false`, so the player can move around the hub freely while hunting lanterns.
- The hub HUD shows `HUB x/y` progress, or `HATCH OPEN` when all required hub lanterns are collected.

## Chamber-01: Entrance Hall

- Three required lanterns: one above each mid ledge and one above the top platform.
- The right-wall exit leads to chamber-02 and is always open.
- The central vine connects the ground to the top platform. The mid ledges are reached by dropping from the top; the player can return to the ground and climb again.
- The electric hazard entry is present in the data but inert until T-042.

## Chamber-02: Vine Hub

- Middle hub room with one required lantern above the left side of the ground.
- Left and right wall doors lead to chambers 01 and 03.
- A floor hatch at `(88, 168, 16, 8)` leads to chamber-04 once the whole hub is cleared.
- The full-height vine and mid ledge are optional traversal routes.

## Chamber-03: Pillar Room

- Left low ledge and right high ledge, each reached by its own vine.
- Two required lanterns, one above each ledge.
- The left-wall door returns to chamber-02.

## Chamber-04: Underhall

- Three vines: left and right to the mid ledges, centre to the top platform.
- Three required lanterns, one on each level.
- The right-wall exit opens after the three lanterns and leads to chamber-05.
- This is the first underworld chamber reached through the hub hatch.

## Chamber-05: Deep Vault

- Three staggered levels: left-low at y128, right-high at y96, top at y64, each served by a vine from the ground.
- Three required lanterns, one on each level.
- The right wall exit opens after the three lanterns and leads into chamber-06; the left door returns to chamber-04.

## Mid-game chambers (T-043)

The chain runs 05 → 06 → 07 → 08 → 09 → 10 → 11 → 12 → 01. Every chamber has a left back door that stays open and a right forward door that opens when its required lanterns are complete.

| Chamber | Name | Layout | Hazards | Enemies |
| ------- | ---- | ------ | ------- | ------- |
| 06 | Spark Gallery | Three vine tiers: left y128, middle y96, right y64 | Electric on the ground, PAN light on the middle platform | Ninja + Yamo |
| 07 | Crusher Walk | Two side vine ledges, optional centre jump lantern | Two sweeping crushers and a mine | Yamo + Ninja |
| 08 | Spore Garden | Centre vine, two mid ledges and a top platform | Two bushes and a bug | Ninja + Yamo |
| 09 | Blade Run | Three vines to left/right/top ledges | Dagger, sword and a PAN light | Ninja + Yamo |
| 10 | Furnace | Centre vine and three lantern tiers | Two fireballs, PAN light and electric arc | Yamo + Ninja |
| 11 | The Gauntlet | Three ascending vine tiers | Electric arc, mine, bush, crusher and bug | Ninja + Yamo |
| 12 | Underworks | Three-vine room with a centre vine and side ledges | Electric arc, dagger, bush and crusher | Ninja + Yamo |

- Each chamber has three required lanterns except 07, which has two required and one optional centre lantern.
- Chamber-12 leads into chamber-13; the late-game chain continues to the Fire Wizard in chamber-20.
- Hazards are placed off the spawn points so the player always has a safe starting position.

## Late-game chambers (T-044)

The chain runs 12 → 13 → 14 → 15 → 16 → 17 → 18 → 19 → 20. Back doors and lantern-gated forward doors follow the same rule as the mid-game.

| Chamber | Name | Layout | Hazards | Enemies |
| ------- | ---- | ------ | ------- | ------- |
| 13 | Iron Bridge | Three ascending vine tiers | Two crushers and a mine | Ninja + Yamo |
| 14 | Shock Gallery | Centre vine, two mid ledges and a top platform | PAN light, electric arc and a mine | Yamo + Ninja |
| 15 | Blade Spire | Three vine tiers | Daggers, a sword and a PAN light | Ninja + Yamo |
| 16 | Shock Chain | Three vine tiers | Three staggered electric/PAN lanes and a mine | Ninja + Yamo |
| 17 | Fire Walk | Centre vine, two mid ledges and a top platform | Three fireballs, a PAN light and a bush | Yamo + Ninja |
| 18 | Web of Traps | Three vine tiers | Electric arc, mine, bush, crusher, sword and bug | Ninja + Yamo |
| 19 | Last Gate | Three vine tiers plus an optional centre lantern | Electric, PAN platform light, mine, crusher, fireball and bug | Ninja + Yamo |

- All seven chambers have safe spawn areas and at least one safe waiting spot before every hazard band.
- Beam and projectile timings stay at T-042 values; no jump or hazard requires frame-perfect input.
- Chamber-16's `loopHazards` fill its two ground beam gaps from loop 2, removing the safe waiting spots and making the upper route the reliable path.
- Chamber-19's right exit opens after its lanterns and leads to chamber-20.

## Chamber-20: Fire Wizard

- Boss chamber: no lanterns, enemies or static hazards; `"boss": true`.
- The wizard sits above the centre platform at `(112, 56, 32, 32)`. He fires aimed fireballs from alternating eyes every 1.4 s, with a 0.3 s warning flash.
- The destruction button is on the left platform at `(24, 112, 16, 8)`. The player climbs the left vine, stands at the button and presses Attack.
- Pressing the button awards 3,000 points, clears all fireballs and starts a two-second defeat flash.
- After the defeat timer, the ending screen appears. Confirm advances to the next loop at chamber-01, keeping score and falls and resetting hub progress.
- The chamber has no exits; `ChamberValidator` allows this only for a boss room with a valid wizard definition.

## Progress and transitions

`ChamberProgress` owns the attempt state:

- collects any uncollected lantern whose 16x16 box intersects Bruce,
- computes required and optional counts from its set; hub chambers share the set owned by `HubProgress`,
- opens an exit when its required lanterns are complete, or immediately when `requiresLanterns` is false,
- additionally requires `hubHatchOpen` when the exit has `requiresHub`,
- finds an open exit whose rectangle intersects Bruce,
- resets the current chamber's lantern set on death.

`ChamberScreen` awards 125 per lantern and 2,000 when entering an exit, then replaces itself with the target chamber from `ChamberCatalog`. Score and falls live in `GameSession` and survive chamber changes and non-final deaths. `GameSession.StartNewRun` clears hub progress.

## Hazard placements (T-042)

| Chamber | Hazard | Position | Notes |
| ------- | ------ | -------- | ----- |
| 01 | Electric | `(96, 168, 64, 8)` | 2 s cycle on the centre floor |
| 03 | PAN floor light | `(96, 168, 64, 8)` | 2.5 s cycle |
| 04 | Mine | `(120, 168, 16, 8)` | Contact fuse on the ground |
| 04 | Dagger | `(240, 160, 8, 8)` | Speed 70, moves left |
| 05 | T'Sung-Lin bush | `(160, 160, 24, 16)` | One-shot explosion |
| 05 | Moving wall | `(104, 160, 48, 8)` | Speed 30, eight-pixel crusher sweeping the 48 px range |
| 05 | Bug | `(240, 160, 8, 8)` | Speed 50, moves left |

Sword and fireball use the shared projectile implementation and are reserved for later chambers and the wizard. All hazards reset with the chamber on a non-final death. Chambers 06-19 combine the same families; their per-room placements are listed in the mid-game and late-game tables above.
