using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Animation;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Screens;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class TitleScreen : IGameScreen
{
	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly AnimationPlayer _lanternPlayer = new();

	public TitleScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_font16 = context.FontSystem.GetFont(16);
		_font8 = context.FontSystem.GetFont(8);
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
		_lanternPlayer.Update(gameTime.ElapsedGameTime.TotalSeconds);

		if (input.WasPressed(InputAction.Confirm))
		{
			_screens.Push(new ChamberScreen(_screens, _context));
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

		spriteBatch.Draw(
			_context.TestCard,
			new Rectangle(16, 56, 128, 96),
			null,
			Color.White,
			0f,
			Vector2.Zero,
			SpriteEffects.None,
			SpriteLayers.World);

		_context.LanternSheet.Draw(
			spriteBatch,
			_lanternPlayer.CurrentFrame,
			new Vector2(168, 72),
			4,
			Color.White,
			SpriteEffects.None,
			SpriteLayers.World);

		if (_context.Debug.OverlayVisible)
		{
			_context.Debug.AddSolid(new Rectangle(16, 56, 128, 96));
			_context.Debug.AddHitbox(new Rectangle(168, 72, 64, 64));
		}

		spriteBatch.DrawString(_font16, "VibeBruceLee", new Vector2(16, 12), Palette.Yellow, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "screen stack online", new Vector2(16, 36), Palette.Cyan, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "Enter: start", new Vector2(16, 160), Palette.White, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "F11: fullscreen  Esc: quit", new Vector2(16, 176), Palette.Muted, layerDepth: SpriteLayers.Hud);
	}
}
