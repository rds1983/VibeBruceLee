using System.Globalization;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Animation;
using VibeBruceLee.Effects;
using VibeBruceLee.Gameplay;
using VibeBruceLee.Input;
using VibeBruceLee.Levels;
using VibeBruceLee.Physics;
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
	private readonly Vector2 _spawn;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly IReadOnlyList<Enemy> _enemies;
	private readonly AnimationPlayer _lanternPlayer = new();
	private readonly LandingDust _dust = new();

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
		_spawn = new Vector2(spawn.X, spawn.Y);
		_player = new Player(
			_world,
			_spawn,
			_geometry.Platforms,
			_geometry.Vines);

		var enemies = new List<Enemy>(_chamber.EnemySpawns.Count);
		var random = new SeededRandom(0x5EED);
		foreach (var enemySpawn in _chamber.EnemySpawns)
		{
			var definition = EnemyDefinitions.Get(enemySpawn.Type);
			var centerX = enemySpawn.X + (definition.BodyWidth * 0.5f);
			var groundY = FindGroundBelow(centerX, enemySpawn.Y);
			var position = new Vector2(enemySpawn.X, groundY - definition.BodyHeight);
			enemies.Add(new Enemy(
				definition,
				position,
				_world,
				_geometry.Platforms,
				_geometry.Vines,
				random));
		}

		_enemies = enemies;

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
		_player.Invincible = _context.Debug.Invincible;
		_player.Update(input, seconds);

		var targetFeet = _player.IsDead
			? (Vector2?)null
			: new Vector2(
				_player.Body.Position.X + (_player.Body.Size.X * 0.5f),
				_player.Body.Position.Y + _player.Body.Size.Y);

		foreach (var enemy in _enemies)
		{
			if (enemy.IsVisible && enemy.BlocksMovement)
			{
				_player.ResolveBodyBlock(enemy.Body.Bounds);
			}
		}

		foreach (var enemy in _enemies)
		{
			enemy.Update(targetFeet, seconds);
		}

		ResolveEnemyCombat();

		if (_player.ConsumeLanded())
		{
			var feet = new Vector2(
				_player.Body.Position.X + (_player.Body.Size.X / 2f),
				_player.Body.Position.Y + _player.Body.Size.Y);
			_dust.Spawn(feet);
		}

		_dust.Update(seconds);

		if (_player.ConsumeDeathFinished())
		{
			if (_context.Session.LoseFall())
			{
				_screens.Push(new GameOverScreen(_screens, _context));
			}
			else
			{
				_player.Respawn(_spawn);
			}
		}

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
		_dust.Draw(spriteBatch, _context.Pixel, SpriteLayers.World);
		DrawHud(spriteBatch);

		if (_context.Debug.OverlayVisible)
		{
			foreach (var platform in _geometry.Platforms)
			{
				_context.Debug.AddSolid(ToRectangle(platform.Bounds));
			}

			var playerBounds = _player.Body.Bounds;
			_context.Debug.AddHurtbox(ToRectangle(playerBounds));

			if (_player.TryGetAttackHitbox(out var attackBox))
			{
				_context.Debug.AddHitbox(ToRectangle(attackBox));
			}

			foreach (var enemy in _enemies)
			{
				if (!enemy.IsVisible)
				{
					continue;
				}

				_context.Debug.AddHurtbox(ToRectangle(enemy.Hurtbox));
				if (enemy.TryGetAttackHitbox(out var enemyAttackBox))
				{
					_context.Debug.AddHitbox(ToRectangle(enemyAttackBox));
				}
			}
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
		foreach (var enemy in _enemies)
		{
			if (!enemy.IsVisible)
			{
				continue;
			}

			var sheet = enemy.Definition.Type == "ninja" ? _context.Ninja : _context.Yamo;
			var position = new Vector2(
				enemy.Body.Position.X + (enemy.Body.Size.X * 0.5f) - (sheet.Layout.FrameWidth * 0.5f),
				enemy.Body.Position.Y);
			var effects = enemy.FacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			var tint = enemy.IsFlashing ? new Color(255, 140, 140) : Color.White;

			sheet.Draw(
				spriteBatch,
				enemy.CurrentFrame,
				position,
				1,
				tint,
				effects,
				SpriteLayers.World);
		}
	}

	private void ResolveEnemyCombat()
	{
		foreach (var enemy in _enemies)
		{
			if (!enemy.IsVisible)
			{
				continue;
			}

			var hit = _player.TryHit(enemy.Hurtbox);
			if (hit is not null)
			{
				var fromLeft = _player.Body.Bounds.Center.X <= enemy.Body.Bounds.Center.X;
				var defeated = enemy.ApplyHit(hit, fromLeft);
				_context.Session.AddScore(hit.Score);

				if (defeated)
				{
					_context.Session.AddScore(enemy.Definition.KnockoutScore);
				}
			}

			if (_player.IsDead || _player.IsInvulnerable)
			{
				continue;
			}

			var enemyHit = enemy.TryHit(_player.Hurtbox);
			if (enemyHit is not null)
			{
				_player.ApplyHit(enemyHit, enemy.Body.Bounds.Center.X <= _player.Body.Bounds.Center.X);
			}
		}
	}

	private float FindGroundBelow(float x, float fromY)
	{
		var ground = (float)GameInfo.VirtualHeight;

		foreach (var platform in _geometry.Platforms)
		{
			if (x >= platform.Bounds.X &&
				x <= platform.Bounds.Right &&
				platform.Bounds.Y >= fromY &&
				platform.Bounds.Y < ground)
			{
				ground = platform.Bounds.Y;
			}
		}

		return ground;
	}

	private void DrawPlayer(SpriteBatch spriteBatch)
	{
		var position = new Vector2(
			_player.Body.Position.X - 3,
			_player.Body.Position.Y);
		var effects = _player.FacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		var tint = _player.IsFlashing ? new Color(255, 140, 140) : Color.White;

		_context.Bruce.Draw(
			spriteBatch,
			_player.CurrentFrame,
			position,
			1,
			tint,
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
		spriteBatch.DrawString(_font8, _context.Session.Score.ToString("D6", CultureInfo.InvariantCulture), new Vector2(30, 2), Palette.White, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "HP " + _player.BlowsRemaining.ToString(CultureInfo.InvariantCulture), new Vector2(96, 2), Palette.White, layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "FALLS " + _context.Session.Falls.ToString(CultureInfo.InvariantCulture), new Vector2(184, 2), Palette.White, layerDepth: SpriteLayers.Hud);

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
