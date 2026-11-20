using VibeBruceLee.Levels;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Electric arc or PAN floor light: off, warning, then a short damaging burst.
/// The cycle repeats and the hazard never despawns.
/// </summary>
public sealed class TimedBeamHazard : Hazard
{
	public const float ActiveSeconds = 0.4f;
	public const float WarningSeconds = 0.3f;
	public const float MinimumPeriodSeconds = 0.8f;

	private readonly float _period;
	private float _time;

	public TimedBeamHazard(HazardDefinition definition, HazardKind kind)
		: base(definition, kind)
	{
		_period = MathF.Max(MinimumPeriodSeconds, PositiveSeconds(definition.PeriodSeconds, 2f));
	}

	public float PeriodSeconds => _period;

	public override void Update(float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		_time = (_time + deltaSeconds) % _period;
		var activeStart = _period - ActiveSeconds;
		var warningStart = activeStart - WarningSeconds;

		IsWarning = _time >= warningStart && _time < activeStart;
		IsActive = _time >= activeStart;
		IsVisible = true;
	}
}
