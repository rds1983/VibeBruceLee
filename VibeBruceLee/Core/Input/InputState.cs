namespace VibeBruceLee.Core.Input;

/// <summary>
/// Action states for one frame.
/// </summary>
public readonly struct InputState
{
	private readonly ulong _held;
	private readonly ulong _pressed;
	private readonly ulong _released;

	internal InputState(ulong held, ulong pressed, ulong released)
	{
		_held = held;
		_pressed = pressed;
		_released = released;
	}

	public bool IsHeld(InputAction action) => IsSet(_held, action);

	public bool WasPressed(InputAction action) => IsSet(_pressed, action);

	public bool WasReleased(InputAction action) => IsSet(_released, action);

	private static bool IsSet(ulong mask, InputAction action) => (mask & (1UL << (int)action)) != 0;
}
