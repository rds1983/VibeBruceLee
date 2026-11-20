using VibeBruceLee.Animation;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Frame maps shared by the Ninja and Yamo sheets: 0-1 idle, 2-5 run,
/// 6-7 attack, 8 hit, 9 fallen.
/// </summary>
public static class EnemyAnimations
{
	public static readonly AnimationClip Idle = new("enemy-idle", [0, 1], 0.4, loop: true);

	public static readonly AnimationClip Run = new("enemy-run", [2, 3, 4, 5], 0.12, loop: true);
}
