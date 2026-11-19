using AssetManagementBase;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Core.Graphics;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Screens;
using VibeBruceLee.Presentation;
using VibeBruceLee.Screens;

namespace VibeBruceLee;

/// <summary>
/// The MonoGame host: fixed 60 Hz, a 256x192 virtual canvas scaled by an
/// integer factor, and a screen stack.
/// </summary>
public sealed class BruceLeeGame : Game
{
	private const int DefaultScale = 5;

	private readonly GraphicsDeviceManager _graphics;
	private SpriteBatch? _spriteBatch;
	private RenderTarget2D? _canvas;
	private FontSystem? _fontSystem;
	private Texture2D? _testCard;
	private SpriteSheet? _lanternSheet;
	private Texture2D? _pixel;
	private ScreenStack? _screens;
	private InputSystem? _input;
	private bool _loaded;

	public BruceLeeGame(bool startFullscreen = false)
	{
		_graphics = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = GameInfo.VirtualWidth * DefaultScale,
			PreferredBackBufferHeight = GameInfo.VirtualHeight * DefaultScale,
			SynchronizeWithVerticalRetrace = true,
			HardwareModeSwitch = false,
			IsFullScreen = startFullscreen
		};

		IsFixedTimeStep = true;
		TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
		IsMouseVisible = true;
		Window.Title = GameInfo.Title;
		Window.AllowUserResizing = true;
		Window.ClientSizeChanged += OnClientSizeChanged;
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);
		_canvas = new RenderTarget2D(
			GraphicsDevice,
			GameInfo.VirtualWidth,
			GameInfo.VirtualHeight,
			false,
			SurfaceFormat.Color,
			DepthFormat.None,
			0,
			RenderTargetUsage.DiscardContents);

		var assets = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));
		var fontSystem = assets.LoadFontSystem("fonts/press-start-2p-regular.ttf");
		var testCard = assets.LoadTexture2D(GraphicsDevice, "art/test-card.png");
		var lanternSheet = new SpriteSheet(
			assets.LoadTexture2D(GraphicsDevice, "art/lantern-sheet.png"),
			16,
			16);

		var pixel = new Texture2D(GraphicsDevice, 1, 1);
		Color[] white = [Color.White];
		pixel.SetData(white);

		_fontSystem = fontSystem;
		_testCard = testCard;
		_lanternSheet = lanternSheet;
		_pixel = pixel;

		var context = new ScreenContext(fontSystem, testCard, lanternSheet, pixel);
		_screens = new ScreenStack();
		_screens.Push(new TitleScreen(_screens, context));
		_input = new InputSystem(InputBindings.CreateDefault());

		_loaded = true;
	}

	protected override void UnloadContent()
	{
		_loaded = false;
		Window.ClientSizeChanged -= OnClientSizeChanged;

		_screens = null;
		_pixel?.Dispose();
		_lanternSheet?.Texture.Dispose();
		_testCard?.Dispose();
		_fontSystem?.Dispose();
		_canvas?.Dispose();
		_spriteBatch?.Dispose();

		base.UnloadContent();
	}

	protected override void Update(GameTime gameTime)
	{
		if (_input is null)
		{
			base.Update(gameTime);
			return;
		}

		_input.Update(
			Keyboard.GetState(),
			GamePad.GetState(PlayerIndex.One),
			gameTime.ElapsedGameTime.TotalSeconds,
			IsActive);

		if (_input.WasPressed(InputAction.ToggleFullscreen))
		{
			_graphics.HardwareModeSwitch = false;
			_graphics.ToggleFullScreen();
		}

		if (_input.WasPressed(InputAction.Cancel))
		{
			Exit();
		}

		_screens?.Update(gameTime, _input);

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		if (_spriteBatch is null || _canvas is null || _screens is null)
		{
			return;
		}

		GraphicsDevice.SetRenderTarget(_canvas);
		GraphicsDevice.Clear(Palette.Background);
		_spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, samplerState: SamplerState.PointClamp);
		_screens.Draw(_spriteBatch);
		_spriteBatch.End();

		GraphicsDevice.SetRenderTarget(null);
		GraphicsDevice.Clear(Color.Black);

		var viewport = GraphicsDevice.Viewport;
		var layout = Letterbox.Calculate(viewport.Width, viewport.Height);

		_spriteBatch.Begin(samplerState: SamplerState.PointClamp);
		_spriteBatch.Draw(_canvas, layout.Destination, Color.White);
		_spriteBatch.End();

		base.Draw(gameTime);
	}

	private void OnClientSizeChanged(object? sender, EventArgs e)
	{
		if (!_loaded)
		{
			return;
		}

		var bounds = Window.ClientBounds;
		if (bounds.Width <= 0 || bounds.Height <= 0)
		{
			return;
		}

		if (_graphics.PreferredBackBufferWidth == bounds.Width &&
			_graphics.PreferredBackBufferHeight == bounds.Height)
		{
			return;
		}

		_graphics.PreferredBackBufferWidth = bounds.Width;
		_graphics.PreferredBackBufferHeight = bounds.Height;
		_graphics.ApplyChanges();
	}
}
