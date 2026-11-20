using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Input;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class ControlsScreen : IGameScreen
{
	public bool IsOverlay => false;

	private static readonly string[] Lines =
	[
		"MOVE      ARROWS OR DPAD",
		"JUMP      X/SPACE OR A",
		"ATTACK    Z/M OR X",
		"CLIMB UP  Q/UP OR STICK UP",
		"DUCK      A/DOWN OR STICK DOWN",
		"PAUSE     ENTER OR START",
		"CONTROLS  C OR B",
		"SOUND     F2 OR Y",
		"FULLSCREEN F11"
	];

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;

	public ControlsScreen(ScreenStack screens, ScreenContext context)
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
		if (input.WasPressed(InputAction.Confirm) || input.WasPressed(InputAction.Controls))
		{
			_screens.Pop();
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

		DrawCentered(spriteBatch, _font16, "CONTROLS", 8, Palette.Yellow);
		DrawCentered(spriteBatch, _font8, "KEYBOARD OR GAMEPAD", 28, Palette.Cyan);

		var y = 44f;
		foreach (var line in Lines)
		{
			spriteBatch.DrawString(_font8, line, new Vector2(8, y), Palette.White, layerDepth: SpriteLayers.Hud);
			y += 11;
		}

		DrawCentered(spriteBatch, _font8, "ENTER OR C: BACK", 164, Palette.Cyan);
		DrawCentered(spriteBatch, _font8, "ESC: QUIT", 178, Palette.Muted);
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
