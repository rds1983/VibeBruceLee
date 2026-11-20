namespace VibeBruceLee.Animation;

/// <summary>
/// Plays an animation clip on a fixed-step clock. Updates are deterministic for
/// a given sequence of delta times.
/// </summary>
public sealed class AnimationPlayer
{
	private AnimationClip? _clip;
	private double _elapsed;
	private int _frameIndex;
	private bool _isPlaying;
	private bool _isFinished;

	public AnimationClip? Clip => _clip;

	public int CurrentFrame => _clip is null ? 0 : _clip.Frames[_frameIndex];

	public int FrameIndex => _frameIndex;

	public bool IsPlaying => _isPlaying;

	public bool IsFinished => _isFinished;

	public void Play(AnimationClip clip)
	{
		ArgumentNullException.ThrowIfNull(clip);

		_clip = clip;
		_frameIndex = 0;
		_elapsed = 0;
		_isPlaying = true;
		_isFinished = false;
	}

	public void Stop()
	{
		_isPlaying = false;
	}

	public void Update(double elapsedSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(elapsedSeconds);

		if (!_isPlaying || _clip is null)
		{
			return;
		}

		_elapsed += elapsedSeconds;
		while (_elapsed >= _clip.FrameDurationSeconds)
		{
			_elapsed -= _clip.FrameDurationSeconds;

			if (_frameIndex + 1 < _clip.Frames.Count)
			{
				_frameIndex++;
			}
			else if (_clip.Loop)
			{
				_frameIndex = 0;
			}
			else
			{
				_isPlaying = false;
				_isFinished = true;
				_elapsed = 0;
				break;
			}
		}
	}
}
