using VibeBruceLee.Core.Animation;

namespace VibeBruceLee.Core.Gameplay;

public enum PlayerState
{
	Idle,
	Run,
	Jump,
	Fall,
	Duck,
	Climb
}

public static class PlayerAnimations
{
	public static readonly AnimationClip Idle = new("player-idle", [0, 1], 0.35, loop: true);

	public static readonly AnimationClip Run = new("player-run", [2, 3, 4, 5], 0.12, loop: true);

	public static readonly AnimationClip Air = new("player-air", [6], 1.0, loop: false);

	public static readonly AnimationClip Duck = new("player-duck", [7], 1.0, loop: false);

	public static readonly AnimationClip Climb = new("player-climb", [14, 15], 0.2, loop: true);
}
