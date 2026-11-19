namespace VibeBruceLee.AssetGen;

public static class SampleArt
{
	private static readonly RgbaColor Yellow = RgbaColor.FromHex(0xFFD800);
	private static readonly RgbaColor Cyan = RgbaColor.FromHex(0x00D8D8);
	private static readonly RgbaColor Magenta = RgbaColor.FromHex(0xD800D8);
	private static readonly RgbaColor White = RgbaColor.FromHex(0xFFFFFF);

	private static readonly IReadOnlyDictionary<char, RgbaColor> LanternPalette = new Dictionary<char, RgbaColor>
	{
		['.'] = RgbaColor.Transparent,
		['Y'] = Yellow,
		['O'] = RgbaColor.FromHex(0xFF6C00),
		['R'] = RgbaColor.FromHex(0xD80000),
		['W'] = White
	};

	public static RgbaImage CreateTestCard()
	{
		var image = new RgbaImage(64, 48);
		image.Clear(RgbaColor.FromHex(0x000000));

		image.FillRect(0, 0, 64, 2, Yellow);
		image.FillRect(0, 46, 64, 2, Yellow);
		image.FillRect(0, 0, 2, 48, Yellow);
		image.FillRect(62, 0, 2, 48, Yellow);

		for (var y = 8; y < 40; y += 8)
		{
			for (var x = 8; x < 56; x += 8)
			{
				var color = ((x / 8) + (y / 8)) % 2 == 0 ? Cyan : Magenta;
				image.FillRect(x, y, 8, 8, color);
			}
		}

		image.FillRect(24, 16, 16, 16, White);
		image.FillRect(28, 20, 8, 8, Cyan);

		return image;
	}

	public static RgbaImage CreateLantern()
	{
		string[] rows =
		[
			"................",
			".......YY.......",
			".......YY.......",
			"......YOOY......",
			".....YOOOOY.....",
			"....YORRRROY....",
			"....YORWWROY....",
			"....YORWWROY....",
			"....YORRRROY....",
			"....YORRRROY....",
			"....YORRRROY....",
			".....YOOOOY.....",
			"......YOOY......",
			".......YY.......",
			".......YY.......",
			"................"
		];

		return PixelArt.FromRows(rows, LanternPalette);
	}
}
