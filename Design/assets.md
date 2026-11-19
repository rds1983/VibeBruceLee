# Assets

All runtime assets live under `VibeBruceLee/Assets` and are copied to `Assets` next to the executable (`PreserveNewest`). `source/` and README files stay in the repository and are not shipped. The game has no MonoGame content pipeline: XNAssets reads the raw files, and FontStashSharp renders text directly.

## Folder layout

```
art/       Sprites, tiles, backgrounds, test cards
audio/     Sound effects and music
fonts/     TTF/OTF fonts for FontStashSharp
levels/    Chamber definitions (JSON, from T-013)
licenses/  License texts for third-party assets
source/    Editable sources for generated assets
```

Folder names are lowercase. Use the same casing on Windows and Linux; asset keys are case-sensitive.

## Asset keys

- Relative to the asset root, forward slashes only: `art/test-card.png`.
- Lowercase with digits, `.`, `_` or `-`; kebab-case is preferred.
- The extension is required.
- Avoid leading slashes, drive letters, `..` segments and absolute paths in game code.
- Keys go straight to XNAssets. It normalizes separators and also supports rooted, absolute and `..` paths; the conventions above keep our asset root self-contained and portable.

Examples:

| Use | Avoid |
| --- | ----- |
| `art/test-card.png` | `Assets/art/test-card.png` |
| `fonts/press-start-2p-regular.ttf` | `C:\Assets\font.ttf` |
| `levels/chamber-01.json` | `../secrets.txt` |

## Loading

The game uses the library types directly; there are no wrapper classes over XNAssets or FontStashSharp.

```csharp
var assets = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));

var texture = assets.LoadTexture2D(GraphicsDevice, "art/test-card.png");
var fontSystem = assets.LoadFontSystem("fonts/press-start-2p-regular.ttf");

spriteBatch.DrawString(fontSystem.GetFont(16), "VibeBruceLee", position, color);
```

Callers own loaded textures and `FontSystem` instances and must dispose them. Keep the `AssetManager` alive for as long as the game loads assets so its cache stays useful.

## Failure diagnostics

XNAssets raises `AssetNotFoundException` with the resolved `AssetPath` when a key cannot be found. Decode and loader errors come straight from XNAssets or its decoders, with the resolved path in the call stack. There is no custom validation layer.

```
Asset not found: 'D:\...\Assets\art\missing.png'
```

A missing asset is a programming error and fails fast at load time rather than rendering a placeholder.

## Hot reload

Not implemented. XNAssets caches loaded assets, and invalidating the cache safely is not worth the complexity yet. Restart the game after changing assets. Revisit when the screen stack and editor workflow exist (T-010+).

## Current assets

| Key | Source | Notes |
| --- | ------ | ----- |
| `art/test-card.png` | Generated for this project by `source/art/generate-test-card.ps1` | 64x48 pixel test card used by the T-004 scene; not a final game asset |
| `fonts/press-start-2p-regular.ttf` | Press Start 2P by CodeMan38, SIL Open Font License 1.1 | Bootstrap pixel font; final UI may extend or replace it in T-053 |
| `licenses/press-start-2p-OFL.txt` | Font license text | Must ship with the font |

## Rules

- All shipped art, audio and fonts are original or compatibly licensed. The 1984 game is reference only.
- Never commit reference maps, screenshots, sprites or music from the original.
- Third-party assets need a license that allows MIT distribution; keep the license text in `licenses/` and record it here.
- Keep editable sources for generated assets in `source/` and document regeneration steps.
- Keep asset files small; no build output or temporary exports.
