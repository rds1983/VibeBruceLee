using VibeBruceLee.Levels;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Lantern state shared by the entrance-hub chambers. Each chamber keeps its
/// own collected set so a death resets only the current room, while moving
/// between hub rooms preserves everything collected so far.
/// </summary>
public sealed class HubProgress
{
	private readonly Dictionary<string, HashSet<string>> _collectedByChamber = new(StringComparer.Ordinal);

	public ISet<string> GetLanternSet(string chamberId)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(chamberId);

		if (!_collectedByChamber.TryGetValue(chamberId, out var collected))
		{
			collected = new HashSet<string>(StringComparer.Ordinal);
			_collectedByChamber.Add(chamberId, collected);
		}

		return collected;
	}

	public void ResetChamber(string chamberId)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(chamberId);

		if (_collectedByChamber.TryGetValue(chamberId, out var collected))
		{
			collected.Clear();
		}
	}

	public void Reset()
	{
		_collectedByChamber.Clear();
	}

	public int RequiredCollected(ChamberCatalog catalog)
	{
		ArgumentNullException.ThrowIfNull(catalog);

		var collected = 0;
		foreach (var chamber in catalog.Chambers)
		{
			if (!chamber.Hub)
			{
				continue;
			}

			var set = GetLanternSet(chamber.Id);
			foreach (var lantern in chamber.Lanterns)
			{
				if (lantern.Required && set.Contains(lantern.Id))
				{
					collected++;
				}
			}
		}

		return collected;
	}

	public static int RequiredTotal(ChamberCatalog catalog)
	{
		ArgumentNullException.ThrowIfNull(catalog);

		var total = 0;
		foreach (var chamber in catalog.Chambers)
		{
			if (chamber.Hub)
			{
				total += chamber.Lanterns.Count(lantern => lantern.Required);
			}
		}

		return total;
	}

	public bool AllRequiredCollected(ChamberCatalog catalog)
	{
		ArgumentNullException.ThrowIfNull(catalog);

		return RequiredCollected(catalog) >= RequiredTotal(catalog);
	}
}
