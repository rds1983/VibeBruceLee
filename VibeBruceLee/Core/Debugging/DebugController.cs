using Microsoft.Xna.Framework;
using VibeBruceLee.Core.Input;

namespace VibeBruceLee.Core.Debugging;

/// <summary>
/// Handles debug-only keys, slow motion and frame stepping. Instantiated only
/// in DEBUG builds; Release builds leave the debug flags at their defaults.
/// </summary>
public sealed class DebugController
{
	private readonly DebugState _state;
	private bool _stepRequested;

	public DebugController(DebugState state)
	{
		ArgumentNullException.ThrowIfNull(state);
		_state = state;
	}

	public bool ShouldUpdate => !_state.Paused || _stepRequested;

	public void Update(InputSystem input)
	{
		ArgumentNullException.ThrowIfNull(input);

		if (input.WasPressed(InputAction.ToggleDebugOverlay))
		{
			_state.OverlayVisible = !_state.OverlayVisible;
		}

		if (input.WasPressed(InputAction.DebugSlowMotion))
		{
			_state.SlowMotion = !_state.SlowMotion;
		}

		if (input.WasPressed(InputAction.DebugPause))
		{
			_state.Paused = !_state.Paused;
			_stepRequested = false;
		}

		if (input.WasPressed(InputAction.DebugFrameStep))
		{
			_state.Paused = true;
			_stepRequested = true;
		}

		if (input.WasPressed(InputAction.DebugInvincible))
		{
			_state.Invincible = !_state.Invincible;
		}

		if (input.WasPressed(InputAction.DebugSkipChamber))
		{
			_state.SkipChamberRequested = true;
		}
	}

	public GameTime ScaleTime(GameTime realTime)
	{
		ArgumentNullException.ThrowIfNull(realTime);

		var elapsed = _state.Paused
			? TimeSpan.FromSeconds(1.0 / 60.0)
			: realTime.ElapsedGameTime;

		if (_state.SlowMotion && !_state.Paused)
		{
			elapsed = TimeSpan.FromTicks(elapsed.Ticks / 4);
		}

		return new GameTime(realTime.TotalGameTime, elapsed);
	}

	public void ConsumeStep()
	{
		_stepRequested = false;
	}
}
