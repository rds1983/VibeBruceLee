using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Input;
using VibeBruceLee.Screens;
using Xunit;

namespace VibeBruceLee.Tests;

public class ScreenStackTests
{
	private readonly InputSystem _input = new(InputBindings.CreateDefault());

	[Fact]
	public void PushSetsCurrentAndEnters()
	{
		var stack = new ScreenStack();
		var title = new FakeScreen();

		stack.Push(title);

		Assert.Same(title, stack.Current);
		Assert.Equal(1, title.EnterCount);
	}

	[Fact]
	public void PopExitsScreenAndRestoresPrevious()
	{
		var stack = new ScreenStack();
		var title = new FakeScreen();
		var chamber = new FakeScreen();
		stack.Push(title);
		stack.Push(chamber);

		var popped = stack.Pop();

		Assert.Same(chamber, popped);
		Assert.Equal(1, chamber.ExitCount);
		Assert.Same(title, stack.Current);
	}

	[Fact]
	public void ReplaceExitsEverythingAndEntersReplacement()
	{
		var stack = new ScreenStack();
		var title = new FakeScreen();
		var chamber = new FakeScreen();
		var pause = new FakeScreen();
		stack.Push(title);
		stack.Push(chamber);

		stack.Replace(pause);

		Assert.Equal(1, title.ExitCount);
		Assert.Equal(1, chamber.ExitCount);
		Assert.Equal(1, pause.EnterCount);
		Assert.Same(pause, stack.Current);
	}

	[Fact]
	public void UpdateRunsOnlyTheTopScreen()
	{
		var stack = new ScreenStack();
		var title = new FakeScreen();
		var chamber = new FakeScreen();
		stack.Push(title);
		stack.Push(chamber);

		stack.Update(new GameTime(), _input);

		Assert.Equal(0, title.UpdateCount);
		Assert.Equal(1, chamber.UpdateCount);
	}

	[Fact]
	public void PopEmptyReturnsNull()
	{
		var stack = new ScreenStack();

		Assert.Null(stack.Pop());
	}

	private sealed class FakeScreen : IGameScreen
	{
		public bool IsOverlay => false;

		public int EnterCount { get; private set; }

		public int ExitCount { get; private set; }

		public int UpdateCount { get; private set; }

		public void Activate() => EnterCount++;

		public void Deactivate() => ExitCount++;

		public void Update(GameTime gameTime, InputSystem input) => UpdateCount++;

		public void Draw(SpriteBatch spriteBatch)
		{
		}
	}
}
