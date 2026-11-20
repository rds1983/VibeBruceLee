using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Input;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class CombatTests
{
	private const float Frame = 1f / 60f;

	private static (Player Player, InputSystem Input) CreatePlayer()
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		var input = new InputSystem(InputBindings.CreateDefault());
		var platforms = new[] { new Platform(new Aabb(0, 160, 256, 8)) };
		var player = new Player(world, new Vector2(40, 138), platforms, []);

		for (var frame = 0; frame < 90; frame++)
		{
			Step(player, input);
		}

		return (player, input);
	}

	private static void Step(Player player, InputSystem input, params Keys[] keys)
	{
		input.Update(new KeyboardState(keys), default, Frame, true);
		player.Update(input, Frame);
	}

	private static void AdvanceToActive(Player player, InputSystem input)
	{
		while (player.AttackFrame < AttackDefinitions.Chop.ActiveStart)
		{
			Step(player, input);
		}
	}

	[Fact]
	public void FrameDataIsCentralized()
	{
		Assert.Equal(1, AttackDefinitions.Chop.ActiveStart);
		Assert.Equal(3, AttackDefinitions.Chop.ActiveEnd);
		Assert.Equal(6, AttackDefinitions.Chop.TotalFrames);
		Assert.True(AttackDefinitions.Chop.IsActive(1));
		Assert.False(AttackDefinitions.Chop.IsActive(0));

		Assert.Equal(2, AttackDefinitions.Kick.ActiveStart);
		Assert.Equal(4, AttackDefinitions.Kick.ActiveEnd);
		Assert.Equal(8, AttackDefinitions.Kick.TotalFrames);
		Assert.Equal(75, AttackDefinitions.Kick.Score);
		Assert.True(AttackDefinitions.Kick.IsActive(2));
	}

	[Fact]
	public void StandingAttackStartsChop()
	{
		var (player, input) = CreatePlayer();

		Step(player, input, Keys.Z);

		Assert.Equal(PlayerState.Attack, player.State);
		Assert.Equal(AttackType.Chop, player.AttackType);
		Assert.Equal(AttackDefinitions.Chop.StartupAnimationFrame, player.CurrentFrame);
	}

	[Fact]
	public void RunningAttackStartsKick()
	{
		var (player, input) = CreatePlayer();
		for (var frame = 0; frame < 30; frame++)
		{
			Step(player, input, Keys.P);
		}

		Step(player, input, Keys.P, Keys.Z);

		Assert.Equal(AttackType.Kick, player.AttackType);
		Assert.Equal(AttackDefinitions.Kick.StartupAnimationFrame, player.CurrentFrame);
	}

	[Fact]
	public void AirborneAttackDoesNothing()
	{
		var (player, input) = CreatePlayer();
		Step(player, input, Keys.X);
		Assert.False(player.Body.IsGrounded);

		Step(player, input, Keys.Z);

		Assert.False(player.IsAttacking);
		Assert.Equal(AttackType.None, player.AttackType);
		Assert.False(input.IsBuffered(InputAction.Attack));
	}

	[Fact]
	public void HitboxIsActiveOnlyDuringActiveFrames()
	{
		var (player, input) = CreatePlayer();
		Step(player, input, Keys.Z);

		Assert.False(player.TryGetAttackHitbox(out _));

		AdvanceToActive(player, input);
		Assert.True(player.TryGetAttackHitbox(out var box));
		Assert.Equal(10, box.Width);

		while (player.AttackFrame < AttackDefinitions.Chop.ActiveEnd)
		{
			Step(player, input);
		}

		Assert.False(player.TryGetAttackHitbox(out _));
	}

	[Fact]
	public void AttackHitboxFlipsWithFacing()
	{
		var (player, input) = CreatePlayer();
		for (var frame = 0; frame < 10; frame++)
		{
			Step(player, input, Keys.O);
		}

		for (var frame = 0; frame < 8; frame++)
		{
			Step(player, input);
		}

		Step(player, input, Keys.Z);
		Assert.True(player.FacingLeft);
		AdvanceToActive(player, input);

		Assert.True(player.TryGetAttackHitbox(out var box));
		Assert.True(box.Right <= player.Body.Bounds.X);
	}

	[Fact]
	public void OneAttackHitsATargetOnce()
	{
		var (player, input) = CreatePlayer();
		Step(player, input, Keys.Z);
		AdvanceToActive(player, input);
		var target = new Aabb(
			player.Body.Bounds.Right,
			player.Body.Bounds.Y + 6,
			10,
			8);

		var first = player.TryHit(target);
		var second = player.TryHit(target);

		Assert.NotNull(first);
		Assert.Null(second);
	}

	[Fact]
	public void ApplyHitCausesHitstunAndKnockback()
	{
		var (player, input) = CreatePlayer();

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.Equal(PlayerState.Hit, player.State);
		Assert.True(player.Body.Velocity.X > 0);
		Assert.Equal(16, player.CurrentFrame);

		for (var frame = 0; frame < 30; frame++)
		{
			Step(player, input);
		}

		Assert.Equal(PlayerState.Idle, player.State);
	}

	[Fact]
	public void KickCausesHitstunAndRecovery()
	{
		var (player, input) = CreatePlayer();

		player.ApplyHit(AttackDefinitions.Kick, fromLeft: false);

		Assert.Equal(PlayerState.Hit, player.State);
		Assert.True(player.Body.Velocity.X < 0);
		Assert.Equal(16, player.CurrentFrame);

		for (var frame = 0; frame < 30; frame++)
		{
			Step(player, input);
		}

		Assert.Equal(PlayerState.Idle, player.State);
	}

	[Fact]
	public void KnockdownAttackRecoversAfterTimer()
	{
		var (player, input) = CreatePlayer();
		var knockdown = AttackDefinitions.Kick with { KnocksDown = true, KnockbackY = -30f };

		player.ApplyHit(knockdown, fromLeft: true);

		Assert.Equal(PlayerState.KnockedDown, player.State);
		Assert.Equal(17, player.CurrentFrame);

		for (var frame = 0; frame < Player.KnockdownFrames + 10; frame++)
		{
			Step(player, input);
		}

		Assert.Equal(PlayerState.Idle, player.State);
	}

	[Fact]
	public void AttackLocksMovement()
	{
		var (player, input) = CreatePlayer();
		Step(player, input, Keys.Z);
		var startX = player.Body.Position.X;

		for (var frame = 0; frame < 3; frame++)
		{
			Step(player, input, Keys.P);
		}

		Assert.Equal(startX, player.Body.Position.X, 1);
		Assert.Equal(0f, player.Body.Velocity.X, 1);
	}

	[Fact]
	public void GettingHitCancelsAnAttack()
	{
		var (player, input) = CreatePlayer();
		Step(player, input, Keys.Z);
		Assert.True(player.IsAttacking);

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.False(player.IsAttacking);
		Assert.Equal(PlayerState.Hit, player.State);
	}

	[Fact]
	public void AttackEndsAfterTotalFrames()
	{
		var (player, input) = CreatePlayer();
		Step(player, input, Keys.Z);

		for (var frame = 0; frame < AttackDefinitions.Chop.TotalFrames + 2; frame++)
		{
			Step(player, input);
		}

		Assert.False(player.IsAttacking);
		Assert.Equal(AttackType.None, player.AttackType);
	}
}
