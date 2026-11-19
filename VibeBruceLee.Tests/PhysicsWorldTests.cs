using Microsoft.Xna.Framework;
using VibeBruceLee.Core.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class PhysicsWorldTests
{
	private const float Frame = 1f / 60f;

	private static PhysicsWorld CreateWorld()
	{
		return new PhysicsWorld(256, 192, 900, 600);
	}

	private static Platform Floor(float y)
	{
		return new Platform(new Aabb(0, y, 256, 8));
	}

	[Fact]
	public void FallsUnderGravityAndLandsOnPlatform()
	{
		var world = CreateWorld();
		var body = new PhysicsBody(new Vector2(50, 50), new Vector2(8, 8));
		var platforms = new[] { Floor(100) };

		for (var frame = 0; frame < 120; frame++)
		{
			world.Step(body, Frame, platforms, []);
		}

		Assert.Equal(92f, body.Position.Y, 2);
		Assert.True(body.IsGrounded);
	}

	[Fact]
	public void LandsOnThinPlatform()
	{
		var world = CreateWorld();
		var body = new PhysicsBody(new Vector2(50, 50), new Vector2(8, 8));
		var platforms = new[] { new Platform(new Aabb(0, 100, 256, 1)) };

		for (var frame = 0; frame < 120; frame++)
		{
			world.Step(body, Frame, platforms, []);
		}

		Assert.Equal(92f, body.Position.Y, 2);
		Assert.True(body.IsGrounded);
	}

	[Fact]
	public void HighSpeedBodyDoesNotTunnelThroughWall()
	{
		var world = CreateWorld();
		var body = new PhysicsBody(new Vector2(0, 20), new Vector2(8, 8))
		{
			Velocity = new Vector2(3000, 0)
		};
		var platforms = new[] { new Platform(new Aabb(20, 0, 4, 192)) };

		world.Step(body, Frame, platforms, []);

		Assert.Equal(12f, body.Position.X, 2);
		Assert.Equal(0f, body.Velocity.X, 2);
	}

	[Fact]
	public void OneWayPlatformLandsFromAbove()
	{
		var world = CreateWorld();
		var body = new PhysicsBody(new Vector2(50, 50), new Vector2(8, 8));
		var platforms = new[] { new Platform(new Aabb(0, 100, 256, 8), OneWay: true) };

		for (var frame = 0; frame < 120; frame++)
		{
			world.Step(body, Frame, platforms, []);
		}

		Assert.Equal(92f, body.Position.Y, 2);
		Assert.True(body.IsGrounded);
	}

	[Fact]
	public void OneWayPlatformIsIgnoredFromBelow()
	{
		var world = CreateWorld();
		var body = new PhysicsBody(new Vector2(50, 120), new Vector2(8, 8))
		{
			Velocity = new Vector2(0, -600)
		};
		var platforms = new[] { new Platform(new Aabb(0, 100, 256, 8), OneWay: true) };

		world.Step(body, 0.1f, platforms, []);

		Assert.True(body.Position.Y < 100);
		Assert.False(body.IsGrounded);
	}

	[Fact]
	public void SolidCeilingStopsUpwardMovement()
	{
		var world = CreateWorld();
		var body = new PhysicsBody(new Vector2(50, 20), new Vector2(8, 8))
		{
			Velocity = new Vector2(0, -600)
		};
		var platforms = new[] { new Platform(new Aabb(0, 0, 256, 8)) };

		world.Step(body, 0.1f, platforms, []);

		Assert.Equal(8f, body.Position.Y, 2);
		Assert.Equal(0f, body.Velocity.Y, 2);
	}

	[Fact]
	public void BodyIsClampedToWorldBounds()
	{
		var world = CreateWorld();
		var body = new PhysicsBody(new Vector2(-10, 10), new Vector2(8, 8))
		{
			Velocity = new Vector2(-100, 0)
		};

		world.Step(body, Frame, [], []);
		Assert.Equal(0f, body.Position.X, 2);

		body.Position = new Vector2(260, 10);
		body.Velocity = new Vector2(100, 0);
		world.Step(body, Frame, [], []);
		Assert.Equal(248f, body.Position.X, 2);
	}

	[Fact]
	public void VerticalVineDisablesGravityAndMovesUp()
	{
		var world = CreateWorld();
		var vine = new Vine(new Aabb(40, 40, 8, 80), VineOrientation.Vertical);
		var body = new PhysicsBody(new Vector2(40, 80), new Vector2(8, 8));
		PhysicsWorld.AttachToVine(body, vine);
		body.Velocity = new Vector2(0, -60);

		world.Step(body, 0.5f, [], [vine]);

		Assert.Equal(50f, body.Position.Y, 2);
		Assert.False(body.IsGrounded);
	}

	[Fact]
	public void HorizontalVineMovesSideways()
	{
		var world = CreateWorld();
		var vine = new Vine(new Aabb(40, 40, 80, 8), VineOrientation.Horizontal);
		var body = new PhysicsBody(new Vector2(40, 40), new Vector2(8, 8));
		PhysicsWorld.AttachToVine(body, vine);
		body.Velocity = new Vector2(40, 0);

		world.Step(body, 0.5f, [], [vine]);

		Assert.Equal(60f, body.Position.X, 2);
		Assert.Equal(40f, body.Position.Y, 2);
	}

	[Fact]
	public void ClimbingPastVineEndDetaches()
	{
		var world = CreateWorld();
		var vine = new Vine(new Aabb(40, 40, 8, 80), VineOrientation.Vertical);
		var body = new PhysicsBody(new Vector2(40, 45), new Vector2(8, 8));
		PhysicsWorld.AttachToVine(body, vine);
		body.Velocity = new Vector2(0, -100);

		world.Step(body, 0.2f, [], [vine]);

		Assert.Null(body.ClimbingVine);
	}

	[Fact]
	public void FindVineAtReturnsMatchingVine()
	{
		var vine = new Vine(new Aabb(40, 40, 8, 80), VineOrientation.Vertical);
		var vines = new[] { vine };

		Assert.Equal(vine, PhysicsWorld.FindVineAt(new Vector2(44, 80), vines));
		Assert.Null(PhysicsWorld.FindVineAt(new Vector2(100, 80), vines));
	}

	[Fact]
	public void IdenticalInputsProduceIdenticalResults()
	{
		var firstWorld = CreateWorld();
		var secondWorld = CreateWorld();
		var first = new PhysicsBody(new Vector2(20, 20), new Vector2(8, 8))
		{
			Velocity = new Vector2(50, 0)
		};
		var second = new PhysicsBody(new Vector2(20, 20), new Vector2(8, 8))
		{
			Velocity = new Vector2(50, 0)
		};
		var platforms = new[] { Floor(100) };

		for (var frame = 0; frame < 120; frame++)
		{
			firstWorld.Step(first, Frame, platforms, []);
			secondWorld.Step(second, Frame, platforms, []);
		}

		Assert.Equal(first.Position.X, second.Position.X, 3);
		Assert.Equal(first.Position.Y, second.Position.Y, 3);
	}
}
