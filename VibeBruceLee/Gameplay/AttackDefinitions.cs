namespace VibeBruceLee.Gameplay;

public enum AttackType
{
	None,
	Chop,
	Kick
}

/// <summary>
/// Central frame data for Bruce's attacks. Frames run at the fixed 60 Hz step.
/// </summary>
public sealed record AttackDefinition(
	AttackType Type,
	string Name,
	int StartupFrames,
	int ActiveFrames,
	int RecoveryFrames,
	int StartupAnimationFrame,
	int ActiveAnimationFrame,
	int RecoveryAnimationFrame,
	int HitboxWidth,
	int HitboxHeight,
	int HitboxOffsetY,
	int HitPoints,
	float KnockbackX,
	float KnockbackY,
	int HitstunFrames,
	bool KnocksDown,
	int Score)
{
	public int ActiveStart => StartupFrames;

	public int ActiveEnd => StartupFrames + ActiveFrames;

	public int TotalFrames => StartupFrames + ActiveFrames + RecoveryFrames;

	public bool IsActive(int frame) => frame >= ActiveStart && frame < ActiveEnd;
}

public static class AttackDefinitions
{
	/// <summary>
	/// Standing chop (the manual's "punch/chop"). Fast, high score.
	/// </summary>
	public static readonly AttackDefinition Chop = new(
		AttackType.Chop,
		"Chop",
		StartupFrames: 1,
		ActiveFrames: 2,
		RecoveryFrames: 3,
		StartupAnimationFrame: 8,
		ActiveAnimationFrame: 9,
		RecoveryAnimationFrame: 8,
		HitboxWidth: 10,
		HitboxHeight: 8,
		HitboxOffsetY: 6,
		HitPoints: 1,
		KnockbackX: 50f,
		KnockbackY: -15f,
		HitstunFrames: 8,
		KnocksDown: false,
		Score: 100);

	/// <summary>
	/// Running kick. Reviewers call this the flying kick; the original manual
	/// documents no separate airborne attack.
	/// </summary>
	public static readonly AttackDefinition Kick = new(
		AttackType.Kick,
		"Kick",
		StartupFrames: 2,
		ActiveFrames: 2,
		RecoveryFrames: 4,
		StartupAnimationFrame: 12,
		ActiveAnimationFrame: 13,
		RecoveryAnimationFrame: 12,
		HitboxWidth: 16,
		HitboxHeight: 8,
		HitboxOffsetY: 10,
		HitPoints: 1,
		KnockbackX: 90f,
		KnockbackY: -20f,
		HitstunFrames: 12,
		KnocksDown: false,
		Score: 75);

	public static AttackDefinition Get(AttackType type)
	{
		return type switch
		{
			AttackType.Chop => Chop,
			AttackType.Kick => Kick,
			_ => throw new ArgumentOutOfRangeException(nameof(type), $"No attack definition for {type}.")
		};
	}
}
