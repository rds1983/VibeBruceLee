using Microsoft.Xna.Framework;
using VibeBruceLee.Physics;

namespace VibeBruceLee.Gameplay;

public enum EnemyWaypointKind
{
	Walk,
	Climb,
	Drop
}

/// <summary>
/// One step of an enemy route. <see cref="FeetY"/> is the platform surface the
/// enemy should finish on; a climb waypoint also carries the vine to use.
/// </summary>
public readonly record struct EnemyWaypoint(
	EnemyWaypointKind Kind,
	float X,
	float FeetY,
	Vine Vine = default);

public readonly record struct EnemySurface(int Index, float Left, float Right, float Top);

/// <summary>
/// Deterministic route planner over platform tops and vertical vines. Enemies
/// walk along the platform they stand on, climb vines to reach higher
/// platforms, and drop to lower ones. The planner never uses randomness, so
/// identical geometry always produces the same route.
/// </summary>
public sealed class EnemyNavigator
{
	private const float SurfaceTolerance = 6f;

	private readonly List<EnemySurface> _surfaces = [];
	private readonly List<NavigationEdge>[] _edges;

	private readonly record struct NavigationEdge(int To, Vine? Vine, bool Drop, float WalkX);

	public EnemyNavigator(IReadOnlyList<Platform> platforms, IReadOnlyList<Vine> vines)
	{
		ArgumentNullException.ThrowIfNull(platforms);
		ArgumentNullException.ThrowIfNull(vines);

		for (var i = 0; i < platforms.Count; i++)
		{
			var bounds = platforms[i].Bounds;
			_surfaces.Add(new EnemySurface(i, bounds.X, bounds.Right, bounds.Y));
		}

		_edges = new List<NavigationEdge>[_surfaces.Count];
		for (var i = 0; i < _edges.Length; i++)
		{
			_edges[i] = [];
		}

		ConnectSameLevel();
		ConnectVines(vines);
		ConnectDrops();
	}

	public EnemySurface? FindSurface(Vector2 feet)
	{
		EnemySurface? best = null;
		var bestDistance = float.MaxValue;

		foreach (var surface in _surfaces)
		{
			if (feet.X < surface.Left - 2f || feet.X > surface.Right + 2f)
			{
				continue;
			}

			var distance = MathF.Abs(surface.Top - feet.Y);
			if (distance <= SurfaceTolerance && distance < bestDistance)
			{
				best = surface;
				bestDistance = distance;
			}
		}

		return best;
	}

	/// <summary>
	/// Returns the waypoints to reach <paramref name="toFeet"/>, or an empty
	/// route when the target surface is not reachable with walking, vines and
	/// drops.
	/// </summary>
	public IReadOnlyList<EnemyWaypoint> Plan(Vector2 fromFeet, Vector2 toFeet)
	{
		var start = FindSurface(fromFeet);
		var target = FindSurface(toFeet);

		if (start is null)
		{
			return [];
		}

		if (target is null)
		{
			return [new EnemyWaypoint(EnemyWaypointKind.Walk, toFeet.X, start.Value.Top)];
		}

		if (start.Value.Index == target.Value.Index)
		{
			return [new EnemyWaypoint(EnemyWaypointKind.Walk, toFeet.X, start.Value.Top)];
		}

		var path = FindPath(start.Value.Index, target.Value.Index);
		if (path is null)
		{
			return [];
		}

		var waypoints = new List<EnemyWaypoint>();
		for (var i = 0; i < path.Count; i++)
		{
			var current = _surfaces[path[i].Surface];
			var next = _surfaces[path[i].Edge.To];
			var edge = path[i].Edge;

			if (edge.Vine is Vine vine)
			{
				var vineX = Math.Clamp(vine.Bounds.Center.X, current.Left, current.Right);
				waypoints.Add(new EnemyWaypoint(EnemyWaypointKind.Walk, vineX, current.Top));
				waypoints.Add(new EnemyWaypoint(EnemyWaypointKind.Climb, vineX, next.Top, vine));
			}
			else if (edge.Drop)
			{
				waypoints.Add(new EnemyWaypoint(
					EnemyWaypointKind.Drop,
					ChooseDropX(current, next, toFeet.X),
					current.Top));
			}
			else
			{
				waypoints.Add(new EnemyWaypoint(EnemyWaypointKind.Walk, edge.WalkX, next.Top));
			}
		}

		var finalX = Math.Clamp(toFeet.X, target.Value.Left, target.Value.Right);
		waypoints.Add(new EnemyWaypoint(EnemyWaypointKind.Walk, finalX, target.Value.Top));
		return waypoints;
	}

	private List<PathStep>? FindPath(int start, int target)
	{
		var previous = new PathStep?[_surfaces.Count];
		var visited = new bool[_surfaces.Count];
		var queue = new Queue<int>();
		queue.Enqueue(start);
		visited[start] = true;

		while (queue.Count > 0)
		{
			var node = queue.Dequeue();
			if (node == target)
			{
				break;
			}

			foreach (var edge in _edges[node])
			{
				if (visited[edge.To])
				{
					continue;
				}

				visited[edge.To] = true;
				previous[edge.To] = new PathStep(node, edge);
				queue.Enqueue(edge.To);
			}
		}

		if (!visited[target])
		{
			return null;
		}

		var path = new List<PathStep>();
		for (var node = target; node != start;)
		{
			var step = previous[node] ?? throw new InvalidOperationException("Broken navigation path.");
			path.Add(step);
			node = step.Surface;
		}

		path.Reverse();
		return path;
	}

	private void ConnectSameLevel()
	{
		for (var i = 0; i < _surfaces.Count; i++)
		{
			for (var j = i + 1; j < _surfaces.Count; j++)
			{
				var a = _surfaces[i];
				var b = _surfaces[j];
				if (MathF.Abs(a.Top - b.Top) > 1f)
				{
					continue;
				}

				var overlapLeft = MathF.Max(a.Left, b.Left);
				var overlapRight = MathF.Min(a.Right, b.Right);
				if (overlapRight < overlapLeft - 2f)
				{
					continue;
				}

				var walkX = overlapRight >= overlapLeft
					? (overlapLeft + overlapRight) * 0.5f
					: (a.Right + b.Left) * 0.5f;

				_edges[i].Add(new NavigationEdge(j, null, false, walkX));
				_edges[j].Add(new NavigationEdge(i, null, false, walkX));
			}
		}
	}

	private void ConnectVines(IReadOnlyList<Vine> vines)
	{
		foreach (var vine in vines)
		{
			if (vine.Orientation != VineOrientation.Vertical)
			{
				continue;
			}

			var centerX = vine.Bounds.Center.X;
			var touched = new List<EnemySurface>();
			foreach (var surface in _surfaces)
			{
				if (centerX < surface.Left - 1f || centerX > surface.Right + 1f)
				{
					continue;
				}

				if (surface.Top < vine.Bounds.Y - 1f || surface.Top > vine.Bounds.Bottom + 1f)
				{
					continue;
				}

				touched.Add(surface);
			}

			touched.Sort((a, b) => a.Top.CompareTo(b.Top));
			for (var i = 0; i < touched.Count - 1; i++)
			{
				var lower = touched[i];
				var upper = touched[i + 1];
				_edges[lower.Index].Add(new NavigationEdge(upper.Index, vine, false, centerX));
				_edges[upper.Index].Add(new NavigationEdge(lower.Index, vine, false, centerX));
			}
		}
	}

	private void ConnectDrops()
	{
		for (var i = 0; i < _surfaces.Count; i++)
		{
			for (var j = 0; j < _surfaces.Count; j++)
			{
				if (i == j)
				{
					continue;
				}

				var higher = _surfaces[i];
				var lower = _surfaces[j];
				if (higher.Top >= lower.Top - 1f || !OverlapsHorizontally(higher, lower))
				{
					continue;
				}

				_edges[i].Add(new NavigationEdge(j, null, true, 0f));
			}
		}
	}

	private static bool OverlapsHorizontally(EnemySurface a, EnemySurface b)
	{
		return MathF.Min(a.Right, b.Right) > MathF.Max(a.Left, b.Left) + 0.5f;
	}

	private static float ChooseDropX(EnemySurface current, EnemySurface next, float targetX)
	{
		var leftEdge = current.Left - 2f;
		var rightEdge = current.Right + 2f;
		var leftValid = leftEdge >= next.Left - 2f && leftEdge <= next.Right + 2f;
		var rightValid = rightEdge >= next.Left - 2f && rightEdge <= next.Right + 2f;

		if (!leftValid)
		{
			return rightEdge;
		}

		if (!rightValid)
		{
			return leftEdge;
		}

		var inside = targetX >= current.Left && targetX <= current.Right;
		if (inside)
		{
			return targetX - current.Left < current.Right - targetX ? leftEdge : rightEdge;
		}

		return targetX < current.Left ? leftEdge : rightEdge;
	}

	private readonly record struct PathStep(int Surface, NavigationEdge Edge);
}
