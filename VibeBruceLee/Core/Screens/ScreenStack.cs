using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Input;

namespace VibeBruceLee.Core.Screens;

public interface IGameScreen
{
	void Activate();

	void Deactivate();

	void Update(GameTime gameTime, FrameInput input);

	void Draw(SpriteBatch spriteBatch);
}

/// <summary>
/// A simple screen stack. Only the top screen updates; all screens draw from
/// bottom to top so a pause screen can overlay the chamber beneath it.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The type is a stack of screens.")]
public sealed class ScreenStack
{
	private readonly List<IGameScreen> _screens = [];

	public IReadOnlyList<IGameScreen> Screens => _screens;

	public IGameScreen? Current => _screens.Count == 0 ? null : _screens[^1];

	public void Push(IGameScreen screen)
	{
		ArgumentNullException.ThrowIfNull(screen);

		_screens.Add(screen);
		screen.Activate();
	}

	public IGameScreen? Pop()
	{
		if (_screens.Count == 0)
		{
			return null;
		}

		var screen = _screens[^1];
		_screens.RemoveAt(_screens.Count - 1);
		screen.Deactivate();
		return screen;
	}

	public void Replace(IGameScreen screen)
	{
		ArgumentNullException.ThrowIfNull(screen);

		while (_screens.Count > 0)
		{
			Pop();
		}

		Push(screen);
	}

	public void Update(GameTime gameTime, FrameInput input)
	{
		Current?.Update(gameTime, input);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		ArgumentNullException.ThrowIfNull(spriteBatch);

		foreach (var screen in _screens)
		{
			screen.Draw(spriteBatch);
		}
	}
}
