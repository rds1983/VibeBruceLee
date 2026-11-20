using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Input;
using Xunit;

namespace VibeBruceLee.Tests;

public class InputSystemTests
{
	private const double Frame = 1.0 / 60.0;

	private static InputSystem CreateSystem()
	{
		return new InputSystem(InputBindings.CreateDefault());
	}

	[Fact]
	public void DefaultBindingsCoverEveryAction()
	{
		var bindings = InputBindings.CreateDefault();

		foreach (var action in Enum.GetValues<InputAction>())
		{
			Assert.NotEmpty(bindings.GetKeyboardKeys(action));
		}
	}

	[Fact]
	public void ControlsKeyIsBound()
	{
		var input = CreateSystem();
		input.Update(new KeyboardState(), default, Frame, true);
		input.Update(new KeyboardState(Keys.C), default, Frame, true);

		Assert.True(input.WasPressed(InputAction.Controls));
	}

	[Fact]
	public void AttackPressIsBufferedAndConsumedOnce()
	{
		var input = CreateSystem();
		input.Update(new KeyboardState(), default, Frame, true);
		input.Update(new KeyboardState(Keys.Z), default, Frame, true);

		Assert.True(input.WasPressed(InputAction.Attack));
		Assert.True(input.IsBuffered(InputAction.Attack));
		Assert.True(input.ConsumeBuffered(InputAction.Attack));
		Assert.False(input.ConsumeBuffered(InputAction.Attack));
	}

	[Fact]
	public void JumpBufferExpires()
	{
		var input = CreateSystem();
		input.Update(new KeyboardState(), default, Frame, true);
		input.Update(new KeyboardState(Keys.Space), default, Frame, true);

		Assert.True(input.IsBuffered(InputAction.Jump));

		input.Update(new KeyboardState(), default, 0.05, true);
		Assert.True(input.IsBuffered(InputAction.Jump));

		input.Update(new KeyboardState(), default, 0.10, true);
		Assert.False(input.IsBuffered(InputAction.Jump));
	}

	[Fact]
	public void NonBufferedActionsCannotBeConsumed()
	{
		var input = CreateSystem();
		input.Update(new KeyboardState(), default, Frame, true);
		input.Update(new KeyboardState(Keys.Enter), default, Frame, true);

		Assert.True(input.WasPressed(InputAction.Confirm));
		Assert.False(input.ConsumeBuffered(InputAction.Confirm));
	}

	[Fact]
	public void FocusLossClearsStateAndSuppressesHeldPresses()
	{
		var input = CreateSystem();
		input.Update(new KeyboardState(), default, Frame, true);
		input.Update(new KeyboardState(Keys.Enter), default, Frame, true);
		Assert.True(input.WasPressed(InputAction.Confirm));

		input.Update(new KeyboardState(Keys.Enter), default, Frame, false);

		Assert.False(input.WasPressed(InputAction.Confirm));
		Assert.False(input.IsHeld(InputAction.Confirm));

		input.Update(new KeyboardState(Keys.Enter), default, Frame, true);

		Assert.True(input.IsHeld(InputAction.Confirm));
		Assert.False(input.WasPressed(InputAction.Confirm));
	}

	[Fact]
	public void HeldActionReportsPressReleaseAndHeldStates()
	{
		var input = CreateSystem();
		input.Update(new KeyboardState(), default, Frame, true);

		input.Update(new KeyboardState(Keys.O), default, Frame, true);
		Assert.True(input.WasPressed(InputAction.MoveLeft));
		Assert.True(input.IsHeld(InputAction.MoveLeft));

		input.Update(new KeyboardState(Keys.O), default, Frame, true);
		Assert.False(input.WasPressed(InputAction.MoveLeft));
		Assert.True(input.IsHeld(InputAction.MoveLeft));

		input.Update(new KeyboardState(), default, Frame, true);
		Assert.True(input.WasReleased(InputAction.MoveLeft));
		Assert.False(input.IsHeld(InputAction.MoveLeft));
	}
}
