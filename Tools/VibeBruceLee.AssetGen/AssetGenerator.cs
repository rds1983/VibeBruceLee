using System.Diagnostics.CodeAnalysis;
using StbImageNext;
using StbImageNext.Encoders;

namespace VibeBruceLee.AssetGen;

public static class AssetGenerator
{
	public static void Generate(string assetRoot)
	{
		var root = GetRoot(assetRoot);
		var artDirectory = Path.Combine(root, "art");
		var audioDirectory = Path.Combine(root, "audio");

		Directory.CreateDirectory(artDirectory);
		Directory.CreateDirectory(audioDirectory);

		WritePng(Path.Combine(artDirectory, "test-card.png"), SampleArt.CreateTestCard());
		WritePng(Path.Combine(artDirectory, "lantern.png"), SampleArt.CreateLantern());
		WritePng(Path.Combine(artDirectory, "lantern-sheet.png"), SampleArt.CreateLanternSheet());
		WritePng(Path.Combine(artDirectory, "bruce.png"), CharacterArt.CreateBruceSheet());
		WritePng(Path.Combine(artDirectory, "ninja.png"), CharacterArt.CreateNinjaSheet());
		WritePng(Path.Combine(artDirectory, "yamo.png"), CharacterArt.CreateYamoSheet());
		WritePng(Path.Combine(artDirectory, "tiles.png"), TileArt.CreateTiles());
		WritePng(Path.Combine(artDirectory, "hud-life.png"), TileArt.CreateHudLife());
		WritePng(Path.Combine(artDirectory, "hud-lantern.png"), TileArt.CreateHudLantern());

		WavWriter.WritePickup(Path.Combine(audioDirectory, "pickup.wav"));
		WriteWave(Path.Combine(audioDirectory, "jump.wav"), WavWriter.CreateJumpSamples());
		WriteWave(Path.Combine(audioDirectory, "land.wav"), WavWriter.CreateLandSamples());
		WriteWave(Path.Combine(audioDirectory, "chop.wav"), WavWriter.CreateChopSamples());
		WriteWave(Path.Combine(audioDirectory, "kick.wav"), WavWriter.CreateKickSamples());
		WriteWave(Path.Combine(audioDirectory, "hit.wav"), WavWriter.CreateHitSamples());
		WriteWave(Path.Combine(audioDirectory, "knockdown.wav"), WavWriter.CreateKnockdownSamples());
		WriteWave(Path.Combine(audioDirectory, "death.wav"), WavWriter.CreateDeathSamples());
		WriteWave(Path.Combine(audioDirectory, "respawn.wav"), WavWriter.CreateRespawnSamples());
		WriteWave(Path.Combine(audioDirectory, "exit.wav"), WavWriter.CreateExitSamples());
		WriteWave(Path.Combine(audioDirectory, "fireball.wav"), WavWriter.CreateFireballSamples());
		WriteWave(Path.Combine(audioDirectory, "wizard-defeat.wav"), WavWriter.CreateWizardDefeatSamples());
	}

	/// <summary>
	/// Checks the committed assets against the generator output. PNG bytes may
	/// differ between platforms because of compression, so pixels are compared.
	/// Returns an empty list when everything is current.
	/// </summary>
	public static IReadOnlyList<string> Verify(string assetRoot)
	{
		var root = GetRoot(assetRoot);
		var errors = new List<string>();

		VerifyPng(Path.Combine(root, "art", "test-card.png"), SampleArt.CreateTestCard(), errors);
		VerifyPng(Path.Combine(root, "art", "lantern.png"), SampleArt.CreateLantern(), errors);
		VerifyPng(Path.Combine(root, "art", "lantern-sheet.png"), SampleArt.CreateLanternSheet(), errors);
		VerifyPng(Path.Combine(root, "art", "bruce.png"), CharacterArt.CreateBruceSheet(), errors);
		VerifyPng(Path.Combine(root, "art", "ninja.png"), CharacterArt.CreateNinjaSheet(), errors);
		VerifyPng(Path.Combine(root, "art", "yamo.png"), CharacterArt.CreateYamoSheet(), errors);
		VerifyPng(Path.Combine(root, "art", "tiles.png"), TileArt.CreateTiles(), errors);
		VerifyPng(Path.Combine(root, "art", "hud-life.png"), TileArt.CreateHudLife(), errors);
		VerifyPng(Path.Combine(root, "art", "hud-lantern.png"), TileArt.CreateHudLantern(), errors);
		VerifyWave(Path.Combine(root, "audio", "pickup.wav"), WavWriter.CreatePickupSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "jump.wav"), WavWriter.CreateJumpSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "land.wav"), WavWriter.CreateLandSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "chop.wav"), WavWriter.CreateChopSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "kick.wav"), WavWriter.CreateKickSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "hit.wav"), WavWriter.CreateHitSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "knockdown.wav"), WavWriter.CreateKnockdownSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "death.wav"), WavWriter.CreateDeathSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "respawn.wav"), WavWriter.CreateRespawnSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "exit.wav"), WavWriter.CreateExitSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "fireball.wav"), WavWriter.CreateFireballSamples(), errors);
		VerifyWave(Path.Combine(root, "audio", "wizard-defeat.wav"), WavWriter.CreateWizardDefeatSamples(), errors);

		return errors;
	}

	private static string GetRoot(string assetRoot)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(assetRoot);
		return Path.GetFullPath(assetRoot);
	}

	private static void WritePng(string path, RgbaImage image)
	{
		using var stream = File.Create(path);
		PngEncoder.EncodeToStream(stream, image.Pixels, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha);
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Verification reports decode failures instead of crashing the CI check.")]
	private static void VerifyPng(string path, RgbaImage expected, List<string> errors)
	{
		if (!File.Exists(path))
		{
			errors.Add($"Missing asset: {path}");
			return;
		}

		try
		{
			var image = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
			if (image.Width != expected.Width || image.Height != expected.Height)
			{
				errors.Add($"{path}: expected {expected.Width}x{expected.Height}, found {image.Width}x{image.Height}.");
				return;
			}

			if (!image.Data.AsSpan().SequenceEqual(expected.Pixels))
			{
				errors.Add($"{path}: pixel data differs from the generator output.");
			}
		}
		catch (Exception ex)
		{
			errors.Add($"{path}: failed to decode PNG: {ex.Message}");
		}
	}

	private static void WriteWave(string path, short[] samples)
	{
		WavWriter.Write(path, samples);
	}

	private static void VerifyWave(string path, short[] expected, List<string> errors)
	{
		if (!File.Exists(path))
		{
			errors.Add($"Missing asset: {path}");
			return;
		}

		using var expectedStream = new MemoryStream();
		WavWriter.Write(expectedStream, expected);

		if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(expectedStream.ToArray()))
		{
			errors.Add($"{path}: audio data differs from the generator output.");
		}
	}
}
