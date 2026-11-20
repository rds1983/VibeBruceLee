using System.Diagnostics.CodeAnalysis;
using AssetManagementBase;
using Microsoft.Xna.Framework.Audio;
using VibeBruceLee.Gameplay;

namespace VibeBruceLee.Audio;

/// <summary>
/// Loads generated sound effects and plays them through MonoGame. Audio is
/// optional: missing files, no audio device or instance limits never crash the
/// game.
/// </summary>
public sealed class GameAudio : IDisposable
{
	private static readonly string[] Keys =
	[
		"audio/jump.wav",
		"audio/land.wav",
		"audio/chop.wav",
		"audio/kick.wav",
		"audio/hit.wav",
		"audio/knockdown.wav",
		"audio/death.wav",
		"audio/respawn.wav",
		"audio/pickup.wav",
		"audio/exit.wav",
		"audio/fireball.wav",
		"audio/wizard-defeat.wav"
	];

	private readonly Dictionary<string, SoundEffect> _sounds = new(StringComparer.Ordinal);

	public GameAudio(AssetManager assets)
	{
		ArgumentNullException.ThrowIfNull(assets);

		foreach (var key in Keys)
		{
			var sound = TryLoad(assets, key);
			if (sound is not null)
			{
				_sounds[key] = sound;
			}
		}
	}

	public AudioSettings Settings { get; } = new();

	public bool IsAvailable => _sounds.Count > 0;

	[SuppressMessage(
		"Design",
		"CA1031:Do not catch general exception types",
		Justification = "Audio playback failures must not interrupt gameplay.")]
	public void Play(string key)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);

		if (Settings.Muted || Settings.Volume <= 0f)
		{
			return;
		}

		if (!_sounds.TryGetValue(key, out var sound))
		{
			return;
		}

		try
		{
			sound.Play(Settings.Volume, 0f, 0f);
		}
		catch (Exception)
		{
			// Audio devices can disappear at runtime; gameplay continues silently.
		}
	}

	public void Dispose()
	{
		foreach (var sound in _sounds.Values)
		{
			sound.Dispose();
		}

		_sounds.Clear();
	}

	[SuppressMessage(
		"Design",
		"CA1031:Do not catch general exception types",
		Justification = "Missing assets or unavailable audio hardware must not crash the game.")]
	private static SoundEffect? TryLoad(AssetManager assets, string key)
	{
		try
		{
			return assets.LoadSoundEffect(key);
		}
		catch (Exception)
		{
			return null;
		}
	}
}
