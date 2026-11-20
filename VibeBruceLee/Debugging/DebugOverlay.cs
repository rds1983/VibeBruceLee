using System.Globalization;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Debugging;

public sealed class DebugOverlay
{
	private const float PanelDepth = 0.01f;
	private const float RectDepth = 0.02f;

	private readonly DebugState _state;
	private readonly Texture2D _pixel;
	private readonly SpriteFontBase _font;
	private float _fps;
	private long _frames;

	public DebugOverlay(FontSystem fontSystem, Texture2D pixel, DebugState state)
	{
		ArgumentNullException.ThrowIfNull(fontSystem);
		ArgumentNullException.ThrowIfNull(pixel);
		ArgumentNullException.ThrowIfNull(state);

		_state = state;
		_pixel = pixel;
		_font = fontSystem.GetFont(8);
	}

	public void Update(double elapsedSeconds)
	{
		_frames++;

		if (elapsedSeconds <= 0)
		{
			return;
		}

		var current = (float)(1.0 / elapsedSeconds);
		_fps = _fps == 0 ? current : (_fps * 0.9f) + (current * 0.1f);
	}

	public void Draw(SpriteBatch spriteBatch, string screenName)
	{
		ArgumentNullException.ThrowIfNull(spriteBatch);
		ArgumentNullException.ThrowIfNull(screenName);

		if (!_state.OverlayVisible)
		{
			return;
		}

		foreach (var rect in _state.Rects)
		{
			Outline(spriteBatch, rect.Bounds, rect.Color);
		}

		spriteBatch.Draw(
			_pixel,
			new Rectangle(0, 0, GameInfo.VirtualWidth, 64),
			null,
			Color.Black,
			0f,
			Vector2.Zero,
			SpriteEffects.None,
			PanelDepth);

		var y = 2f;
		DrawLine(spriteBatch, $"FPS {_fps.ToString("0.0", CultureInfo.InvariantCulture)}  FRAME {_frames}", ref y);
		DrawLine(spriteBatch, $"SCREEN {screenName}", ref y);

		var status = _state.Paused ? "PAUSED" : _state.SlowMotion ? "SLOW 1/4" : "RUN";
		var god = _state.Invincible ? "GOD ON" : "GOD OFF";
		DrawLine(spriteBatch, $"{status}  {god}  RECTS {_state.Rects.Count}", ref y);
		DrawLine(spriteBatch, "F1 overlay  F3 slow  F4 pause", ref y);
		DrawLine(spriteBatch, "F5 step  F6 god  F7 skip", ref y);
	}

	private void DrawLine(SpriteBatch spriteBatch, string text, ref float y)
	{
		spriteBatch.DrawString(_font, text, new Vector2(3, y), Color.White, layerDepth: SpriteLayers.Ui);
		y += 10;
	}

	private void Outline(SpriteBatch spriteBatch, Rectangle bounds, Color color)
	{
		spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), null, color, 0f, Vector2.Zero, SpriteEffects.None, RectDepth);
		spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), null, color, 0f, Vector2.Zero, SpriteEffects.None, RectDepth);
		spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), null, color, 0f, Vector2.Zero, SpriteEffects.None, RectDepth);
		spriteBatch.Draw(_pixel, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), null, color, 0f, Vector2.Zero, SpriteEffects.None, RectDepth);
	}
}
