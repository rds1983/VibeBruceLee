using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Lantern and exit state for one chamber attempt. A hub chamber passes in the
/// set owned by <see cref="HubProgress"/> so collected lanterns survive room
/// changes; other chambers use a private set.
/// </summary>
public sealed class ChamberProgress
{
	public const int LanternScore = 125;
	public const int ExitScore = 2000;
	public const int LanternSize = 16;

	private readonly ChamberDefinition _chamber;
	private readonly ISet<string> _collected;

	public ChamberProgress(ChamberDefinition chamber, ISet<string>? collected = null)
	{
		ArgumentNullException.ThrowIfNull(chamber);

		_chamber = chamber;
		_collected = collected ?? new HashSet<string>(StringComparer.Ordinal);
	}

	public int RequiredTotal => _chamber.Lanterns.Count(lantern => lantern.Required);

	public int RequiredCollected => _chamber.Lanterns.Count(lantern => lantern.Required && _collected.Contains(lantern.Id));

	public int OptionalCollected => _chamber.Lanterns.Count(lantern => !lantern.Required && _collected.Contains(lantern.Id));

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
			collected++;
		}

		return collected;
	}

	public bool IsExitOpen(ExitDefinition exit, bool hubHatchOpen = false)
	{
		ArgumentNullException.ThrowIfNull(exit);

		var lanternsOpen = !exit.RequiresLanterns || RequiredComplete;
		var hubOpen = !exit.RequiresHub || hubHatchOpen;
		return lanternsOpen && hubOpen;
	}

	public bool TryFindOpenExit(Aabb playerBounds, out ExitDefinition? exit)
	{
		return TryFindOpenExit(playerBounds, false, out exit);
	}

	public bool TryFindOpenExit(Aabb playerBounds, bool hubHatchOpen, out ExitDefinition? exit)
	{
		foreach (var candidate in _chamber.Exits)
		{
			if (!IsExitOpen(candidate, hubHatchOpen))
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
	}
}
