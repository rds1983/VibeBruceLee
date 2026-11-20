namespace VibeBruceLee.Gameplay;

public enum EnemyState
{
	Idle,
	Patrol,
	Chase,
	Attack,
	Jumping,
	Stunned,
	KnockedDown,
	Respawning
}

/// <summary>
/// Shared tuning for an enemy type. The Ninja and Yamo differ only through
/// these values and their attack frame data; the state machine is shared.
/// </summary>
public sealed record EnemyDefinition(
	string Type,
	int MaxHits,
	int BodyWidth,
	int BodyHeight,
	float RunSpeed,
	float PatrolSpeed,
	float ClimbSpeed,
	int IdleFrames,
	float SightRange,
	float AttackRange,
	int AttackCooldownFrames,
	int KnockdownFrames,
	int RespawnFrames,
	int HitFlashFrames,
	int HitInvulnerabilityFrames,
	int KnockoutScore,
	AttackDefinition Attack,
	bool CanJump,
	float JumpSpeed,
	float JumpForwardSpeed,
	int JumpWindupFrames,
	float JumpRange,
	int JumpCooldownFrames,
	AttackDefinition? JumpAttack,
	AttackDefinition? SecondaryAttack,
	float SecondaryAttackRange,
	int SecondaryAttackCooldownFrames,
	float PreferredRange,
	bool BlocksMovement);

public static class EnemyDefinitions
{
	/// <summary>
	/// Fast pursuer. T-031 refines the bokken strike and jump attacks.
	/// </summary>
	public static readonly AttackDefinition NinjaStrike = new(
		AttackType.Chop,
		"Ninja strike",
		StartupFrames: 4,
		ActiveFrames: 3,
		RecoveryFrames: 8,
		StartupAnimationFrame: 6,
		ActiveAnimationFrame: 7,
		RecoveryAnimationFrame: 6,
		HitboxWidth: 12,
		HitboxHeight: 8,
		HitboxOffsetY: 6,
		HitPoints: 1,
		KnockbackX: 55f,
		KnockbackY: -15f,
		HitstunFrames: 10,
		KnocksDown: false,
		Score: 0,
		ForwardSpeed: 20f);

	/// <summary>
	/// Airborne bokken strike. Active frames happen during the middle of the
	/// fixed jump arc, so the whole leap is readable and jumpable.
	/// </summary>
	public static readonly AttackDefinition NinjaJumpAttack = new(
		AttackType.Kick,
		"Ninja leap strike",
		StartupFrames: 8,
		ActiveFrames: 14,
		RecoveryFrames: 12,
		StartupAnimationFrame: 4,
		ActiveAnimationFrame: 7,
		RecoveryAnimationFrame: 4,
		HitboxWidth: 14,
		HitboxHeight: 10,
		HitboxOffsetY: 4,
		HitPoints: 1,
		KnockbackX: 80f,
		KnockbackY: -25f,
		HitstunFrames: 14,
		KnocksDown: false,
		Score: 0,
		ForwardSpeed: 0f);

	/// <summary>
	/// Fast close-range wrestler blow.
	/// </summary>
	public static readonly AttackDefinition YamoPunch = new(
		AttackType.Chop,
		"Yamo punch",
		StartupFrames: 6,
		ActiveFrames: 3,
		RecoveryFrames: 8,
		StartupAnimationFrame: 6,
		ActiveAnimationFrame: 7,
		RecoveryAnimationFrame: 6,
		HitboxWidth: 12,
		HitboxHeight: 8,
		HitboxOffsetY: 7,
		HitPoints: 1,
		KnockbackX: 50f,
		KnockbackY: -15f,
		HitstunFrames: 10,
		KnocksDown: false,
		Score: 0,
		ForwardSpeed: 15f);

	/// <summary>
	/// Slow mid-range kick. The long wind-up and recovery make it punishable.
	/// </summary>
	public static readonly AttackDefinition YamoCrush = new(
		AttackType.Kick,
		"Yamo crush kick",
		StartupFrames: 12,
		ActiveFrames: 4,
		RecoveryFrames: 14,
		StartupAnimationFrame: 6,
		ActiveAnimationFrame: 7,
		RecoveryAnimationFrame: 6,
		HitboxWidth: 18,
		HitboxHeight: 10,
		HitboxOffsetY: 8,
		HitPoints: 1,
		KnockbackX: 95f,
		KnockbackY: -35f,
		HitstunFrames: 16,
		KnocksDown: true,
		Score: 0,
		ForwardSpeed: 30f);

	public static readonly EnemyDefinition Ninja = new(
		Type: "ninja",
		MaxHits: 2,
		BodyWidth: 10,
		BodyHeight: 22,
		RunSpeed: 70f,
		PatrolSpeed: 35f,
		ClimbSpeed: 70f,
		IdleFrames: 30,
		SightRange: 180f,
		AttackRange: 22f,
		AttackCooldownFrames: 45,
		KnockdownFrames: 50,
		RespawnFrames: 180,
		HitFlashFrames: 8,
		HitInvulnerabilityFrames: 10,
		KnockoutScore: 200,
		Attack: NinjaStrike,
		CanJump: true,
		JumpSpeed: 300f,
		JumpForwardSpeed: 110f,
		JumpWindupFrames: 12,
		JumpRange: 80f,
		JumpCooldownFrames: 90,
		JumpAttack: NinjaJumpAttack,
		SecondaryAttack: null,
		SecondaryAttackRange: 0f,
		SecondaryAttackCooldownFrames: 0,
		PreferredRange: 22f,
		BlocksMovement: false);

	public static readonly EnemyDefinition Yamo = new(
		Type: "yamo",
		MaxHits: 3,
		BodyWidth: 14,
		BodyHeight: 26,
		RunSpeed: 45f,
		PatrolSpeed: 24f,
		ClimbSpeed: 45f,
		IdleFrames: 40,
		SightRange: 150f,
		AttackRange: 24f,
		AttackCooldownFrames: 45,
		KnockdownFrames: 60,
		RespawnFrames: 240,
		HitFlashFrames: 8,
		HitInvulnerabilityFrames: 10,
		KnockoutScore: 450,
		Attack: YamoPunch,
		CanJump: false,
		JumpSpeed: 0f,
		JumpForwardSpeed: 0f,
		JumpWindupFrames: 0,
		JumpRange: 0f,
		JumpCooldownFrames: 0,
		JumpAttack: null,
		SecondaryAttack: YamoCrush,
		SecondaryAttackRange: 38f,
		SecondaryAttackCooldownFrames: 100,
		PreferredRange: 22f,
		BlocksMovement: true);

	public static EnemyDefinition Get(string type)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(type);

		return type switch
		{
			"ninja" => Ninja,
			"yamo" => Yamo,
			_ => throw new ArgumentOutOfRangeException(nameof(type), $"No enemy definition for '{type}'.")
		};
	}
}
