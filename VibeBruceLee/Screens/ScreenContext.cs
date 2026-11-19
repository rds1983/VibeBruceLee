using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Debugging;
using VibeBruceLee.Core.Graphics;

namespace VibeBruceLee.Screens;

/// <summary>
/// Shared, game-owned assets handed to screens. The game disposes them.
/// </summary>
public sealed class ScreenContext
{
	public ScreenContext(
		FontSystem fontSystem,
		SpriteSheet bruce,
		SpriteSheet ninja,
		SpriteSheet yamo,
		SpriteSheet tiles,
		SpriteSheet lanternSheet,
		Texture2D hudLife,
		Texture2D hudLantern,
		Texture2D pixel,
		DebugState debug)
	{
		ArgumentNullException.ThrowIfNull(fontSystem);
		ArgumentNullException.ThrowIfNull(bruce);
		ArgumentNullException.ThrowIfNull(ninja);
		ArgumentNullException.ThrowIfNull(yamo);
		ArgumentNullException.ThrowIfNull(tiles);
		ArgumentNullException.ThrowIfNull(lanternSheet);
		ArgumentNullException.ThrowIfNull(hudLife);
		ArgumentNullException.ThrowIfNull(hudLantern);
		ArgumentNullException.ThrowIfNull(pixel);
		ArgumentNullException.ThrowIfNull(debug);

		FontSystem = fontSystem;
		Bruce = bruce;
		Ninja = ninja;
		Yamo = yamo;
		Tiles = tiles;
		LanternSheet = lanternSheet;
		HudLife = hudLife;
		HudLantern = hudLantern;
		Pixel = pixel;
		Debug = debug;
	}

	public FontSystem FontSystem { get; }

	public SpriteSheet Bruce { get; }

	public SpriteSheet Ninja { get; }

	public SpriteSheet Yamo { get; }

	public SpriteSheet Tiles { get; }

	public SpriteSheet LanternSheet { get; }

	public Texture2D HudLife { get; }

	public Texture2D HudLantern { get; }

	public Texture2D Pixel { get; }

	public DebugState Debug { get; }
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
