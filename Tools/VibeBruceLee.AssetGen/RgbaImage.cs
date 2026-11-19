namespace VibeBruceLee.AssetGen;

public readonly record struct RgbaColor(byte R, byte G, byte B, byte A)
{
	public static RgbaColor FromHex(uint rgb) => new(
		(byte)(rgb >> 16),
		(byte)(rgb >> 8),
		(byte)rgb,
		255);

	public static RgbaColor Transparent => new(0, 0, 0, 0);
}

public sealed class RgbaImage
{
	public RgbaImage(int width, int height)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

		Width = width;
		Height = height;
		Pixels = new byte[width * height * 4];
	}

	public int Width { get; }

	public int Height { get; }

	public byte[] Pixels { get; }

	public void Clear(RgbaColor color)
	{
		for (var i = 0; i < Pixels.Length; i += 4)
		{
			Pixels[i] = color.R;
			Pixels[i + 1] = color.G;
			Pixels[i + 2] = color.B;
			Pixels[i + 3] = color.A;
		}
	}

	public void SetPixel(int x, int y, RgbaColor color)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(x);
		ArgumentOutOfRangeException.ThrowIfNegative(y);

		if (x >= Width || y >= Height)
		{
			throw new ArgumentOutOfRangeException(nameof(x), $"Pixel ({x}, {y}) is outside the {Width}x{Height} image.");
		}

		var index = (y * Width + x) * 4;
		Pixels[index] = color.R;
		Pixels[index + 1] = color.G;
		Pixels[index + 2] = color.B;
		Pixels[index + 3] = color.A;
	}

	public void FillRect(int x, int y, int width, int height, RgbaColor color)
	{
		if (width <= 0 || height <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(width), "Rectangle dimensions must be positive.");
		}

		if (x < 0 || y < 0 || x + width > Width || y + height > Height)
		{
			throw new ArgumentOutOfRangeException(nameof(x), $"Rectangle ({x}, {y}, {width}, {height}) is outside the {Width}x{Height} image.");
		}

		for (var row = y; row < y + height; row++)
		{
			for (var column = x; column < x + width; column++)
			{
				var index = (row * Width + column) * 4;
				Pixels[index] = color.R;
				Pixels[index + 1] = color.G;
				Pixels[index + 2] = color.B;
				Pixels[index + 3] = color.A;
			}
		}
	}
}
