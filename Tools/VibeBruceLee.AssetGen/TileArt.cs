namespace VibeBruceLee.AssetGen;

public static class TileArt
{
	private static readonly RgbaColor Brick = RgbaColor.FromHex(0x902020);
	private static readonly RgbaColor BrickLight = RgbaColor.FromHex(0xD06040);
	private static readonly RgbaColor Mortar = RgbaColor.FromHex(0x301010);
	private static readonly RgbaColor Stone = RgbaColor.FromHex(0x707880);
	private static readonly RgbaColor StoneLight = RgbaColor.FromHex(0xB0B8C0);
	private static readonly RgbaColor StoneDark = RgbaColor.FromHex(0x404850);
	private static readonly RgbaColor Dark = RgbaColor.FromHex(0x202030);
	private static readonly RgbaColor DarkLight = RgbaColor.FromHex(0x383850);
	private static readonly RgbaColor Green = RgbaColor.FromHex(0x30A040);
	private static readonly RgbaColor GreenLight = RgbaColor.FromHex(0x80E060);
	private static readonly RgbaColor Skin = RgbaColor.FromHex(0xE8B080);
	private static readonly RgbaColor Hair = RgbaColor.FromHex(0x181818);
	private static readonly RgbaColor Yellow = RgbaColor.FromHex(0xFFD800);
	private static readonly RgbaColor Red = RgbaColor.FromHex(0xD82020);
	private static readonly RgbaColor White = RgbaColor.FromHex(0xFFFFFF);

	public static RgbaImage CreateTiles()
	{
		var image = new RgbaImage(32, 8);

		DrawBrick(image, 0);
		DrawStone(image, 8);
		DrawVine(image, 16);
		DrawDark(image, 24);

		return image;
	}

	public static RgbaImage CreateHudLife()
	{
		var image = new RgbaImage(8, 8);
		image.FillRect(1, 1, 6, 2, Hair);
		image.FillRect(1, 3, 6, 4, Skin);
		image.FillRect(2, 4, 1, 1, Hair);
		image.FillRect(5, 4, 1, 1, Hair);
		image.FillRect(2, 6, 4, 1, Red);
		return image;
	}

	public static RgbaImage CreateHudLantern()
	{
		var image = new RgbaImage(8, 8);
		image.FillRect(3, 0, 2, 1, Yellow);
		image.FillRect(2, 1, 4, 1, Yellow);
		image.FillRect(1, 2, 6, 4, Red);
		image.FillRect(2, 2, 4, 1, Yellow);
		image.FillRect(3, 3, 2, 2, White);
		image.FillRect(2, 6, 4, 1, Yellow);
		image.FillRect(3, 7, 2, 1, Yellow);
		return image;
	}

	private static void DrawBrick(RgbaImage image, int x)
	{
		image.FillRect(x, 0, 8, 8, Brick);
		image.FillRect(x, 0, 8, 1, Mortar);
		image.FillRect(x, 4, 8, 1, Mortar);
		image.FillRect(x, 0, 1, 4, Mortar);
		image.FillRect(x + 4, 4, 1, 4, Mortar);
		image.FillRect(x + 1, 1, 2, 2, BrickLight);
		image.FillRect(x + 5, 5, 2, 2, BrickLight);
	}

	private static void DrawStone(RgbaImage image, int x)
	{
		image.FillRect(x, 0, 8, 8, Stone);
		image.FillRect(x, 0, 8, 1, StoneLight);
		image.FillRect(x, 0, 1, 8, StoneLight);
		image.FillRect(x, 7, 8, 1, StoneDark);
		image.FillRect(x + 7, 0, 1, 8, StoneDark);
		image.FillRect(x + 2, 2, 3, 2, StoneLight);
	}

	private static void DrawVine(RgbaImage image, int x)
	{
		image.FillRect(x + 3, 0, 2, 8, Green);
		image.FillRect(x + 2, 1, 1, 1, GreenLight);
		image.FillRect(x + 5, 2, 1, 1, GreenLight);
		image.FillRect(x + 2, 4, 1, 1, GreenLight);
		image.FillRect(x + 5, 5, 2, 1, GreenLight);
		image.FillRect(x + 2, 6, 1, 1, GreenLight);
	}

	private static void DrawDark(RgbaImage image, int x)
	{
		image.FillRect(x, 0, 8, 8, Dark);
		image.FillRect(x, 0, 8, 1, DarkLight);
		image.FillRect(x + 2, 3, 2, 2, DarkLight);
	}
}
