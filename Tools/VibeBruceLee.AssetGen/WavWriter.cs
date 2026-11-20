using System.Text;

namespace VibeBruceLee.AssetGen;

public static class WavWriter
{
	public const int SampleRate = 22050;
	public const int PickupFrequency = 880;

	public static void WritePickup(string path)
	{
		Write(path, CreatePickupSamples());
	}

	public static short[] CreatePickupSamples()
	{
		var sampleCount = SampleRate * 12 / 100;
		var samples = new short[sampleCount];
		const int amplitude = 9000;

		for (var i = 0; i < sampleCount; i++)
		{
			var phase = (long)i * PickupFrequency * 2 / SampleRate;
			var envelope = amplitude * (sampleCount - i) / sampleCount;
			samples[i] = (short)(phase % 2 == 0 ? envelope : -envelope);
		}

		return samples;
	}

	public static short[] CreateJumpSamples()
	{
		return CreateSweep(0.14f, 220f, 520f, 9000);
	}

	public static short[] CreateLandSamples()
	{
		return CreateSweep(0.10f, 110f, 55f, 7000, noiseMix: 0.45f, seed: 11);
	}

	public static short[] CreateChopSamples()
	{
		return CreateSweep(0.10f, 700f, 240f, 8500, noiseMix: 0.4f, seed: 23);
	}

	public static short[] CreateKickSamples()
	{
		return CreateSweep(0.16f, 340f, 120f, 8500, noiseMix: 0.3f, seed: 37);
	}

	public static short[] CreateHitSamples()
	{
		return CreateSweep(0.10f, 420f, 110f, 9000, noiseMix: 0.7f, seed: 53);
	}

	public static short[] CreateKnockdownSamples()
	{
		return CreateSweep(0.28f, 500f, 95f, 9000, noiseMix: 0.25f, seed: 71);
	}

	public static short[] CreateDeathSamples()
	{
		return CreateSweep(0.62f, 480f, 70f, 8500, noiseMix: 0.15f, seed: 89);
	}

	public static short[] CreateRespawnSamples()
	{
		return CreateArpeggio(0.30f, [330f, 440f, 660f], 8000);
	}

	public static short[] CreateExitSamples()
	{
		return CreateArpeggio(0.40f, [440f, 660f, 880f, 1100f], 8000);
	}

	public static short[] CreateFireballSamples()
	{
		return CreateSweep(0.25f, 320f, 80f, 9000, noiseMix: 0.75f, seed: 101);
	}

	public static short[] CreateWizardDefeatSamples()
	{
		return CreateSweep(0.80f, 520f, 40f, 11000, noiseMix: 0.6f, seed: 131);
	}

	private static short[] CreateSweep(
		float seconds,
		float startFrequency,
		float endFrequency,
		int amplitude,
		float noiseMix = 0f,
		uint seed = 1)
	{
		var sampleCount = Math.Max(1, (int)(SampleRate * seconds));
		var samples = new short[sampleCount];
		var phase = 0f;
		var state = seed;

		for (var i = 0; i < sampleCount; i++)
		{
			var time = i / (float)sampleCount;
			var frequency = startFrequency + ((endFrequency - startFrequency) * time);
			phase += frequency / SampleRate;
			phase -= MathF.Floor(phase);

			var square = phase < 0.5f ? 1f : -1f;
			state = (state * 1664525u) + 1013904223u;
			var noise = (((state >> 9) & 0xFFFF) / 32768f) - 1f;
			var envelope = 1f - time;
			var value = ((square * (1f - noiseMix)) + (noise * noiseMix)) * amplitude * envelope;
			samples[i] = (short)Math.Clamp(value, short.MinValue, short.MaxValue);
		}

		return samples;
	}

	private static short[] CreateArpeggio(float seconds, float[] frequencies, int amplitude)
	{
		var sampleCount = Math.Max(1, (int)(SampleRate * seconds));
		var samples = new short[sampleCount];
		var phase = 0f;

		for (var i = 0; i < sampleCount; i++)
		{
			var time = i / (float)sampleCount;
			var note = Math.Min(frequencies.Length - 1, (int)(time * frequencies.Length));
			phase += frequencies[note] / SampleRate;
			phase -= MathF.Floor(phase);

			var envelope = 1f - time;
			samples[i] = (short)((phase < 0.5f ? 1f : -1f) * amplitude * envelope);
		}

		return samples;
	}

	public static void Write(string path, ReadOnlySpan<short> samples)
	{
		ArgumentNullException.ThrowIfNull(path);

		using var stream = File.Create(path);
		Write(stream, samples);
	}

	public static void Write(Stream stream, ReadOnlySpan<short> samples)
	{
		ArgumentNullException.ThrowIfNull(stream);

		const short channels = 1;
		const short bitsPerSample = 16;
		var dataSize = samples.Length * sizeof(short);

		using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

		writer.Write(Encoding.ASCII.GetBytes("RIFF"));
		writer.Write(36 + dataSize);
		writer.Write(Encoding.ASCII.GetBytes("WAVE"));
		writer.Write(Encoding.ASCII.GetBytes("fmt "));
		writer.Write(16);
		writer.Write((short)1);
		writer.Write(channels);
		writer.Write(SampleRate);
		writer.Write(SampleRate * channels * bitsPerSample / 8);
		writer.Write((short)(channels * bitsPerSample / 8));
		writer.Write(bitsPerSample);
		writer.Write(Encoding.ASCII.GetBytes("data"));
		writer.Write(dataSize);

		foreach (var sample in samples)
		{
			writer.Write(sample);
		}
	}
}
