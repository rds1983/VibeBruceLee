using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Input;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class PlayerTests
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

	private static (Player Player, InputSystem Input) CreatePlayer(
		Vector2 spawn,
		IReadOnlyList<Platform> platforms,
		IReadOnlyList<Vine>? vines = null)
	{
		var world = CreateWorld();
		var input = new InputSystem(InputBindings.CreateDefault());
		var player = new Player(world, spawn, platforms, vines ?? []);
		return (player, input);
	}

	private static void Step(Player player, InputSystem input, params Keys[] keys)
	{
		input.Update(new KeyboardState(keys), default, Frame, true);
		player.Update(input, Frame);
	}

	private static void Settle(Player player, InputSystem input)
	{
		for (var frame = 0; frame < 90; frame++)
		{
			Step(player, input);
		}
	}

	[Fact]
	public void FallsAndIdlesOnFloor()
	{
		var (player, input) = CreatePlayer(new Vector2(40, 100), [Floor(160)]);

		Settle(player, input);

		Assert.True(player.Body.IsGrounded);
		Assert.Equal(PlayerState.Idle, player.State);
		Assert.Equal(138f, player.Body.Position.Y, 1);
	}

	[Fact]
	public void FallsBeforeLanding()
	{
		var (player, input) = CreatePlayer(new Vector2(40, 100), [Floor(160)]);

		Step(player, input);

		Assert.Equal(PlayerState.Fall, player.State);
		Assert.Equal(6, player.CurrentFrame);
	}

	[Fact]
	public void RunsRightAndFacesRight()
	{
		var (player, input) = CreatePlayer(new Vector2(40, 138), [Floor(160)]);
		Settle(player, input);
		var startX = player.Body.Position.X;

		for (var frame = 0; frame < 30; frame++)
		{
			Step(player, input, Keys.P);
		}

		Assert.True(player.Body.Position.X > startX);
		Assert.Equal(PlayerState.Run, player.State);
		Assert.False(player.FacingLeft);
	}

	[Fact]
	public void RunDeceleratesToIdle()
	{
		var (player, input) = CreatePlayer(new Vector2(40, 138), [Floor(160)]);
		Settle(player, input);

		for (var frame = 0; frame < 30; frame++)
		{
			Step(player, input, Keys.P);
		}

		for (var frame = 0; frame < 60; frame++)
		{
			Step(player, input);
		}

		Assert.InRange(player.Body.Velocity.X, -0.01f, 0.01f);
		Assert.Equal(PlayerState.Idle, player.State);
	}

	[Fact]
	public void DuckingStopsHorizontalMovement()
	{
		var (player, input) = CreatePlayer(new Vector2(40, 138), [Floor(160)]);
		Settle(player, input);

		for (var frame = 0; frame < 20; frame++)
		{
			Step(player, input, Keys.P);
		}

		for (var frame = 0; frame < 5; frame++)
		{
			Step(player, input, Keys.P, Keys.A);
		}

		Assert.Equal(PlayerState.Duck, player.State);
		Assert.Equal(0f, player.Body.Velocity.X, 1);
		Assert.Equal(7, player.CurrentFrame);
	}

	[Fact]
	public void JumpRisesAndLands()
	{
		var (player, input) = CreatePlayer(new Vector2(40, 138), [Floor(160)]);
		Settle(player, input);
		var groundY = player.Body.Position.Y;

		Step(player, input, Keys.X);

		Assert.Equal(PlayerState.Jump, player.State);
		Assert.True(player.Body.Velocity.Y < 0);
		Assert.Equal(6, player.CurrentFrame);

		for (var frame = 0; frame < 120; frame++)
		{
			Step(player, input);
		}

		Assert.True(player.Body.IsGrounded);
		Assert.Equal(PlayerState.Idle, player.State);
		Assert.Equal(groundY, player.Body.Position.Y, 1);
	}

	[Fact]
	public void ReleasingJumpCutsRise()
	{
		var (player, input) = CreatePlayer(new Vector2(40, 138), [Floor(160)]);
		Settle(player, input);

		Step(player, input, Keys.X);
		var risingVelocity = player.Body.Velocity.Y;

		Step(player, input);

		Assert.True(player.Body.Velocity.Y > risingVelocity);
		Assert.True(player.Body.Velocity.Y > -Player.JumpSpeed);
	}

	[Fact]
	public void AttachesToVerticalVineAndClimbsUp()
	{
		var vine = new Vine(new Aabb(40, 60, 8, 80), VineOrientation.Vertical);
		var (player, input) = CreatePlayer(new Vector2(40, 80), [], [vine]);

		Step(player, input, Keys.Q);

		Assert.Equal(PlayerState.Climb, player.State);
		Assert.NotNull(player.Body.ClimbingVine);

		var startY = player.Body.Position.Y;
		for (var frame = 0; frame < 10; frame++)
		{
			Step(player, input, Keys.Q);
		}

		Assert.True(player.Body.Position.Y < startY);
		Assert.False(player.Body.IsGrounded);
		Assert.True(player.CurrentFrame is 14 or 15);
	}

	[Fact]
	public void ClimbsOntoPlatformAtVineTop()
	{
		var vine = new Vine(new Aabb(40, 60, 8, 80), VineOrientation.Vertical);
		var platforms = new[] { new Platform(new Aabb(0, 60, 256, 8)) };
		var (player, input) = CreatePlayer(new Vector2(40, 80), platforms, [vine]);

		Step(player, input, Keys.Q);
		Assert.Equal(PlayerState.Climb, player.State);

		for (var frame = 0; frame < 60; frame++)
		{
			Step(player, input, Keys.Q);
		}

		Assert.Null(player.Body.ClimbingVine);
		Assert.True(player.Body.IsGrounded);
		Assert.Equal(38f, player.Body.Position.Y, 1);
		Assert.Equal(PlayerState.Idle, player.State);
	}

	[Fact]
	public void HorizontalVineMovesSideways()
	{
		var vine = new Vine(new Aabb(40, 40, 80, 8), VineOrientation.Horizontal);
		var (player, input) = CreatePlayer(new Vector2(40, 34), [], [vine]);

		Step(player, input, Keys.Q);
		var startX = player.Body.Position.X;

		for (var frame = 0; frame < 10; frame++)
		{
			Step(player, input, Keys.P);
		}

		Assert.True(player.Body.Position.X > startX);
		Assert.Equal(34f, player.Body.Position.Y, 1);
		Assert.Equal(PlayerState.Climb, player.State);
	}

	[Fact]
	public void JumpDetachesFromVine()
	{
		var vine = new Vine(new Aabb(40, 60, 8, 80), VineOrientation.Vertical);
		var (player, input) = CreatePlayer(new Vector2(40, 80), [], [vine]);
		Step(player, input, Keys.Q);
		Assert.NotNull(player.Body.ClimbingVine);

		Step(player, input, Keys.X);

		Assert.Null(player.Body.ClimbingVine);
		Assert.Equal(PlayerState.Jump, player.State);
	}

	[Fact]
	public void TakingAHitWhileClimbingDetachesFromVine()
	{
		var vine = new Vine(new Aabb(40, 60, 8, 80), VineOrientation.Vertical);
		var (player, input) = CreatePlayer(new Vector2(40, 80), [], [vine]);
		Step(player, input, Keys.Q);
		Assert.NotNull(player.Body.ClimbingVine);

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.Null(player.Body.ClimbingVine);
		Assert.Equal(PlayerState.Hit, player.State);
	}

	[Fact]
	public void ResolveBodyBlockPushesPlayerOutOfAReservedBody()
	{
		var (player, _) = CreatePlayer(new Vector2(44, 138), [Floor(160)]);
		var blocker = new Aabb(50, 130, 14, 22);

		player.ResolveBodyBlock(blocker);

		Assert.Equal(40f, player.Body.Position.X, 1);
		Assert.False(player.Body.Bounds.Intersects(blocker));

		player.Body.Position = new Vector2(56, 138);

		player.ResolveBodyBlock(blocker);

		Assert.Equal(64f, player.Body.Position.X, 1);
		Assert.False(player.Body.Bounds.Intersects(blocker));
	}

	[Fact]
	public void ResolveBodyBlockIgnoresNonOverlappingBodies()
	{
		var (player, _) = CreatePlayer(new Vector2(10, 138), [Floor(160)]);
		var start = player.Body.Position;
		var blocker = new Aabb(50, 130, 14, 22);

		player.ResolveBodyBlock(blocker);

		Assert.Equal(start, player.Body.Position);
	}
}
