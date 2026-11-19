using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Animation;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Screens;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class ChamberScreen : IGameScreen
{
	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly AnimationPlayer _brucePlayer = new();
	private readonly AnimationPlayer _ninjaPlayer = new();
	private readonly AnimationPlayer _yamoPlayer = new();
	private readonly AnimationPlayer _lanternPlayer = new();

	public ChamberScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_font16 = context.FontSystem.GetFont(16);
		_font8 = context.FontSystem.GetFont(8);
		_brucePlayer.Play(ScreenAnimations.BruceRun);
		_ninjaPlayer.Play(ScreenAnimations.NinjaIdle);
		_yamoPlayer.Play(ScreenAnimations.YamoIdle);
		_lanternPlayer.Play(ScreenAnimations.Lantern);
	}

	public void Activate()
	{
	}

	public void Deactivate()
	{
	}

	public void Update(GameTime gameTime, InputSystem input)
	{
		var seconds = gameTime.ElapsedGameTime.TotalSeconds;
		_brucePlayer.Update(seconds);
		_ninjaPlayer.Update(seconds);
		_yamoPlayer.Update(seconds);
		_lanternPlayer.Update(seconds);

		if (input.WasPressed(InputAction.Pause))
		{
			_screens.Push(new PauseScreen(_screens, _context));
		}
		else if (input.WasPressed(InputAction.DebugGameOver))
		{
			_screens.Push(new GameOverScreen(_screens, _context));
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(
			_context.Pixel,
			new Rectangle(0, 0, GameInfo.VirtualWidth, GameInfo.VirtualHeight),
			null,
			Palette.Background,
			0f,
			Vector2.Zero,
			SpriteEffects.None,
			SpriteLayers.Background);

		for (var x = 0; x < GameInfo.VirtualWidth; x += 8)
		{
			_context.Tiles.Draw(spriteBatch, 0, new Vector2(x, 160), 1, Color.White, SpriteEffects.None, SpriteLayers.World);
			_context.Tiles.Draw(spriteBatch, 1, new Vector2(x, 168), 1, Color.White, SpriteEffects.None, SpriteLayers.World);
		}

		_context.Bruce.Draw(
			spriteBatch,
			_brucePlayer.CurrentFrame,
			new Vector2(32, 136),
			1,
			Color.White,
			SpriteEffects.None,
			SpriteLayers.World);

		_context.Ninja.Draw(
			spriteBatch,
			_ninjaPlayer.CurrentFrame,
			new Vector2(96, 136),
			1,
			Color.White,
			SpriteEffects.None,
			SpriteLayers.World);

		_context.Yamo.Draw(
			spriteBatch,
			_yamoPlayer.CurrentFrame,
			new Vector2(184, 132),
			1,
			Color.White,
			SpriteEffects.None,
			SpriteLayers.World);

		_context.LanternSheet.Draw(
			spriteBatch,
			_lanternPlayer.CurrentFrame,
			new Vector2(208, 24),
			2,
			Color.White,
			SpriteEffects.None,
			SpriteLayers.World);

		spriteBatch.Draw(
			_context.Pixel,
			new Rectangle(0, 0, GameInfo.VirtualWidth, 12),
			null,
			Color.Black,
			0f,
			Vector2.Zero,
			SpriteEffects.None,
			SpriteLayers.Hud);

		spriteBatch.Draw(_context.HudLife, new Rectangle(4, 2, 8, 8), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.Hud);
		spriteBatch.Draw(_context.HudLantern, new Rectangle(16, 2, 8, 8), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "000000", new Vector2(30, 2), Palette.White, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "FALLS 5", new Vector2(196, 2), Palette.White, layerDepth: SpriteLayers.Hud);

		spriteBatch.DrawString(_font16, "CHAMBER 1", new Vector2(8, 16), Palette.Yellow, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "P: pause   G: game over", new Vector2(8, 176), Palette.Muted, layerDepth: SpriteLayers.Hud);

		if (_context.Debug.OverlayVisible)
		{
			_context.Debug.AddSolid(new Rectangle(0, 160, GameInfo.VirtualWidth, 16));
			_context.Debug.AddHitbox(new Rectangle(32, 136, 16, 24));
			_context.Debug.AddHurtbox(new Rectangle(96, 136, 16, 24));
			_context.Debug.AddHurtbox(new Rectangle(184, 132, 24, 28));
		}
	}
}
