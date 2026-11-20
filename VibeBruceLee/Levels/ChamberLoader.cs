using System.Text.Json;
using System.Text.Json.Serialization;

namespace VibeBruceLee.Levels;

public static class ChamberLoader
{
	private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
	{
		AllowTrailingCommas = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
	};

	public static ChamberDefinition Load(string path)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);

		if (!File.Exists(path))
		{
			throw new FileNotFoundException($"Chamber file not found: '{path}'.", path);
		}

		ChamberDefinition? chamber;
		try
		{
			chamber = JsonSerializer.Deserialize<ChamberDefinition>(File.ReadAllText(path), Options);
		}
		catch (JsonException ex)
		{
			throw new InvalidDataException($"Chamber file '{path}' is not valid JSON: {ex.Message}", ex);
		}

		if (chamber is null)
		{
			throw new InvalidDataException($"Chamber file '{path}' contains no chamber data.");
		}

		var errors = ChamberValidator.Validate(chamber);
		if (errors.Count > 0)
		{
			throw new InvalidDataException(FormatErrors($"Chamber file '{path}' is invalid", errors));
		}

		return chamber;
	}

	public static IReadOnlyList<ChamberDefinition> LoadAll(string directory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(directory);

		if (!Directory.Exists(directory))
		{
			throw new DirectoryNotFoundException($"Chamber directory not found: '{directory}'.");
		}

		var files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
		Array.Sort(files, StringComparer.Ordinal);

		if (files.Length == 0)
		{
			throw new InvalidDataException($"No chamber files found in '{directory}'.");
		}

		var chambers = new List<ChamberDefinition>(files.Length);
		foreach (var file in files)
		{
			chambers.Add(Load(file));
		}

		var errors = ChamberValidator.ValidateSet(chambers);
		if (errors.Count > 0)
		{
			throw new InvalidDataException(FormatErrors($"Chamber set '{directory}' is invalid", errors));
		}

		return chambers;
	}

	private static string FormatErrors(string heading, IReadOnlyList<string> errors)
	{
		return $"{heading}:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", errors)}";
	}
}
