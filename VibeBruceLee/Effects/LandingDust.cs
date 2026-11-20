using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBruceLee.Effects;

/// <summary>
/// Small deterministic landing-dust burst.
/// </summary>
public sealed class LandingDust
{
	private static readonly float[] DustOffsets = [-50f, -30f, -10f, 10f, 30f, 50f];

	private readonly List<Particle> _particles = [];

	public int ParticleCount => _particles.Count;

	public void Spawn(Vector2 feet)
	{
		foreach (var offset in DustOffsets)
		{
			_particles.Add(new Particle
			{
				Position = new Vector2(feet.X + (offset / 10f), feet.Y - 1),
				Velocity = new Vector2(offset, -30f),
				Life = 0.25f,
				MaxLife = 0.25f
			});
		}
	}

	public void Update(float deltaSeconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

		for (var i = _particles.Count - 1; i >= 0; i--)
		{
			var particle = _particles[i];
			particle.Position += particle.Velocity * deltaSeconds;
			particle.Velocity = new Vector2(particle.Velocity.X, particle.Velocity.Y + (120f * deltaSeconds));
			particle.Life -= deltaSeconds;

			if (particle.Life <= 0)
			{
				_particles.RemoveAt(i);
			}
			else
			{
				_particles[i] = particle;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, Texture2D pixel, float layerDepth)
	{
		ArgumentNullException.ThrowIfNull(spriteBatch);
		ArgumentNullException.ThrowIfNull(pixel);

		foreach (var particle in _particles)
		{
			var alpha = (byte)(220 * (particle.Life / particle.MaxLife));
			var color = new Color((byte)220, (byte)200, (byte)150, alpha);
			spriteBatch.Draw(
				pixel,
				new Rectangle((int)particle.Position.X, (int)particle.Position.Y, 2, 2),
				null,
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				layerDepth);
		}
	}

	private struct Particle
	{
		public Vector2 Position;

		public Vector2 Velocity;

		public float Life;

		public float MaxLife;
	}
}
