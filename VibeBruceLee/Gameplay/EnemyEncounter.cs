using Microsoft.Xna.Framework;
using VibeBruceLee.Input;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Headless combat coordinator for Bruce and the enemies in a chamber. It owns
/// attack pacing, hit resolution, friendly fire and enemy separation so the
/// rules can be tested without a window.
/// </summary>
public sealed class EnemyEncounter
{
	public const int PressureFrames = 30;

	private readonly Player _player;
	private readonly IReadOnlyList<Enemy> _enemies;
	private readonly GameSession? _session;
	private int _pressureFrames;

	public EnemyEncounter(Player player, IReadOnlyList<Enemy> enemies, GameSession? session = null)
	{
		ArgumentNullException.ThrowIfNull(player);
		ArgumentNullException.ThrowIfNull(enemies);

		_player = player;
		_enemies = enemies;
		_session = session;
	}

	public int PlayerHitsTaken { get; private set; }

	public void Update(InputSystem input, float deltaSeconds)
	{
		ArgumentNullException.ThrowIfNull(input);
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		_player.Update(input, deltaSeconds);
		if (_pressureFrames > 0)
		{
			_pressureFrames--;
		}

		foreach (var enemy in _enemies)
		{
			if (enemy.IsVisible && enemy.BlocksMovement)
			{
				_player.ResolveBodyBlock(enemy.Body.Bounds);
			}
		}

		var targetFeet = GetTargetFeet();
		Enemy? activeAttacker = null;
		foreach (var enemy in _enemies)
		{
			if (enemy.IsVisible && enemy.IsAttacking)
			{
				activeAttacker = enemy;
				break;
			}
		}

		foreach (var enemy in _enemies)
		{
			var mayAttack = activeAttacker is null
				? _pressureFrames <= 0
				: ReferenceEquals(activeAttacker, enemy);

			enemy.Update(targetFeet, deltaSeconds, mayAttack);

			if (mayAttack && enemy.IsAttacking)
			{
				activeAttacker = enemy;
			}
		}

		ResolveAttacks();
		SeparateEnemies();
	}

	/// <summary>
	/// Resolves all active hitboxes. Public so scripted encounter tests can
	/// exercise friendly fire without a graphics device.
	/// </summary>
	public void ResolveAttacks()
	{
		ResolvePlayerAttacks();
		ResolveEnemyAttacks();
	}

	public static void Separate(Enemy a, Enemy b)
	{
		ArgumentNullException.ThrowIfNull(a);
		ArgumentNullException.ThrowIfNull(b);

		if (!CanSeparate(a) || !CanSeparate(b) || !a.Body.Bounds.Intersects(b.Body.Bounds))
		{
			return;
		}

		var overlap = MathF.Min(a.Body.Bounds.Right, b.Body.Bounds.Right) -
			MathF.Max(a.Body.Bounds.X, b.Body.Bounds.X);
		if (overlap <= 0f)
		{
			return;
		}

		var totalWidth = a.Body.Size.X + b.Body.Size.X;
		var aShare = b.Body.Size.X / totalWidth;
		var bShare = a.Body.Size.X / totalWidth;
		var direction = a.Feet.X <= b.Feet.X ? -1f : 1f;

		a.Body.Position = new Vector2(
			ClampX(a.Body.Position.X + (direction * overlap * aShare), a.Body.Size.X),
			a.Body.Position.Y);
		b.Body.Position = new Vector2(
			ClampX(b.Body.Position.X - (direction * overlap * bShare), b.Body.Size.X),
			b.Body.Position.Y);
	}

	private void ResolvePlayerAttacks()
	{
		foreach (var enemy in _enemies)
		{
			if (!enemy.CanBeHit)
			{
				continue;
			}

			var hit = _player.TryHit(enemy.Hurtbox);
			if (hit is null)
			{
				continue;
			}

			var fromLeft = _player.Body.Bounds.Center.X <= enemy.Body.Bounds.Center.X;
			var defeated = enemy.ApplyHit(hit, fromLeft);
			_session?.AddScore(hit.Score);

			if (defeated)
			{
				_session?.AddScore(enemy.Definition.KnockoutScore);
			}
		}
	}

	private void ResolveEnemyAttacks()
	{
		foreach (var attacker in _enemies)
		{
			if (!attacker.IsVisible || !attacker.IsAttacking)
			{
				continue;
			}

			var hitAnotherEnemy = false;
			foreach (var target in _enemies)
			{
				if (ReferenceEquals(attacker, target) || !target.CanBeHit)
				{
					continue;
				}

				var friendlyHit = attacker.TryHit(target.Hurtbox);
				if (friendlyHit is null)
				{
					continue;
				}

				target.ApplyHit(
					friendlyHit,
					attacker.Body.Bounds.Center.X <= target.Body.Bounds.Center.X);
				hitAnotherEnemy = true;
				break;
			}

			if (hitAnotherEnemy || _player.IsDead || _player.IsInvulnerable)
			{
				continue;
			}

			var hit = attacker.TryHit(_player.Hurtbox);
			if (hit is null)
			{
				continue;
			}

			_player.ApplyHit(
				hit,
				attacker.Body.Bounds.Center.X <= _player.Body.Bounds.Center.X);
			PlayerHitsTaken++;
			_pressureFrames = PressureFrames;
		}
	}

	private void SeparateEnemies()
	{
		for (var pass = 0; pass < 2; pass++)
		{
			for (var i = 0; i < _enemies.Count; i++)
			{
				for (var j = i + 1; j < _enemies.Count; j++)
				{
					Separate(_enemies[i], _enemies[j]);
				}
			}
		}
	}

	private Vector2? GetTargetFeet()
	{
		return _player.IsDead
			? null
			: new Vector2(
				_player.Body.Position.X + (_player.Body.Size.X * 0.5f),
				_player.Body.Position.Y + _player.Body.Size.Y);
	}

	private static bool CanSeparate(Enemy enemy)
	{
		return enemy.IsVisible &&
			enemy.Body.IsGrounded &&
			enemy.Body.ClimbingVine is null &&
			enemy.State is not (EnemyState.Stunned or EnemyState.KnockedDown or EnemyState.Respawning);
	}

	private static float ClampX(float x, float width)
	{
		return Math.Clamp(x, 0f, MathF.Max(0f, GameInfo.VirtualWidth - width));
	}
}
