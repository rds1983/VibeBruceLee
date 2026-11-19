using Microsoft.Xna.Framework;
using VibeBruceLee.Presentation;
using Xunit;

namespace VibeBruceLee.Tests;

public class LetterboxTests
{
	[Fact]
	public void DefaultWindowFillsExactly()
	{
		var layout = Letterbox.Calculate(1280, 960);

		Assert.Equal(5, layout.Scale);
		Assert.Equal(new Rectangle(0, 0, 1280, 960), layout.Destination);
	}

	[Fact]
	public void WideWindowLetterboxesHorizontally()
	{
		var layout = Letterbox.Calculate(1000, 960);

		Assert.Equal(3, layout.Scale);
		Assert.Equal(new Rectangle(116, 192, 768, 576), layout.Destination);
	}

	[Fact]
	public void FullHdLetterboxesVertically()
	{
		var layout = Letterbox.Calculate(1920, 1080);

		Assert.Equal(5, layout.Scale);
		Assert.Equal(new Rectangle(320, 60, 1280, 960), layout.Destination);
	}

	[Fact]
	public void SmallWindowUsesScaleOne()
	{
		var layout = Letterbox.Calculate(300, 200);

		Assert.Equal(1, layout.Scale);
		Assert.Equal(new Rectangle(22, 4, 256, 192), layout.Destination);
	}

	[Fact]
	public void InvalidSizeThrows()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => Letterbox.Calculate(0, 960));
		Assert.Throws<ArgumentOutOfRangeException>(() => Letterbox.Calculate(1280, -1));
	}
}
