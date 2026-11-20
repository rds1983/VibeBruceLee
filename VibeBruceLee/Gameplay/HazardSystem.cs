using VibeBruceLee.Levels;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Builds, updates and resets the hazards of one chamber. Damage is resolved
/// here so every hazard type shares the same player/enemy rules.
/// </summary>
public sealed class HazardSystem
{
	private readonly IReadOnlyList<HazardDefinition> _definitions;
	private readonly List<Hazard> _hazards = [];

	public HazardSystem(IReadOnlyList<HazardDefinition> definitions)
	{
		ArgumentNullException.ThrowIfNull(definitions);

		_definitions = definitions;
		Reset();
	}

	public IReadOnlyList<Hazard> Hazards => _hazards;

	public void Add(Hazard hazard)
	{
		ArgumentNullException.ThrowIfNull(hazard);
		_hazards.Add(hazard);
	}

	public void Clear()
	{
		_hazards.Clear();
	}

	public void Update(float deltaSeconds, Player player, IReadOnlyList<Enemy> enemies)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);
		ArgumentNullException.ThrowIfNull(player);
		ArgumentNullException.ThrowIfNull(enemies);

		for (var i = _hazards.Count - 1; i >= 0; i--)
		{
			var hazard = _hazards[i];
			hazard.Update(deltaSeconds);

			if (hazard.IsActive)
			{
				if (!player.IsDead && player.Hurtbox.Intersects(hazard.Bounds))
				{
					player.Kill();
				}

				foreach (var enemy in enemies)
				{
					if (!enemy.CanBeHit || !enemy.Hurtbox.Intersects(hazard.Bounds))
					{
						continue;
					}

					enemy.ApplyHazard(enemy.Body.Bounds.Center.X >= hazard.Bounds.Center.X);
				}
			}
			else
			{
				if (!player.IsDead && player.Hurtbox.Intersects(hazard.Bounds))
				{
					hazard.OnOverlap();
				}

				foreach (var enemy in enemies)
				{
					if (enemy.CanBeHit && enemy.Hurtbox.Intersects(hazard.Bounds))
					{
						hazard.OnOverlap();
					}
				}
			}

			if (hazard.IsFinished)
			{
				_hazards.RemoveAt(i);
			}
		}
	}

	public void Reset()
	{
		_hazards.Clear();

		foreach (var definition in _definitions)
		{
			_hazards.Add(Create(definition));
		}
	}

	private static Hazard Create(HazardDefinition definition)
	{
		return definition.Type switch
		{
			"electric" => new TimedBeamHazard(definition, HazardKind.Electric),
			"floor-light" => new TimedBeamHazard(definition, HazardKind.FloorLight),
			"bush" => new BushHazard(definition),
			"mine" => new MineHazard(definition),
			"moving-wall" => new MovingWallHazard(definition),
			"dagger" => new ProjectileHazard(definition, HazardKind.Dagger),
			"sword" => new ProjectileHazard(definition, HazardKind.Sword),
			"bug" => new ProjectileHazard(definition, HazardKind.Bug),
			"fireball" => new ProjectileHazard(definition, HazardKind.Fireball),
			_ => throw new ArgumentOutOfRangeException(nameof(definition), $"Unknown hazard type '{definition.Type}'.")
		};
	}
}
