namespace VibeBruceLee.Gameplay;

/// <summary>
/// Per-loop enemy tuning. Loop 1 is the baseline; later loops speed enemies
/// up, shorten their decision/attack delays and remove the respawn wait.
/// </summary>
public static class EnemyScaling
{
	public const float SpeedStep = 0.15f;
	public const float CooldownStep = 0.1f;
	public const float SightStep = 0.1f;
	public const float MaxSpeedMultiplier = 2f;
	public const float MinCooldownMultiplier = 0.5f;
	public const float MaxSightMultiplier = 1.5f;

	public static EnemyDefinition Apply(EnemyDefinition definition, int loop)
	{
		ArgumentNullException.ThrowIfNull(definition);

		if (loop <= 1)
		{
			return definition;
		}

		var steps = loop - 1;
		var speed = MathF.Min(MaxSpeedMultiplier, 1f + (SpeedStep * steps));
		var cooldown = MathF.Max(MinCooldownMultiplier, 1f - (CooldownStep * steps));
		var sight = MathF.Min(MaxSightMultiplier, 1f + (SightStep * steps));

		return definition with
		{
			RunSpeed = definition.RunSpeed * speed,
			PatrolSpeed = definition.PatrolSpeed * speed,
			ClimbSpeed = definition.ClimbSpeed * speed,
			SightRange = definition.SightRange * sight,
			IdleFrames = Math.Max(5, (int)(definition.IdleFrames * cooldown)),
			AttackCooldownFrames = Math.Max(5, (int)(definition.AttackCooldownFrames * cooldown)),
			SecondaryAttackCooldownFrames = Math.Max(0, (int)(definition.SecondaryAttackCooldownFrames * cooldown)),
			JumpCooldownFrames = Math.Max(0, (int)(definition.JumpCooldownFrames * cooldown)),
			JumpForwardSpeed = definition.JumpForwardSpeed * speed,
			RespawnFrames = 0
		};
	}
}
