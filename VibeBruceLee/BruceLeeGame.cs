using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VibeBruceLee;

/// <summary>
/// The MonoGame host. Rendering is added in later tasks; for now it only opens
/// a window, runs a fixed 60 Hz loop and exits on Escape.
/// </summary>
public sealed class BruceLeeGame : Game
{
    private const int Scale = 5;

    private readonly GraphicsDeviceManager _graphics;

    public BruceLeeGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = GameInfo.VirtualWidth * Scale,
            PreferredBackBufferHeight = GameInfo.VirtualHeight * Scale,
            SynchronizeWithVerticalRetrace = true
        };

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        IsMouseVisible = true;
        Window.Title = GameInfo.Title;
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var gamepad = GamePad.GetState(PlayerIndex.One);

        if (keyboard.IsKeyDown(Keys.Escape) || gamepad.Buttons.Back == ButtonState.Pressed)
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(12, 12, 20));
        base.Draw(gameTime);
    }
}
