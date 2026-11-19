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

	public static void Write(string path, ReadOnlySpan<short> samples)
	{
		ArgumentNullException.ThrowIfNull(path);

		const short channels = 1;
		const short bitsPerSample = 16;
		var dataSize = samples.Length * sizeof(short);

		using var stream = File.Create(path);
		using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: false);

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
