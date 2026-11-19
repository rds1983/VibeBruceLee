using StbImageNext;
using StbImageNext.Encoders;

namespace VibeBruceLee.AssetGen;

public static class AssetGenerator
{
	public static void Generate(string assetRoot)
	{
		if (string.IsNullOrWhiteSpace(assetRoot))
		{
			throw new ArgumentException("Asset root must be specified.", nameof(assetRoot));
		}

		var root = Path.GetFullPath(assetRoot);
		var artDirectory = Path.Combine(root, "art");
		var audioDirectory = Path.Combine(root, "audio");

		Directory.CreateDirectory(artDirectory);
		Directory.CreateDirectory(audioDirectory);

		var testCard = SampleArt.CreateTestCard();
		using (var stream = File.Create(Path.Combine(artDirectory, "test-card.png")))
		{
			PngEncoder.EncodeToStream(stream, testCard.Pixels, testCard.Width, testCard.Height, ColorComponents.RedGreenBlueAlpha);
		}

		var lantern = SampleArt.CreateLantern();
		using (var stream = File.Create(Path.Combine(artDirectory, "lantern.png")))
		{
			PngEncoder.EncodeToStream(stream, lantern.Pixels, lantern.Width, lantern.Height, ColorComponents.RedGreenBlueAlpha);
		}

		WavWriter.WritePickup(Path.Combine(audioDirectory, "pickup.wav"));
	}
}

