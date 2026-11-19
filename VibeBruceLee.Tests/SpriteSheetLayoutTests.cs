using Microsoft.Xna.Framework;
using VibeBruceLee.Core.Graphics;
using Xunit;

namespace VibeBruceLee.Tests;

public class SpriteSheetLayoutTests
{
	[Fact]
	public void SingleRowSheetIndexesFramesLeftToRight()
	{
		var layout = new SpriteSheetLayout(64, 16, 16, 16);

		Assert.Equal(4, layout.Columns);
		Assert.Equal(1, layout.Rows);
		Assert.Equal(4, layout.FrameCount);
		Assert.Equal(new Rectangle(0, 0, 16, 16), layout.GetSourceRectangle(0));
		Assert.Equal(new Rectangle(48, 0, 16, 16), layout.GetSourceRectangle(3));
	}

	[Fact]
	public void GridSheetIndexesFramesRowMajor()
	{
		var layout = new SpriteSheetLayout(32, 32, 16, 16);

		Assert.Equal(2, layout.Columns);
		Assert.Equal(2, layout.Rows);
		Assert.Equal(new Rectangle(0, 16, 16, 16), layout.GetSourceRectangle(2));
		Assert.Equal(new Rectangle(16, 16, 16, 16), layout.GetSourceRectangle(3));
	}

	[Fact]
	public void InvalidFrameIndexThrows()
	{
		var layout = new SpriteSheetLayout(64, 16, 16, 16);

		Assert.Throws<ArgumentOutOfRangeException>(() => layout.GetSourceRectangle(-1));
		Assert.Throws<ArgumentOutOfRangeException>(() => layout.GetSourceRectangle(4));
	}

	[Fact]
	public void FrameMustDivideTextureExactly()
	{
		Assert.Throws<ArgumentException>(() => new SpriteSheetLayout(30, 16, 16, 16));
		Assert.Throws<ArgumentException>(() => new SpriteSheetLayout(64, 20, 16, 16));
	}

	[Fact]
	public void ZeroSizesAreRejected()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new SpriteSheetLayout(0, 16, 16, 16));
		Assert.Throws<ArgumentOutOfRangeException>(() => new SpriteSheetLayout(64, 16, 0, 16));
	}
}
