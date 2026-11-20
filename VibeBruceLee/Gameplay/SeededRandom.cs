namespace VibeBruceLee.Gameplay;

/// <summary>
/// Small deterministic xorshift32 source. Unlike <see cref="Random"/>, the
/// sequence is fully specified by this class, so replays stay stable across
/// runtimes.
/// </summary>
public sealed class SeededRandom
{
	private uint _state;

	public SeededRandom(uint seed)
	{
		_state = seed == 0 ? 0x9E3779B9u : seed;
	}

	public uint NextUInt()
	{
		var value = _state;
		value ^= value << 13;
		value ^= value >> 17;
		value ^= value << 5;
		_state = value;
		return value;
	}

	public int NextInt(int maxExclusive)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxExclusive);

		return (int)(NextUInt() % (uint)maxExclusive);
	}

	public int NextInt(int minInclusive, int maxExclusive)
	{
		if (maxExclusive <= minInclusive)
		{
			throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Upper bound must be greater than the lower bound.");
		}

		return minInclusive + NextInt(maxExclusive - minInclusive);
	}

	/// <summary>
	/// Returns a value in [0, 1).
	/// </summary>
	public float NextFloat()
	{
		return (NextUInt() >> 8) * (1f / 16777216f);
	}

	public bool NextBool()
	{
		return (NextUInt() & 1) == 1;
	}
}
