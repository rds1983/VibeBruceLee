using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Straight-line projectile used by daggers, swords, bugs and fireballs. It
/// damages on contact and despawns after leaving the screen or timing out.
/// </summary>
public sealed class ProjectileHazard : Hazard
{
	public const float DefaultSpeed = 80f;
	public const float LifetimeSeconds = 8f;

	private readonly int _direction;
	private readonly float _speed;
	private float _life = LifetimeSeconds;

	public ProjectileHazard(HazardDefinition definition, HazardKind kind)
		: base(definition, kind)
	{
		_direction = definition.DirectionX >= 0 ? 1 : -1;
		_speed = definition.Speed is > 0 ? (float)definition.Speed.Value : DefaultSpeed;
		IsActive = true;
	}

	public override void Update(float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		if (IsFinished)
		{
			return;
		}

		_life -= deltaSeconds;
		var x = Bounds.X + (_direction * _speed * deltaSeconds);
		Bounds = new Aabb(x, Bounds.Y, Bounds.Width, Bounds.Height);

		var leftScreen = Bounds.Right < 0f;
		var rightScreen = Bounds.X > GameInfo.VirtualWidth;
		if (_life <= 0f || leftScreen || rightScreen)
		{
			IsActive = false;
			IsVisible = false;
			IsFinished = true;
		}
	}
}
