using Microsoft.Xna.Framework;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Fire Wizard boss. He fires aimed fireballs from alternating eyes and is
/// destroyed only by pressing the chamber button.
/// </summary>
public sealed class WizardBoss
{
	public const float FireballSpeed = 90f;
	public const float WarningSeconds = 0.3f;
	public const float DefeatSeconds = 2f;
	public const float FireFlashSeconds = 0.15f;
	public const float MinimumPeriodSeconds = 0.6f;
	public const int Score = 3000;

	private readonly float _period;
	private float _cooldown;
	private float _fireFlash;
	private float _defeatTimer;
	private int _eyeIndex;

	public WizardBoss(WizardDefinition definition)
	{
		ArgumentNullException.ThrowIfNull(definition);

		Bounds = new Aabb(definition.X, definition.Y, definition.Width, definition.Height);
		ButtonBounds = new Aabb(
			definition.ButtonX,
			definition.ButtonY,
			definition.ButtonWidth,
			definition.ButtonHeight);
		_period = MathF.Max(MinimumPeriodSeconds, (float)definition.FireballPeriodSeconds);
		_cooldown = _period;
	}

	public Aabb Bounds { get; }

	public Aabb ButtonBounds { get; }

	public bool IsDefeated { get; private set; }

	public bool IsWarning { get; private set; }

	public bool IsFiring => _fireFlash > 0f;

	public bool IsDefeatFinished => IsDefeated && _defeatTimer <= 0f;

	public bool PlayerAtButton(Aabb playerBounds)
	{
		return !IsDefeated && playerBounds.Intersects(ButtonBounds);
	}

	public void Update(float deltaSeconds, Aabb playerBounds, HazardSystem hazards)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);
		ArgumentNullException.ThrowIfNull(hazards);

		if (_fireFlash > 0f)
		{
			_fireFlash -= deltaSeconds;
		}

		if (IsDefeated)
		{
			_defeatTimer = MathF.Max(0f, _defeatTimer - deltaSeconds);
			return;
		}

		_cooldown -= deltaSeconds;
		IsWarning = _cooldown <= WarningSeconds && _cooldown > 0f;

		if (_cooldown <= 0f)
		{
			SpawnFireball(playerBounds, hazards);
			_cooldown += _period;
			IsWarning = false;
		}
	}

	public bool TryPressButton(Aabb playerBounds, bool attackPressed)
	{
		if (IsDefeated || !attackPressed || !playerBounds.Intersects(ButtonBounds))
		{
			return false;
		}

		IsDefeated = true;
		IsWarning = false;
		_defeatTimer = DefeatSeconds;
		return true;
	}

	public void Reset()
	{
		IsDefeated = false;
		IsWarning = false;
		_cooldown = _period;
		_fireFlash = 0f;
		_defeatTimer = 0f;
		_eyeIndex = 0;
	}

	private void SpawnFireball(Aabb playerBounds, HazardSystem hazards)
	{
		var eye = _eyeIndex == 0 ? LeftEye : RightEye;
		var direction = playerBounds.Center - eye;
		if (direction.LengthSquared() < 1f)
		{
			direction = new Vector2(0f, 1f);
		}

		direction.Normalize();
		var velocity = direction * FireballSpeed;

		hazards.Add(new AimedFireballHazard(
			eye - new Vector2(AimedFireballHazard.Size / 2f, AimedFireballHazard.Size / 2f),
			velocity));

		_fireFlash = FireFlashSeconds;
		_eyeIndex ^= 1;
	}

	private Vector2 LeftEye => new(
		Bounds.X + (Bounds.Width * 0.32f),
		Bounds.Y + (Bounds.Height * 0.42f));

	private Vector2 RightEye => new(
		Bounds.X + (Bounds.Width * 0.68f),
		Bounds.Y + (Bounds.Height * 0.42f));
}
