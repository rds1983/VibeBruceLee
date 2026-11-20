using System.Buffers.Binary;
using VibeBruceLee.AssetGen;
using Xunit;

namespace VibeBruceLee.Tests;

public sealed class AssetGeneratorTests : IDisposable
{
	private static readonly string[] GeneratedFiles =
	[
		"art/test-card.png",
		"art/lantern.png",
		"art/lantern-sheet.png",
		"art/bruce.png",
		"art/ninja.png",
		"art/yamo.png",
		"art/tiles.png",
		"art/hud-life.png",
		"art/hud-lantern.png",
		"audio/pickup.wav",
		"audio/jump.wav",
		"audio/land.wav",
		"audio/chop.wav",
		"audio/kick.wav",
		"audio/hit.wav",
		"audio/knockdown.wav",
		"audio/death.wav",
		"audio/respawn.wav",
		"audio/exit.wav",
		"audio/fireball.wav",
		"audio/wizard-defeat.wav"
	];

	private readonly string _root;
	private readonly string _secondRoot;

	public AssetGeneratorTests()
	{
		_root = Path.Combine(Path.GetTempPath(), "vbl-assetgen-" + Guid.NewGuid().ToString("N"));
		_secondRoot = _root + "-second";
	}

	[Fact]
	public void GenerateWritesSpritesAndSound()
	{
		AssetGenerator.Generate(_root);

		foreach (var relativePath in GeneratedFiles)
		{
			Assert.True(File.Exists(Path.Combine(_root, relativePath)), $"Missing {relativePath}");
		}

		AssertPngSize(Path.Combine(_root, "art/test-card.png"), 64, 48);
		AssertPngSize(Path.Combine(_root, "art/lantern.png"), 16, 16);
		AssertPngSize(Path.Combine(_root, "art/lantern-sheet.png"), 64, 16);
		AssertPngSize(Path.Combine(_root, "art/bruce.png"), 320, 24);
		AssertPngSize(Path.Combine(_root, "art/ninja.png"), 160, 24);
		AssertPngSize(Path.Combine(_root, "art/yamo.png"), 240, 28);
		AssertPngSize(Path.Combine(_root, "art/tiles.png"), 32, 8);
		AssertPngSize(Path.Combine(_root, "art/hud-life.png"), 8, 8);
		AssertPngSize(Path.Combine(_root, "art/hud-lantern.png"), 8, 8);
		foreach (var sound in GeneratedFiles.Where(file => file.EndsWith(".wav", StringComparison.Ordinal)))
		{
			AssertWave(Path.Combine(_root, sound));
		}

		Assert.Empty(AssetGenerator.Verify(_root));
	}

	[Fact]
	public void VerifyReportsMissingOrCorruptAssets()
	{
		AssetGenerator.Generate(_root);

		File.Delete(Path.Combine(_root, "art/lantern.png"));
		var missing = AssetGenerator.Verify(_root);
		Assert.Contains(missing, error => error.Contains("lantern.png", StringComparison.Ordinal));

		AssetGenerator.Generate(_root);
		File.WriteAllText(Path.Combine(_root, "art/test-card.png"), "not a png");
		var corrupt = AssetGenerator.Verify(_root);
		Assert.Contains(corrupt, error => error.Contains("test-card.png", StringComparison.Ordinal));
	}

	[Fact]
	public void GenerateIsDeterministic()
	{
		AssetGenerator.Generate(_root);
		AssetGenerator.Generate(_secondRoot);

		foreach (var relativePath in GeneratedFiles)
		{
			var first = File.ReadAllBytes(Path.Combine(_root, relativePath));
			var second = File.ReadAllBytes(Path.Combine(_secondRoot, relativePath));
			Assert.Equal(first, second);
		}
	}

	private static void AssertPngSize(string path, int width, int height)
	{
		var bytes = File.ReadAllBytes(path);

		Assert.True(bytes.Length > 24);
		Assert.Equal(0x89, bytes[0]);
		Assert.Equal((byte)'P', bytes[1]);
		Assert.Equal((byte)'N', bytes[2]);
		Assert.Equal((byte)'G', bytes[3]);
		Assert.Equal((uint)width, BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16, 4)));
		Assert.Equal((uint)height, BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4)));
	}

	private static void AssertWave(string path)
	{
		var bytes = File.ReadAllBytes(path);

		Assert.True(bytes.Length > 44);
		Assert.Equal((byte)'R', bytes[0]);
		Assert.Equal((byte)'I', bytes[1]);
		Assert.Equal((byte)'F', bytes[2]);
		Assert.Equal((byte)'F', bytes[3]);
		Assert.Equal((byte)'W', bytes[8]);
		Assert.Equal((byte)'A', bytes[9]);
		Assert.Equal((byte)'V', bytes[10]);
		Assert.Equal((byte)'E', bytes[11]);
		Assert.Equal(1, BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(22, 2)));
		Assert.Equal(WavWriter.SampleRate, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(24, 4)));
		Assert.Equal(16, BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(34, 2)));
	}

	public void Dispose()
	{
		if (Directory.Exists(_root))
		{
			Directory.Delete(_root, true);
		}

		if (Directory.Exists(_secondRoot))
		{
			Directory.Delete(_secondRoot, true);
		}

		GC.SuppressFinalize(this);
	}
}
