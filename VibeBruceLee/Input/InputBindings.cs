using Microsoft.Xna.Framework.Input;

namespace VibeBruceLee.Input;

/// <summary>
/// Central keyboard and gamepad bindings. Change default controls here.
/// </summary>
public sealed class InputBindings
{
	private const float StickThreshold = 0.5f;

	private readonly Dictionary<InputAction, Keys[]> _keyboard;
	private readonly Dictionary<InputAction, Buttons[]> _gamePad;

	public InputBindings(
		IReadOnlyDictionary<InputAction, Keys[]> keyboard,
		IReadOnlyDictionary<InputAction, Buttons[]> gamePad)
	{
		ArgumentNullException.ThrowIfNull(keyboard);
		ArgumentNullException.ThrowIfNull(gamePad);

		_keyboard = new Dictionary<InputAction, Keys[]>(keyboard);
		_gamePad = new Dictionary<InputAction, Buttons[]>(gamePad);
	}

	public static InputBindings CreateDefault()
	{
		return new InputBindings(
			new Dictionary<InputAction, Keys[]>
			{
				[InputAction.MoveLeft] = [Keys.O, Keys.Left],
				[InputAction.MoveRight] = [Keys.P, Keys.Right],
				[InputAction.MoveUp] = [Keys.Q, Keys.Up],
				[InputAction.MoveDown] = [Keys.A, Keys.Down],
				[InputAction.Jump] = [Keys.X, Keys.Space],
				[InputAction.Attack] = [Keys.Z, Keys.M],
				[InputAction.Pause] = [Keys.Enter],
				[InputAction.Confirm] = [Keys.Enter],
				[InputAction.Cancel] = [Keys.Escape],
				[InputAction.ToggleFullscreen] = [Keys.F11],
				[InputAction.ToggleSound] = [Keys.F2],
				[InputAction.DebugGameOver] = [Keys.G],
				[InputAction.ToggleDebugOverlay] = [Keys.F1],
				[InputAction.DebugSlowMotion] = [Keys.F3],
				[InputAction.DebugPause] = [Keys.F4],
				[InputAction.DebugFrameStep] = [Keys.F5],
				[InputAction.DebugInvincible] = [Keys.F6],
				[InputAction.DebugSkipChamber] = [Keys.F7]
			},
			new Dictionary<InputAction, Buttons[]>
			{
				[InputAction.MoveLeft] = [Buttons.DPadLeft],
				[InputAction.MoveRight] = [Buttons.DPadRight],
				[InputAction.MoveUp] = [Buttons.DPadUp],
				[InputAction.MoveDown] = [Buttons.DPadDown],
				[InputAction.Jump] = [Buttons.A],
				[InputAction.Attack] = [Buttons.X],
				[InputAction.Pause] = [Buttons.Start],
				[InputAction.Confirm] = [Buttons.A],
				[InputAction.Cancel] = [Buttons.Back],
				[InputAction.ToggleSound] = [Buttons.Y]
			});
	}

	public IReadOnlyList<Keys> GetKeyboardKeys(InputAction action)
	{
		return _keyboard.TryGetValue(action, out var keys) ? keys : [];
	}

	public IReadOnlyList<Buttons> GetGamePadButtons(InputAction action)
	{
		return _gamePad.TryGetValue(action, out var buttons) ? buttons : [];
	}

	public bool IsHeld(InputAction action, KeyboardState keyboard, GamePadState gamePad)
	{
		var keys = GetKeyboardKeys(action);
		for (var i = 0; i < keys.Count; i++)
		{
			if (keyboard.IsKeyDown(keys[i]))
			{
				return true;
			}
		}

		if (!gamePad.IsConnected)
		{
			return false;
		}

		var buttons = GetGamePadButtons(action);
		for (var i = 0; i < buttons.Count; i++)
		{
			if (gamePad.IsButtonDown(buttons[i]))
			{
				return true;
			}
		}

		var stick = gamePad.ThumbSticks.Left;
		return action switch
		{
			InputAction.MoveLeft => stick.X <= -StickThreshold,
			InputAction.MoveRight => stick.X >= StickThreshold,
			InputAction.MoveUp => stick.Y >= StickThreshold,
			InputAction.MoveDown => stick.Y <= -StickThreshold,
			_ => false
		};
	}
}
