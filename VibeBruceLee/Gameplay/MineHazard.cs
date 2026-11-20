using VibeBruceLee.Levels;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Proximity mine: armed until something overlaps it, then a short fuse,
/// explosion and despawn.
/// </summary>
public sealed class MineHazard : Hazard
{
	public const float FuseSeconds = 0.35f;
	public const float ExplosionSeconds = 0.25f;

	private float _fuseTime;
	private float _explosionTime;
	private bool _triggered;

	public MineHazard(HazardDefinition definition)
		: base(definition, HazardKind.Mine)
	{
	}

	public bool IsArmed => !_triggered && !IsFinished;

	public override void OnOverlap()
	{
		if (!_triggered && !IsFinished)
		{
			_triggered = true;
			IsWarning = true;
		}
	}

	public override void Update(float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		if (!_triggered || IsFinished)
		{
			return;
		}

		if (_fuseTime < FuseSeconds)
		{
			_fuseTime += deltaSeconds;
			IsWarning = true;
			IsActive = false;
			return;
		}

		_explosionTime += deltaSeconds;
		IsWarning = false;
		IsActive = _explosionTime < ExplosionSeconds;

		if (_explosionTime >= ExplosionSeconds)
		{
			IsActive = false;
			IsVisible = false;
			IsFinished = true;
		}
	}
}
