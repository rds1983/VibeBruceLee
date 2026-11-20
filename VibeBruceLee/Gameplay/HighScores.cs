using System.Text.Json;

namespace VibeBruceLee.Gameplay;

public sealed record HighScoreEntry(string Initials, int Score, int Loop);

/// <summary>
/// Persisted all-time high-score table. Missing or malformed files are treated
/// as an empty table; write failures leave the in-memory table intact.
/// </summary>
public sealed class HighScores
{
	public const int MaxEntries = 5;

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true
	};

	private readonly string _path;
	private readonly List<HighScoreEntry> _entries;

	public HighScores(string path)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);

		_path = path;
		_entries = Load(path);
	}

	public IReadOnlyList<HighScoreEntry> Entries => _entries;

	public int TopScore => _entries.Count > 0 ? _entries[0].Score : 0;

	public static string DefaultPath
	{
		get
		{
			var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (string.IsNullOrWhiteSpace(root))
			{
				root = AppContext.BaseDirectory;
			}

			return Path.Combine(root, "VibeBruceLee", "highscores.json");
		}
	}

	public bool Qualifies(int score)
	{
		if (score <= 0)
		{
			return false;
		}

		return _entries.Count < MaxEntries || score > _entries[^1].Score;
	}

	public void Add(string initials, int score, int loop)
	{
		if (score <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(score), "Only positive scores enter the table.");
		}

		_entries.Add(new HighScoreEntry(
			NormalizeInitials(initials),
			score,
			Math.Max(1, loop)));
		_entries.Sort(Compare);
		if (_entries.Count > MaxEntries)
		{
			_entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
		}

		Save();
	}

	public static string NormalizeInitials(string? initials)
	{
		var letters = new List<char>(3);
		foreach (var character in (initials ?? string.Empty).ToUpperInvariant())
		{
			if (char.IsAsciiLetter(character))
			{
				letters.Add(character);
				if (letters.Count == 3)
				{
					break;
				}
			}
		}

		while (letters.Count < 3)
		{
			letters.Add('A');
		}

		return new string([.. letters]);
	}

	private static List<HighScoreEntry> Load(string path)
	{
		if (!File.Exists(path))
		{
			return [];
		}

		try
		{
			var json = File.ReadAllText(path);
			var loaded = JsonSerializer.Deserialize<List<HighScoreEntry?>>(json, JsonOptions);
			if (loaded is null)
			{
				return [];
			}

			var entries = loaded
				.Where(entry => entry is not null && entry.Score > 0)
				.Select(entry => new HighScoreEntry(
					NormalizeInitials(entry!.Initials),
					entry.Score,
					Math.Max(1, entry.Loop)))
				.ToList();

			entries.Sort(Compare);
			if (entries.Count > MaxEntries)
			{
				entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
			}

			return entries;
		}
		catch (JsonException)
		{
			return [];
		}
		catch (IOException)
		{
			return [];
		}
		catch (UnauthorizedAccessException)
		{
			return [];
		}
	}

	private void Save()
	{
		try
		{
			var directory = Path.GetDirectoryName(_path);
			if (!string.IsNullOrWhiteSpace(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var temporary = _path + ".tmp";
			File.WriteAllText(temporary, JsonSerializer.Serialize(_entries, JsonOptions));
			File.Move(temporary, _path, overwrite: true);
		}
		catch (IOException)
		{
			// The in-memory table remains usable even if the disk is unavailable.
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	private static int Compare(HighScoreEntry a, HighScoreEntry b)
	{
		var score = b.Score.CompareTo(a.Score);
		if (score != 0)
		{
			return score;
		}

		var loop = b.Loop.CompareTo(a.Loop);
		return loop != 0 ? loop : string.CompareOrdinal(a.Initials, b.Initials);
	}
}
