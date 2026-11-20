using VibeBruceLee.Gameplay;
using VibeBruceLee.Levels;
using Xunit;

namespace VibeBruceLee.Tests;

public class GameSessionTests
{
	[Fact]
	public void StartsWithFiveFalls()
	{
		var session = new GameSession();

		Assert.Equal(GameSession.StartingFalls, session.Falls);
		Assert.Equal(0, session.Score);
		Assert.False(session.IsGameOver);
	}

	[Fact]
	public void LosingFallsEventuallyEndsTheRun()
	{
		var session = new GameSession();

		for (var fall = 0; fall < GameSession.StartingFalls - 1; fall++)
		{
			Assert.False(session.LoseFall());
		}

		Assert.True(session.LoseFall());
		Assert.True(session.IsGameOver);
	}

	[Fact]
	public void ExtraFallsAreAwardedEveryThirtyThousandPoints()
	{
		var session = new GameSession();

		session.AddScore(GameSession.ExtraFallScore - 1);
		Assert.Equal(GameSession.StartingFalls, session.Falls);

		session.AddScore(1);
		Assert.Equal(GameSession.StartingFalls + 1, session.Falls);

		session.AddScore(GameSession.ExtraFallScore);
		Assert.Equal(GameSession.StartingFalls + 2, session.Falls);
		Assert.Equal(2, session.ExtraFallsAwarded);
	}

	[Fact]
	public void StartNewRunResetsScoreAndFalls()
	{
		var session = new GameSession();
		session.AddScore(65000);
		session.LoseFall();

		session.StartNewRun();

		Assert.Equal(0, session.Score);
		Assert.Equal(GameSession.StartingFalls, session.Falls);
		Assert.Equal(0, session.ExtraFallsAwarded);
	}

	[Fact]
	public void LoopsAdvanceWithoutResettingScoreOrFalls()
	{
		var session = new GameSession();
		session.AddScore(1250);
		session.LoseFall();
		session.Hub.GetLanternSet("chamber-01").Add("lantern-1");

		session.AdvanceLoop();

		Assert.Equal(2, session.Loop);
		Assert.Equal(1250, session.Score);
		Assert.Equal(GameSession.StartingFalls - 1, session.Falls);
		Assert.Equal(0, session.Hub.RequiredCollected(CreateCatalog()));
	}

	[Fact]
	public void StartNewRunResetsTheLoop()
	{
		var session = new GameSession();
		session.AdvanceLoop();
		session.AdvanceLoop();

		session.StartNewRun();

		Assert.Equal(1, session.Loop);
	}

	private static ChamberCatalog CreateCatalog()
	{
		return new ChamberCatalog([
			new ChamberDefinition
			{
				Id = "chamber-01",
				Name = "Test",
				Hub = true,
				Lanterns = [new LanternDefinition { Id = "lantern-1", X = 16, Y = 40, Required = true }]
			}
		]);
	}

	[Fact]
	public void NegativeScoreIsRejected()
	{
		var session = new GameSession();

		Assert.Throws<ArgumentOutOfRangeException>(() => session.AddScore(-1));
	}
}
