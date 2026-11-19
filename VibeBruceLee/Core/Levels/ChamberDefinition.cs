namespace VibeBruceLee.Core.Levels;

public sealed class ChamberDefinition
{
	public string Id { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;

	public int Width { get; set; } = GameInfo.VirtualWidth;

	public int Height { get; set; } = GameInfo.VirtualHeight;

	public PointDefinition? PlayerSpawn { get; set; }

	public List<PlatformDefinition> Platforms { get; set; } = [];

	public List<VineDefinition> Vines { get; set; } = [];

	public List<LanternDefinition> Lanterns { get; set; } = [];

	public List<EnemySpawnDefinition> EnemySpawns { get; set; } = [];

	public List<HazardDefinition> Hazards { get; set; } = [];

	public List<ExitDefinition> Exits { get; set; } = [];

	public List<DecorDefinition> Decor { get; set; } = [];
}

public sealed class PointDefinition
{
	public int X { get; set; }

	public int Y { get; set; }
}

public sealed class PlatformDefinition
{
	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public bool OneWay { get; set; }
}

public sealed class VineDefinition
{
	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public string Orientation { get; set; } = "vertical";
}

public sealed class LanternDefinition
{
	public string Id { get; set; } = string.Empty;

	public int X { get; set; }

	public int Y { get; set; }

	public bool Required { get; set; } = true;
}

public sealed class EnemySpawnDefinition
{
	public string Type { get; set; } = string.Empty;

	public int X { get; set; }

	public int Y { get; set; }
}

public sealed class HazardDefinition
{
	public string Type { get; set; } = string.Empty;

	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public double? PeriodSeconds { get; set; }

	public double? Speed { get; set; }
}

public sealed class ExitDefinition
{
	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public string Target { get; set; } = string.Empty;

	public bool RequiresLanterns { get; set; } = true;
}

public sealed class DecorDefinition
{
	public string Sprite { get; set; } = string.Empty;

	public int X { get; set; }

	public int Y { get; set; }

	public int Scale { get; set; } = 1;
}
