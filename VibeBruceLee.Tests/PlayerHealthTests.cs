using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Input;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class PlayerHealthTests
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

	private static void Step(Player player, InputSystem input)
	{
		input.Update(new KeyboardState(), default, Frame, true);
		player.Update(input, Frame);
	}

	private static void Advance(Player player, InputSystem input, int frames)
	{
		for (var frame = 0; frame < frames; frame++)
		{
			Step(player, input);
		}
	}

	[Fact]
	public void ThreeBlowsCauseADeath()
	{
		var (player, input) = CreatePlayer();

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		Assert.Equal(2, player.BlowsRemaining);
		Assert.False(player.IsDead);
		Advance(player, input, Player.HitInvulnerabilityFrames + 2);

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		Assert.Equal(1, player.BlowsRemaining);
		Assert.False(player.IsDead);
		Advance(player, input, Player.HitInvulnerabilityFrames + 2);

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.Equal(0, player.BlowsRemaining);
		Assert.True(player.IsDead);
		Assert.Equal(PlayerState.Dead, player.State);
		Assert.Equal(18, player.CurrentFrame);
	}

	[Fact]
	public void DeathFinishesAfterTimer()
	{
		var (player, input) = CreatePlayer();
		player.Kill();

		Advance(player, input, Player.DeathFrames);

		Assert.True(player.ConsumeDeathFinished());
		Assert.False(player.ConsumeDeathFinished());
	}

	[Fact]
	public void RespawnRestoresBlowsAndInvulnerability()
	{
		var (player, input) = CreatePlayer();
		player.Kill();
		Advance(player, input, Player.DeathFrames);

		player.Respawn(new Vector2(32, 138));

		Assert.False(player.IsDead);
		Assert.Equal(Player.BlowsPerFall, player.BlowsRemaining);
		Assert.True(player.IsInvulnerable);

		player.ApplyHit(AttackDefinitions.Kick, fromLeft: true);
		Assert.Equal(Player.BlowsPerFall, player.BlowsRemaining);

		Advance(player, input, Player.RespawnInvulnerabilityFrames + 2);

		player.ApplyHit(AttackDefinitions.Kick, fromLeft: true);
		Assert.Equal(Player.BlowsPerFall - 1, player.BlowsRemaining);
	}

	[Fact]
	public void HitInvulnerabilityBlocksRepeatedHits()
	{
		var (player, input) = CreatePlayer();

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.Equal(2, player.BlowsRemaining);

		Advance(player, input, Player.HitInvulnerabilityFrames + 2);
		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.Equal(1, player.BlowsRemaining);
	}

	[Fact]
	public void InvincibleFlagIgnoresHits()
	{
		var (player, _) = CreatePlayer();
		player.Invincible = true;

		player.ApplyHit(AttackDefinitions.Kick, fromLeft: true);

		Assert.Equal(Player.BlowsPerFall, player.BlowsRemaining);
		Assert.False(player.IsHurt);
		Assert.False(player.IsDead);
	}

	[Fact]
	public void KillStartsDeathImmediately()
	{
		var (player, _) = CreatePlayer();

		player.Kill();

		Assert.True(player.IsDead);
		Assert.Equal(0, player.BlowsRemaining);
		Assert.Equal(PlayerState.Dead, player.State);
	}
}
