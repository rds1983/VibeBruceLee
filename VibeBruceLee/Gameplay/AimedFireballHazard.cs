using Microsoft.Xna.Framework;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Fireball launched by the wizard. Unlike the fixed-axis projectile hazard it
/// carries a velocity vector aimed at Bruce when it is spawned.
/// </summary>
public sealed class AimedFireballHazard : Hazard
{
	public const int Size = 8;
	public const float LifetimeSeconds = 8f;

	private readonly Vector2 _velocity;
	private float _life = LifetimeSeconds;

	public AimedFireballHazard(Vector2 position, Vector2 velocity)
		: base(
			new HazardDefinition
			{
				Type = "fireball",
				X = (int)position.X,
				Y = (int)position.Y,
				Width = Size,
				Height = Size
			},
			HazardKind.Fireball)
	{
		_velocity = velocity;
		IsActive = true;
	}

	public Vector2 Velocity => _velocity;

	public override void Update(float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		if (IsFinished)
		{
			return;
		}

		_life -= deltaSeconds;
		Bounds = new Aabb(
			Bounds.X + (_velocity.X * deltaSeconds),
			Bounds.Y + (_velocity.Y * deltaSeconds),
			Bounds.Width,
			Bounds.Height);

		var leftScreen = Bounds.Right < 0f;
		var rightScreen = Bounds.X > GameInfo.VirtualWidth;
		var topScreen = Bounds.Bottom < 0f;
		var bottomScreen = Bounds.Y > GameInfo.VirtualHeight;

		if (_life <= 0f || leftScreen || rightScreen || topScreen || bottomScreen)
		{
			IsActive = false;
			IsVisible = false;
			IsFinished = true;
		}
	}
}
