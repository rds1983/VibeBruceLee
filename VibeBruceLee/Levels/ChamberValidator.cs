namespace VibeBruceLee.Levels;

public static class ChamberValidator
{
	private static readonly string[] EnemyTypes = ["ninja", "yamo"];

	private static readonly string[] HazardTypes =
	[
		"electric",
		"mine",
		"moving-wall",
		"floor-light",
		"dagger",
		"bug",
		"bush",
		"fireball"
	];

	private static readonly string[] VineOrientations = ["vertical", "horizontal"];

	public static IReadOnlyList<string> Validate(ChamberDefinition chamber)
	{
		ArgumentNullException.ThrowIfNull(chamber);

		var errors = new List<string>();

		if (string.IsNullOrWhiteSpace(chamber.Id))
		{
			errors.Add("id is required.");
		}

		if (string.IsNullOrWhiteSpace(chamber.Name))
		{
			errors.Add("name is required.");
		}

		if (chamber.Width != GameInfo.VirtualWidth || chamber.Height != GameInfo.VirtualHeight)
		{
			errors.Add($"size must be {GameInfo.VirtualWidth}x{GameInfo.VirtualHeight}.");
		}

		if (chamber.PlayerSpawn is null)
		{
			errors.Add("playerSpawn is required.");
		}
		else
		{
			ValidatePoint(errors, "playerSpawn", chamber.PlayerSpawn.X, chamber.PlayerSpawn.Y, chamber);
		}

		if (chamber.Platforms is null)
		{
			errors.Add("platforms must be an array.");
		}
		else
		{
			for (var i = 0; i < chamber.Platforms.Count; i++)
			{
				var platform = chamber.Platforms[i];
				if (platform is null)
				{
					errors.Add($"platforms[{i}] must be an object.");
					continue;
				}

				ValidateRect(errors, $"platforms[{i}]", platform.X, platform.Y, platform.Width, platform.Height, chamber);
			}
		}

		if (chamber.Vines is null)
		{
			errors.Add("vines must be an array.");
		}
		else
		{
			for (var i = 0; i < chamber.Vines.Count; i++)
			{
				var vine = chamber.Vines[i];
				if (vine is null)
				{
					errors.Add($"vines[{i}] must be an object.");
					continue;
				}

				if (Array.IndexOf(VineOrientations, vine.Orientation) < 0)
				{
					errors.Add($"vines[{i}].orientation must be one of: {string.Join(", ", VineOrientations)}.");
				}

				ValidateRect(errors, $"vines[{i}]", vine.X, vine.Y, vine.Width, vine.Height, chamber);
			}
		}

		if (chamber.Lanterns is null)
		{
			errors.Add("lanterns must be an array.");
		}
		else
		{
			var lanternIds = new HashSet<string>(StringComparer.Ordinal);
			for (var i = 0; i < chamber.Lanterns.Count; i++)
			{
				var lantern = chamber.Lanterns[i];
				if (lantern is null)
				{
					errors.Add($"lanterns[{i}] must be an object.");
					continue;
				}

				if (string.IsNullOrWhiteSpace(lantern.Id))
				{
					errors.Add($"lanterns[{i}].id is required.");
				}
				else if (!lanternIds.Add(lantern.Id))
				{
					errors.Add($"lanterns[{i}].id '{lantern.Id}' is duplicated.");
				}

				ValidatePoint(errors, $"lanterns[{i}]", lantern.X, lantern.Y, chamber);
			}
		}

		if (chamber.EnemySpawns is null)
		{
			errors.Add("enemySpawns must be an array.");
		}
		else
		{
			for (var i = 0; i < chamber.EnemySpawns.Count; i++)
			{
				var spawn = chamber.EnemySpawns[i];
				if (spawn is null)
				{
					errors.Add($"enemySpawns[{i}] must be an object.");
					continue;
				}

				if (Array.IndexOf(EnemyTypes, spawn.Type) < 0)
				{
					errors.Add($"enemySpawns[{i}].type must be one of: {string.Join(", ", EnemyTypes)}.");
				}

				ValidatePoint(errors, $"enemySpawns[{i}]", spawn.X, spawn.Y, chamber);
			}
		}

		if (chamber.Hazards is null)
		{
			errors.Add("hazards must be an array.");
		}
		else
		{
			for (var i = 0; i < chamber.Hazards.Count; i++)
			{
				var hazard = chamber.Hazards[i];
				if (hazard is null)
				{
					errors.Add($"hazards[{i}] must be an object.");
					continue;
				}

				if (Array.IndexOf(HazardTypes, hazard.Type) < 0)
				{
					errors.Add($"hazards[{i}].type must be one of: {string.Join(", ", HazardTypes)}.");
				}

				if (hazard.PeriodSeconds is <= 0)
				{
					errors.Add($"hazards[{i}].periodSeconds must be positive.");
				}

				if (hazard.Speed is < 0)
				{
					errors.Add($"hazards[{i}].speed must not be negative.");
				}

				ValidateRect(errors, $"hazards[{i}]", hazard.X, hazard.Y, hazard.Width, hazard.Height, chamber);
			}
		}

		if (chamber.Exits is null)
		{
			errors.Add("exits must be an array.");
		}
		else
		{
			if (chamber.Exits.Count == 0)
			{
				errors.Add("at least one exit is required.");
			}

			for (var i = 0; i < chamber.Exits.Count; i++)
			{
				var exit = chamber.Exits[i];
				if (exit is null)
				{
					errors.Add($"exits[{i}] must be an object.");
					continue;
				}

				if (string.IsNullOrWhiteSpace(exit.Target))
				{
					errors.Add($"exits[{i}].target is required.");
				}

				ValidateRect(errors, $"exits[{i}]", exit.X, exit.Y, exit.Width, exit.Height, chamber);
			}
		}

		if (chamber.Decor is null)
		{
			errors.Add("decor must be an array.");
		}
		else
		{
			for (var i = 0; i < chamber.Decor.Count; i++)
			{
				var decor = chamber.Decor[i];
				if (decor is null)
				{
					errors.Add($"decor[{i}] must be an object.");
					continue;
				}

				if (string.IsNullOrWhiteSpace(decor.Sprite))
				{
					errors.Add($"decor[{i}].sprite is required.");
				}

				if (decor.Scale < 1)
				{
					errors.Add($"decor[{i}].scale must be at least 1.");
				}

				ValidatePoint(errors, $"decor[{i}]", decor.X, decor.Y, chamber);
			}
		}

		return errors;
	}

	public static IReadOnlyList<string> ValidateSet(IReadOnlyList<ChamberDefinition> chambers)
	{
		ArgumentNullException.ThrowIfNull(chambers);

		var errors = new List<string>();
		var ids = new HashSet<string>(StringComparer.Ordinal);

		foreach (var chamber in chambers)
		{
			if (string.IsNullOrWhiteSpace(chamber.Id))
			{
				continue;
			}

			if (!ids.Add(chamber.Id))
			{
				errors.Add($"chamber id '{chamber.Id}' is duplicated.");
			}
		}

		foreach (var chamber in chambers)
		{
			if (chamber.Exits is null)
			{
				continue;
			}

			for (var i = 0; i < chamber.Exits.Count; i++)
			{
				var exit = chamber.Exits[i];
				if (exit is null || string.IsNullOrWhiteSpace(exit.Target))
				{
					continue;
				}

				if (!ids.Contains(exit.Target))
				{
					errors.Add($"exits[{i}] targets unknown chamber '{exit.Target}'.");
				}
			}
		}

		return errors;
	}

	private static void ValidateRect(
		List<string> errors,
		string path,
		int x,
		int y,
		int width,
		int height,
		ChamberDefinition chamber)
	{
		if (width <= 0 || height <= 0)
		{
			errors.Add($"{path}: width and height must be positive.");
			return;
		}

		if (x < 0 || y < 0 || x + width > chamber.Width || y + height > chamber.Height)
		{
			errors.Add($"{path}: rectangle ({x}, {y}, {width}, {height}) is outside {chamber.Width}x{chamber.Height}.");
		}
	}

	private static void ValidatePoint(List<string> errors, string path, int x, int y, ChamberDefinition chamber)
	{
		if (x < 0 || y < 0 || x >= chamber.Width || y >= chamber.Height)
		{
			errors.Add($"{path}: point ({x}, {y}) is outside {chamber.Width}x{chamber.Height}.");
		}
	}
}
