using VibeBruceLee.Levels;
using Xunit;

namespace VibeBruceLee.Tests;

public class ChamberCatalogTests
{
	private static ChamberDefinition Chamber(string id)
	{
		return new ChamberDefinition
		{
			Id = id,
			Name = id
		};
	}

	[Fact]
	public void FirstAndLookupFollowTheLoadedOrder()
	{
		var catalog = new ChamberCatalog([
			Chamber("chamber-01"),
			Chamber("chamber-02"),
			Chamber("chamber-03")
		]);

		Assert.Equal("chamber-01", catalog.First.Id);
		Assert.Equal("chamber-02", catalog.Get("chamber-02").Id);
		Assert.True(catalog.TryGet("chamber-03", out var chamber));
		Assert.Equal("chamber-03", chamber.Id);
		Assert.False(catalog.TryGet("missing", out _));
	}

	[Fact]
	public void MissingChamberThrows()
	{
		var catalog = new ChamberCatalog([Chamber("chamber-01")]);

		Assert.Throws<KeyNotFoundException>(() => catalog.Get("missing"));
	}

	[Fact]
	public void DuplicateIdsAreRejected()
	{
		Assert.Throws<ArgumentException>(() => new ChamberCatalog([
			Chamber("chamber-01"),
			Chamber("chamber-01")
		]));
	}

	[Fact]
	public void EmptyCatalogIsRejected()
	{
		Assert.Throws<ArgumentException>(() => new ChamberCatalog([]));
	}
}
