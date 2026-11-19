using Microsoft.Xna.Framework;
using VibeBruceLee;
using Xunit;

namespace VibeBruceLee.Tests;

public class GameInfoTests
{
	[Fact]
	public void VirtualCanvasMatchesZxSpectrumResolution()
	{
		Assert.Equal(256, GameInfo.VirtualWidth);
		Assert.Equal(192, GameInfo.VirtualHeight);
	}

	[Fact]
	public void TitleIsSet()
	{
		Assert.False(string.IsNullOrWhiteSpace(GameInfo.Title));
	}

	[Fact]
	public void GameTypeIsAMonoGameGame()
	{
		Assert.True(typeof(Game).IsAssignableFrom(typeof(BruceLeeGame)));
	}
}
