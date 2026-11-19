using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Core.Debugging;
using VibeBruceLee.Core.Input;
using Xunit;

namespace VibeBruceLee.Tests;

public class DebugControllerTests
{
	private const double Frame = 1.0 / 60.0;

	private static InputSystem CreateInput()
	{
		return new InputSystem(InputBindings.CreateDefault());
	}

	private static void Tap(InputSystem input, DebugController controller, Keys key)
	{
		input.Update(new KeyboardState(), default, Frame, true);
		input.Update(new KeyboardState(key), default, Frame, true);
		controller.Update(input);
	}

	[Fact]
	public void TogglesSlowMotionInvincibilityAndOverlay()
	{
		var state = new DebugState();
		var controller = new DebugController(state);
		var input = CreateInput();
		input.Update(new KeyboardState(), default, Frame, true);
		controller.Update(input);

		Tap(input, controller, Keys.F1);
		Tap(input, controller, Keys.F3);
		Tap(input, controller, Keys.F6);

		Assert.True(state.OverlayVisible);
		Assert.True(state.SlowMotion);
		Assert.True(state.Invincible);
	}

	[Fact]
	public void PauseAndFrameStepControlSimulation()
	{
		var state = new DebugState();
		var controller = new DebugController(state);
		var input = CreateInput();
		input.Update(new KeyboardState(), default, Frame, true);
		controller.Update(input);

		Tap(input, controller, Keys.F4);

		Assert.True(state.Paused);
		Assert.False(controller.ShouldUpdate);

		Tap(input, controller, Keys.F5);

		Assert.True(controller.ShouldUpdate);
		var scaled = controller.ScaleTime(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1)));
		Assert.Equal(Frame, scaled.ElapsedGameTime.TotalSeconds, 4);

		controller.ConsumeStep();
		Assert.False(controller.ShouldUpdate);

		Tap(input, controller, Keys.F4);
		Assert.False(state.Paused);
	}

	[Fact]
	public void SlowMotionScalesSimulationTime()
	{
		var state = new DebugState();
		var controller = new DebugController(state);
		var input = CreateInput();
		input.Update(new KeyboardState(), default, Frame, true);
		controller.Update(input);

		Tap(input, controller, Keys.F3);

		var scaled = controller.ScaleTime(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1)));
		Assert.Equal(0.25, scaled.ElapsedGameTime.TotalSeconds, 4);
	}

	[Fact]
	public void SkipChamberIsRequestedAndConsumed()
	{
		var state = new DebugState();
		var controller = new DebugController(state);
		var input = CreateInput();
		input.Update(new KeyboardState(), default, Frame, true);
		controller.Update(input);

		Tap(input, controller, Keys.F7);

		Assert.True(state.ConsumeSkipChamber());
		Assert.False(state.ConsumeSkipChamber());
	}

	[Fact]
	public void BeginFrameClearsRegisteredRects()
	{
		var state = new DebugState();
		state.AddHitbox(new Rectangle(1, 2, 3, 4));
		state.AddSolid(new Rectangle(5, 6, 7, 8));

		Assert.Equal(2, state.Rects.Count);

		state.BeginFrame();

		Assert.Empty(state.Rects);
	}
}
