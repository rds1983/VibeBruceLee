using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Lantern and exit state for one chamber attempt. Reset on death; the score
/// already awarded is intentionally kept by the session.
/// </summary>
public sealed class ChamberProgress
{
	public const int LanternScore = 125;
	public const int ExitScore = 2000;
	public const int LanternSize = 16;

	private readonly ChamberDefinition _chamber;
	private readonly HashSet<string> _collected = new(StringComparer.Ordinal);

	public ChamberProgress(ChamberDefinition chamber)
	{
		ArgumentNullException.ThrowIfNull(chamber);

		_chamber = chamber;
		RequiredTotal = chamber.Lanterns.Count(lantern => lantern.Required);
	}

	public int RequiredTotal { get; }

	public int RequiredCollected { get; private set; }

	public int OptionalCollected { get; private set; }

	public int TotalCollected => RequiredCollected + OptionalCollected;

	public bool RequiredComplete => RequiredCollected >= RequiredTotal;

	public bool IsCollected(LanternDefinition lantern)
	{
		ArgumentNullException.ThrowIfNull(lantern);

		return _collected.Contains(lantern.Id);
	}

	/// <summary>
	/// Collects every lantern touching <paramref name="playerBounds"/> and
	/// returns how many were new.
	/// </summary>
	public int Collect(Aabb playerBounds)
	{
		var collected = 0;

		foreach (var lantern in _chamber.Lanterns)
		{
			if (_collected.Contains(lantern.Id))
			{
				continue;
			}

			var bounds = new Aabb(
				lantern.X - (LanternSize / 2f),
				lantern.Y - (LanternSize / 2f),
				LanternSize,
				LanternSize);

			if (!bounds.Intersects(playerBounds))
			{
				continue;
			}

			_collected.Add(lantern.Id);
			if (lantern.Required)
			{
				RequiredCollected++;
			}
			else
			{
				OptionalCollected++;
			}

			collected++;
		}

		return collected;
	}

	public bool IsExitOpen(ExitDefinition exit)
	{
		ArgumentNullException.ThrowIfNull(exit);

		return !exit.RequiresLanterns || RequiredComplete;
	}

	public bool TryFindOpenExit(Aabb playerBounds, out ExitDefinition? exit)
	{
		foreach (var candidate in _chamber.Exits)
		{
			if (!IsExitOpen(candidate))
			{
				continue;
			}

			var bounds = new Aabb(candidate.X, candidate.Y, candidate.Width, candidate.Height);
			if (bounds.Intersects(playerBounds))
			{
				exit = candidate;
				return true;
			}
		}

		exit = null;
		return false;
	}

	public void Reset()
	{
		_collected.Clear();
		RequiredCollected = 0;
		OptionalCollected = 0;
	}
}
