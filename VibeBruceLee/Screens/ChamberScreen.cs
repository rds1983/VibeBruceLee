using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Animation;
using VibeBruceLee.Core.Gameplay;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Levels;
using VibeBruceLee.Core.Physics;
using VibeBruceLee.Core.Screens;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class ChamberScreen : IGameScreen
{
	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly ChamberDefinition _chamber;
	private readonly ChamberGeometry _geometry;
	private readonly PhysicsWorld _world;
	private readonly Player _player;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly AnimationPlayer _ninjaPlayer = new();
	private readonly AnimationPlayer _yamoPlayer = new();
	private readonly AnimationPlayer _lanternPlayer = new();

	public ChamberScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_chamber = context.Chamber;
		_geometry = ChamberGeometry.From(_chamber);
		_world = new PhysicsWorld(GameInfo.VirtualWidth, GameInfo.VirtualHeight, 900, 600);
		_font16 = context.FontSystem.GetFont(16);
		_font8 = context.FontSystem.GetFont(8);

		var spawn = _chamber.PlayerSpawn ?? new PointDefinition { X = 32, Y = 144 };
		_player = new Player(
			_world,
			new Vector2(spawn.X, spawn.Y),
			_geometry.Platforms,
			_geometry.Vines);

		_ninjaPlayer.Play(ScreenAnimations.NinjaIdle);
		_yamoPlayer.Play(ScreenAnimations.YamoIdle);
		_lanternPlayer.Play(ScreenAnimations.Lantern);
	}

	public void Activate()
	{
	}

	public void Deactivate()
	{
	}

	public void Update(GameTime gameTime, InputSystem input)
	{
		var seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_player.Update(input, seconds);
		_ninjaPlayer.Update(seconds);
		_yamoPlayer.Update(seconds);
		_lanternPlayer.Update(seconds);

		if (input.WasPressed(InputAction.Pause))
		{
			_screens.Push(new PauseScreen(_screens, _context));
		}
		else if (input.WasPressed(InputAction.DebugGameOver))
		{
			_screens.Push(new GameOverScreen(_screens, _context));
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(
			_context.Pixel,
			new Rectangle(0, 0, GameInfo.VirtualWidth, GameInfo.VirtualHeight),
			null,
			Palette.Background,
			0f,
			Vector2.Zero,
			SpriteEffects.None,
			SpriteLayers.Background);

		DrawGeometry(spriteBatch);
		DrawLanterns(spriteBatch);
		DrawEnemies(spriteBatch);
		DrawPlayer(spriteBatch);
		DrawHud(spriteBatch);

		if (_context.Debug.OverlayVisible)
		{
			foreach (var platform in _geometry.Platforms)
			{
				_context.Debug.AddSolid(ToRectangle(platform.Bounds));
			}

			var playerBounds = _player.Body.Bounds;
			_context.Debug.AddHitbox(ToRectangle(playerBounds));
		}
	}

	private void DrawGeometry(SpriteBatch spriteBatch)
	{
		foreach (var platform in _chamber.Platforms)
		{
			var tile = platform.OneWay ? 1 : 0;
			for (var y = platform.Y; y < platform.Y + platform.Height; y += 8)
			{
				for (var x = platform.X; x < platform.X + platform.Width; x += 8)
				{
					_context.Tiles.Draw(spriteBatch, tile, new Vector2(x, y), 1, Color.White, SpriteEffects.None, SpriteLayers.World);
				}
			}
		}

		foreach (var vine in _chamber.Vines)
		{
			if (vine.Orientation == "horizontal")
			{
				for (var x = vine.X; x < vine.X + vine.Width; x += 8)
				{
					_context.Tiles.Draw(spriteBatch, 2, new Vector2(x, vine.Y), 1, Color.White, SpriteEffects.None, SpriteLayers.World);
				}
			}
			else
			{
				for (var y = vine.Y; y < vine.Y + vine.Height; y += 8)
				{
					_context.Tiles.Draw(spriteBatch, 2, new Vector2(vine.X, y), 1, Color.White, SpriteEffects.None, SpriteLayers.World);
				}
			}
		}
	}

	private void DrawLanterns(SpriteBatch spriteBatch)
	{
		foreach (var lantern in _chamber.Lanterns)
		{
			_context.LanternSheet.Draw(
				spriteBatch,
				_lanternPlayer.CurrentFrame,
				new Vector2(lantern.X - 8, lantern.Y - 8),
				1,
				Color.White,
				SpriteEffects.None,
				SpriteLayers.World);
		}
	}

	private void DrawEnemies(SpriteBatch spriteBatch)
	{
		foreach (var spawn in _chamber.EnemySpawns)
		{
			if (spawn.Type == "ninja")
			{
				_context.Ninja.Draw(
					spriteBatch,
					_ninjaPlayer.CurrentFrame,
					new Vector2(spawn.X, spawn.Y),
					1,
					Color.White,
					SpriteEffects.None,
					SpriteLayers.World);
			}
			else
			{
				_context.Yamo.Draw(
					spriteBatch,
					_yamoPlayer.CurrentFrame,
					new Vector2(spawn.X, spawn.Y),
					1,
					Color.White,
					SpriteEffects.None,
					SpriteLayers.World);
			}
		}
	}

	private void DrawPlayer(SpriteBatch spriteBatch)
	{
		var position = new Vector2(
			_player.Body.Position.X - 3,
			_player.Body.Position.Y - 2);
		var effects = _player.FacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		_context.Bruce.Draw(
			spriteBatch,
			_player.CurrentFrame,
			position,
			1,
			Color.White,
			effects,
			SpriteLayers.World);
	}

	private void DrawHud(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(
			_context.Pixel,
			new Rectangle(0, 0, GameInfo.VirtualWidth, 12),
			null,
			Color.Black,
			0f,
			Vector2.Zero,
			SpriteEffects.None,
			SpriteLayers.Hud);

		spriteBatch.Draw(_context.HudLife, new Rectangle(4, 2, 8, 8), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.Hud);
		spriteBatch.Draw(_context.HudLantern, new Rectangle(16, 2, 8, 8), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "000000", new Vector2(30, 2), Palette.White, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "FALLS 5", new Vector2(196, 2), Palette.White, layerDepth: SpriteLayers.Hud);

		spriteBatch.DrawString(_font16, "CHAMBER 1", new Vector2(8, 16), Palette.Yellow, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, $"{_player.State}", new Vector2(8, 36), Palette.Cyan, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "P: pause   G: game over", new Vector2(8, 176), Palette.Muted, layerDepth: SpriteLayers.Hud);
	}

	private static Rectangle ToRectangle(Aabb bounds)
	{
		return new Rectangle(
			(int)bounds.X,
			(int)bounds.Y,
			(int)bounds.Width,
			(int)bounds.Height);
	}
}
