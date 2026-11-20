using Microsoft.Xna.Framework;

namespace VibeBruceLee.Physics;

/// <summary>
/// Deterministic fixed-step physics for axis-aligned bodies. Movement is split
/// into sub-pixel steps so fast bodies cannot tunnel through thin platforms.
/// </summary>
public sealed class PhysicsWorld
{
	public PhysicsWorld(int width, int height, float gravity, float maxFallSpeed)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
		ArgumentOutOfRangeException.ThrowIfNegative(gravity);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxFallSpeed);

		Width = width;
		Height = height;
		Gravity = gravity;
		MaxFallSpeed = maxFallSpeed;
	}

	public int Width { get; }

	public int Height { get; }

	public float Gravity { get; }

	public float MaxFallSpeed { get; }

	public static Vine? FindVineAt(Vector2 point, IReadOnlyList<Vine> vines)
	{
		ArgumentNullException.ThrowIfNull(vines);

		foreach (var vine in vines)
		{
			if (vine.Bounds.Contains(point))
			{
				return vine;
			}
		}

		return null;
	}

	public static void AttachToVine(PhysicsBody body, Vine vine)
	{
		ArgumentNullException.ThrowIfNull(body);

		body.ClimbingVine = vine;
		body.Velocity = Vector2.Zero;
		body.IsGrounded = false;
	}

	public void Step(
		PhysicsBody body,
		float deltaSeconds,
		IReadOnlyList<Platform> platforms,
		IReadOnlyList<Vine> vines)
	{
		ArgumentNullException.ThrowIfNull(body);
		ArgumentNullException.ThrowIfNull(platforms);
		ArgumentNullException.ThrowIfNull(vines);
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		if (body.ClimbingVine is Vine vine)
		{
			var climbVelocity = vine.Orientation == VineOrientation.Vertical
				? new Vector2(0, body.Velocity.Y)
				: new Vector2(body.Velocity.X, 0);

			body.Velocity = climbVelocity;
			body.Position += climbVelocity * deltaSeconds;
			ClampToWorld(body);
			body.IsGrounded = false;

			if (!vine.Bounds.Contains(body.Bounds.Center))
			{
				body.ClimbingVine = null;
			}

			return;
		}

		body.IsGrounded = false;
		body.Velocity = new Vector2(
			body.Velocity.X,
			Math.Min(body.Velocity.Y + (Gravity * deltaSeconds), MaxFallSpeed));

		MoveHorizontal(body, platforms, body.Velocity.X * deltaSeconds);
		MoveVertical(body, platforms, body.Velocity.Y * deltaSeconds);
		ClampToWorld(body);
	}

	private static void MoveHorizontal(PhysicsBody body, IReadOnlyList<Platform> platforms, float delta)
	{
		if (delta == 0)
		{
			return;
		}

		var steps = Math.Max(1, (int)MathF.Ceiling(MathF.Abs(delta)));
		var step = delta / steps;

		for (var i = 0; i < steps; i++)
		{
			body.Position = new Vector2(body.Position.X + step, body.Position.Y);

			foreach (var platform in platforms)
			{
				if (platform.OneWay || !body.Bounds.Intersects(platform.Bounds))
				{
					continue;
				}

				var x = step > 0
					? platform.Bounds.X - body.Size.X
					: platform.Bounds.Right;

				body.Position = new Vector2(x, body.Position.Y);
				body.Velocity = new Vector2(0, body.Velocity.Y);
				return;
			}
		}
	}

	private static void MoveVertical(PhysicsBody body, IReadOnlyList<Platform> platforms, float delta)
	{
		if (delta == 0)
		{
			return;
		}

		var steps = Math.Max(1, (int)MathF.Ceiling(MathF.Abs(delta)));
		var step = delta / steps;

		for (var i = 0; i < steps; i++)
		{
			var previousBottom = body.Bounds.Bottom;
			body.Position = new Vector2(body.Position.X, body.Position.Y + step);

			foreach (var platform in platforms)
			{
				if (!body.Bounds.Intersects(platform.Bounds))
				{
					continue;
				}

				if (platform.OneWay)
				{
					if (step <= 0 || previousBottom > platform.Bounds.Y + 0.001f)
					{
						continue;
					}
				}

				if (step > 0)
				{
					body.Position = new Vector2(body.Position.X, platform.Bounds.Y - body.Size.Y);
					body.Velocity = new Vector2(body.Velocity.X, 0);
					body.IsGrounded = true;
					return;
				}

				body.Position = new Vector2(body.Position.X, platform.Bounds.Bottom);
				body.Velocity = new Vector2(body.Velocity.X, 0);
				return;
			}
		}
	}

	private void ClampToWorld(PhysicsBody body)
	{
		var maxX = Math.Max(0, Width - body.Size.X);
		var x = Math.Clamp(body.Position.X, 0, maxX);
		if (x != body.Position.X)
		{
			body.Position = new Vector2(x, body.Position.Y);
			body.Velocity = new Vector2(0, body.Velocity.Y);
		}

		var maxY = Math.Max(0, Height - body.Size.Y);
		var y = Math.Clamp(body.Position.Y, 0, maxY);
		if (y != body.Position.Y)
		{
			body.Position = new Vector2(body.Position.X, y);
			body.Velocity = new Vector2(body.Velocity.X, 0);

			if (y >= maxY)
			{
				body.IsGrounded = true;
			}
		}
	}
}
