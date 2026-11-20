using VibeBruceLee.Animation;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Shared idle/run frame maps. Hit, fallen, jump and attack frames live on the
/// enemy definition so the Ninja and Yamo sheets can use different poses.
/// </summary>
public static class EnemyAnimations
{
	public static readonly AnimationClip Idle = new("enemy-idle", [0, 1], 0.4, loop: true);

	public static readonly AnimationClip Run = new("enemy-run", [2, 3, 4, 5], 0.12, loop: true);
}
