namespace VibeBruceLee.AssetGen;

/// <summary>
/// First-pass fighter sprites. Poses are defined in frame-local pixel space
/// and rendered with simple primitives so animation variants stay consistent.
/// </summary>
public static class CharacterArt
{
	private static readonly RgbaColor Skin = RgbaColor.FromHex(0xE8B080);
	private static readonly RgbaColor GreenSkin = RgbaColor.FromHex(0x50B050);
	private static readonly RgbaColor Hair = RgbaColor.FromHex(0x181818);
	private static readonly RgbaColor Yellow = RgbaColor.FromHex(0xFFD800);
	private static readonly RgbaColor Black = RgbaColor.FromHex(0x181818);
	private static readonly RgbaColor DarkBlue = RgbaColor.FromHex(0x202040);
	private static readonly RgbaColor Red = RgbaColor.FromHex(0xD82020);
	private static readonly RgbaColor Green = RgbaColor.FromHex(0x206020);
	private static readonly RgbaColor Brown = RgbaColor.FromHex(0x604020);

	private static readonly FighterStyle BruceStyle = new(
		16, 24, 6, 5, 6, 2,
		Skin, Hair, Yellow, RgbaColor.FromHex(0xB89000), Black, Black,
		Bald: false, Mask: false, Headband: false, BareArms: false, SideStripe: true);

	private static readonly FighterStyle NinjaStyle = new(
		16, 24, 6, 5, 6, 2,
		Skin, Hair, DarkBlue, RgbaColor.FromHex(0x101028), Red, Black,
		Bald: false, Mask: true, Headband: true, BareArms: false, SideStripe: false);

	private static readonly FighterStyle YamoStyle = new(
		24, 28, 10, 8, 14, 3,
		GreenSkin, GreenSkin, Green, RgbaColor.FromHex(0x1E5020), Brown, Brown,
		Bald: true, Mask: false, Headband: false, BareArms: true, SideStripe: false);

	public static RgbaImage CreateBruceSheet()
	{
		return BuildSheet(16, 24, 20, DrawBruceFrame);
	}

	public static RgbaImage CreateNinjaSheet()
	{
		return BuildSheet(16, 24, 14, DrawNinjaFrame);
	}

	public static RgbaImage CreateYamoSheet()
	{
		return BuildSheet(24, 28, 12, DrawYamoFrame);
	}

	private static void DrawBruceFrame(RgbaImage image, int frame)
	{
		switch (frame)
		{
			case 0:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 3, 14, 12, 14, 5, 22, 10, 22, 15, 0));
				break;
			case 1:
				DrawFighter(image, BruceStyle, MakePose(5, 2, 3, 15, 12, 15, 5, 22, 10, 22, 15, 0));
				break;
			case 2:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 13, 10, 3, 13, 12, 21, 4, 22, 15, 1));
				break;
			case 3:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 3, 10, 13, 13, 4, 21, 12, 22, 15, 0));
				break;
			case 4:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 12, 11, 4, 14, 13, 22, 3, 21, 15, 1));
				break;
			case 5:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 4, 11, 12, 14, 3, 22, 13, 21, 15, 0));
				break;
			case 6:
				DrawFighter(image, BruceStyle, MakePose(5, 0, 4, 4, 12, 4, 5, 18, 11, 18, 12, 0));
				break;
			case 7:
				DrawFighter(image, BruceStyle, MakePose(5, 9, 3, 18, 12, 18, 5, 22, 10, 22, 18, 0));
				break;
			case 8:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 14, 8, 3, 13, 5, 22, 10, 22, 15, 1));
				break;
			case 9:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 14, 10, 3, 14, 5, 22, 10, 22, 15, 1));
				break;
			case 10:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 4, 9, 10, 12, 15, 13, 8, 22, 15, 0));
				break;
			case 11:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 4, 10, 10, 13, 14, 12, 8, 22, 15, 0));
				break;
			case 12:
				DrawFighter(image, BruceStyle, MakePose(5, 2, 3, 8, 12, 5, 15, 10, 6, 16, 12, 0));
				break;
			case 13:
				DrawFighter(image, BruceStyle, MakePose(5, 3, 3, 9, 12, 6, 14, 9, 5, 15, 13, 0));
				break;
			case 14:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 5, 2, 11, 2, 6, 18, 10, 18, 15, 0));
				break;
			case 15:
				DrawFighter(image, BruceStyle, MakePose(5, 1, 5, 4, 11, 1, 6, 18, 10, 18, 15, 0));
				break;
			case 16:
				DrawFighter(image, BruceStyle, MakePose(7, 2, 2, 6, 14, 7, 4, 22, 11, 22, 15, 0));
				break;
			case 17:
				DrawFighter(image, BruceStyle, MakePose(7, 10, 2, 16, 14, 18, 5, 22, 10, 22, 18, 0));
				break;
			case 18:
				DrawFallen(image, BruceStyle, 18);
				break;
			case 19:
				DrawFallen(image, BruceStyle, 20);
				break;
		}
	}

	private static void DrawNinjaFrame(RgbaImage image, int frame)
	{
		switch (frame)
		{
			case 0:
				DrawFighter(image, NinjaStyle, MakePose(5, 1, 3, 14, 12, 14, 5, 22, 10, 22, 15, 0));
				break;
			case 1:
				DrawFighter(image, NinjaStyle, MakePose(5, 2, 3, 15, 12, 15, 5, 22, 10, 22, 15, 0));
				break;
			case 2:
				DrawFighter(image, NinjaStyle, MakePose(5, 1, 13, 10, 3, 13, 12, 21, 4, 22, 15, 1));
				break;
			case 3:
				DrawFighter(image, NinjaStyle, MakePose(5, 1, 3, 10, 13, 13, 4, 21, 12, 22, 15, 0));
				break;
			case 4:
				DrawFighter(image, NinjaStyle, MakePose(5, 1, 12, 11, 4, 14, 13, 22, 3, 21, 15, 1));
				break;
			case 5:
				DrawFighter(image, NinjaStyle, MakePose(5, 1, 4, 11, 12, 14, 3, 22, 13, 21, 15, 0));
				break;
			case 6:
				DrawFighter(image, NinjaStyle, MakePose(5, 1, 14, 8, 3, 13, 5, 22, 10, 22, 15, 1));
				image.DrawLine(13, 8, 15, 5, Brown);
				break;
			case 7:
				DrawFighter(image, NinjaStyle, MakePose(5, 1, 14, 7, 3, 13, 5, 22, 10, 22, 15, 1));
				image.DrawLine(13, 7, 15, 4, Brown);
				break;
			case 8:
				DrawFighter(image, NinjaStyle, MakePose(7, 2, 2, 6, 14, 7, 4, 22, 11, 22, 15, 0));
				break;
			case 9:
				DrawFallen(image, NinjaStyle, 18);
				break;
			case 10:
				DrawFighter(image, NinjaStyle, MakePose(5, 6, 3, 16, 12, 16, 5, 23, 10, 23, 18, 0));
				break;
			case 11:
				DrawFighter(image, NinjaStyle, MakePose(5, 2, 4, 6, 12, 6, 6, 18, 10, 18, 12, 0));
				image.DrawLine(12, 7, 15, 5, Brown);
				break;
			case 12:
				DrawFighter(image, NinjaStyle, MakePose(5, 2, 14, 8, 3, 13, 6, 18, 10, 18, 12, 1));
				image.DrawLine(13, 8, 15, 5, Brown);
				break;
			case 13:
				DrawFighter(image, NinjaStyle, MakePose(5, 3, 4, 15, 12, 15, 5, 22, 10, 22, 16, 0));
				break;
		}
	}

	private static void DrawYamoFrame(RgbaImage image, int frame)
	{
		switch (frame)
		{
			case 0:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 2, 17, 21, 17, 6, 26, 18, 26, 19, 0));
				break;
			case 1:
				DrawFighter(image, YamoStyle, MakePose(7, 2, 2, 18, 21, 18, 6, 26, 18, 26, 19, 0));
				break;
			case 2:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 21, 12, 1, 16, 19, 25, 5, 26, 19, 1));
				break;
			case 3:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 1, 12, 21, 16, 5, 25, 19, 26, 19, 0));
				break;
			case 4:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 20, 14, 2, 17, 20, 26, 4, 25, 19, 1));
				break;
			case 5:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 2, 14, 20, 17, 4, 26, 20, 25, 19, 0));
				break;
			case 6:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 22, 11, 2, 17, 6, 26, 18, 26, 19, 1));
				break;
			case 7:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 21, 13, 2, 18, 6, 26, 18, 26, 19, 1));
				break;
			case 8:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 2, 17, 21, 17, 6, 26, 18, 24, 19, 0));
				break;
			case 9:
				DrawFighter(image, YamoStyle, MakePose(7, 1, 2, 17, 21, 17, 6, 26, 22, 18, 19, 1));
				break;
			case 10:
				DrawFighter(image, YamoStyle, MakePose(9, 2, 2, 9, 22, 10, 4, 26, 20, 26, 19, 0));
				break;
			case 11:
				DrawFallen(image, YamoStyle, 20);
				break;
		}
	}

	private static void DrawFighter(RgbaImage image, FighterStyle style, FighterPose pose)
	{
		var center = image.Width / 2;
		var torsoX = center - (style.TorsoWidth / 2) + pose.Lean;
		var torsoTop = pose.HeadY + style.HeadHeight;
		var torsoHeight = Math.Max(1, pose.HipY - torsoTop);
		var shoulderY = torsoTop + 1;
		var hipLeft = center - 2;
		var hipRight = center + 1;

		DrawThickLine(image, hipLeft, pose.HipY, pose.LeftFootX, pose.LeftFootY, style.Outfit, style.Thickness);
		DrawThickLine(image, hipRight, pose.HipY, pose.RightFootX, pose.RightFootY, style.Outfit, style.Thickness);

		var armColor = style.BareArms ? style.Skin : style.Outfit;
		DrawThickLine(image, torsoX + 1, shoulderY, pose.LeftHandX, pose.LeftHandY, armColor, style.Thickness);
		DrawThickLine(image, torsoX + style.TorsoWidth - 2, shoulderY, pose.RightHandX, pose.RightHandY, armColor, style.Thickness);

		SafeFill(image, pose.LeftHandX, pose.LeftHandY, 2, 2, style.Skin);
		SafeFill(image, pose.RightHandX, pose.RightHandY, 2, 2, style.Skin);
		SafeFill(image, pose.LeftFootX, pose.LeftFootY, 2, 1, style.Shoes);
		SafeFill(image, pose.RightFootX, pose.RightFootY, 2, 1, style.Shoes);

		SafeFill(image, torsoX, torsoTop, style.TorsoWidth, torsoHeight, style.Outfit);
		SafeFill(image, torsoX, torsoTop, 1, torsoHeight, style.Shade);
		if (style.SideStripe)
		{
			SafeFill(image, torsoX + style.TorsoWidth - 1, torsoTop, 1, torsoHeight, style.Accent);
		}

		SafeFill(image, torsoX, pose.HipY - 1, style.TorsoWidth, 1, style.Accent);

		SafeFill(image, pose.HeadX, pose.HeadY, style.HeadWidth, style.HeadHeight, style.Skin);

		if (style.Mask)
		{
			SafeFill(image, pose.HeadX, pose.HeadY, style.HeadWidth, style.HeadHeight, style.Outfit);
			SafeFill(image, pose.HeadX + 2, pose.HeadY + 2, 1, 2, RgbaColor.FromHex(0xFFFFFF));
			SafeFill(image, pose.HeadX + style.HeadWidth - 3, pose.HeadY + 2, 1, 2, RgbaColor.FromHex(0xFFFFFF));
		}
		else
		{
			if (!style.Bald)
			{
				SafeFill(image, pose.HeadX, pose.HeadY, style.HeadWidth, 2, style.Hair);
			}

			SafeFill(image, pose.HeadX + 2, pose.HeadY + 2, 1, 2, Black);
			SafeFill(image, pose.HeadX + style.HeadWidth - 3, pose.HeadY + 2, 1, 2, Black);
		}

		if (style.Headband)
		{
			SafeFill(image, pose.HeadX, pose.HeadY + 1, style.HeadWidth, 1, style.Accent);
		}
	}

	private static void DrawFallen(RgbaImage image, FighterStyle style, int y)
	{
		var headX = image.Width - style.HeadWidth - 2;
		var torsoX = 2;
		var torsoWidth = image.Width - style.HeadWidth - 5;

		SafeFill(image, torsoX, y + 2, torsoWidth, 3, style.Outfit);
		SafeFill(image, headX, y, style.HeadWidth, style.HeadHeight, style.Skin);

		if (!style.Bald)
		{
			SafeFill(image, headX, y, style.HeadWidth, 2, style.Hair);
		}

		image.DrawLine(torsoX + 1, y + 4, 1, image.Height - 2, style.Outfit);
		image.DrawLine(torsoX + torsoWidth - 2, y + 5, 0, image.Height - 4, style.Outfit);
		image.DrawLine(headX, y + 2, headX - 5, y + 5, style.Skin);
	}

	private static FighterPose MakePose(
		int headX,
		int headY,
		int leftHandX,
		int leftHandY,
		int rightHandX,
		int rightHandY,
		int leftFootX,
		int leftFootY,
		int rightFootX,
		int rightFootY,
		int hipY,
		int lean)
	{
		return new FighterPose(headX, headY, leftHandX, leftHandY, rightHandX, rightHandY, leftFootX, leftFootY, rightFootX, rightFootY, hipY, lean);
	}

	private static void DrawThickLine(RgbaImage image, int x0, int y0, int x1, int y1, RgbaColor color, int thickness)
	{
		for (var offset = 0; offset < thickness; offset++)
		{
			image.DrawLine(x0 + offset, y0, x1 + offset, y1, color);
		}
	}

	private static void SafeFill(RgbaImage image, int x, int y, int width, int height, RgbaColor color)
	{
		for (var row = 0; row < height; row++)
		{
			for (var column = 0; column < width; column++)
			{
				var pixelX = x + column;
				var pixelY = y + row;
				if (pixelX >= 0 && pixelY >= 0 && pixelX < image.Width && pixelY < image.Height)
				{
					image.SetPixel(pixelX, pixelY, color);
				}
			}
		}
	}

	private static RgbaImage BuildSheet(int frameWidth, int frameHeight, int frameCount, Action<RgbaImage, int> drawFrame)
	{
		var sheet = new RgbaImage(frameWidth * frameCount, frameHeight);
		for (var frame = 0; frame < frameCount; frame++)
		{
			var image = new RgbaImage(frameWidth, frameHeight);
			drawFrame(image, frame);
			sheet.Blit(image, frame * frameWidth, 0);
		}

		return sheet;
	}

	private readonly record struct FighterPose(
		int HeadX,
		int HeadY,
		int LeftHandX,
		int LeftHandY,
		int RightHandX,
		int RightHandY,
		int LeftFootX,
		int LeftFootY,
		int RightFootX,
		int RightFootY,
		int HipY,
		int Lean);

	private sealed record FighterStyle(
		int Width,
		int Height,
		int HeadWidth,
		int HeadHeight,
		int TorsoWidth,
		int Thickness,
		RgbaColor Skin,
		RgbaColor Hair,
		RgbaColor Outfit,
		RgbaColor Shade,
		RgbaColor Accent,
		RgbaColor Shoes,
		bool Bald,
		bool Mask,
		bool Headband,
		bool BareArms,
		bool SideStripe);
}
