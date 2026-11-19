using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBruceLee.Core.Graphics;

public readonly struct SpriteSheetLayout
{
	public SpriteSheetLayout(int textureWidth, int textureHeight, int frameWidth, int frameHeight)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(textureWidth);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(textureHeight);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameWidth);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameHeight);

		if (textureWidth % frameWidth != 0 || textureHeight % frameHeight != 0)
		{
			throw new ArgumentException("Frame size must divide the texture size exactly.");
		}

		FrameWidth = frameWidth;
		FrameHeight = frameHeight;
		Columns = textureWidth / frameWidth;
		Rows = textureHeight / frameHeight;
	}

	public int FrameWidth { get; }

	public int FrameHeight { get; }

	public int Columns { get; }

	public int Rows { get; }

	public int FrameCount => Columns * Rows;

	public Rectangle GetSourceRectangle(int frameIndex)
	{
		if (frameIndex < 0 || frameIndex >= FrameCount)
		{
			throw new ArgumentOutOfRangeException(nameof(frameIndex), $"Frame {frameIndex} is outside 0..{FrameCount - 1}.");
		}

		var column = frameIndex % Columns;
		var row = frameIndex / Columns;
		return new Rectangle(column * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
	}
}

/// <summary>
/// A texture split into equally sized frames in row-major order.
/// </summary>
public sealed class SpriteSheet
{
	public SpriteSheet(Texture2D texture, int frameWidth, int frameHeight)
	{
		ArgumentNullException.ThrowIfNull(texture);

		Texture = texture;
		Layout = new SpriteSheetLayout(texture.Width, texture.Height, frameWidth, frameHeight);
	}

	public Texture2D Texture { get; }

	public SpriteSheetLayout Layout { get; }

	public void Draw(
		SpriteBatch spriteBatch,
		int frameIndex,
		Vector2 position,
		int scale,
		Color color,
		SpriteEffects effects,
		float layerDepth)
	{
		ArgumentNullException.ThrowIfNull(spriteBatch);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(scale);

		var source = Layout.GetSourceRectangle(frameIndex);
		var destination = new Rectangle(
			(int)position.X,
			(int)position.Y,
			source.Width * scale,
			source.Height * scale);

		spriteBatch.Draw(Texture, destination, source, color, 0f, Vector2.Zero, effects, layerDepth);
	}
}
