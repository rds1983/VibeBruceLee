using VibeBruceLee.Levels;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

/// <summary>
/// One placed hazard. Subclasses own timing and movement; the chamber's
/// <see cref="HazardSystem"/> resolves damage against the player and enemies.
/// </summary>
public abstract class Hazard
{
	protected Hazard(HazardDefinition definition, HazardKind kind)
	{
		ArgumentNullException.ThrowIfNull(definition);

		Definition = definition;
		Kind = kind;
		Bounds = new Aabb(definition.X, definition.Y, definition.Width, definition.Height);
	}

	public HazardDefinition Definition { get; }

	public HazardKind Kind { get; }

	public Aabb Bounds { get; protected set; }

	public bool IsActive { get; protected set; }

	public bool IsWarning { get; protected set; }

	public bool IsVisible { get; protected set; } = true;

	public bool IsFinished { get; protected set; }

	public abstract void Update(float deltaSeconds);

	/// <summary>
	/// Called when a target overlaps a non-active hazard. Mines use this to
	/// start their fuse; hazard types that do not react ignore it.
	/// </summary>
	public virtual void OnOverlap()
	{
	}

	protected static float PositiveSeconds(double? value, float fallback)
	{
		return value is > 0 ? (float)value.Value : fallback;
	}
}
