namespace VibeBruceLee.AssetGen;

public static class SampleArt
{
	private static readonly RgbaColor Yellow = RgbaColor.FromHex(0xFFD800);
	private static readonly RgbaColor Cyan = RgbaColor.FromHex(0x00D8D8);
	private static readonly RgbaColor Magenta = RgbaColor.FromHex(0xD800D8);
	private static readonly RgbaColor White = RgbaColor.FromHex(0xFFFFFF);

	private static readonly string[] LanternRows =
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

	private static readonly IReadOnlyDictionary<char, RgbaColor> NormalLantern = CreateLanternPalette(
		RgbaColor.FromHex(0xFF6C00),
		RgbaColor.FromHex(0xD80000),
		White);

	private static readonly IReadOnlyDictionary<char, RgbaColor> GlowLantern = CreateLanternPalette(
		RgbaColor.FromHex(0xFFB000),
		RgbaColor.FromHex(0xFF4000),
		RgbaColor.FromHex(0xFFFFC0));

	private static readonly IReadOnlyDictionary<char, RgbaColor> DimLantern = CreateLanternPalette(
		RgbaColor.FromHex(0xB05000),
		RgbaColor.FromHex(0x800000),
		RgbaColor.FromHex(0xB0B0B0));

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
		return PixelArt.FromRows(LanternRows, NormalLantern);
	}

	public static RgbaImage CreateLanternSheet()
	{
		var sheet = new RgbaImage(64, 16);

		sheet.Blit(PixelArt.FromRows(LanternRows, NormalLantern), 0, 0);
		sheet.Blit(PixelArt.FromRows(LanternRows, GlowLantern), 16, 0);
		sheet.Blit(PixelArt.FromRows(LanternRows, DimLantern), 32, 0);
		sheet.Blit(PixelArt.FromRows(LanternRows, NormalLantern), 48, 0);

		return sheet;
	}

	private static Dictionary<char, RgbaColor> CreateLanternPalette(RgbaColor orange, RgbaColor red, RgbaColor highlight)
	{
		return new Dictionary<char, RgbaColor>
		{
			['.'] = RgbaColor.Transparent,
			['Y'] = Yellow,
			['O'] = orange,
			['R'] = red,
			['W'] = highlight
		};
	}
}
