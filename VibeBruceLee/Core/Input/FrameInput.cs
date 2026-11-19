using Microsoft.Xna.Framework.Input;

namespace VibeBruceLee.Core.Input;

/// <summary>
/// Keyboard and gamepad state for one frame, including the previous frame so
/// screens can detect presses without duplicating state tracking.
/// </summary>
public readonly struct FrameInput
{
	public FrameInput(
		KeyboardState keyboard,
		KeyboardState previousKeyboard,
		GamePadState gamePad,
		GamePadState previousGamePad)
	{
		Keyboard = keyboard;
		PreviousKeyboard = previousKeyboard;
		GamePad = gamePad;
		PreviousGamePad = previousGamePad;
	}

	public KeyboardState Keyboard { get; }

	public KeyboardState PreviousKeyboard { get; }

	public GamePadState GamePad { get; }

	public GamePadState PreviousGamePad { get; }

	public bool WasKeyPressed(Keys key) => Keyboard.IsKeyDown(key) && PreviousKeyboard.IsKeyUp(key);

	public bool WasButtonPressed(Buttons button) => GamePad.IsButtonDown(button) && PreviousGamePad.IsButtonUp(button);
}
