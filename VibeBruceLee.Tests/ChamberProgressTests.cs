using VibeBruceLee.Gameplay;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class ChamberProgressTests
{
	private static ChamberDefinition CreateChamber(bool exitRequiresLanterns = true)
	{
		return new ChamberDefinition
		{
			Id = "test",
			Name = "Test",
			PlayerSpawn = new PointDefinition { X = 32, Y = 144 },
			Lanterns =
			[
				new LanternDefinition { Id = "a", X = 40, Y = 40, Required = true },
				new LanternDefinition { Id = "b", X = 80, Y = 40, Required = true },
				new LanternDefinition { Id = "c", X = 120, Y = 40, Required = false }
			],
			Exits =
			[
				new ExitDefinition
				{
					X = 240,
					Y = 160,
					Width = 16,
					Height = 16,
					Target = "next",
					RequiresLanterns = exitRequiresLanterns
				}
			]
		};
	}

	private static Aabb PlayerAt(float x, float feetY)
	{
		return new Aabb(x - 5f, feetY - 22f, 10f, 22f);
	}

	[Fact]
	public void RequiredLanternsGateTheExit()
	{
		var progress = new ChamberProgress(CreateChamber());
		var exit = CreateChamber().Exits[0];

		Assert.Equal(2, progress.RequiredTotal);
		Assert.False(progress.RequiredComplete);
		Assert.False(progress.IsExitOpen(exit));

		progress.Collect(PlayerAt(40, 60));
		Assert.Equal(1, progress.RequiredCollected);
		Assert.False(progress.IsExitOpen(exit));

		progress.Collect(PlayerAt(80, 60));
		Assert.Equal(2, progress.RequiredCollected);
		Assert.True(progress.RequiredComplete);
		Assert.True(progress.IsExitOpen(exit));
	}

	[Fact]
	public void OptionalLanternsScoreButDoNotGateTheExit()
	{
		var progress = new ChamberProgress(CreateChamber());

		progress.Collect(PlayerAt(40, 60));
		progress.Collect(PlayerAt(80, 60));
		Assert.True(progress.RequiredComplete);

		progress.Collect(PlayerAt(120, 60));

		Assert.Equal(2, progress.RequiredCollected);
		Assert.Equal(1, progress.OptionalCollected);
		Assert.Equal(3, progress.TotalCollected);
	}

	[Fact]
	public void CollectedLanternsAreNotCollectedTwice()
	{
		var progress = new ChamberProgress(CreateChamber());

		Assert.Equal(1, progress.Collect(PlayerAt(40, 60)));
		Assert.Equal(0, progress.Collect(PlayerAt(40, 60)));
		Assert.Equal(1, progress.RequiredCollected);
	}

	[Fact]
	public void OpenExitIsFoundOnlyWhenTouched()
	{
		var progress = new ChamberProgress(CreateChamber());

		Assert.False(progress.TryFindOpenExit(PlayerAt(100, 176), out _));

		progress.Collect(PlayerAt(40, 60));
		progress.Collect(PlayerAt(80, 60));

		Assert.False(progress.TryFindOpenExit(PlayerAt(100, 176), out _));
		Assert.True(progress.TryFindOpenExit(PlayerAt(246, 176), out var exit));
		Assert.Equal("next", exit!.Target);
	}

	[Fact]
	public void ResetClosesTheExitAndRestoresLanterns()
	{
		var progress = new ChamberProgress(CreateChamber());
		progress.Collect(PlayerAt(40, 60));
		progress.Collect(PlayerAt(80, 60));
		Assert.True(progress.RequiredComplete);

		progress.Reset();

		Assert.Equal(0, progress.RequiredCollected);
		Assert.Equal(0, progress.OptionalCollected);
		Assert.False(progress.RequiredComplete);
		Assert.Equal(1, progress.Collect(PlayerAt(40, 60)));
	}

	[Fact]
	public void ExitWithoutLanternRequirementIsOpenImmediately()
	{
		var chamber = CreateChamber(exitRequiresLanterns: false);
		var progress = new ChamberProgress(chamber);
		var exit = chamber.Exits[0];

		Assert.False(progress.RequiredComplete);
		Assert.True(progress.IsExitOpen(exit));
	}

	[Fact]
	public void ChamberWithoutRequiredLanternsIsComplete()
	{
		var chamber = CreateChamber();
		chamber.Lanterns.Clear();

		var progress = new ChamberProgress(chamber);

		Assert.Equal(0, progress.RequiredTotal);
		Assert.True(progress.RequiredComplete);
	}
}
