using Microsoft.Xna.Framework;

namespace VibeBruceLee.Core.Debugging;

public sealed class DebugState
{
	private readonly List<DebugRect> _rects = [];

	public bool OverlayVisible { get; set; }

	public bool SlowMotion { get; set; }

	public bool Paused { get; set; }

	public bool Invincible { get; set; }

	public bool SkipChamberRequested { get; set; }

	public IReadOnlyList<DebugRect> Rects => _rects;

	public void BeginFrame()
	{
		_rects.Clear();
	}

	public void AddHitbox(Rectangle bounds) => Add(bounds, new Color(255, 80, 80));

	public void AddHurtbox(Rectangle bounds) => Add(bounds, new Color(80, 255, 80));

	public void AddSolid(Rectangle bounds) => Add(bounds, new Color(80, 180, 255));

	public bool ConsumeSkipChamber()
	{
		if (!SkipChamberRequested)
		{
			return false;
		}

		SkipChamberRequested = false;
		return true;
	}

	private void Add(Rectangle bounds, Color color)
	{
		_rects.Add(new DebugRect(bounds, color));
	}
}

public readonly record struct DebugRect(Rectangle Bounds, Color Color);
