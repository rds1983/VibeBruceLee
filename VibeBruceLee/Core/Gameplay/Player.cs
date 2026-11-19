using Microsoft.Xna.Framework;
using VibeBruceLee.Core.Animation;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Physics;

namespace VibeBruceLee.Core.Gameplay;

/// <summary>
/// Bruce's locomotion state machine on top of <see cref="PhysicsWorld"/>.
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

	private readonly PhysicsWorld _world;
	private readonly IReadOnlyList<Platform> _platforms;
	private readonly IReadOnlyList<Vine> _vines;
	private readonly AnimationPlayer _animation = new();
	private bool _jumpHeld;

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

	public int CurrentFrame => _animation.CurrentFrame;

	public void Update(InputSystem input, float deltaSeconds)
	{
		ArgumentNullException.ThrowIfNull(input);
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		var grounded = Body.IsGrounded;
		var move = Direction(input.IsHeld(InputAction.MoveRight), input.IsHeld(InputAction.MoveLeft));
		var down = input.IsHeld(InputAction.MoveDown);

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

	private void UpdateState(InputSystem input)
	{
		if (Body.ClimbingVine is not null)
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
