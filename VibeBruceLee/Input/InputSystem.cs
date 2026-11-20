using Microsoft.Xna.Framework.Input;

namespace VibeBruceLee.Input;

/// <summary>
/// Polls keyboard and gamepad states into actions, with short jump and attack
/// buffers. Call <see cref="Update"/> once per simulation frame.
/// </summary>
public sealed class InputSystem
{
	public const double BufferSeconds = 0.12;

	private static readonly InputAction[] AllActions = Enum.GetValues<InputAction>();

	private readonly InputBindings _bindings;
	private ulong _previousHeld;
	private double _jumpBuffer;
	private double _attackBuffer;
	private bool _suppressPressed;
	private bool _gamePadConnected;
	private InputState _state;

	public InputSystem(InputBindings bindings)
	{
		ArgumentNullException.ThrowIfNull(bindings);
		_bindings = bindings;
	}

	public InputState State => _state;

	public bool IsHeld(InputAction action) => _state.IsHeld(action);

	public bool WasPressed(InputAction action) => _state.WasPressed(action);

	public bool WasReleased(InputAction action) => _state.WasReleased(action);

	public bool IsBuffered(InputAction action) => action switch
	{
		InputAction.Jump => _jumpBuffer > 0,
		InputAction.Attack => _attackBuffer > 0,
		_ => false
	};

	public bool ConsumeBuffered(InputAction action)
	{
		if (!IsBuffered(action))
		{
			return false;
		}

		switch (action)
		{
			case InputAction.Jump:
				_jumpBuffer = 0;
				return true;
			case InputAction.Attack:
				_attackBuffer = 0;
				return true;
			default:
				return false;
		}
	}

	public void Update(KeyboardState keyboard, GamePadState gamePad, double elapsedSeconds, bool focused)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(elapsedSeconds);

		if (!focused)
		{
			Reset();
			return;
		}

		if (gamePad.IsConnected != _gamePadConnected)
		{
			_gamePadConnected = gamePad.IsConnected;
			ClearFrameState();
		}

		_jumpBuffer = Math.Max(0, _jumpBuffer - elapsedSeconds);
		_attackBuffer = Math.Max(0, _attackBuffer - elapsedSeconds);

		var held = 0UL;
		foreach (var action in AllActions)
		{
			if (_bindings.IsHeld(action, keyboard, gamePad))
			{
				held |= Mask(action);
			}
		}

		var pressed = _suppressPressed ? 0UL : held & ~_previousHeld;
		var released = ~held & _previousHeld;
		_suppressPressed = false;

		if ((pressed & Mask(InputAction.Jump)) != 0)
		{
			_jumpBuffer = BufferSeconds;
		}

		if ((pressed & Mask(InputAction.Attack)) != 0)
		{
			_attackBuffer = BufferSeconds;
		}

		_state = new InputState(held, pressed, released);
		_previousHeld = held;
	}

	public void Reset()
	{
		ClearFrameState();
	}

	private void ClearFrameState()
	{
		_state = default;
		_previousHeld = 0;
		_jumpBuffer = 0;
		_attackBuffer = 0;
		_suppressPressed = true;
	}

	private static ulong Mask(InputAction action) => 1UL << (int)action;
}
