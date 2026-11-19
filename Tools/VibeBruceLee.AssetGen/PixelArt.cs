namespace VibeBruceLee.AssetGen;

public static class PixelArt
{
	public static RgbaImage FromRows(IReadOnlyList<string> rows, IReadOnlyDictionary<char, RgbaColor> palette)
	{
		ArgumentNullException.ThrowIfNull(rows);
		ArgumentNullException.ThrowIfNull(palette);

		if (rows.Count == 0)
		{
			throw new ArgumentException("A sprite needs at least one row.", nameof(rows));
		}

		var width = rows[0].Length;
		if (width == 0)
		{
			throw new ArgumentException("Sprite rows must not be empty.", nameof(rows));
		}

		for (var y = 0; y < rows.Count; y++)
		{
			if (rows[y].Length != width)
			{
				throw new ArgumentException($"Row {y} has width {rows[y].Length}; expected {width}.", nameof(rows));
			}
		}

		var image = new RgbaImage(width, rows.Count);
		for (var y = 0; y < rows.Count; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var symbol = rows[y][x];
				if (!palette.TryGetValue(symbol, out var color))
				{
					throw new ArgumentException($"No palette entry for '{symbol}' at ({x}, {y}).", nameof(palette));
				}

				image.SetPixel(x, y, color);
			}
		}

		return image;
	}
}
