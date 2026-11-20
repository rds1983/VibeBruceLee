namespace VibeBruceLee.AssetGen;

/// <summary>
/// 32x32 character portraits and the two-frame wizard portrait used by the
/// title and boss room.
/// </summary>
public static class PortraitArt
{
	private static readonly RgbaColor BruceSkin = RgbaColor.FromHex(0xE8B080);
	private static readonly RgbaColor Hair = RgbaColor.FromHex(0x181818);
	private static readonly RgbaColor BruceOutfit = RgbaColor.FromHex(0xFFD800);
	private static readonly RgbaColor BruceAccent = RgbaColor.FromHex(0x181818);
	private static readonly RgbaColor NinjaSkin = RgbaColor.FromHex(0xE8B080);
	private static readonly RgbaColor NinjaOutfit = RgbaColor.FromHex(0x202040);
	private static readonly RgbaColor NinjaAccent = RgbaColor.FromHex(0xD82020);
	private static readonly RgbaColor YamoSkin = RgbaColor.FromHex(0x50B050);
	private static readonly RgbaColor YamoOutfit = RgbaColor.FromHex(0x206020);
	private static readonly RgbaColor YamoAccent = RgbaColor.FromHex(0x604020);
	private static readonly RgbaColor Purple = RgbaColor.FromHex(0x6A2088);
	private static readonly RgbaColor PurpleDark = RgbaColor.FromHex(0x3A1050);
	private static readonly RgbaColor Beard = RgbaColor.FromHex(0xB0B0B0);
	private static readonly RgbaColor White = RgbaColor.FromHex(0xFFFFFF);
	private static readonly RgbaColor Yellow = RgbaColor.FromHex(0xFFD800);
	private static readonly RgbaColor Orange = RgbaColor.FromHex(0xFF6000);

	public static RgbaImage CreatePortraits()
	{
		var sheet = new RgbaImage(96, 32);
		DrawPortrait(sheet, 0, BruceSkin, Hair, BruceOutfit, BruceAccent, mask: false, bald: false);
		DrawPortrait(sheet, 32, NinjaSkin, Hair, NinjaOutfit, NinjaAccent, mask: true, bald: false);
		DrawPortrait(sheet, 64, YamoSkin, YamoSkin, YamoOutfit, YamoAccent, mask: false, bald: true);
		return sheet;
	}

	public static RgbaImage CreateWizardSheet()
	{
		var sheet = new RgbaImage(64, 32);
		DrawWizard(sheet, 0, glow: false);
		DrawWizard(sheet, 32, glow: true);
		return sheet;
	}

	private static void DrawPortrait(
		RgbaImage image,
		int offsetX,
		RgbaColor skin,
		RgbaColor hair,
		RgbaColor outfit,
		RgbaColor accent,
		bool mask,
		bool bald)
	{
		image.FillRect(offsetX + 3, 23, 26, 9, outfit);
		image.FillRect(offsetX + 3, 26, 26, 1, accent);
		image.FillRect(offsetX + 14, 19, 4, 5, skin);

		var face = mask ? outfit : skin;
		image.FillRect(offsetX + 8, 4, 16, 16, face);

		if (!bald)
		{
			image.FillRect(offsetX + 8, 4, 16, 4, hair);
		}

		if (mask)
		{
			image.FillRect(offsetX + 8, 6, 16, 1, accent);
			image.FillRect(offsetX + 11, 10, 3, 3, White);
			image.FillRect(offsetX + 18, 10, 3, 3, White);
			image.FillRect(offsetX + 12, 11, 1, 1, RgbaColor.FromHex(0x181818));
			image.FillRect(offsetX + 19, 11, 1, 1, RgbaColor.FromHex(0x181818));
		}
		else
		{
			image.FillRect(offsetX + 11, 10, 2, 2, RgbaColor.FromHex(0x181818));
			image.FillRect(offsetX + 19, 10, 2, 2, RgbaColor.FromHex(0x181818));
			image.FillRect(offsetX + 14, 17, 4, 1, RgbaColor.FromHex(0x181818));
		}

		if (bald)
		{
			image.FillRect(offsetX + 8, 4, 16, 1, RgbaColor.FromHex(0x307030));
			image.FillRect(offsetX + 9, 7, 1, 1, RgbaColor.FromHex(0x307030));
			image.FillRect(offsetX + 22, 7, 1, 1, RgbaColor.FromHex(0x307030));
		}
	}

	private static void DrawWizard(RgbaImage image, int offsetX, bool glow)
	{
		image.FillRect(offsetX + 2, 7, 28, 3, PurpleDark);
		image.FillRect(offsetX + 8, 1, 16, 6, PurpleDark);
		image.FillRect(offsetX + 8, 10, 16, 16, Purple);
		image.FillRect(offsetX + 9, 23, 14, 6, Beard);
		image.FillRect(offsetX + 12, 26, 8, 2, Purple);

		var eye = glow ? Yellow : White;
		image.FillRect(offsetX + 11, 13, 4, 4, eye);
		image.FillRect(offsetX + 18, 13, 4, 4, eye);
		image.FillRect(offsetX + 12, 14, 2, 2, glow ? Orange : RgbaColor.FromHex(0x181818));
		image.FillRect(offsetX + 19, 14, 2, 2, glow ? Orange : RgbaColor.FromHex(0x181818));
		image.FillRect(offsetX + 14, 20, 4, 1, RgbaColor.FromHex(0x3A1050));
	}
}
