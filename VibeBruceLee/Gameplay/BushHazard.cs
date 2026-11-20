using VibeBruceLee.Levels;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// T'Sung-Lin bush: hidden, briefly visible as a warning, then a damaging
/// explosion. Despawns after the explosion and returns on chamber reset.
/// </summary>
public sealed class BushHazard : Hazard
{
	public const float HiddenSeconds = 1.2f;
	public const float WarningSeconds = 0.45f;
	public const float ExplosionSeconds = 0.35f;

	private float _time;

	public BushHazard(HazardDefinition definition)
		: base(definition, HazardKind.Bush)
	{
		IsVisible = false;
	}

	public override void Update(float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		_time += deltaSeconds;
		var warningStart = HiddenSeconds;
		var explosionStart = warningStart + WarningSeconds;
		var finish = explosionStart + ExplosionSeconds;

		IsVisible = _time >= warningStart && _time < finish;
		IsWarning = _time >= warningStart && _time < explosionStart;
		IsActive = _time >= explosionStart && _time < finish;
		IsFinished = _time >= finish;

		if (IsFinished)
		{
			IsVisible = false;
			IsActive = false;
			IsWarning = false;
		}
	}
}
