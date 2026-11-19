using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Rendering;

namespace VibeBruceLee;

/// <summary>
/// The MonoGame host. The screen stack arrives in T-010; for now it hosts the
/// asset pipeline smoke test and exits on Escape.
/// </summary>
public sealed class BruceLeeGame : Game
{
	private const int Scale = 5;

	private readonly GraphicsDeviceManager _graphics;
	private SpriteBatch? _spriteBatch;
	private AssetPipelineTestScene? _testScene;

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

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		var assets = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));
		_testScene = new AssetPipelineTestScene(assets, GraphicsDevice);
	}

	protected override void UnloadContent()
	{
		_testScene?.Dispose();
		_spriteBatch?.Dispose();
		base.UnloadContent();
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

		if (_spriteBatch is not null && _testScene is not null)
		{
			_spriteBatch.Begin(samplerState: SamplerState.PointClamp);
			_testScene.Draw(_spriteBatch);
			_spriteBatch.End();
		}

		base.Draw(gameTime);
	}
}
