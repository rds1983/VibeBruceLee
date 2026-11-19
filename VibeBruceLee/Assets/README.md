# Game assets

All runtime assets live in this folder. The project copies the runtime folders to the build output unchanged and loads files with XNAssets directly. Readmes stay in the repository. See `Design/assets.md` for key rules, diagnostics and the asset pipeline.

```
art/       Sprites, tiles and backgrounds
audio/     Sound effects and music
fonts/     Fonts used by FontStashSharp
levels/    Chamber definitions
licenses/  License texts for third-party assets
```

Generated assets come from `Tools/VibeBruceLee.AssetGen`. Regenerate from the repository root:

```sh
dotnet run --project Tools/VibeBruceLee.AssetGen -- VibeBruceLee/Assets
```

Current bootstrap assets:

- `art/test-card.png` - 64x48 asset-pipeline test card.
- `art/lantern.png` - 16x16 lantern sample sprite.
- `audio/pickup.wav` - 0.12 s pickup beep.
- `fonts/press-start-2p-regular.ttf` - Press Start 2P by CodeMan38, SIL Open Font License 1.1.
- `licenses/press-start-2p-OFL.txt` - the font license.

Everything shipped is original to this project or compatibly licensed. The 1984 game is used only as a reference; no original sprites, audio or fonts are redistributed.
