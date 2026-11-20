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
    { "x": 248, "y": 152, "width": 8, "height": 24, "target": "chamber-02" }
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
| `platforms` | Rectangles. `oneWay` is optional and defaults to `false`. |
| `vines` | Rectangles with `orientation` `vertical` or `horizontal`. |
| `lanterns` | `id` unique per chamber; point position; `required` defaults to `true`. |
| `enemySpawns` | `type` is `ninja` or `yamo`; point position. |
| `hazards` | `type` from the allowed list; rectangle; optional `periodSeconds` (> 0) and `speed` (>= 0). |
| `exits` | Rectangle; `target` chamber id; `requiresLanterns` defaults to `true`. |
| `decor` | `sprite` asset key; point position; integer `scale` >= 1. |

Allowed hazard types: `electric`, `mine`, `moving-wall`, `floor-light`, `dagger`, `bug`, `bush`, `fireball`.

## Validation

- JSON is strict: unknown fields are rejected. Comments and trailing commas are allowed.
- Every rectangle must have positive size and fit inside the chamber.
- Every point must be inside the chamber.
- At least one exit is required.
- Lantern ids are unique per chamber.
- Chamber ids are unique across the set.
- Every exit target must exist in the set.
- Errors are aggregated with field paths such as `platforms[0]` and thrown as `InvalidDataException` by `ChamberLoader`.

## Loading

```csharp
var levelsDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "levels");
var chambers = ChamberLoader.LoadAll(levelsDirectory);
```

`LoadAll` returns chambers sorted by file name. `ChamberCatalog` keeps that order, exposes `First`, and resolves exit targets by id. The two committed files are a first pass; chamber-01 is the T-040 playable entrance slice, and chamber-02 is a temporary one-lantern return room until T-041 replaces it with the real entrance hub.

## Chamber-01: Entrance Hall (T-040)

- Three required lanterns: one above each mid ledge and one above the top platform.
- The exit is a door on the right wall at `(248, 152, 8, 24)`, reachable from the ground floor. It stays locked until all three required lanterns are collected.
- The central vine connects the ground to the top platform. The mid ledges are reached by dropping from the top; the player can return to the ground and climb again.
- The electric hazard entry is present in the data but inert until T-042.
- On a fall, lanterns and enemies reset and the exit closes again. Score and falls are kept. Losing the last fall opens the game-over screen.

## Chamber-02: Vine Shaft (temporary)

- A one-lantern return room used to exercise the chamber transition before T-041.
- The lantern is above the left side of the ground; the vine and mid ledge remain from the format sample.
- The right-wall exit returns to chamber-01, producing a two-chamber loop.
- T-041 replaces this with the real second entrance chamber and the three-room hub.

## Progress and transitions

`ChamberProgress` owns the attempt state:

- collects any uncollected lantern whose 16x16 box intersects Bruce,
- tracks required and optional counts separately,
- opens an exit when its required lanterns are complete, or immediately when `requiresLanterns` is false,
- finds an open exit whose rectangle intersects Bruce,
- resets all lantern state on death.

`ChamberScreen` awards 125 per lantern and 2,000 when entering an exit, then replaces itself with the target chamber from `ChamberCatalog`. Score and falls live in `GameSession` and survive chamber changes and non-final deaths.
