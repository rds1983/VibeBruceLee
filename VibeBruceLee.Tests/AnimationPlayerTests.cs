using VibeBruceLee.Core.Animation;
using Xunit;

namespace VibeBruceLee.Tests;

public class AnimationPlayerTests
{
	[Fact]
	public void PlayStartsAtFirstFrame()
	{
		var player = new AnimationPlayer();
		var clip = new AnimationClip("test", [0, 1, 2], 0.1, loop: false);

		player.Play(clip);

		Assert.Same(clip, player.Clip);
		Assert.Equal(0, player.CurrentFrame);
		Assert.True(player.IsPlaying);
		Assert.False(player.IsFinished);
	}

	[Fact]
	public void UpdateAdvancesFramesAtClipDuration()
	{
		var player = new AnimationPlayer();
		player.Play(new AnimationClip("test", [0, 1, 2], 0.1, loop: false));

		player.Update(0.09);
		Assert.Equal(0, player.CurrentFrame);

		player.Update(0.02);
		Assert.Equal(1, player.CurrentFrame);

		player.Update(0.1);
		Assert.Equal(2, player.CurrentFrame);
		Assert.True(player.IsPlaying);

		player.Update(0.1);
		Assert.Equal(2, player.CurrentFrame);
		Assert.False(player.IsPlaying);
		Assert.True(player.IsFinished);
	}

	[Fact]
	public void LoopingClipWrapsToFirstFrame()
	{
		var player = new AnimationPlayer();
		player.Play(new AnimationClip("test", [0, 1], 0.1, loop: true));

		player.Update(0.25);

		Assert.Equal(0, player.CurrentFrame);
		Assert.True(player.IsPlaying);
	}

	[Fact]
	public void LargeDeltaAdvancesMultipleFrames()
	{
		var player = new AnimationPlayer();
		player.Play(new AnimationClip("test", [0, 1, 2], 0.1, loop: false));

		player.Update(0.25);

		Assert.Equal(2, player.CurrentFrame);
	}

	[Fact]
	public void PlayRestartsAtFirstFrame()
	{
		var player = new AnimationPlayer();
		var clip = new AnimationClip("test", [0, 1, 2], 0.1, loop: true);
		player.Play(clip);
		player.Update(0.15);

		player.Play(clip);

		Assert.Equal(0, player.CurrentFrame);
		Assert.True(player.IsPlaying);
	}

	[Fact]
	public void StopFreezesTheCurrentFrame()
	{
		var player = new AnimationPlayer();
		player.Play(new AnimationClip("test", [0, 1, 2], 0.1, loop: true));
		player.Update(0.15);

		player.Stop();
		player.Update(0.5);

		Assert.Equal(1, player.CurrentFrame);
		Assert.False(player.IsPlaying);
	}

	[Fact]
	public void InvalidClipsAreRejected()
	{
		Assert.Throws<ArgumentException>(() => new AnimationClip("test", [], 0.1));
		Assert.Throws<ArgumentOutOfRangeException>(() => new AnimationClip("test", [0], 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => new AnimationClip("test", [-1], 0.1));
	}
}
