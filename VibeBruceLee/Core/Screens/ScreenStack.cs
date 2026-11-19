using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Input;

namespace VibeBruceLee.Core.Screens;

public interface IGameScreen
{
	bool IsOverlay { get; }

	void Activate();

	void Deactivate();

	void Update(GameTime gameTime, InputSystem input);

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

	public void Update(GameTime gameTime, InputSystem input)
	{
		Current?.Update(gameTime, input);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		ArgumentNullException.ThrowIfNull(spriteBatch);

		if (_screens.Count == 0)
		{
			return;
		}

		// Draw the deepest full screen plus any overlays above it.
		var first = _screens.Count - 1;
		while (first > 0 && _screens[first].IsOverlay)
		{
			first--;
		}

		for (var i = first; i < _screens.Count; i++)
		{
			_screens[i].Draw(spriteBatch);
		}
	}
}
