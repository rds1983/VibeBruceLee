using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Crusher that sweeps horizontally. The definition rectangle is the travel
/// range; the moving wall itself is eight pixels wide and as tall as the
/// rectangle.
/// </summary>
public sealed class MovingWallHazard : Hazard
{
	public const int WallWidth = 8;

	private readonly float _startX;
	private readonly float _endX;
	private readonly float _speed;
	private int _direction;

	public MovingWallHazard(HazardDefinition definition)
		: base(definition, HazardKind.MovingWall)
	{
		_startX = definition.X;
		_endX = definition.X + MathF.Max(0f, definition.Width - WallWidth);
		_speed = PositiveSeconds(definition.Speed, 40f);
		_direction = definition.DirectionX >= 0 ? 1 : -1;
		IsActive = true;

		Bounds = new Aabb(
			_direction > 0 ? _startX : _endX,
			definition.Y,
			MathF.Min(WallWidth, definition.Width),
			definition.Height);
	}

	public override void Update(float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		var x = Bounds.X + (_direction * _speed * deltaSeconds);

		if (x >= _endX)
		{
			x = _endX;
			_direction = -1;
		}
		else if (x <= _startX)
		{
			x = _startX;
			_direction = 1;
		}

		Bounds = new Aabb(
			ClampX(x),
			Definition.Y,
			MathF.Min(WallWidth, Definition.Width),
			Definition.Height);
		IsActive = true;
		IsVisible = true;
	}

	private float ClampX(float x)
	{
		return Math.Clamp(x, _startX, _endX);
	}
}
