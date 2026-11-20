using Microsoft.Xna.Framework;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;
using Xunit;

namespace VibeBruceLee.Tests;

public class WizardBossTests
{
	private const float Frame = 1f / 60f;

	private static WizardDefinition Definition()
	{
		return new WizardDefinition
		{
			X = 112,
			Y = 16,
			Width = 32,
			Height = 32,
			ButtonX = 24,
			ButtonY = 112,
			ButtonWidth = 16,
			ButtonHeight = 8,
			FireballPeriodSeconds = 1.4
		};
	}

	private static Player CreatePlayer()
	{
		var world = new PhysicsWorld(256, 192, 900, 600);
		return new Player(world, new Vector2(32, 154), [], []);
	}

	[Fact]
	public void FiresAfterWarningAndAimsAtPlayer()
	{
		var boss = new WizardBoss(Definition());
		var hazards = new HazardSystem([]);
		var player = new Aabb(32, 154, 10, 22);

		boss.Update(1f, player, hazards);
		Assert.Empty(hazards.Hazards);
		Assert.False(boss.IsWarning);

		boss.Update(0.2f, player, hazards);
		Assert.True(boss.IsWarning);
		Assert.Empty(hazards.Hazards);

		boss.Update(0.2f, player, hazards);

		var fireball = Assert.IsType<AimedFireballHazard>(Assert.Single(hazards.Hazards));
		Assert.True(boss.IsFiring);
		Assert.True(fireball.Velocity.X < 0f);
		Assert.True(fireball.Velocity.Y > 0f);
	}

	[Fact]
	public void FireballCanKillThePlayer()
	{
		var boss = new WizardBoss(Definition());
		var hazards = new HazardSystem([]);
		var player = CreatePlayer();

		boss.Update(1.4f, player.Hurtbox, hazards);
		Assert.Single(hazards.Hazards);

		for (var frame = 0; frame < 300 && !player.IsDead; frame++)
		{
			hazards.Update(Frame, player, []);
		}

		Assert.True(player.IsDead);
	}

	[Fact]
	public void ButtonNeedsOverlapAndAttack()
	{
		var boss = new WizardBoss(Definition());
		var button = new Aabb(26, 100, 10, 22);
		var away = new Aabb(100, 154, 10, 22);

		Assert.False(boss.TryPressButton(away, attackPressed: true));
		Assert.False(boss.TryPressButton(button, attackPressed: false));
		Assert.True(boss.TryPressButton(button, attackPressed: true));
		Assert.True(boss.IsDefeated);
		Assert.False(boss.TryPressButton(button, attackPressed: true));
	}

	[Fact]
	public void DefeatStopsFireballsAndFinishes()
	{
		var boss = new WizardBoss(Definition());
		var hazards = new HazardSystem([]);
		var player = new Aabb(26, 100, 10, 22);

		Assert.True(boss.TryPressButton(player, attackPressed: true));

		boss.Update(1f, player, hazards);
		Assert.Empty(hazards.Hazards);
		Assert.False(boss.IsDefeatFinished);

		boss.Update(WizardBoss.DefeatSeconds, player, hazards);
		Assert.True(boss.IsDefeatFinished);
	}

	[Fact]
	public void FireballDespawnsOffScreen()
	{
		var fireball = new AimedFireballHazard(new Vector2(248, 100), new Vector2(100, 0));

		fireball.Update(1f);

		Assert.True(fireball.IsFinished);
		Assert.False(fireball.IsVisible);
		Assert.False(fireball.IsActive);
	}
}
