using System.Globalization;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Input;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class HighScoreScreen : IGameScreen
{
	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;

	public HighScoreScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_font16 = context.FontSystem.GetFont(16);
		_font8 = context.FontSystem.GetFont(8);
	}

	public void Activate()
	{
	}

	public void Deactivate()
	{
	}

	public void Update(GameTime gameTime, InputSystem input)
	{
		if (input.WasPressed(InputAction.Confirm))
		{
			_screens.Replace(new TitleScreen(_screens, _context));
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

		DrawCentered(spriteBatch, _font16, "HIGH SCORES", 16, Palette.Yellow);

		var entries = _context.HighScores.Entries;
		for (var i = 0; i < entries.Count; i++)
		{
			var entry = entries[i];
			var text = (i + 1).ToString(CultureInfo.InvariantCulture) +
				"  " + entry.Initials +
				"  " + entry.Score.ToString("D6", CultureInfo.InvariantCulture) +
				"  L" + entry.Loop.ToString(CultureInfo.InvariantCulture);
			spriteBatch.DrawString(
				_font8,
				text,
				new Vector2(48, 54 + (i * 14)),
				Palette.White,
				layerDepth: SpriteLayers.Hud);
		}

		if (entries.Count == 0)
		{
			DrawCentered(spriteBatch, _font8, "NO SCORES YET", 96, Palette.Muted);
		}

		DrawCentered(spriteBatch, _font8, "ENTER: TITLE", 152, Palette.White);
		DrawCentered(spriteBatch, _font8, "ESC: QUIT", 170, Palette.Muted);
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
