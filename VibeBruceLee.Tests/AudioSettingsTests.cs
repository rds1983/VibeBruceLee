using VibeBruceLee.Gameplay;
using Xunit;

namespace VibeBruceLee.Tests;

public class AudioSettingsTests
{
	[Fact]
	public void VolumeStartsAtTheDefaultAndClamps()
	{
		var settings = new AudioSettings();

		Assert.Equal(AudioSettings.DefaultVolume, settings.Volume);
		Assert.False(settings.Muted);

		settings.SetVolume(5f);
		Assert.Equal(1f, settings.Volume);

		settings.SetVolume(-2f);
		Assert.Equal(0f, settings.Volume);
	}

	[Fact]
	public void VolumeAdjustmentUsesTheStep()
	{
		var settings = new AudioSettings();
		settings.SetVolume(0.5f);

		settings.AdjustVolume(AudioSettings.VolumeStep);
		Assert.Equal(0.6f, settings.Volume, 3);

		settings.AdjustVolume(-AudioSettings.VolumeStep * 2);
		Assert.Equal(0.4f, settings.Volume, 3);
	}

	[Fact]
	public void MuteToggles()
	{
		var settings = new AudioSettings();

		settings.ToggleMute();
		Assert.True(settings.Muted);

		settings.ToggleMute();
		Assert.False(settings.Muted);
	}
}
