namespace VibeBruceLee.Gameplay;

/// <summary>
/// Score and fall reserve for a single run.
/// </summary>
public sealed class GameSession
{
	public const int StartingFalls = 5;
	public const int ExtraFallScore = 30000;

	public GameSession()
	{
		StartNewRun();
	}

	public int Score { get; private set; }

	public int Falls { get; private set; }

	public int ExtraFallsAwarded { get; private set; }

	public int Loop { get; private set; } = 1;

	public HubProgress Hub { get; } = new();

	public bool IsGameOver => Falls <= 0;

	public void StartNewRun()
	{
		Score = 0;
		Falls = StartingFalls;
		ExtraFallsAwarded = 0;
		Loop = 1;
		Hub.Reset();
	}

	public void AdvanceLoop()
	{
		Loop++;
		Hub.Reset();
	}

	public void AddScore(int points)
	{
		if (points < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(points), "Score cannot decrease.");
		}

		Score += points;
		var earned = Score / ExtraFallScore;

		while (ExtraFallsAwarded < earned)
		{
			ExtraFallsAwarded++;
			Falls++;
		}
	}

	public bool LoseFall()
	{
		if (Falls > 0)
		{
			Falls--;
		}

		return Falls <= 0;
	}
}
