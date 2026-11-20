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
	private readonly EnemyEncounter _encounter;
	private readonly ChamberProgress _progress;
	private readonly HubProgress _hub;
	private readonly WizardBoss? _wizard;
	private readonly bool _isHubChamber;
	private readonly string _title;
	private readonly Vector2 _spawn;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;
	private readonly IReadOnlyList<Enemy> _enemies;
	private readonly HazardSystem _hazards;
	private readonly AnimationPlayer _lanternPlayer = new();
	private readonly LandingDust _dust = new();

	public ChamberScreen(ScreenStack screens, ScreenContext context, ChamberDefinition chamber)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(chamber);

		_screens = screens;
		_context = context;
		_chamber = chamber;
		_progress = new ChamberProgress(
			_chamber,
			_chamber.Hub ? context.Session.Hub.GetLanternSet(_chamber.Id) : null);
		_hub = context.Session.Hub;
		_isHubChamber = _chamber.Hub;
		_wizard = _chamber.Wizard is WizardDefinition wizard ? new WizardBoss(wizard) : null;
		_title = _chamber.Name.ToUpperInvariant();
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
		_encounter = new EnemyEncounter(_player, _enemies, _context.Session);
		_hazards = new HazardSystem(_chamber.Hazards);

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
		_encounter.Update(input, seconds);
		_wizard?.Update(seconds, _player.Body.Bounds, _hazards);
		_hazards.Update(seconds, _player, _enemies);

		if (_wizard is not null)
		{
			if (!_player.IsDead &&
				_wizard.TryPressButton(_player.Body.Bounds, input.WasPressed(InputAction.Attack)))
			{
				_context.Session.AddScore(WizardBoss.Score);
				_hazards.Clear();
			}

			if (_wizard.IsDefeatFinished)
			{
				_screens.Push(new EndingScreen(_screens, _context));
				return;
			}
		}

		if (!_player.IsDead)
		{
			var collected = _progress.Collect(_player.Body.Bounds);
			if (collected > 0)
			{
				_context.Session.AddScore(collected * ChamberProgress.LanternScore);
			}

			var hubHatchOpen = _hub.AllRequiredCollected(_context.Chambers);
			if (_progress.TryFindOpenExit(_player.Body.Bounds, hubHatchOpen, out var exit))
			{
				_context.Session.AddScore(ChamberProgress.ExitScore);
				_screens.Replace(new ChamberScreen(
					_screens,
					_context,
					_context.Chambers.Get(exit!.Target)));
				return;
			}
		}

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
				ResetChamber();
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
		DrawExits(spriteBatch);
		DrawHazards(spriteBatch);
		DrawWizard(spriteBatch);
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

			foreach (var hazard in _hazards.Hazards)
			{
				if (hazard.IsActive)
				{
					_context.Debug.AddHitbox(ToRectangle(hazard.Bounds));
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

	private void ResetChamber()
	{
		_progress.Reset();

		foreach (var enemy in _enemies)
		{
			enemy.Respawn();
		}

		_encounter.Reset();
		_hazards.Reset();
		_wizard?.Reset();
		_dust.Clear();
	}

	private void DrawExits(SpriteBatch spriteBatch)
	{
		var hubHatchOpen = _hub.AllRequiredCollected(_context.Chambers);

		foreach (var exit in _chamber.Exits)
		{
			var open = _progress.IsExitOpen(exit, hubHatchOpen);
			var color = open ? Palette.Yellow : new Color(80, 60, 40);

			spriteBatch.Draw(
				_context.Pixel,
				new Rectangle(exit.X, exit.Y, exit.Width, exit.Height),
				null,
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				SpriteLayers.World);

			if (open && exit.Width >= 4)
			{
				spriteBatch.Draw(
					_context.Pixel,
					new Rectangle(exit.X + 2, exit.Y + 2, Math.Max(1, exit.Width - 4), Math.Max(1, exit.Height - 4)),
					null,
					Palette.Cyan,
					0f,
					Vector2.Zero,
					SpriteEffects.None,
					SpriteLayers.World);
			}
		}
	}

	private void DrawHazards(SpriteBatch spriteBatch)
	{
		foreach (var hazard in _hazards.Hazards)
		{
			if (!hazard.IsVisible)
			{
				continue;
			}

			spriteBatch.Draw(
				_context.Pixel,
				new Rectangle(
					(int)hazard.Bounds.X,
					(int)hazard.Bounds.Y,
					(int)hazard.Bounds.Width,
					(int)hazard.Bounds.Height),
				null,
				GetHazardColor(hazard),
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				SpriteLayers.World);
		}
	}

	private static Color GetHazardColor(Hazard hazard)
	{
		if (hazard.IsWarning)
		{
			return new Color(255, 150, 40);
		}

		if (!hazard.IsActive)
		{
			return hazard.Kind switch
			{
				HazardKind.Electric => new Color(0, 60, 60),
				HazardKind.FloorLight => new Color(80, 70, 0),
				HazardKind.Mine => new Color(120, 40, 40),
				_ => new Color(70, 70, 70)
			};
		}

		return hazard.Kind switch
		{
			HazardKind.Electric => Palette.Cyan,
			HazardKind.FloorLight => Palette.Yellow,
			HazardKind.Bush => new Color(80, 200, 80),
			HazardKind.Mine => new Color(255, 160, 0),
			HazardKind.MovingWall => new Color(160, 160, 160),
			HazardKind.Dagger => new Color(200, 200, 200),
			HazardKind.Sword => Palette.White,
			HazardKind.Bug => new Color(100, 220, 60),
			HazardKind.Fireball => new Color(255, 90, 20),
			_ => Palette.White
		};
	}

	private void DrawWizard(SpriteBatch spriteBatch)
	{
		if (_wizard is null)
		{
			return;
		}

		var bounds = _wizard.Bounds;
		var head = _wizard.IsDefeated ? new Color(220, 60, 60) : new Color(90, 30, 120);
		var hat = new Color(40, 10, 60);
		var eye = _wizard.IsWarning || _wizard.IsFiring ? Palette.Yellow : Palette.White;

		spriteBatch.Draw(_context.Pixel, new Rectangle((int)bounds.X - 4, (int)bounds.Y - 4, (int)bounds.Width + 8, 4), null, hat, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);
		spriteBatch.Draw(_context.Pixel, new Rectangle((int)bounds.X + 6, (int)bounds.Y - 12, (int)bounds.Width - 12, 8), null, hat, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);
		spriteBatch.Draw(_context.Pixel, new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height), null, head, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);

		var leftEyeX = (int)(bounds.X + (bounds.Width * 0.32f)) - 2;
		var rightEyeX = (int)(bounds.X + (bounds.Width * 0.68f)) - 2;
		var eyeY = (int)(bounds.Y + (bounds.Height * 0.35f));
		spriteBatch.Draw(_context.Pixel, new Rectangle(leftEyeX, eyeY, 4, 4), null, eye, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);
		spriteBatch.Draw(_context.Pixel, new Rectangle(rightEyeX, eyeY, 4, 4), null, eye, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);
		spriteBatch.Draw(_context.Pixel, new Rectangle((int)bounds.X + 8, (int)bounds.Y + (int)(bounds.Height * 0.7f), (int)bounds.Width - 16, 3), null, hat, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);

		var button = _wizard.ButtonBounds;
		var buttonColor = _wizard.IsDefeated ? new Color(80, 200, 80) : new Color(200, 40, 40);
		spriteBatch.Draw(_context.Pixel, new Rectangle((int)button.X, (int)button.Y, (int)button.Width, (int)button.Height), null, buttonColor, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);
		spriteBatch.Draw(_context.Pixel, new Rectangle((int)button.X + 2, (int)button.Y + 2, Math.Max(1, (int)button.Width - 4), Math.Max(1, (int)button.Height - 4)), null, _wizard.IsDefeated ? Palette.Yellow : Palette.White, 0f, Vector2.Zero, SpriteEffects.None, SpriteLayers.World);
	}

	private void DrawLanterns(SpriteBatch spriteBatch)
	{
		foreach (var lantern in _chamber.Lanterns)
		{
			if (_progress.IsCollected(lantern))
			{
				continue;
			}

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
		spriteBatch.DrawString(
			_font8,
			_chamber.Boss
				? "BOSS"
				: "L " + _progress.RequiredCollected.ToString(CultureInfo.InvariantCulture) + "/" + _progress.RequiredTotal.ToString(CultureInfo.InvariantCulture),
			new Vector2(136, 2),
			Palette.White,
			layerDepth: SpriteLayers.Hud);
		spriteBatch.DrawString(_font8, "FALLS " + _context.Session.Falls.ToString(CultureInfo.InvariantCulture), new Vector2(184, 2), Palette.White, layerDepth: SpriteLayers.Hud);

		spriteBatch.DrawString(_font16, _title, new Vector2(8, 16), Palette.Yellow, layerDepth: SpriteLayers.Hud);

		if (_wizard is not null)
		{
			if (_wizard.IsDefeated)
			{
				spriteBatch.DrawString(_font8, "WIZARD DESTROYED", new Vector2(8, 36), Palette.Yellow, layerDepth: SpriteLayers.Hud);
			}
			else if (_wizard.PlayerAtButton(_player.Body.Bounds))
			{
				spriteBatch.DrawString(_font8, "PRESS ATTACK", new Vector2(8, 36), Palette.Yellow, layerDepth: SpriteLayers.Hud);
			}
			else
			{
				spriteBatch.DrawString(_font8, "ATTACK THE BUTTON", new Vector2(8, 36), Palette.Cyan, layerDepth: SpriteLayers.Hud);
			}
		}
		else if (_isHubChamber)
		{
			if (_hub.AllRequiredCollected(_context.Chambers))
			{
				spriteBatch.DrawString(_font8, "HATCH OPEN", new Vector2(8, 36), Palette.Yellow, layerDepth: SpriteLayers.Hud);
			}
			else
			{
				spriteBatch.DrawString(
					_font8,
					"HUB " + _hub.RequiredCollected(_context.Chambers).ToString(CultureInfo.InvariantCulture) +
						"/" + HubProgress.RequiredTotal(_context.Chambers).ToString(CultureInfo.InvariantCulture),
					new Vector2(8, 36),
					Palette.Cyan,
					layerDepth: SpriteLayers.Hud);
			}
		}
		else if (_progress.RequiredComplete)
		{
			spriteBatch.DrawString(_font8, "EXIT OPEN", new Vector2(8, 36), Palette.Yellow, layerDepth: SpriteLayers.Hud);
		}
		else
		{
			spriteBatch.DrawString(_font8, $"{_player.State}", new Vector2(8, 36), Palette.Cyan, layerDepth: SpriteLayers.Hud);
		}

		spriteBatch.DrawString(_font8, "P: pause", new Vector2(8, 176), Palette.Muted, layerDepth: SpriteLayers.Hud);
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
