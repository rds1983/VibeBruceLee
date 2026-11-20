namespace VibeBruceLee.Gameplay;

/// <summary>
/// User-facing audio state shared by every screen.
/// </summary>
public sealed class AudioSettings
{
	public const float DefaultVolume = 0.7f;
	public const float VolumeStep = 0.1f;

	public float Volume { get; private set; } = DefaultVolume;

	public bool Muted { get; private set; }

	public void ToggleMute()
	{
		Muted = !Muted;
	}

	public void SetVolume(float volume)
	{
		Volume = Math.Clamp(volume, 0f, 1f);
	}

	public void AdjustVolume(float delta)
	{
		SetVolume(Volume + delta);
	}
}
