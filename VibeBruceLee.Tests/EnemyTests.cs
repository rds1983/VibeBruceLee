using Microsoft.Xna.Framework;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class EnemyTests
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

	private static Enemy CreateEnemy(
		EnemyDefinition definition,
		Vector2 spawn,
		IReadOnlyList<Platform> platforms,
		IReadOnlyList<Vine>? vines = null,
		uint seed = 1)
	{
		return new Enemy(definition, spawn, CreateWorld(), platforms, vines ?? [], new SeededRandom(seed));
	}

	private static Vector2 OnFloor(EnemyDefinition definition, float x, float floorTop)
	{
		return new Vector2(x, floorTop - definition.BodyHeight);
	}

	private static void Step(Enemy enemy, Vector2? target, int frames = 1)
	{
		for (var i = 0; i < frames; i++)
		{
			enemy.Update(target, Frame);
		}
	}

	[Fact]
	public void FallsAndStandsOnFloor()
	{
		var enemy = CreateEnemy(EnemyDefinitions.Ninja, new Vector2(40, 80), [Floor(176)]);

		Step(enemy, null, 90);

		Assert.True(enemy.Body.IsGrounded);
		Assert.Equal(176f, enemy.Feet.Y, 1);
		Assert.Equal(154f, enemy.Body.Position.Y, 1);
	}

	[Fact]
	public void PatrolTurnsAtPlatformEdges()
	{
		var enemy = CreateEnemy(EnemyDefinitions.Ninja, OnFloor(EnemyDefinitions.Ninja, 128, 176), [Floor(176)]);
		var sawLeft = false;
		var sawRight = false;

		for (var frame = 0; frame < 1200; frame++)
		{
			enemy.Update(null, Frame);
			sawLeft |= enemy.FacingLeft;
			sawRight |= !enemy.FacingLeft;
			Assert.InRange(enemy.Feet.X, 1f, 255f);
		}

		Assert.True(sawLeft);
		Assert.True(sawRight);
		Assert.True(enemy.Body.IsGrounded);
	}

	[Fact]
	public void ChasesAndAttacksTargetOnSamePlatform()
	{
		var enemy = CreateEnemy(EnemyDefinitions.Ninja, OnFloor(EnemyDefinitions.Ninja, 40, 176), [Floor(176)]);
		var target = new Vector2(120, 176);
		var sawChase = false;
		var sawAttack = false;

		for (var frame = 0; frame < 400; frame++)
		{
			enemy.Update(target, Frame);
			sawChase |= enemy.State == EnemyState.Chase;
			sawAttack |= enemy.State == EnemyState.Attack;
		}

		Assert.True(sawChase);
		Assert.True(sawAttack);
		Assert.True(enemy.Feet.X > 60f);
	}

	[Fact]
	public void AttackHitboxIsActiveOnceAndFlipsWithFacing()
	{
		var enemy = CreateEnemy(EnemyDefinitions.Ninja, OnFloor(EnemyDefinitions.Ninja, 40, 176), [Floor(176)]);
		var target = new Vector2(80, 176);
		var frames = 0;

		while (enemy.State != EnemyState.Attack || enemy.AttackFrame < EnemyDefinitions.NinjaStrike.ActiveStart)
		{
			enemy.Update(target, Frame);
			frames++;
			Assert.True(frames < 500);
		}

		enemy.Update(target, Frame);
		Assert.True(enemy.TryGetAttackHitbox(out var box));
		Assert.Equal(EnemyDefinitions.NinjaStrike.HitboxWidth, box.Width);
		Assert.False(enemy.FacingLeft);

		var hitbox = new Aabb(box.X, box.Y, box.Width, box.Height);
		Assert.NotNull(enemy.TryHit(hitbox));
		Assert.Null(enemy.TryHit(hitbox));
	}

	[Fact]
	public void NavigatorPlansVineClimbToUpperPlatform()
	{
		var platforms = new[]
		{
			Floor(176),
			new Platform(new Aabb(96, 64, 64, 8))
		};
		var vine = new Vine(new Aabb(120, 64, 8, 112), VineOrientation.Vertical);
		var navigator = new EnemyNavigator(platforms, [vine]);

		var route = navigator.Plan(new Vector2(40, 176), new Vector2(128, 64));

		Assert.NotEmpty(route);
		Assert.Contains(route, waypoint => waypoint.Kind == EnemyWaypointKind.Climb);
		var climb = route.First(waypoint => waypoint.Kind == EnemyWaypointKind.Climb);
		Assert.Equal(124f, climb.X, 1);
		Assert.Equal(64f, climb.FeetY, 1);
		Assert.Equal(64f, climb.Vine.Bounds.Y, 1);
	}

	[Fact]
	public void NavigatorReturnsEmptyForUnreachableUpperPlatform()
	{
		var platforms = new[]
		{
			Floor(176),
			new Platform(new Aabb(96, 64, 64, 8))
		};
		var navigator = new EnemyNavigator(platforms, []);

		var route = navigator.Plan(new Vector2(40, 176), new Vector2(128, 64));

		Assert.Empty(route);
	}

	[Fact]
	public void EnemyClimbsVineToReachTargetAbove()
	{
		var platforms = new[]
		{
			Floor(176),
			new Platform(new Aabb(96, 64, 64, 8))
		};
		var vine = new Vine(new Aabb(120, 64, 8, 112), VineOrientation.Vertical);
		var enemy = CreateEnemy(
			EnemyDefinitions.Ninja,
			OnFloor(EnemyDefinitions.Ninja, 40, 176),
			platforms,
			[vine]);
		var target = new Vector2(128, 64);
		var climbed = false;

		for (var frame = 0; frame < 900; frame++)
		{
			enemy.Update(target, Frame);
			climbed |= enemy.Body.ClimbingVine is not null;
		}

		Assert.True(climbed);
		Assert.True(enemy.Body.IsGrounded);
		Assert.Equal(64f, enemy.Feet.Y, 1);
		Assert.InRange(enemy.Feet.X, 100f, 140f);
	}

	[Fact]
	public void FirstHitStunsAndSecondKnocksOut()
	{
		var enemy = CreateEnemy(EnemyDefinitions.Ninja, OnFloor(EnemyDefinitions.Ninja, 40, 176), [Floor(176)]);
		Step(enemy, null, 5);

		var defeated = enemy.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.False(defeated);
		Assert.Equal(EnemyState.Stunned, enemy.State);
		Assert.Equal(1, enemy.HitsRemaining);
		Assert.True(enemy.Body.Velocity.X > 0);

		Step(enemy, null, EnemyDefinitions.Ninja.HitInvulnerabilityFrames + 3);

		defeated = enemy.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		Assert.True(defeated);
		Assert.Equal(EnemyState.KnockedDown, enemy.State);
		Assert.Equal(0, enemy.HitsRemaining);
	}

	[Fact]
	public void KnockedOutEnemyRespawnsAtSpawn()
	{
		var spawn = OnFloor(EnemyDefinitions.Ninja, 40, 176);
		var enemy = CreateEnemy(EnemyDefinitions.Ninja, spawn, [Floor(176)]);
		Step(enemy, null, 5);

		enemy.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		Step(enemy, null, EnemyDefinitions.Ninja.HitInvulnerabilityFrames + 3);
		enemy.ApplyHit(AttackDefinitions.Chop, fromLeft: true);

		var sawRespawn = false;
		for (var frame = 0; frame < EnemyDefinitions.Ninja.KnockdownFrames + EnemyDefinitions.Ninja.RespawnFrames + 10; frame++)
		{
			enemy.Update(null, Frame);
			if (enemy.State == EnemyState.Respawning)
			{
				sawRespawn = true;
				Assert.False(enemy.IsVisible);
			}
		}

		Assert.True(sawRespawn);
		Assert.Equal(EnemyState.Idle, enemy.State);
		Assert.Equal(EnemyDefinitions.Ninja.MaxHits, enemy.HitsRemaining);
		Assert.Equal(spawn.X, enemy.Body.Position.X, 1);
		Assert.Equal(spawn.Y, enemy.Body.Position.Y, 1);
	}

	[Fact]
	public void KnockdownHitRecoversAndKeepsFighting()
	{
		var enemy = CreateEnemy(EnemyDefinitions.Yamo, OnFloor(EnemyDefinitions.Yamo, 40, 176), [Floor(176)]);
		Step(enemy, null, 5);
		var knockdown = AttackDefinitions.Kick with { KnocksDown = true };

		enemy.ApplyHit(knockdown, fromLeft: true);

		Assert.Equal(EnemyState.KnockedDown, enemy.State);
		Assert.Equal(EnemyDefinitions.Yamo.MaxHits - 1, enemy.HitsRemaining);

		Step(enemy, new Vector2(120, 176), EnemyDefinitions.Yamo.KnockdownFrames + 5);

		Assert.NotEqual(EnemyState.KnockedDown, enemy.State);
		Assert.NotEqual(EnemyState.Respawning, enemy.State);
		Assert.Equal(EnemyDefinitions.Yamo.MaxHits - 1, enemy.HitsRemaining);
	}

	[Fact]
	public void HitsAreIgnoredWhileKnockedDown()
	{
		var enemy = CreateEnemy(EnemyDefinitions.Ninja, OnFloor(EnemyDefinitions.Ninja, 40, 176), [Floor(176)]);
		Step(enemy, null, 5);

		enemy.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		Step(enemy, null, EnemyDefinitions.Ninja.HitInvulnerabilityFrames + 3);
		enemy.ApplyHit(AttackDefinitions.Chop, fromLeft: true);
		var hitsAfterKnockout = enemy.HitsRemaining;

		var defeated = enemy.ApplyHit(AttackDefinitions.Kick, fromLeft: true);

		Assert.False(defeated);
		Assert.Equal(hitsAfterKnockout, enemy.HitsRemaining);
	}

	[Fact]
	public void SeededRandomIsDeterministic()
	{
		var first = new SeededRandom(12345);
		var second = new SeededRandom(12345);
		var different = new SeededRandom(54321);

		for (var i = 0; i < 128; i++)
		{
			Assert.Equal(first.NextUInt(), second.NextUInt());
		}

		Assert.NotEqual(new SeededRandom(12345).NextUInt(), different.NextUInt());
	}

	[Fact]
	public void SeededRandomStaysInRange()
	{
		var random = new SeededRandom(7);

		for (var i = 0; i < 256; i++)
		{
			Assert.InRange(random.NextInt(10), 0, 9);
			Assert.InRange(random.NextInt(5, 15), 5, 14);
			Assert.InRange(random.NextFloat(), 0f, 0.99999994f);
		}
	}
}
