using VibeBruceLee.Gameplay;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class HubProgressTests
{
	private static ChamberDefinition Chamber(string id, bool hub, int required, int optional = 0)
	{
		var chamber = new ChamberDefinition
		{
			Id = id,
			Name = id,
			Hub = hub
		};

		for (var i = 0; i < required; i++)
		{
			chamber.Lanterns.Add(new LanternDefinition { Id = $"req-{i}", X = 16 + (i * 16), Y = 40, Required = true });
		}

		for (var i = 0; i < optional; i++)
		{
			chamber.Lanterns.Add(new LanternDefinition { Id = $"opt-{i}", X = 80 + (i * 16), Y = 40, Required = false });
		}

		return chamber;
	}

	private static ChamberCatalog CreateCatalog()
	{
		return new ChamberCatalog([
			Chamber("chamber-01", hub: true, required: 2, optional: 1),
			Chamber("chamber-02", hub: true, required: 1),
			Chamber("chamber-03", hub: true, required: 1),
			Chamber("chamber-04", hub: false, required: 3)
		]);
	}

	[Fact]
	public void HubCountsRequiredLanternsAcrossRooms()
	{
		var catalog = CreateCatalog();
		var hub = new HubProgress();

		Assert.Equal(4, HubProgress.RequiredTotal(catalog));
		Assert.Equal(0, hub.RequiredCollected(catalog));
		Assert.False(hub.AllRequiredCollected(catalog));

		hub.GetLanternSet("chamber-01").Add("req-0");
		hub.GetLanternSet("chamber-02").Add("req-0");

		Assert.Equal(2, hub.RequiredCollected(catalog));

		hub.GetLanternSet("chamber-01").Add("req-1");
		hub.GetLanternSet("chamber-03").Add("req-0");

		Assert.Equal(4, hub.RequiredCollected(catalog));
		Assert.True(hub.AllRequiredCollected(catalog));
	}

	[Fact]
	public void OptionalAndNonHubLanternsDoNotCount()
	{
		var catalog = CreateCatalog();
		var hub = new HubProgress();

		hub.GetLanternSet("chamber-01").Add("opt-0");
		hub.GetLanternSet("chamber-04").Add("req-0");

		Assert.Equal(0, hub.RequiredCollected(catalog));
		Assert.False(hub.AllRequiredCollected(catalog));
	}

	[Fact]
	public void ResettingOneHubRoomReclosesTheHatch()
	{
		var catalog = CreateCatalog();
		var hub = new HubProgress();
		foreach (var chamber in catalog.Chambers.Where(chamber => chamber.Hub))
		{
			var set = hub.GetLanternSet(chamber.Id);
			foreach (var lantern in chamber.Lanterns.Where(lantern => lantern.Required))
			{
				set.Add(lantern.Id);
			}
		}

		Assert.True(hub.AllRequiredCollected(catalog));

		hub.ResetChamber("chamber-02");

		Assert.Equal(3, hub.RequiredCollected(catalog));
		Assert.False(hub.AllRequiredCollected(catalog));
	}

	[Fact]
	public void StartNewRunClearsHubProgress()
	{
		var session = new GameSession();
		session.Hub.GetLanternSet("chamber-01").Add("req-0");

		session.StartNewRun();

		Assert.Equal(0, session.Hub.RequiredCollected(CreateCatalog()));
	}

	[Fact]
	public void SharedHubSetPersistsAcrossProgressInstances()
	{
		var catalog = CreateCatalog();
		var chamber = catalog.Get("chamber-01");
		var hub = new HubProgress();
		var first = new ChamberProgress(chamber, hub.GetLanternSet(chamber.Id));
		var player = new Aabb(chamber.Lanterns[0].X - 5f, chamber.Lanterns[0].Y - 20f, 10f, 22f);

		Assert.Equal(1, first.Collect(player));

		var second = new ChamberProgress(chamber, hub.GetLanternSet(chamber.Id));
		Assert.True(second.IsCollected(chamber.Lanterns[0]));
		Assert.Equal(1, second.RequiredCollected);

		second.Reset();

		Assert.False(first.IsCollected(chamber.Lanterns[0]));
	}

	[Fact]
	public void SampleChambersFormTheExpectedHub()
	{
		var directory = Path.Combine(AppContext.BaseDirectory, "Assets", "levels");
		var catalog = new ChamberCatalog(ChamberLoader.LoadAll(directory));

		Assert.Equal(5, catalog.Chambers.Count);
		Assert.True(catalog.Get("chamber-01").Hub);
		Assert.True(catalog.Get("chamber-02").Hub);
		Assert.True(catalog.Get("chamber-03").Hub);
		Assert.False(catalog.Get("chamber-04").Hub);
		Assert.False(catalog.Get("chamber-05").Hub);
		Assert.Equal(6, HubProgress.RequiredTotal(catalog));

		var hatch = catalog.Get("chamber-02").Exits.Single(exit => exit.RequiresHub);
		Assert.Equal("chamber-04", hatch.Target);
		Assert.False(hatch.RequiresLanterns);
	}

	[Fact]
	public void HubExitStaysLockedUntilTheHatchFlagOpens()
	{
		var chamber = new ChamberDefinition
		{
			Id = "chamber-02",
			Name = "Hatch",
			Exits =
			[
				new ExitDefinition
				{
					X = 88,
					Y = 168,
					Width = 16,
					Height = 8,
					Target = "chamber-04",
					RequiresLanterns = false,
					RequiresHub = true
				}
			]
		};
		var progress = new ChamberProgress(chamber);
		var exit = chamber.Exits[0];
		var player = new Aabb(86, 154, 10, 22);

		Assert.False(progress.IsExitOpen(exit, hubHatchOpen: false));
		Assert.True(progress.IsExitOpen(exit, hubHatchOpen: true));
		Assert.False(progress.TryFindOpenExit(player, hubHatchOpen: false, out _));
		Assert.True(progress.TryFindOpenExit(player, hubHatchOpen: true, out var found));
		Assert.Equal("chamber-04", found!.Target);
	}
}
