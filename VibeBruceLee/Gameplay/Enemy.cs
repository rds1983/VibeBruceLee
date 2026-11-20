using Microsoft.Xna.Framework;
using VibeBruceLee.Animation;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Shared enemy behaviour: patrol, chase, attack, hit reactions, knockout and
/// respawn. Platform and vine movement comes from <see cref="EnemyNavigator"/>.
/// </summary>
public sealed class Enemy
{
	public const float EdgeMargin = 2f;
	public const float Drag = 600f;
	public const float StandoffMargin = 2f;
	public const float StandoffTolerance = 4f;

	private readonly PhysicsWorld _world;
	private readonly IReadOnlyList<Platform> _platforms;
	private readonly IReadOnlyList<Vine> _vines;
	private readonly EnemyNavigator _navigator;
	private readonly SeededRandom _random;
	private readonly Vector2 _spawn;
	private readonly AnimationPlayer _animation = new();
	private readonly List<EnemyWaypoint> _route = [];
	private int _routeIndex;
	private int _plannedSurface = -1;
	private int _plannedTargetSurface = -1;
	private int _idleFrames;
	private int _patrolDirection;
	private int _attackFrame;
	private int _attackCooldownFrames;
	private int _attackCooldownAfter;
	private AttackDefinition? _attack;
	private bool _hasHitThisAttack;
	private int _jumpFrame;
	private int _jumpCooldownFrames;
	private float _jumpTargetX;
	private int _hitstunFrames;
	private int _knockdownFrames;
	private int _respawnFrames;
	private int _hitFlashFrames;
	private int _invulnerableFrames;

	public Enemy(
		EnemyDefinition definition,
		Vector2 spawn,
		PhysicsWorld world,
		IReadOnlyList<Platform> platforms,
		IReadOnlyList<Vine> vines,
		SeededRandom? random = null)
	{
		ArgumentNullException.ThrowIfNull(definition);
		ArgumentNullException.ThrowIfNull(world);
		ArgumentNullException.ThrowIfNull(platforms);
		ArgumentNullException.ThrowIfNull(vines);

		Definition = definition;
		_world = world;
		_platforms = platforms;
		_vines = vines;
		_navigator = new EnemyNavigator(platforms, vines);
		_random = random ?? new SeededRandom(1);
		_spawn = spawn;
		Body = new PhysicsBody(spawn, new Vector2(definition.BodyWidth, definition.BodyHeight));
		HitsRemaining = definition.MaxHits;
		_idleFrames = definition.IdleFrames;
		_patrolDirection = _random.NextBool() ? 1 : -1;
		_animation.Play(EnemyAnimations.Idle);
	}

	public EnemyDefinition Definition { get; }

	public PhysicsBody Body { get; }

	public EnemyState State { get; private set; } = EnemyState.Idle;

	public bool FacingLeft { get; private set; }

	public int HitsRemaining { get; private set; }

	public bool IsAttacking => State is EnemyState.Attack or EnemyState.Jumping;

	public bool IsJumping => State == EnemyState.Jumping;

	public int AttackFrame => _attackFrame;

	public int AttackCooldownRemaining => _attackCooldownFrames;

	public int JumpCooldownRemaining => _jumpCooldownFrames;

	public AttackDefinition? CurrentAttack => GetCurrentAttack();

	public bool BlocksMovement => Definition.BlocksMovement;

	public bool IsFlashing => _hitFlashFrames > 0;

	public bool IsVisible => State != EnemyState.Respawning;

	public Vector2 Feet => new(Body.Position.X + (Body.Size.X * 0.5f), Body.Position.Y + Body.Size.Y);

	public float StandoffDistance => MathF.Max(
		8f,
		(Definition.PreferredRange > 0f ? Definition.PreferredRange : Definition.AttackRange) - StandoffMargin);

	public Aabb Hurtbox => Body.Bounds;

	public int CurrentFrame
	{
		get
		{
			switch (State)
			{
				case EnemyState.KnockedDown:
				case EnemyState.Respawning:
					return 9;
				case EnemyState.Stunned:
					return 8;
				case EnemyState.Attack:
					return GetAttackAnimationFrame(_attack ?? Definition.Attack, _attackFrame);
				case EnemyState.Jumping:
					if (_jumpFrame <= Definition.JumpWindupFrames)
					{
						return 0;
					}

					return GetAttackAnimationFrame(
						Definition.JumpAttack ?? Definition.Attack,
						Math.Max(0, _jumpFrame - Definition.JumpWindupFrames - 1));
				default:
					return _animation.CurrentFrame;
			}
		}
	}

	/// <summary>
	/// Advances the enemy. A null target keeps it patrolling, which also keeps
	/// headless tests independent of the player type.
	/// </summary>
	public void Update(Vector2? targetFeet, float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		if (_hitFlashFrames > 0)
		{
			_hitFlashFrames--;
		}

		if (_invulnerableFrames > 0)
		{
			_invulnerableFrames--;
		}

		switch (State)
		{
			case EnemyState.Respawning:
				TickRespawn();
				return;
			case EnemyState.KnockedDown:
				TickKnockdown(deltaSeconds);
				return;
			case EnemyState.Stunned:
				TickStunned(deltaSeconds);
				return;
			case EnemyState.Attack:
				TickAttack(deltaSeconds);
				return;
			case EnemyState.Jumping:
				TickJump(deltaSeconds);
				return;
		}

		if (_attackCooldownFrames > 0)
		{
			_attackCooldownFrames--;
		}

		if (_jumpCooldownFrames > 0)
		{
			_jumpCooldownFrames--;
		}

		if (targetFeet is null)
		{
			if (State == EnemyState.Idle)
			{
				TickIdle(deltaSeconds);
			}
			else
			{
				EnterPatrol();
				UpdatePatrol(deltaSeconds);
			}

			return;
		}

		var target = targetFeet.Value;
		if (State == EnemyState.Idle)
		{
			if (SeesTarget(target))
			{
				EnterChase();
				return;
			}

			TickIdle(deltaSeconds);
			return;
		}

		if (State == EnemyState.Patrol)
		{
			if (SeesTarget(target))
			{
				EnterChase();
				return;
			}

			UpdatePatrol(deltaSeconds);
			return;
		}

		UpdateChase(target, deltaSeconds);
	}

	public bool TryGetAttackHitbox(out Aabb hitbox)
	{
		var attack = GetCurrentAttack();
		if (attack is null || _hasHitThisAttack || !attack.IsActive(GetCurrentAttackFrame()))
		{
			hitbox = default;
			return false;
		}

		var body = Body.Bounds;
		var x = FacingLeft
			? body.X - attack.HitboxWidth
			: body.Right;

		hitbox = new Aabb(
			x,
			body.Y + attack.HitboxOffsetY,
			attack.HitboxWidth,
			attack.HitboxHeight);

		return true;
	}

	/// <summary>
	/// Checks a target against the active attack box. An attack can hit once.
	/// </summary>
	public AttackDefinition? TryHit(Aabb target)
	{
		if (!TryGetAttackHitbox(out var hitbox) || !hitbox.Intersects(target))
		{
			return null;
		}

		_hasHitThisAttack = true;
		return GetCurrentAttack();
	}

	/// <summary>
	/// Applies a hit and returns true when this hit defeats the enemy. Defeated
	/// enemies remain down for a while and then respawn at their spawn point.
	/// </summary>
	public bool ApplyHit(AttackDefinition hit, bool fromLeft)
	{
		ArgumentNullException.ThrowIfNull(hit);

		if (!IsVisible ||
			State is EnemyState.KnockedDown or EnemyState.Respawning ||
			_invulnerableFrames > 0)
		{
			return false;
		}

		_hitFlashFrames = Definition.HitFlashFrames;
		_invulnerableFrames = Definition.HitInvulnerabilityFrames;
		HitsRemaining = Math.Max(0, HitsRemaining - 1);
		Body.Velocity = new Vector2(
			(fromLeft ? 1 : -1) * hit.KnockbackX,
			hit.KnockbackY);
		_attack = null;
		_attackFrame = 0;
		_hasHitThisAttack = false;
		_attackCooldownFrames = 0;
		_attackCooldownAfter = 0;
		_jumpFrame = 0;
		_jumpCooldownFrames = Definition.JumpCooldownFrames;
		ClearRoute();

		if (HitsRemaining <= 0)
		{
			EnterKnockdown();
			return true;
		}

		if (hit.KnocksDown)
		{
			EnterKnockdown();
		}
		else
		{
			State = EnemyState.Stunned;
			_hitstunFrames = hit.HitstunFrames;
		}

		return false;
	}

	/// <summary>
	/// Puts a defeated enemy back at its spawn with full health.
	/// </summary>
	public void Respawn()
	{
		Body.Position = _spawn;
		Body.Velocity = Vector2.Zero;
		Body.ClimbingVine = null;
		Body.IsGrounded = false;
		HitsRemaining = Definition.MaxHits;
		_hitstunFrames = 0;
		_knockdownFrames = 0;
		_hitFlashFrames = 0;
		_invulnerableFrames = 0;
		_attack = null;
		_attackFrame = 0;
		_hasHitThisAttack = false;
		_attackCooldownFrames = 0;
		_attackCooldownAfter = 0;
		_jumpFrame = 0;
		_jumpCooldownFrames = 0;
		_jumpTargetX = 0;
		_idleFrames = Definition.IdleFrames;
		_patrolDirection = _random.NextBool() ? 1 : -1;
		ClearRoute();
		State = EnemyState.Idle;
		_animation.Play(EnemyAnimations.Idle);
	}

	private void TickIdle(float deltaSeconds)
	{
		Body.Velocity = new Vector2(0, Body.Velocity.Y);
		StepPhysics(deltaSeconds);
		UpdateAnimation(deltaSeconds);

		_idleFrames--;
		if (_idleFrames <= 0)
		{
			EnterPatrol();
		}
	}

	private void UpdatePatrol(float deltaSeconds)
	{
		if (Body.ClimbingVine is not null)
		{
			Body.ClimbingVine = null;
		}

		var surface = _navigator.FindSurface(Feet);
		if (surface is null)
		{
			Body.Velocity = new Vector2(0, Body.Velocity.Y);
			StepPhysics(deltaSeconds);
			UpdateAnimation(deltaSeconds);
			return;
		}

		var centerX = Body.Position.X + (Body.Size.X * 0.5f);
		var halfWidth = Body.Size.X * 0.5f;
		var leftBound = MathF.Max(surface.Value.Left, 0) + halfWidth + EdgeMargin;
		var rightBound = MathF.Min(surface.Value.Right, _world.Width) - halfWidth - EdgeMargin;

		if (centerX >= rightBound)
		{
			_patrolDirection = -1;
		}
		else if (centerX <= leftBound)
		{
			_patrolDirection = 1;
		}

		Body.Velocity = new Vector2(_patrolDirection * Definition.PatrolSpeed, Body.Velocity.Y);
		FacingLeft = _patrolDirection < 0;
		StepPhysics(deltaSeconds);
		UpdateAnimation(deltaSeconds);
	}

	private void UpdateChase(Vector2 targetFeet, float deltaSeconds)
	{
		if (Body.ClimbingVine is not null)
		{
			FollowClimb(deltaSeconds);
			return;
		}

		var feet = Feet;
		if (!Body.IsGrounded)
		{
			var direction = MathF.Sign(targetFeet.X - feet.X);
			Body.Velocity = new Vector2(
				MoveTowards(Body.Velocity.X, direction * Definition.RunSpeed * 0.5f, 250f * deltaSeconds),
				Body.Velocity.Y);
			FacingLeft = Body.Velocity.X < -1f || (MathF.Abs(Body.Velocity.X) <= 1f && FacingLeft);
			StepPhysics(deltaSeconds);
			UpdateAnimation(deltaSeconds);
			return;
		}

		var surface = _navigator.FindSurface(feet);
		var maxAttackRange = MathF.Max(Definition.AttackRange, Definition.SecondaryAttackRange);
		if (surface is not null &&
			MathF.Abs(feet.Y - targetFeet.Y) <= 3f &&
			MathF.Abs(targetFeet.X - feet.X) <= maxAttackRange &&
			_attackCooldownFrames <= 0)
		{
			StartAttack(targetFeet);
			return;
		}

		var targetSurface = _navigator.FindSurface(targetFeet);
		if (surface is not null && ShouldJump(feet, targetFeet, surface.Value, targetSurface))
		{
			StartJump(targetFeet);
			return;
		}

		var sameSurface = surface is not null &&
			targetSurface is not null &&
			surface.Value.Index == targetSurface.Value.Index;
		if (sameSurface || (surface is not null && MathF.Abs(feet.Y - targetFeet.Y) <= 3f))
		{
			KeepTargetDistance(targetFeet, deltaSeconds);
			return;
		}

		if (surface is not null)
		{
			var surfaceIndex = surface.Value.Index;
			var targetSurfaceIndex = targetSurface?.Index ?? -1;
			if (_plannedSurface != surfaceIndex || _plannedTargetSurface != targetSurfaceIndex)
			{
				_plannedSurface = surfaceIndex;
				_plannedTargetSurface = targetSurfaceIndex;
				_route.Clear();
				_route.AddRange(_navigator.Plan(feet, targetFeet));
				_routeIndex = 0;
			}
		}

		FollowRoute(targetFeet, deltaSeconds);
	}

	private bool ShouldJump(
		Vector2 feet,
		Vector2 targetFeet,
		EnemySurface surface,
		EnemySurface? targetSurface)
	{
		if (!Definition.CanJump || Definition.JumpAttack is null || _jumpCooldownFrames > 0)
		{
			return false;
		}

		var distanceX = MathF.Abs(targetFeet.X - feet.X);
		if (distanceX <= Definition.AttackRange + 1f || distanceX > Definition.JumpRange)
		{
			return false;
		}

		var heightGain = feet.Y - targetFeet.Y;
		if (heightGain > 24f || heightGain < -96f)
		{
			return false;
		}

		if (targetSurface is not null && targetSurface.Value.Index == surface.Index)
		{
			return true;
		}

		return _navigator.Plan(feet, targetFeet).Count == 0;
	}

	private void StartJump(Vector2 targetFeet)
	{
		State = EnemyState.Jumping;
		_jumpFrame = 0;
		_jumpTargetX = targetFeet.X;
		_hasHitThisAttack = false;
		FacingLeft = targetFeet.X < Feet.X;
		Body.Velocity = Vector2.Zero;
		ClearRoute();
	}

	private void TickJump(float deltaSeconds)
	{
		var windup = Definition.JumpWindupFrames;
		_jumpFrame++;

		if (_jumpFrame <= windup)
		{
			Body.Velocity = new Vector2(0, Body.Velocity.Y);
			StepPhysics(deltaSeconds);
			return;
		}

		if (_jumpFrame == windup + 1)
		{
			var direction = MathF.Sign(_jumpTargetX - Feet.X);
			if (direction == 0)
			{
				direction = FacingLeft ? -1 : 1;
			}

			FacingLeft = direction < 0;
			Body.Velocity = new Vector2(
				direction * Definition.JumpForwardSpeed,
				-Definition.JumpSpeed);
		}

		StepPhysics(deltaSeconds);

		var attack = Definition.JumpAttack ?? Definition.Attack;
		if ((_jumpFrame > windup + 2 && Body.IsGrounded) ||
			_jumpFrame > windup + attack.TotalFrames + 20)
		{
			EndJump();
		}
	}

	private void EndJump()
	{
		State = EnemyState.Chase;
		_jumpCooldownFrames = Definition.JumpCooldownFrames;
		_jumpFrame = 0;
		_hasHitThisAttack = false;
		ClearRoute();
	}

	private AttackDefinition? GetCurrentAttack()
	{
		return State switch
		{
			EnemyState.Attack => _attack,
			EnemyState.Jumping => Definition.JumpAttack,
			_ => null
		};
	}

	private int GetCurrentAttackFrame()
	{
		return State == EnemyState.Jumping
			? Math.Max(0, _jumpFrame - Definition.JumpWindupFrames - 1)
			: _attackFrame;
	}

	private static int GetAttackAnimationFrame(AttackDefinition attack, int frame)
	{
		if (frame < attack.ActiveStart)
		{
			return attack.StartupAnimationFrame;
		}

		return frame < attack.ActiveEnd
			? attack.ActiveAnimationFrame
			: attack.RecoveryAnimationFrame;
	}

	private void FollowRoute(Vector2 targetFeet, float deltaSeconds)
	{
		if (_routeIndex >= _route.Count)
		{
			KeepTargetDistance(targetFeet, deltaSeconds);
			return;
		}

		var waypoint = _route[_routeIndex];
		if (waypoint.Kind == EnemyWaypointKind.Climb)
		{
			if (MathF.Abs(Feet.X - waypoint.X) > 4f)
			{
				WalkToward(waypoint.X, deltaSeconds);
				return;
			}

			AttachToVine(waypoint.Vine);
			return;
		}

		if (_routeIndex == _route.Count - 1 && waypoint.Kind == EnemyWaypointKind.Walk)
		{
			KeepTargetDistance(targetFeet, deltaSeconds);
			return;
		}

		var difference = waypoint.X - Feet.X;
		if (MathF.Abs(difference) <= 1f)
		{
			_routeIndex++;
			if (_routeIndex >= _route.Count)
			{
				KeepTargetDistance(targetFeet, deltaSeconds);
				return;
			}

			waypoint = _route[_routeIndex];
		}

		var direction = MathF.Sign(waypoint.X - Feet.X);
		if (direction != 0)
		{
			FacingLeft = direction < 0;
		}

		Body.Velocity = new Vector2(direction * Definition.RunSpeed, Body.Velocity.Y);
		StepPhysics(deltaSeconds);
		UpdateAnimation(deltaSeconds);
	}

	private void KeepTargetDistance(Vector2 targetFeet, float deltaSeconds)
	{
		var delta = targetFeet.X - Feet.X;
		var distance = MathF.Abs(delta);
		var standoff = StandoffDistance;

		if (distance > standoff)
		{
			WalkToward(targetFeet.X, deltaSeconds);
			return;
		}

		if (distance < standoff - StandoffTolerance)
		{
			var targetDirection = delta > 0f
				? 1
				: delta < 0f
					? -1
					: FacingLeft ? -1 : 1;
			var retreatDirection = -targetDirection;

			FacingLeft = targetDirection < 0;
			Body.Velocity = new Vector2(retreatDirection * Definition.RunSpeed, Body.Velocity.Y);
			StepPhysics(deltaSeconds);
			UpdateAnimation(deltaSeconds);
			return;
		}

		if (delta != 0f)
		{
			FacingLeft = delta < 0f;
		}

		Body.Velocity = new Vector2(
			MoveTowards(Body.Velocity.X, 0, Drag * deltaSeconds),
			Body.Velocity.Y);
		StepPhysics(deltaSeconds);
		UpdateAnimation(deltaSeconds);
	}

	private void WalkToward(float x, float deltaSeconds)
	{
		var direction = MathF.Sign(x - Feet.X);
		if (direction != 0)
		{
			FacingLeft = direction < 0;
		}

		Body.Velocity = new Vector2(direction * Definition.RunSpeed, Body.Velocity.Y);
		StepPhysics(deltaSeconds);
		UpdateAnimation(deltaSeconds);
	}

	private void FollowClimb(float deltaSeconds)
	{
		if (Body.ClimbingVine is not Vine vine ||
			_routeIndex >= _route.Count ||
			_route[_routeIndex].Kind != EnemyWaypointKind.Climb ||
			_route[_routeIndex].Vine != vine)
		{
			Body.ClimbingVine = null;
			StepPhysics(deltaSeconds);
			return;
		}

		var waypoint = _route[_routeIndex];
		var targetY = waypoint.FeetY - Body.Size.Y;
		var difference = targetY - Body.Position.Y;
		var step = Definition.ClimbSpeed * deltaSeconds;

		if (MathF.Abs(difference) <= step)
		{
			Body.Position = new Vector2(Body.Position.X, targetY);
			Body.ClimbingVine = null;
			Body.Velocity = Vector2.Zero;
			Body.IsGrounded = true;
			_routeIndex++;
			_plannedSurface = -1;
			UpdateAnimation(deltaSeconds);
			return;
		}

		Body.Position = new Vector2(Body.Position.X, Body.Position.Y + (MathF.Sign(difference) * step));
		Body.Velocity = new Vector2(0, MathF.Sign(difference) * Definition.ClimbSpeed);
		UpdateAnimation(deltaSeconds);
	}

	private void AttachToVine(Vine vine)
	{
		Body.ClimbingVine = vine;
		Body.Position = new Vector2(vine.Bounds.Center.X - (Body.Size.X * 0.5f), Body.Position.Y);
		Body.Velocity = Vector2.Zero;
		Body.IsGrounded = false;
	}

	private void StartAttack(Vector2 targetFeet)
	{
		var distance = MathF.Abs(targetFeet.X - Feet.X);
		var attack = Definition.Attack;
		var cooldown = Definition.AttackCooldownFrames;

		if (Definition.SecondaryAttack is not null &&
			distance > Definition.AttackRange &&
			distance <= Definition.SecondaryAttackRange)
		{
			attack = Definition.SecondaryAttack;
			cooldown = Definition.SecondaryAttackCooldownFrames;
		}

		State = EnemyState.Attack;
		_attack = attack;
		_attackCooldownAfter = cooldown;
		_attackFrame = 0;
		_hasHitThisAttack = false;
		FacingLeft = targetFeet.X < Feet.X;

		var forward = attack.ForwardSpeed;
		Body.Velocity = forward > 0
			? new Vector2((FacingLeft ? -1 : 1) * forward, Body.Velocity.Y)
			: new Vector2(0, Body.Velocity.Y);
	}

	private void TickAttack(float deltaSeconds)
	{
		var attack = _attack ?? Definition.Attack;
		_attackFrame++;
		if (_attackFrame >= attack.TotalFrames)
		{
			State = EnemyState.Chase;
			_attackCooldownFrames = _attackCooldownAfter;
			_attack = null;
			_attackCooldownAfter = 0;
			_attackFrame = 0;
			_hasHitThisAttack = false;
			ClearRoute();
		}
		else if (attack.ForwardSpeed <= 0)
		{
			ApplyDrag(deltaSeconds);
		}

		StepPhysics(deltaSeconds);
	}

	private void TickStunned(float deltaSeconds)
	{
		_hitstunFrames--;
		ApplyDrag(deltaSeconds);
		StepPhysics(deltaSeconds);
		UpdateAnimation(deltaSeconds);

		if (_hitstunFrames <= 0)
		{
			State = EnemyState.Chase;
			ClearRoute();
		}
	}

	private void TickKnockdown(float deltaSeconds)
	{
		_knockdownFrames--;
		ApplyDrag(deltaSeconds);
		StepPhysics(deltaSeconds);

		if (_knockdownFrames <= 0)
		{
			if (HitsRemaining <= 0)
			{
				BeginRespawn();
			}
			else
			{
				State = EnemyState.Chase;
				ClearRoute();
			}
		}
	}

	private void TickRespawn()
	{
		_respawnFrames--;
		if (_respawnFrames <= 0)
		{
			Respawn();
		}
	}

	private void BeginRespawn()
	{
		State = EnemyState.Respawning;
		_respawnFrames = Definition.RespawnFrames;
		Body.Velocity = Vector2.Zero;
		Body.ClimbingVine = null;
	}

	private void EnterKnockdown()
	{
		State = EnemyState.KnockedDown;
		_knockdownFrames = Definition.KnockdownFrames;
	}

	private void EnterChase()
	{
		State = EnemyState.Chase;
		ClearRoute();
	}

	private void EnterPatrol()
	{
		State = EnemyState.Patrol;
		ClearRoute();
	}

	private void ClearRoute()
	{
		_route.Clear();
		_routeIndex = 0;
		_plannedSurface = -1;
		_plannedTargetSurface = -1;
	}

	private bool SeesTarget(Vector2 targetFeet)
	{
		var feet = Feet;
		var deltaX = targetFeet.X - feet.X;
		var deltaY = targetFeet.Y - feet.Y;
		return (deltaX * deltaX) + (deltaY * deltaY) <= Definition.SightRange * Definition.SightRange;
	}

	private void StepPhysics(float deltaSeconds)
	{
		_world.Step(Body, deltaSeconds, _platforms, _vines);
	}

	private void ApplyDrag(float deltaSeconds)
	{
		Body.Velocity = new Vector2(
			MoveTowards(Body.Velocity.X, 0, Drag * deltaSeconds),
			Body.Velocity.Y);
	}

	private void UpdateAnimation(float deltaSeconds)
	{
		var clip = MathF.Abs(Body.Velocity.X) > 4f
			? EnemyAnimations.Run
			: EnemyAnimations.Idle;

		if (_animation.Clip != clip)
		{
			_animation.Play(clip);
		}

		_animation.Update(deltaSeconds);
	}

	private static float MoveTowards(float current, float target, float maxDelta)
	{
		var difference = target - current;
		if (MathF.Abs(difference) <= maxDelta)
		{
			return target;
		}

		return current + (MathF.Sign(difference) * maxDelta);
	}
}
