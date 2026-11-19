namespace VibeBruceLee.Core.Animation;

public sealed class AnimationClip
{
	public AnimationClip(string name, IReadOnlyList<int> frames, double frameDurationSeconds, bool loop = true)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(frames);

		if (frames.Count == 0)
		{
			throw new ArgumentException("An animation needs at least one frame.", nameof(frames));
		}

		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameDurationSeconds);

		var frameCopy = new int[frames.Count];
		for (var i = 0; i < frames.Count; i++)
		{
			if (frames[i] < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(frames), "Frame indices must not be negative.");
			}

			frameCopy[i] = frames[i];
		}

		Name = name;
		Frames = frameCopy;
		FrameDurationSeconds = frameDurationSeconds;
		Loop = loop;
	}

	public string Name { get; }

	public IReadOnlyList<int> Frames { get; }

	public double FrameDurationSeconds { get; }

	public bool Loop { get; }
}
