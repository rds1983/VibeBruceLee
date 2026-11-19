using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBruceLee.Screens;

/// <summary>
/// Shared, game-owned assets handed to screens. The game disposes them.
/// </summary>
public sealed class ScreenContext
{
	public ScreenContext(FontSystem fontSystem, Texture2D testCard, Texture2D lantern, Texture2D pixel)
	{
		ArgumentNullException.ThrowIfNull(fontSystem);
		ArgumentNullException.ThrowIfNull(testCard);
		ArgumentNullException.ThrowIfNull(lantern);
		ArgumentNullException.ThrowIfNull(pixel);

		FontSystem = fontSystem;
		TestCard = testCard;
		Lantern = lantern;
		Pixel = pixel;
	}

	public FontSystem FontSystem { get; }

	public Texture2D TestCard { get; }

	public Texture2D Lantern { get; }

	public Texture2D Pixel { get; }
}

public static class Palette
{
	public static readonly Color Background = new(12, 12, 20);
	public static readonly Color Yellow = new(255, 216, 0);
	public static readonly Color Cyan = new(0, 216, 216);
	public static readonly Color Magenta = new(216, 0, 216);
	public static readonly Color Muted = new(160, 160, 160);
	public static readonly Color White = Color.White;
}
