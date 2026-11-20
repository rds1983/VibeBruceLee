using System.Globalization;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Input;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class GameOverScreen : IGameScreen
{
	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly bool _isNewHighScore;

	public GameOverScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_font16 = context.FontSystem.GetFont(16);
		_font8 = context.FontSystem.GetFont(8);
		_isNewHighScore = context.HighScores.Qualifies(context.Session.Score);
	}

	public void Activate()
	{
	}

	public void Deactivate()
	{
	}

	public void Update(GameTime gameTime, InputSystem input)
	{
		if (!input.WasPressed(InputAction.Confirm))
		{
			return;
		}

		if (_isNewHighScore)
		{
			_screens.Push(new InitialsScreen(_screens, _context));
		}
		else
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

		DrawCentered(spriteBatch, _font16, "GAME OVER", 16, Palette.Yellow);
		DrawCentered(
			spriteBatch,
			_font8,
			"SCORE " + _context.Session.Score.ToString("D6", CultureInfo.InvariantCulture),
			36,
			Palette.White);

		if (_isNewHighScore)
		{
			DrawCentered(spriteBatch, _font8, "NEW HIGH SCORE", 52, Palette.Yellow);
		}

		DrawCentered(spriteBatch, _font8, "HIGH SCORES", 72, Palette.Cyan);
		DrawTable(spriteBatch);

		DrawCentered(
			spriteBatch,
			_font8,
			_isNewHighScore ? "ENTER: INITIALS" : "ENTER: TITLE",
			150,
			Palette.White);
		DrawCentered(spriteBatch, _font8, "ESC: QUIT", 166, Palette.Muted);
	}

	private void DrawTable(SpriteBatch spriteBatch)
	{
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
				new Vector2(48, 84 + (i * 12)),
				Palette.White,
				layerDepth: SpriteLayers.Hud);
		}
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
