using Microsoft.Xna.Framework;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class HazardTests
{
	private static HazardDefinition Definition(
		string type,
		int x = 36,
		int y = 154,
		int width = 20,
		int height = 22,
		double? period = null,
		double? speed = null,
		int direction = 1)
	{
		return new HazardDefinition
		{
			Type = type,
			X = x,
			Y = y,
			Width = width,
			Height = height,
			PeriodSeconds = period,
			Speed = speed,
			DirectionX = direction
		};
	}

	private static Player CreatePlayer(float x = 40, float y = 154)
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		var platforms = new[] { new Platform(new Aabb(0, 176, 256, 8)) };
		return new Player(world, new Vector2(x, y), platforms, []);
	}

	private static Enemy CreateEnemy(float x = 40, float y = 154)
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		var platforms = new[] { new Platform(new Aabb(0, 176, 256, 8)) };
		return new Enemy(
			EnemyDefinitions.Ninja,
			new Vector2(x, y),
			world,
			platforms,
			[],
			new SeededRandom(1));
	}

	[Fact]
	public void ElectricBeamWarnsThenDamagesThePlayer()
	{
		var system = new HazardSystem([Definition("electric", period: 1.0)]);
		var hazard = system.Hazards[0];
		var player = CreatePlayer();

		system.Update(0.1f, player, []);
		Assert.False(hazard.IsActive);
		Assert.False(hazard.IsWarning);
		Assert.False(player.IsDead);

		system.Update(0.3f, player, []);
		Assert.True(hazard.IsWarning);
		Assert.False(hazard.IsActive);
		Assert.False(player.IsDead);

		system.Update(0.3f, player, []);
		Assert.True(hazard.IsActive);
		Assert.True(player.IsDead);

		var hazards = system.Hazards;
		system.Update(1.0f, player, []);
		Assert.Single(hazards);
		Assert.False(hazards[0].IsFinished);
	}

	[Fact]
	public void ActiveBeamKnocksOutAnEnemy()
	{
		var system = new HazardSystem([Definition("electric", period: 0.8)]);
		var enemy = CreateEnemy();
		var player = CreatePlayer(200, 154);

		system.Update(0.5f, player, [enemy]);

		Assert.Equal(0, enemy.HitsRemaining);
		Assert.Equal(EnemyState.KnockedDown, enemy.State);
		Assert.False(enemy.CanBeHit);
	}

	[Fact]
	public void BushWarnsExplodesAndDespawns()
	{
		var system = new HazardSystem([Definition("bush")]);
		var hazard = system.Hazards[0];
		var player = CreatePlayer();

		system.Update(0.1f, player, []);
		Assert.False(hazard.IsVisible);
		Assert.False(hazard.IsActive);

		system.Update(BushHazard.HiddenSeconds, player, []);
		Assert.True(hazard.IsVisible);
		Assert.True(hazard.IsWarning);
		Assert.False(hazard.IsActive);
		Assert.False(player.IsDead);

		system.Update(BushHazard.WarningSeconds, player, []);
		Assert.True(hazard.IsActive);
		Assert.True(player.IsDead);

		system.Update(BushHazard.ExplosionSeconds + 0.1f, player, []);
		Assert.Empty(system.Hazards);
	}

	[Fact]
	public void MineTriggersFusesExplodesAndDespawns()
	{
		var system = new HazardSystem([Definition("mine")]);
		var hazard = system.Hazards[0];
		var player = CreatePlayer();

		system.Update(0.01f, player, []);
		Assert.False(hazard.IsActive);
		Assert.True(hazard.IsWarning);
		Assert.False(player.IsDead);

		system.Update(MineHazard.FuseSeconds, player, []);
		Assert.False(hazard.IsActive);
		Assert.False(player.IsDead);

		system.Update(0.05f, player, []);
		Assert.True(hazard.IsActive);
		Assert.True(player.IsDead);

		system.Update(MineHazard.ExplosionSeconds + 0.1f, player, []);
		Assert.Empty(system.Hazards);
	}

	[Fact]
	public void MovingWallPatrolsAndDamagesOnContact()
	{
		var system = new HazardSystem([Definition("moving-wall", x: 10, y: 100, width: 40, height: 8, speed: 40)]);
		var hazard = system.Hazards[0];
		var farPlayer = CreatePlayer(200, 154);

		system.Update(0.5f, farPlayer, []);
		Assert.Equal(30f, hazard.Bounds.X, 1);
		Assert.True(hazard.IsActive);

		system.Update(2f, farPlayer, []);
		Assert.Equal(42f, hazard.Bounds.X, 1);

		system.Update(0.5f, farPlayer, []);
		Assert.True(hazard.Bounds.X < 42f);
		Assert.Single(system.Hazards);

		var hitSystem = new HazardSystem([Definition("moving-wall", x: 36, y: 154, width: 40, height: 22, speed: 40)]);
		var player = CreatePlayer();
		hitSystem.Update(0.01f, player, []);

		Assert.True(player.IsDead);
	}

	[Fact]
	public void ProjectileMovesDamagesAndDespawns()
	{
		var system = new HazardSystem([Definition("dagger", x: 10, y: 160, width: 8, height: 8, speed: 100)]);
		var hazard = system.Hazards[0];
		var farPlayer = CreatePlayer(200, 154);

		system.Update(0.5f, farPlayer, []);
		Assert.Equal(60f, hazard.Bounds.X, 1);
		Assert.True(hazard.IsActive);

		system.Update(2f, farPlayer, []);
		Assert.Empty(system.Hazards);

		var hitSystem = new HazardSystem([Definition("bug", x: 36, y: 160, width: 8, height: 8, speed: 50, direction: -1)]);
		var player = CreatePlayer();
		hitSystem.Update(0.01f, player, []);

		Assert.True(player.IsDead);
	}

	[Fact]
	public void HazardResetRestoresConsumedHazards()
	{
		var system = new HazardSystem([Definition("mine")]);
		var player = CreatePlayer();

		system.Update(0.01f, player, []);
		system.Update(MineHazard.FuseSeconds + 0.01f, player, []);
		system.Update(0.05f, player, []);
		system.Update(MineHazard.ExplosionSeconds + 0.1f, player, []);
		Assert.Empty(system.Hazards);

		system.Reset();

		var mine = Assert.IsType<MineHazard>(Assert.Single(system.Hazards));
		Assert.True(mine.IsArmed);
	}

	[Fact]
	public void FloorLightUsesTheTimedBeamCycle()
	{
		var system = new HazardSystem([Definition("floor-light", period: 2.0)]);
		var hazard = system.Hazards[0];
		hazard.Update(2f - TimedBeamHazard.ActiveSeconds);

		Assert.True(hazard.IsActive);
		Assert.False(hazard.IsFinished);
	}
}
