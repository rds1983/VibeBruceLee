using Microsoft.Xna.Framework;

namespace VibeBruceLee.Physics;

public sealed class PhysicsBody
{
	public PhysicsBody(Vector2 position, Vector2 size)
	{
		if (size.X <= 0 || size.Y <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(size), "Body size must be positive.");
		}

		Position = position;
		Size = size;
	}

	public Vector2 Position { get; set; }

	public Vector2 Size { get; }

	public Vector2 Velocity { get; set; }

	public bool IsGrounded { get; set; }

	public Vine? ClimbingVine { get; set; }

	public Aabb Bounds => new(Position.X, Position.Y, Size.X, Size.Y);
}
