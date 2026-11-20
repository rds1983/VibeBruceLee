using Microsoft.Xna.Framework;
using VibeBruceLee.Animation;
using VibeBruceLee.Input;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Bruce's locomotion and combat state machine on top of <see cref="PhysicsWorld"/>.
/// </summary>
public sealed class Player
{
	public const float RunSpeed = 80f;
	public const float RunAcceleration = 600f;
	public const float RunFriction = 900f;
	public const float JumpSpeed = 240f;
	public const float JumpCutMultiplier = 0.4f;
	public const float ClimbSpeed = 60f;
	public const int BodyWidth = 10;
	public const int BodyHeight = 22;
	public const int KnockdownFrames = 24;
	public const float RunAttackThreshold = 20f;

	private readonly PhysicsWorld _world;
	private readonly IReadOnlyList<Platform> _platforms;
	private readonly IReadOnlyList<Vine> _vines;
	private readonly AnimationPlayer _animation = new();
	private bool _jumpHeld;
	private AttackDefinition? _attack;
	private int _attackFrame;
	private bool _hasHitThisAttack;
	private int _hitstunFrames;
	private int _knockdownFrames;

	public Player(
		PhysicsWorld world,
		Vector2 spawn,
		IReadOnlyList<Platform> platforms,
		IReadOnlyList<Vine> vines)
	{
		ArgumentNullException.ThrowIfNull(world);
		ArgumentNullException.ThrowIfNull(platforms);
		ArgumentNullException.ThrowIfNull(vines);

		_world = world;
		_platforms = platforms;
		_vines = vines;
		Body = new PhysicsBody(spawn, new Vector2(BodyWidth, BodyHeight));
		_animation.Play(PlayerAnimations.Idle);
	}

	public PhysicsBody Body { get; }

	public PlayerState State { get; private set; } = PlayerState.Idle;

	public bool FacingLeft { get; private set; }

	public bool IsAttacking => _attack is not null;

	public AttackType AttackType => _attack?.Type ?? AttackType.None;

	public int AttackFrame => _attackFrame;

	public bool IsHurt => _hitstunFrames > 0 || _knockdownFrames > 0;

	public Aabb Hurtbox => Body.Bounds;

	public int CurrentFrame
	{
		get
		{
			if (_knockdownFrames > 0)
			{
				return 17;
			}

			if (_hitstunFrames > 0)
			{
				return 16;
			}

			if (_attack is not null)
			{
				if (_attackFrame < _attack.ActiveStart)
				{
					return _attack.StartupAnimationFrame;
				}

				if (_attackFrame < _attack.ActiveEnd)
				{
					return _attack.ActiveAnimationFrame;
				}

				return _attack.RecoveryAnimationFrame;
			}

			return _animation.CurrentFrame;
		}
	}

	public void Update(InputSystem input, float deltaSeconds)
	{
		ArgumentNullException.ThrowIfNull(input);
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		if (_knockdownFrames > 0)
		{
			_knockdownFrames--;
			ApplyHitDrag(deltaSeconds);
			_world.Step(Body, deltaSeconds, _platforms, _vines);
			UpdateState(input);
			UpdateAnimation(deltaSeconds);
			return;
		}

		if (_hitstunFrames > 0)
		{
			_hitstunFrames--;
			ApplyHitDrag(deltaSeconds);
			_world.Step(Body, deltaSeconds, _platforms, _vines);
			UpdateState(input);
			UpdateAnimation(deltaSeconds);
			return;
		}

		if (_attack is not null)
		{
			_attackFrame++;
			if (_attackFrame >= _attack.TotalFrames)
			{
				EndAttack();
			}
			else
			{
				ApplyHitDrag(deltaSeconds);
			}

			_world.Step(Body, deltaSeconds, _platforms, _vines);
			UpdateState(input);
			UpdateAnimation(deltaSeconds);
			return;
		}

		var grounded = Body.IsGrounded;
		var move = Direction(input.IsHeld(InputAction.MoveRight), input.IsHeld(InputAction.MoveLeft));
		var down = input.IsHeld(InputAction.MoveDown);
		var canAttack = grounded && Body.ClimbingVine is null && !down;
		if (!canAttack && input.WasPressed(InputAction.Attack))
		{
			input.ConsumeBuffered(InputAction.Attack);
		}

		var attackPressed = canAttack &&
			(input.WasPressed(InputAction.Attack) || input.ConsumeBuffered(InputAction.Attack));

		if (attackPressed)
		{
			StartAttack(ChooseAttack());
			input.ConsumeBuffered(InputAction.Attack);
			_world.Step(Body, deltaSeconds, _platforms, _vines);
			UpdateState(input);
			UpdateAnimation(deltaSeconds);
			return;
		}

		if (Body.ClimbingVine is Vine vine)
		{
			var jump = input.WasPressed(InputAction.Jump) || input.ConsumeBuffered(InputAction.Jump);
			if (jump)
			{
				Body.ClimbingVine = null;
				Body.Velocity = new Vector2(move * RunSpeed * 0.5f, -JumpSpeed);
				_jumpHeld = true;
				input.ConsumeBuffered(InputAction.Jump);
			}
			else
			{
				var vertical = Direction(input.IsHeld(InputAction.MoveUp), input.IsHeld(InputAction.MoveDown));
				Body.Velocity = vine.Orientation == VineOrientation.Vertical
					? new Vector2(0, -vertical * ClimbSpeed)
					: new Vector2(move * ClimbSpeed, 0);
			}
		}
		else
		{
			if (grounded && down)
			{
				Body.Velocity = new Vector2(0, Body.Velocity.Y);
			}
			else
			{
				var target = move * RunSpeed;
				var rate = move == 0 ? RunFriction : RunAcceleration;
				Body.Velocity = new Vector2(
					MoveTowards(Body.Velocity.X, target, rate * deltaSeconds),
					Body.Velocity.Y);
			}

			if (input.WasPressed(InputAction.MoveUp))
			{
				var foundVine = PhysicsWorld.FindVineAt(Body.Bounds.Center, _vines);
				if (foundVine is not null)
				{
					PhysicsWorld.AttachToVine(Body, foundVine.Value);
					Body.Velocity = Vector2.Zero;
					grounded = false;
				}
			}

			if (Body.ClimbingVine is null)
			{
				if (grounded && (input.WasPressed(InputAction.Jump) || input.ConsumeBuffered(InputAction.Jump)))
				{
					Body.Velocity = new Vector2(Body.Velocity.X, -JumpSpeed);
					_jumpHeld = true;
					input.ConsumeBuffered(InputAction.Jump);
				}
				else if (_jumpHeld && !input.IsHeld(InputAction.Jump) && Body.Velocity.Y < 0)
				{
					Body.Velocity = new Vector2(Body.Velocity.X, Body.Velocity.Y * JumpCutMultiplier);
					_jumpHeld = false;
				}
				else if (!input.IsHeld(InputAction.Jump))
				{
					_jumpHeld = false;
				}
			}
		}

		if (move != 0 && Body.ClimbingVine?.Orientation != VineOrientation.Horizontal)
		{
			FacingLeft = move < 0;
		}

		_world.Step(Body, deltaSeconds, _platforms, _vines);
		UpdateState(input);
		UpdateAnimation(deltaSeconds);
	}

	public bool TryGetAttackHitbox(out Aabb hitbox)
	{
		if (_attack is null || _hasHitThisAttack || !_attack.IsActive(_attackFrame))
		{
			hitbox = default;
			return false;
		}

		var body = Body.Bounds;
		var x = FacingLeft
			? body.X - _attack.HitboxWidth
			: body.Right;

		hitbox = new Aabb(
			x,
			body.Y + _attack.HitboxOffsetY,
			_attack.HitboxWidth,
			_attack.HitboxHeight);

		return true;
	}

	/// <summary>
	/// Checks one target against the active hitbox. Each attack can hit a target
	/// at most once; returns the attack data on a hit.
	/// </summary>
	public AttackDefinition? TryHit(Aabb target)
	{
		if (!TryGetAttackHitbox(out var hitbox) || !hitbox.Intersects(target))
		{
			return null;
		}

		_hasHitThisAttack = true;
		return _attack;
	}

	public void ApplyHit(AttackDefinition hit, bool fromLeft)
	{
		ArgumentNullException.ThrowIfNull(hit);

		_attack = null;
		_attackFrame = 0;
		_hasHitThisAttack = false;
		_hitstunFrames = hit.KnocksDown ? 0 : hit.HitstunFrames;
		_knockdownFrames = hit.KnocksDown ? KnockdownFrames : 0;
		Body.Velocity = new Vector2(
			(fromLeft ? 1 : -1) * hit.KnockbackX,
			hit.KnockbackY);

		State = hit.KnocksDown ? PlayerState.KnockedDown : PlayerState.Hit;
	}

	private AttackType ChooseAttack()
	{
		return MathF.Abs(Body.Velocity.X) > RunAttackThreshold
			? AttackType.Kick
			: AttackType.Chop;
	}

	private void StartAttack(AttackType type)
	{
		_attack = AttackDefinitions.Get(type);
		_attackFrame = 0;
		_hasHitThisAttack = false;

		if (type == AttackType.Chop)
		{
			Body.Velocity = new Vector2(0, Body.Velocity.Y);
		}
	}

	private void EndAttack()
	{
		_attack = null;
		_attackFrame = 0;
		_hasHitThisAttack = false;
	}

	private void ApplyHitDrag(float deltaSeconds)
	{
		Body.Velocity = new Vector2(
			MoveTowards(Body.Velocity.X, 0, RunFriction * deltaSeconds),
			Body.Velocity.Y);
	}

	private void UpdateState(InputSystem input)
	{
		if (_knockdownFrames > 0)
		{
			State = PlayerState.KnockedDown;
		}
		else if (_hitstunFrames > 0)
		{
			State = PlayerState.Hit;
		}
		else if (_attack is not null)
		{
			State = PlayerState.Attack;
		}
		else if (Body.ClimbingVine is not null)
		{
			State = PlayerState.Climb;
		}
		else if (Body.IsGrounded)
		{
			if (input.IsHeld(InputAction.MoveDown))
			{
				State = PlayerState.Duck;
			}
			else
			{
				State = MathF.Abs(Body.Velocity.X) > 4 ? PlayerState.Run : PlayerState.Idle;
			}
		}
		else
		{
			State = Body.Velocity.Y < 0 ? PlayerState.Jump : PlayerState.Fall;
		}
	}

	private void UpdateAnimation(float deltaSeconds)
	{
		if (State is PlayerState.Attack or PlayerState.Hit or PlayerState.KnockedDown)
		{
			return;
		}

		var clip = State switch
		{
			PlayerState.Run => PlayerAnimations.Run,
			PlayerState.Jump or PlayerState.Fall => PlayerAnimations.Air,
			PlayerState.Duck => PlayerAnimations.Duck,
			PlayerState.Climb => PlayerAnimations.Climb,
			_ => PlayerAnimations.Idle
		};

		if (_animation.Clip != clip)
		{
			_animation.Play(clip);
		}

		_animation.Update(deltaSeconds);
	}

	private static int Direction(bool positive, bool negative)
	{
		return (positive ? 1 : 0) - (negative ? 1 : 0);
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
