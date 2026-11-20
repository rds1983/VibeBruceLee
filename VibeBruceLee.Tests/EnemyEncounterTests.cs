using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Input;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class EnemyEncounterTests
{
	private const float Frame = 1f / 60f;

	private static (Player Player, InputSystem Input, Enemy Ninja, Enemy Yamo, EnemyEncounter Encounter, GameSession Session) CreateEncounter()
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		var platforms = new[] { new Platform(new Aabb(0, 176, 256, 8)) };
		var player = new Player(world, new Vector2(32, 154), platforms, []);
		var input = new InputSystem(InputBindings.CreateDefault());
		var session = new GameSession();
		var ninja = new Enemy(
			EnemyDefinitions.Ninja,
			new Vector2(220, 154),
			world,
			platforms,
			[],
			new SeededRandom(7));
		var yamo = new Enemy(
			EnemyDefinitions.Yamo,
			new Vector2(80, 150),
			world,
			platforms,
			[],
			new SeededRandom(9));
		var encounter = new EnemyEncounter(player, [ninja, yamo], session);
		return (player, input, ninja, yamo, encounter, session);
	}

	private static void Step(InputSystem input, EnemyEncounter encounter)
	{
		input.Update(new KeyboardState(), default, Frame, true);
		encounter.Update(input, Frame);
	}

	[Fact]
	public void ScriptedEncounterKeepsOneAttackerAtATime()
	{
		var stage = CreateEncounter();

		for (var frame = 0; frame < 400; frame++)
		{
			Step(stage.Input, stage.Encounter);
			var attackers = (stage.Ninja.IsAttacking ? 1 : 0) + (stage.Yamo.IsAttacking ? 1 : 0);
			Assert.InRange(attackers, 0, 1);
		}

		Assert.True(stage.Encounter.PlayerHitsTaken > 0);
	}

	[Fact]
	public void HitCreatesABriefPressureWindow()
	{
		var stage = CreateEncounter();
		var hitsSeen = 0;
		var hitFrame = -1;
		var attackStartsAfterHit = 0;
		var wasAttacking = new[] { false, false };

		for (var frame = 0; frame < 400; frame++)
		{
			Step(stage.Input, stage.Encounter);

			if (stage.Encounter.PlayerHitsTaken > hitsSeen)
			{
				hitsSeen = stage.Encounter.PlayerHitsTaken;
				hitFrame = frame;
			}

			var attacking = new[] { stage.Ninja.IsAttacking, stage.Yamo.IsAttacking };
			if (hitFrame >= 0 &&
				frame > hitFrame &&
				frame < hitFrame + EnemyEncounter.PressureFrames)
			{
				for (var i = 0; i < attacking.Length; i++)
				{
					if (attacking[i] && !wasAttacking[i])
					{
						attackStartsAfterHit++;
					}
				}
			}

			wasAttacking = attacking;
		}

		Assert.True(hitsSeen > 0);
		Assert.Equal(0, attackStartsAfterHit);
	}

	[Fact]
	public void OverlappingEnemiesAreSeparated()
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		var platforms = new[] { new Platform(new Aabb(0, 176, 256, 8)) };
		var player = new Player(world, new Vector2(10, 154), platforms, []);
		var input = new InputSystem(InputBindings.CreateDefault());
		var ninja = new Enemy(
			EnemyDefinitions.Ninja,
			new Vector2(200, 154),
			world,
			platforms,
			[],
			new SeededRandom(3));
		var yamo = new Enemy(
			EnemyDefinitions.Yamo,
			new Vector2(200, 150),
			world,
			platforms,
			[],
			new SeededRandom(4));
		var encounter = new EnemyEncounter(player, [ninja, yamo]);

		Assert.True(ninja.Body.Bounds.Intersects(yamo.Body.Bounds));

		Step(input, encounter);

		Assert.False(ninja.Body.Bounds.Intersects(yamo.Body.Bounds));
	}

	[Fact]
	public void FriendlyFireDamagesTheOtherEnemyWithoutScore()
	{
		var stage = CreateEncounter();
		stage.Ninja.Body.Position = new Vector2(100, 154);
		stage.Yamo.Body.Position = new Vector2(80, 150);
		var target = new Vector2(stage.Yamo.Feet.X + 30f, 176);
		var frames = 0;

		while (!stage.Yamo.TryGetAttackHitbox(out _) && frames < 90)
		{
			stage.Yamo.Update(target, Frame);
			frames++;
		}

		Assert.True(stage.Yamo.TryGetAttackHitbox(out _));

		var hitsBefore = stage.Ninja.HitsRemaining;
		stage.Encounter.ResolveAttacks();

		Assert.Equal(hitsBefore - 1, stage.Ninja.HitsRemaining);
		Assert.Equal(EnemyState.KnockedDown, stage.Ninja.State);
		Assert.Equal(0, stage.Session.Score);
		Assert.Equal(0, stage.Encounter.PlayerHitsTaken);
	}

	[Fact]
	public void ScriptedEncounterIsDeterministic()
	{
		var first = RunSnapshot();
		var second = RunSnapshot();

		Assert.Equal(first, second);
	}

	private static string RunSnapshot()
	{
		var stage = CreateEncounter();

		for (var frame = 0; frame < 300; frame++)
		{
			Step(stage.Input, stage.Encounter);
		}

		return string.Create(
			CultureInfo.InvariantCulture,
			$"{stage.Player.Body.Position.X:F2},{stage.Player.Body.Position.Y:F2},{stage.Player.BlowsRemaining},{stage.Encounter.PlayerHitsTaken},{stage.Session.Score}|" +
			$"{stage.Ninja.Body.Position.X:F2},{stage.Ninja.Body.Position.Y:F2},{stage.Ninja.State},{stage.Ninja.HitsRemaining}|" +
			$"{stage.Yamo.Body.Position.X:F2},{stage.Yamo.Body.Position.Y:F2},{stage.Yamo.State},{stage.Yamo.HitsRemaining}");
	}
}
