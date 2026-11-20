using AssetManagementBase;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Debugging;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Graphics;
using VibeBruceLee.Input;
using VibeBruceLee.Levels;
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
	private SpriteSheet? _bruceSheet;
	private SpriteSheet? _ninjaSheet;
	private SpriteSheet? _yamoSheet;
	private SpriteSheet? _tilesSheet;
	private SpriteSheet? _lanternSheet;
	private Texture2D? _hudLife;
	private Texture2D? _hudLantern;
	private Texture2D? _pixel;
	private ScreenStack? _screens;
	private InputSystem? _input;
	private readonly DebugState _debugState = new();
	private readonly bool _startDebugOverlay;
	private bool _loaded;
#if DEBUG
	private DebugController? _debugController;
	private DebugOverlay? _debugOverlay;
#endif

	public BruceLeeGame(bool startFullscreen = false, bool startDebugOverlay = false)
	{
		_startDebugOverlay = startDebugOverlay;
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
		var bruce = new SpriteSheet(assets.LoadTexture2D(GraphicsDevice, "art/bruce.png"), 16, 24);
		var ninja = new SpriteSheet(assets.LoadTexture2D(GraphicsDevice, "art/ninja.png"), 16, 24);
		var yamo = new SpriteSheet(assets.LoadTexture2D(GraphicsDevice, "art/yamo.png"), 24, 28);
		var tiles = new SpriteSheet(assets.LoadTexture2D(GraphicsDevice, "art/tiles.png"), 8, 8);
		var lanternSheet = new SpriteSheet(
			assets.LoadTexture2D(GraphicsDevice, "art/lantern-sheet.png"),
			16,
			16);
		var hudLife = assets.LoadTexture2D(GraphicsDevice, "art/hud-life.png");
		var hudLantern = assets.LoadTexture2D(GraphicsDevice, "art/hud-lantern.png");
		var chambers = new ChamberCatalog(ChamberLoader.LoadAll(Path.Combine(
			AppContext.BaseDirectory,
			"Assets",
			"levels")));
		var session = new GameSession();
		var highScores = new HighScores(HighScores.DefaultPath);

		var pixel = new Texture2D(GraphicsDevice, 1, 1);
		Color[] white = [Color.White];
		pixel.SetData(white);

		_fontSystem = fontSystem;
		_bruceSheet = bruce;
		_ninjaSheet = ninja;
		_yamoSheet = yamo;
		_tilesSheet = tiles;
		_lanternSheet = lanternSheet;
		_hudLife = hudLife;
		_hudLantern = hudLantern;
		_pixel = pixel;

		var context = new ScreenContext(
			fontSystem,
			bruce,
			ninja,
			yamo,
			tiles,
			lanternSheet,
			hudLife,
			hudLantern,
			pixel,
			_debugState,
			chambers,
			session,
			highScores);
		_screens = new ScreenStack();
		_screens.Push(new TitleScreen(_screens, context));
		_input = new InputSystem(InputBindings.CreateDefault());

		_debugState.OverlayVisible = _startDebugOverlay;
#if DEBUG
		_debugController = new DebugController(_debugState);
		_debugOverlay = new DebugOverlay(fontSystem, pixel, _debugState);
#endif

		_loaded = true;
	}

	protected override void UnloadContent()
	{
		_loaded = false;
		Window.ClientSizeChanged -= OnClientSizeChanged;

		_screens = null;
		_pixel?.Dispose();
		_hudLife?.Dispose();
		_hudLantern?.Dispose();
		_lanternSheet?.Texture.Dispose();
		_yamoSheet?.Texture.Dispose();
		_tilesSheet?.Texture.Dispose();
		_ninjaSheet?.Texture.Dispose();
		_bruceSheet?.Texture.Dispose();
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

#if DEBUG
		_debugController?.Update(_input);
		_debugOverlay?.Update(gameTime.ElapsedGameTime.TotalSeconds);
#endif

		if (_input.WasPressed(InputAction.ToggleFullscreen))
		{
			_graphics.HardwareModeSwitch = false;
			_graphics.ToggleFullScreen();
		}

		if (_input.WasPressed(InputAction.Cancel))
		{
			Exit();
		}

		var simulationTime = gameTime;
#if DEBUG
		if (_debugController is not null)
		{
			if (!_debugController.ShouldUpdate)
			{
				base.Update(gameTime);
				return;
			}

			simulationTime = _debugController.ScaleTime(gameTime);
		}
#endif

		_screens?.Update(simulationTime, _input);

#if DEBUG
		_debugController?.ConsumeStep();
#endif

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
		_debugState.BeginFrame();
		_screens.Draw(_spriteBatch);
#if DEBUG
		_debugOverlay?.Draw(_spriteBatch, _screens.Current?.GetType().Name ?? "none");
#endif
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
