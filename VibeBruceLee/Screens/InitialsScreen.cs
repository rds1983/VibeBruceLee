using System.Globalization;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Input;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class InitialsScreen : IGameScreen
{
	private const int MinLetter = 'A';
	private const int MaxLetter = 'Z';

	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font24;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly char[] _letters = ['A', 'A', 'A'];
	private int _index;
	private double _blink;

	public InitialsScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_font24 = context.FontSystem.GetFont(24);
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
		_blink += gameTime.ElapsedGameTime.TotalSeconds;

		if (input.WasPressed(InputAction.MoveLeft))
		{
			_index = (_index + 2) % _letters.Length;
		}
		else if (input.WasPressed(InputAction.MoveRight))
		{
			_index = (_index + 1) % _letters.Length;
		}

		if (input.WasPressed(InputAction.MoveUp))
		{
			ChangeLetter(1);
		}
		else if (input.WasPressed(InputAction.MoveDown))
		{
			ChangeLetter(-1);
		}

		if (input.WasPressed(InputAction.Confirm))
		{
			_context.HighScores.Add(new string(_letters), _context.Session.Score, _context.Session.Loop);
			_screens.Replace(new HighScoreScreen(_screens, _context));
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

		DrawCentered(spriteBatch, _font16, "NEW HIGH SCORE", 20, Palette.Yellow);
		DrawCentered(
			spriteBatch,
			_font8,
			"SCORE " + _context.Session.Score.ToString("D6", CultureInfo.InvariantCulture),
			48,
			Palette.White);
		DrawCentered(spriteBatch, _font8, "ENTER YOUR INITIALS", 66, Palette.Cyan);

		for (var i = 0; i < _letters.Length; i++)
		{
			var color = i == _index ? Palette.Yellow : Palette.White;
			spriteBatch.DrawString(
				_font24,
				_letters[i].ToString(),
				new Vector2(88 + (i * 32), 82),
				color,
				layerDepth: SpriteLayers.Hud);
		}

		if (((int)(_blink * 3) & 1) == 0)
		{
			spriteBatch.Draw(
				_context.Pixel,
				new Rectangle(88 + (_index * 32), 108, 24, 2),
				null,
				Palette.Yellow,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				SpriteLayers.Hud);
		}

		DrawCentered(spriteBatch, _font8, "UP/DOWN: LETTER", 122, Palette.White);
		DrawCentered(spriteBatch, _font8, "LEFT/RIGHT: MOVE", 132, Palette.White);
		DrawCentered(spriteBatch, _font8, "ENTER: SAVE", 142, Palette.White);
		DrawCentered(spriteBatch, _font8, "ESC: QUIT", 168, Palette.Muted);
	}

	private void ChangeLetter(int direction)
	{
		var letter = _letters[_index] + direction;
		if (letter > MaxLetter)
		{
			letter = MinLetter;
		}
		else if (letter < MinLetter)
		{
			letter = MaxLetter;
		}

		_letters[_index] = (char)letter;
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
