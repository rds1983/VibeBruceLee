using Microsoft.Xna.Framework;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class EnemyScalingTests
{
	private const float Frame = 1f / 60f;

	[Fact]
	public void LoopOneUsesTheBaseDefinition()
	{
		var scaled = EnemyScaling.Apply(EnemyDefinitions.Ninja, 1);

		Assert.Equal(EnemyDefinitions.Ninja.RunSpeed, scaled.RunSpeed);
		Assert.Equal(EnemyDefinitions.Ninja.AttackCooldownFrames, scaled.AttackCooldownFrames);
		Assert.Equal(EnemyDefinitions.Ninja.RespawnFrames, scaled.RespawnFrames);
	}

	[Fact]
	public void LaterLoopsSpeedEnemiesUpAndRemoveRespawnDelay()
	{
		var scaled = EnemyScaling.Apply(EnemyDefinitions.Ninja, 2);

		Assert.True(scaled.RunSpeed > EnemyDefinitions.Ninja.RunSpeed);
		Assert.True(scaled.AttackCooldownFrames < EnemyDefinitions.Ninja.AttackCooldownFrames);
		Assert.True(scaled.SightRange > EnemyDefinitions.Ninja.SightRange);
		Assert.Equal(0, scaled.RespawnFrames);
	}

	[Fact]
	public void ScalingIsCapped()
	{
		var scaled = EnemyScaling.Apply(EnemyDefinitions.Yamo, 100);

		Assert.Equal(EnemyDefinitions.Yamo.RunSpeed * EnemyScaling.MaxSpeedMultiplier, scaled.RunSpeed, 3);
		Assert.Equal(
			(int)(EnemyDefinitions.Yamo.AttackCooldownFrames * EnemyScaling.MinCooldownMultiplier),
			scaled.AttackCooldownFrames);
	}

	[Fact]
	public void ScaledEnemyRespawnsImmediatelyAfterKnockdown()
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		var platforms = new[] { new Platform(new Aabb(0, 176, 256, 8)) };
		var definition = EnemyScaling.Apply(EnemyDefinitions.Ninja, 2);
		var spawn = new Vector2(40, 176 - definition.BodyHeight);
		var enemy = new Enemy(definition, spawn, world, platforms, [], new SeededRandom(1));

		Assert.True(enemy.ApplyHazard(fromLeft: true));
		Assert.Equal(EnemyState.KnockedDown, enemy.State);

		for (var frame = 0; frame < definition.KnockdownFrames + 2; frame++)
		{
			enemy.Update(null, Frame);
		}

		Assert.Equal(EnemyState.Idle, enemy.State);
		Assert.Equal(definition.MaxHits, enemy.HitsRemaining);
		Assert.Equal(spawn.X, enemy.Body.Position.X, 1);
	}

	[Fact]
	public void LoopHazardsOnlyAppearFromLoopTwo()
	{
		var loopHazard = new HazardDefinition
		{
			Type = "electric",
			X = 0,
			Y = 168,
			Width = 16,
			Height = 8,
			PeriodSeconds = 1.6
		};

		var loopOne = new HazardSystem([], [loopHazard], 1);
		var loopTwo = new HazardSystem([], [loopHazard], 2);

		Assert.Empty(loopOne.Hazards);
		Assert.Single(loopTwo.Hazards);
	}
}
