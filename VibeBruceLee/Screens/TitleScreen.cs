using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Animation;
using VibeBruceLee.Input;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class TitleScreen : IGameScreen
{
	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _logo;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly AnimationPlayer _lanternPlayer = new();
	private double _blinkTime;

	public TitleScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_logo = context.FontSystem.GetFont(24);
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
		_blinkTime += gameTime.ElapsedGameTime.TotalSeconds;

		if (input.WasPressed(InputAction.Controls))
		{
			_screens.Push(new ControlsScreen(_screens, _context));
		}
		else if (input.WasPressed(InputAction.Confirm))
		{
			_context.Session.StartNewRun();
			_screens.Push(new ChamberScreen(_screens, _context, _context.Chambers.First));
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

		_context.Portraits.Draw(
			spriteBatch,
			0,
			new Vector2(8, 134),
			1,
			Color.White,
			SpriteEffects.None,
			SpriteLayers.World);

		_context.LanternSheet.Draw(
			spriteBatch,
			_lanternPlayer.CurrentFrame,
			new Vector2(216, 142),
			2,
			Color.White,
			SpriteEffects.None,
			SpriteLayers.World);

		DrawCentered(spriteBatch, _font16, "VIBE", 8, Palette.Cyan);
		DrawCentered(spriteBatch, _logo, "BRUCE LEE", 24, Palette.Yellow);
		DrawCentered(spriteBatch, _font8, "THE FORTRESS OF THE FIRE WIZARD", 52, Palette.Muted);

		DrawCentered(spriteBatch, _font8, "THE FIRE WIZARD HAS TAKEN", 66, Palette.White);
		DrawCentered(spriteBatch, _font8, "THE FORTRESS. GATHER THE", 76, Palette.White);
		DrawCentered(spriteBatch, _font8, "LANTERNS, THEN DESTROY HIM.", 86, Palette.White);

		if (((int)(_blinkTime * 2.5) & 1) == 0)
		{
			DrawCentered(spriteBatch, _font16, "PRESS ENTER", 96, Palette.Yellow);
		}

		DrawCentered(spriteBatch, _font8, "ENTER / A: START", 114, Palette.Cyan);
		DrawCentered(spriteBatch, _font8, "C / B: CONTROLS", 124, Palette.Cyan);
		DrawCentered(
			spriteBatch,
			_font8,
			"TOP " + _context.HighScores.TopScore.ToString("D6", System.Globalization.CultureInfo.InvariantCulture),
			162,
			Palette.White);
		DrawCentered(spriteBatch, _font8, "F2 SOUND  F11 FULL  ESC QUIT", 180, Palette.Muted);
	}

	private static void DrawCentered(SpriteBatch spriteBatch, SpriteFontBase font, string text, float y, Color color)
	{
		var size = font.MeasureString(text);
		spriteBatch.DrawString(
			font,
			text,
			new Vector2((GameInfo.VirtualWidth - size.X) / 2, y),
			color,
			layerDepth: SpriteLayers.Hud);
	}
}
