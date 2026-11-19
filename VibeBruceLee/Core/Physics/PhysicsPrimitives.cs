using Microsoft.Xna.Framework;

namespace VibeBruceLee.Core.Physics;

public readonly record struct Aabb(float X, float Y, float Width, float Height)
{
	public float Right => X + Width;

	public float Bottom => Y + Height;

	public Vector2 Center => new(X + (Width * 0.5f), Y + (Height * 0.5f));

	public bool Intersects(Aabb other)
	{
		return X < other.Right && Right > other.X && Y < other.Bottom && Bottom > other.Y;
	}

	public bool Contains(Vector2 point)
	{
		return point.X >= X && point.X <= Right && point.Y >= Y && point.Y <= Bottom;
	}
}

public readonly record struct Platform(Aabb Bounds, bool OneWay = false);

public enum VineOrientation
{
	Vertical,
	Horizontal
}

public readonly record struct Vine(Aabb Bounds, VineOrientation Orientation);
