# ADR-0002: Raw asset pipeline with XNAssets and FontStashSharp

- **Status:** Accepted
- **Date:** 2026-11-20
- **Deciders:** Developer, implementing agent

## Context

The game needs textures, fonts, sounds and chamber data. The stack mandates MonoGame, XNAssets and FontStashSharp. The MonoGame content pipeline adds `.mgcb` build steps, tool dependencies and platform friction, while XNAssets is designed to load raw files. We also need clear failure diagnostics and testable code, without adding layers over libraries that are already small.

## Decision

1. **Raw assets:** keep assets under `VibeBruceLee/Assets`, copy them to `Assets` next to the executable, and load them with XNAssets. Do not use the MonoGame content pipeline.
2. **Direct library use:** game code creates `AssetManager.CreateFileAssetManager` and uses XNAssets and FontStashSharp types directly (`Texture2D`, `FontSystem`, `SpriteBatch.DrawString`). Do not wrap them in services or facades.
3. **Keys are conventions:** pass relative keys such as `art/test-card.png` straight to XNAssets. It normalizes separators and resolves paths; avoid rooted or absolute paths and `..` in game code so the asset root stays self-contained. Do not add a key-validation layer. Missing assets raise XNAssets' `AssetNotFoundException` with the resolved path.
4. **Text:** load TTF files through `AssetManager.LoadFontSystem`, then call `FontSystem.GetFont` and draw with FontStashSharp extensions directly.
5. **Bootstrap assets:** a generated 64x48 test card and Press Start 2P (SIL OFL 1.1, license shipped) prove the pipeline. Final art and fonts arrive in T-016 and T-053.
6. **No hot reload for now:** assets are cached by XNAssets; restart after changes. Revisit if the workflow demands it.
7. **Fail fast:** missing or invalid assets throw at load time; no silent placeholders.

## Consequences

- Adding an asset means dropping a file into the right folder and using its relative key; no pipeline rebuild.
- Linux and Windows resolve the same keys because separators are normalized and folders are lowercase.
- Key errors are actionable without a debugger; XNAssets reports the resolved path and decode errors surface directly.
- There are fewer layers and fewer types to maintain; call sites own their textures and `FontSystem` instances and must dispose them.
- Nothing stops game code from passing an absolute or traversing key; this is an internal code convention, not a security boundary.
- Loaded assets are cached for the process lifetime; keep the `AssetManager` alive. Hot reload requires a restart.
- Font rendering depends on fine-grained glyph rasterization; keep font sizes and use integer scales for pixel art.
- There is no asset logic worth unit-testing; the smoke scene and normal runs verify loading, and tests focus on game logic instead.

## Alternatives considered

- **MonoGame content pipeline:** rejected for build complexity, extra tooling and platform-specific output.
- **Wrapper services over XNAssets and FontStashSharp:** rejected as unnecessary indirection; the library APIs are small and diagnostics already include the path.
- **A key-validation layer:** rejected for the same reason; internal asset keys are code constants, not user input.
- **Direct `Texture2D.FromStream` / `File.ReadAllBytes`:** works, but duplicates XNAssets and loses its caching and format support.
- **Embedded resources:** awkward for user-editable levels and asset iteration.
- **System fonts:** licensing and availability vary by machine; an OFL font ships deterministically.

## Related

- `Design/assets.md`
- `Design/architecture.md`
- ADR-0001 (engine and resolution)
