using Microsoft.Xna.Framework;

namespace VibeBruceLee.Presentation;

/// <summary>
/// Integer-scaled, center-aligned placement of the 256x192 logical canvas
/// inside the window. Any leftover space is letterboxed.
/// </summary>
public static class Letterbox
{
	public static ViewportLayout Calculate(int windowWidth, int windowHeight)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowWidth);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowHeight);

		var scale = Math.Max(1, Math.Min(
			windowWidth / GameInfo.VirtualWidth,
			windowHeight / GameInfo.VirtualHeight));

		var width = GameInfo.VirtualWidth * scale;
		var height = GameInfo.VirtualHeight * scale;

		return new ViewportLayout(
			scale,
			new Rectangle(
				(windowWidth - width) / 2,
				(windowHeight - height) / 2,
				width,
				height));
	}
}

public readonly record struct ViewportLayout(int Scale, Rectangle Destination);
