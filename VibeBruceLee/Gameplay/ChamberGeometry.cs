using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// Converts validated chamber data into physics primitives.
/// </summary>
public sealed class ChamberGeometry
{
	private ChamberGeometry(IReadOnlyList<Platform> platforms, IReadOnlyList<Vine> vines)
	{
		Platforms = platforms;
		Vines = vines;
	}

	public IReadOnlyList<Platform> Platforms { get; }

	public IReadOnlyList<Vine> Vines { get; }

	public static ChamberGeometry From(ChamberDefinition chamber)
	{
		ArgumentNullException.ThrowIfNull(chamber);

		var platforms = new List<Platform>(chamber.Platforms.Count);
		foreach (var platform in chamber.Platforms)
		{
			platforms.Add(new Platform(
				new Aabb(platform.X, platform.Y, platform.Width, platform.Height),
				platform.OneWay));
		}

		var vines = new List<Vine>(chamber.Vines.Count);
		foreach (var vine in chamber.Vines)
		{
			var orientation = vine.Orientation == "horizontal"
				? VineOrientation.Horizontal
				: VineOrientation.Vertical;

			vines.Add(new Vine(
				new Aabb(vine.X, vine.Y, vine.Width, vine.Height),
				orientation));
		}

		return new ChamberGeometry(platforms, vines);
	}
}
