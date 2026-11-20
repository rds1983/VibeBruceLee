namespace VibeBruceLee.AssetGen;

/// <summary>
/// Dark temple-wall backdrop shared by the chambers. It stays low-contrast so
/// platforms, hazards and fighters remain readable.
/// </summary>
public static class StageArt
{
	private static readonly RgbaColor Background = RgbaColor.FromHex(0x0C0C16);
	private static readonly RgbaColor Brick = RgbaColor.FromHex(0x171726);
	private static readonly RgbaColor BrickLight = RgbaColor.FromHex(0x202034);
	private static readonly RgbaColor Mortar = RgbaColor.FromHex(0x090910);
	private static readonly RgbaColor Stone = RgbaColor.FromHex(0x252538);
	private static readonly RgbaColor StoneLight = RgbaColor.FromHex(0x34344C);
	private static readonly RgbaColor Torch = RgbaColor.FromHex(0x6A4018);
	private static readonly RgbaColor Flame = RgbaColor.FromHex(0xFF8000);
	private static readonly RgbaColor FlameLight = RgbaColor.FromHex(0xFFD800);

	public static RgbaImage CreateBackdrop()
	{
		var image = new RgbaImage(256, 192);
		image.Clear(Background);

		for (var y = 12; y < 176; y += 12)
		{
			var offset = ((y / 12) & 1) == 0 ? 0 : 8;
			for (var x = -8 + offset; x < 256; x += 16)
			{
				DrawBrick(image, x, y);
			}

			image.FillRect(0, y + 8, 256, 1, Mortar);
		}

		DrawPillar(image, 4);
		DrawPillar(image, 232);
		image.FillRect(0, 4, 256, 6, Stone);
		image.FillRect(0, 4, 256, 1, StoneLight);
		image.FillRect(0, 9, 256, 1, Mortar);

		DrawTorch(image, 40, 70);
		DrawTorch(image, 204, 70);
		DrawTorch(image, 120, 120);

		return image;
	}

	private static void DrawBrick(RgbaImage image, int x, int y)
	{
		if (x + 14 <= 0 || x >= 256)
		{
			return;
		}

		var left = Math.Max(0, x);
		var width = Math.Min(14, 256 - left);
		if (width <= 0)
		{
			return;
		}

		image.FillRect(left, y, width, 7, Brick);
		if (left + 1 < 256)
		{
			image.FillRect(left + 1, y + 1, Math.Max(1, width - 2), 1, BrickLight);
		}

		image.FillRect(left, y, 1, 7, Mortar);
	}

	private static void DrawPillar(RgbaImage image, int x)
	{
		image.FillRect(x, 10, 20, 166, Stone);
		image.FillRect(x, 10, 2, 166, StoneLight);
		image.FillRect(x + 18, 10, 2, 166, Mortar);
		for (var y = 24; y < 172; y += 24)
		{
			image.FillRect(x + 2, y, 16, 2, Mortar);
		}
	}

	private static void DrawTorch(RgbaImage image, int x, int y)
	{
		image.FillRect(x + 2, y + 12, 4, 10, Torch);
		image.FillRect(x, y + 4, 8, 8, RgbaColor.FromHex(0x401810));
		image.FillRect(x + 1, y + 2, 6, 7, Flame);
		image.FillRect(x + 3, y, 2, 5, FlameLight);
	}
}
