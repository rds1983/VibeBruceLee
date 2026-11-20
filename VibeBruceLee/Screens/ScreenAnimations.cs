using VibeBruceLee.Animation;

namespace VibeBruceLee.Screens;

public static class ScreenAnimations
{
	public static readonly AnimationClip Lantern = new("lantern", [0, 1, 2, 3], 0.15, loop: true);

	public static readonly AnimationClip BruceIdle = new("bruce-idle", [0, 1], 0.35, loop: true);

	public static readonly AnimationClip BruceRun = new("bruce-run", [2, 3, 4, 5], 0.12, loop: true);
}
