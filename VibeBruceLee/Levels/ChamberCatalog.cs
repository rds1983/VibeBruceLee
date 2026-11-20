namespace VibeBruceLee.Levels;

/// <summary>
/// Immutable set of loaded chambers, looked up by id. The order matches the
/// sorted file order, so the first chamber is stable.
/// </summary>
public sealed class ChamberCatalog
{
	private readonly List<ChamberDefinition> _ordered;
	private readonly Dictionary<string, ChamberDefinition> _byId;

	public ChamberCatalog(IReadOnlyList<ChamberDefinition> chambers)
	{
		ArgumentNullException.ThrowIfNull(chambers);

		if (chambers.Count == 0)
		{
			throw new ArgumentException("A chamber catalog needs at least one chamber.", nameof(chambers));
		}

		_ordered = new List<ChamberDefinition>(chambers.Count);
		_byId = new Dictionary<string, ChamberDefinition>(chambers.Count, StringComparer.Ordinal);

		foreach (var chamber in chambers)
		{
			ArgumentNullException.ThrowIfNull(chamber);

			if (!_byId.TryAdd(chamber.Id, chamber))
			{
				throw new ArgumentException($"Duplicate chamber id '{chamber.Id}'.", nameof(chambers));
			}

			_ordered.Add(chamber);
		}
	}

	public IReadOnlyList<ChamberDefinition> Chambers => _ordered;

	public ChamberDefinition First => _ordered[0];

	public ChamberDefinition Get(string id)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);

		if (!_byId.TryGetValue(id, out var chamber))
		{
			throw new KeyNotFoundException($"Chamber '{id}' is not loaded.");
		}

		return chamber;
	}

	public bool TryGet(string id, out ChamberDefinition chamber)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);

		return _byId.TryGetValue(id, out chamber!);
	}
}
