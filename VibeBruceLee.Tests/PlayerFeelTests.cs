using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Effects;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Input;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class PlayerFeelTests
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

	[Fact]
	public void HitstopFreezesThenReleases()
	{
		var (player, input) = CreatePlayer();
		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		var startX = player.Body.Position.X;

		for (var frame = 0; frame < Player.HitstopFrames; frame++)
		{
			Step(player, input);
			Assert.Equal(startX, player.Body.Position.X, 1);
		}

		Step(player, input);
		Assert.True(player.Body.Position.X > startX);
	}

	[Fact]
	public void TakingAHitFlashes()
	{
		var (player, input) = CreatePlayer();

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		Assert.True(player.IsFlashing);

		for (var frame = 0; frame < Player.HitFlashFrames + 2; frame++)
		{
			Step(player, input);
		}

		Assert.False(player.IsFlashing);
	}

	[Fact]
	public void LandingIsReportedOnce()
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		var input = new InputSystem(InputBindings.CreateDefault());
		var platforms = new[] { new Platform(new Aabb(0, 160, 256, 8)) };
		var player = new Player(world, new Vector2(40, 100), platforms, []);
		var landed = false;

		for (var frame = 0; frame < 120; frame++)
		{
			Step(player, input);
			if (player.ConsumeLanded())
			{
				landed = true;
				break;
			}
		}

		Assert.True(landed);
		Assert.False(player.ConsumeLanded());
	}

	[Fact]
	public void SoundEventsAreRaised()
	{
		var (player, input) = CreatePlayer();
		var sounds = new List<PlayerSound>();
		player.SoundRequested += sounds.Add;

		Step(player, input, Keys.X);
		Assert.Contains(PlayerSound.Jump, sounds);

		for (var frame = 0; frame < 120; frame++)
		{
			Step(player, input);
			if (sounds.Contains(PlayerSound.Land))
			{
				break;
			}
		}

		Assert.Contains(PlayerSound.Land, sounds);

		Step(player, input, Keys.Z);
		Assert.Contains(PlayerSound.Chop, sounds);

		player.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		Assert.Contains(PlayerSound.Hit, sounds);

		for (var frame = 0; frame < Player.HitstopFrames + Player.HitInvulnerabilityFrames + 2; frame++)
		{
			Step(player, input);
		}

		player.Kill();
		Assert.Contains(PlayerSound.Death, sounds);

		player.Respawn(new Vector2(40, 138));
		Assert.Contains(PlayerSound.Respawn, sounds);
	}

	[Fact]
	public void LandingDustCanBeCleared()
	{
		var dust = new LandingDust();
		dust.Spawn(new Vector2(50, 100));
		Assert.Equal(6, dust.ParticleCount);

		dust.Clear();

		Assert.Equal(0, dust.ParticleCount);
	}

	[Fact]
	public void LandingDustLivesAndFades()
	{
		var dust = new LandingDust();

		dust.Spawn(new Vector2(50, 100));
		Assert.Equal(6, dust.ParticleCount);

		dust.Update(1f);
		Assert.Equal(0, dust.ParticleCount);
	}
}
