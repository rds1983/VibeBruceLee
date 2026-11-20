using VibeBruceLee.Gameplay;
using Xunit;

namespace VibeBruceLee.Tests;

public sealed class HighScoresTests : IDisposable
{
	private readonly string _path;

	public HighScoresTests()
	{
		_path = Path.Combine(Path.GetTempPath(), "vbl-highscores-" + Guid.NewGuid().ToString("N") + ".json");
	}

	[Fact]
	public void PositiveScoresQualifyUntilTheTableIsFull()
	{
		var scores = new HighScores(_path);

		Assert.False(scores.Qualifies(0));
		Assert.True(scores.Qualifies(10));

		for (var i = 1; i <= HighScores.MaxEntries; i++)
		{
			scores.Add("AAA", i * 100, 1);
		}

		Assert.False(scores.Qualifies(100));
		Assert.True(scores.Qualifies(101));
	}

	[Fact]
	public void EntriesAreSortedAndTrimmed()
	{
		var scores = new HighScores(_path);
		scores.Add("LOW", 100, 1);
		scores.Add("TOP", 900, 2);
		scores.Add("MID", 500, 1);
		scores.Add("FOU", 400, 1);
		scores.Add("FIV", 300, 1);
		scores.Add("SIX", 200, 1);

		Assert.Equal(HighScores.MaxEntries, scores.Entries.Count);
		Assert.Equal("TOP", scores.Entries[0].Initials);
		Assert.Equal(900, scores.TopScore);
		Assert.DoesNotContain(scores.Entries, entry => entry.Initials == "LOW");
	}

	[Fact]
	public void ScoresSurviveReloadingFromDisk()
	{
		var scores = new HighScores(_path);
		scores.Add("BRU", 1250, 2);
		scores.Add("YAM", 700, 1);

		var reloaded = new HighScores(_path);

		Assert.Equal(2, reloaded.Entries.Count);
		Assert.Equal("BRU", reloaded.Entries[0].Initials);
		Assert.Equal(1250, reloaded.Entries[0].Score);
		Assert.Equal(2, reloaded.Entries[0].Loop);
	}

	[Fact]
	public void MalformedFileFailsSafely()
	{
		File.WriteAllText(_path, "{ this is not json");

		var scores = new HighScores(_path);

		Assert.Empty(scores.Entries);
		Assert.True(scores.Qualifies(1));
	}

	[Fact]
	public void MissingFileStartsEmpty()
	{
		var scores = new HighScores(_path);

		Assert.Empty(scores.Entries);
		Assert.Equal(0, scores.TopScore);
	}

	[Fact]
	public void InitialsAreNormalizedToThreeLetters()
	{
		Assert.Equal("ABC", HighScores.NormalizeInitials("abc"));
		Assert.Equal("ABC", HighScores.NormalizeInitials("a1b2c3"));
		Assert.Equal("AAA", HighScores.NormalizeInitials(null));
		Assert.Equal("A", HighScores.NormalizeInitials("A")[..1]);
	}

	public void Dispose()
	{
		if (File.Exists(_path))
		{
			File.Delete(_path);
		}

		var temp = _path + ".tmp";
		if (File.Exists(temp))
		{
			File.Delete(temp);
		}
	}
}
