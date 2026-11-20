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

- `art/bruce.png` - 20-frame Bruce animation sheet.
- `art/ninja.png` - 14-frame Ninja sheet.
- `art/yamo.png` - 12-frame Yamo sheet.
- `art/tiles.png` - 4-tile 8x8 atlas.
- `art/backdrop.png` - 256x192 temple wall.
- `art/portraits.png` - Bruce/Ninja/Yamo 32x32 portraits.
- `art/wizard.png` - two 32x32 wizard frames.
- `art/hud-life.png`, `art/hud-lantern.png` - HUD icons.
- `art/test-card.png` - 64x48 pipeline/debug test card.
- `art/lantern.png` - 16x16 lantern sample sprite.
- `art/lantern-sheet.png` - 64x16 four-frame lantern animation.
- `audio/pickup.wav` - lantern pickup beep.
- `audio/jump.wav`, `land.wav`, `chop.wav`, `kick.wav`, `hit.wav`, `knockdown.wav`, `death.wav`, `respawn.wav`, `exit.wav`, `fireball.wav`, `wizard-defeat.wav` - generated effects.
- `levels/chamber-01.json` to `chamber-20.json` - chamber data.
- `fonts/press-start-2p-regular.ttf` - Press Start 2P by CodeMan38, SIL Open Font License 1.1.
- `licenses/press-start-2p-OFL.txt` - the font license.

Everything shipped is original to this project or compatibly licensed. The 1984 game is used only as a reference; no original sprites, audio or fonts are redistributed.
