using VibeBruceLee.Core.Levels;
using Xunit;

namespace VibeBruceLee.Tests;

public sealed class ChamberLoaderTests : IDisposable
{
	private const string ValidChamber = """
		{
		  "id": "test-chamber",
		  "name": "Test Chamber",
		  "width": 256,
		  "height": 192,
		  "playerSpawn": { "x": 16, "y": 16 },
		  "platforms": [ { "x": 0, "y": 176, "width": 256, "height": 16 } ],
		  "vines": [ { "x": 120, "y": 64, "width": 8, "height": 112, "orientation": "vertical" } ],
		  "lanterns": [ { "id": "lantern-1", "x": 40, "y": 100, "required": true } ],
		  "enemySpawns": [ { "type": "ninja", "x": 200, "y": 144 } ],
		  "hazards": [ { "type": "mine", "x": 80, "y": 168, "width": 16, "height": 8 } ],
		  "exits": [ { "x": 120, "y": 184, "width": 16, "height": 8, "target": "test-chamber" } ],
		  "decor": [ { "sprite": "art/lantern.png", "x": 8, "y": 8, "scale": 2 } ]
		}
		""";

	private readonly string _directory;

	public ChamberLoaderTests()
	{
		_directory = Path.Combine(Path.GetTempPath(), "vbl-levels-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_directory);
	}

	[Fact]
	public void LoadsValidChamber()
	{
		var path = Write("chamber.json", ValidChamber);

		var chamber = ChamberLoader.Load(path);

		Assert.Equal("test-chamber", chamber.Id);
		Assert.Equal("Test Chamber", chamber.Name);
		Assert.Single(chamber.Platforms);
		Assert.Single(chamber.Lanterns);
		Assert.Equal("test-chamber", chamber.Exits[0].Target);
	}

	[Fact]
	public void MissingFileThrows()
	{
		Assert.Throws<FileNotFoundException>(() => ChamberLoader.Load(Path.Combine(_directory, "missing.json")));
	}

	[Fact]
	public void MalformedJsonThrowsWithFileContext()
	{
		var path = Write("broken.json", "{ \"id\": ");

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("not valid JSON", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void UnknownPropertyIsRejected()
	{
		var path = Write("typo.json", ValidChamber.Replace("\"name\":", "\"bogus\": true, \"name\":"));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("bogus", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void OutOfBoundsPlatformIsReported()
	{
		var path = Write("bounds.json", ValidChamber.Replace(
			"\"x\": 0, \"y\": 176, \"width\": 256",
			"\"x\": 250, \"y\": 176, \"width\": 16"));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("platforms[0]", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void MissingPlayerSpawnIsReported()
	{
		var path = Write("spawn.json", ValidChamber.Replace(
			"  \"playerSpawn\": { \"x\": 16, \"y\": 16 },\r\n",
			string.Empty).Replace(
			"  \"playerSpawn\": { \"x\": 16, \"y\": 16 },\n",
			string.Empty));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("playerSpawn", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void DuplicateLanternIdsAreReported()
	{
		var path = Write("lanterns.json", ValidChamber.Replace(
			"\"lanterns\": [ { \"id\": \"lantern-1\", \"x\": 40, \"y\": 100, \"required\": true } ]",
			"\"lanterns\": [ { \"id\": \"lantern-1\", \"x\": 40, \"y\": 100 }, { \"id\": \"lantern-1\", \"x\": 80, \"y\": 100 } ]"));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("duplicated", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void UnknownHazardTypeIsReported()
	{
		var path = Write("hazard.json", ValidChamber.Replace("\"type\": \"mine\"", "\"type\": \"lava\""));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("hazards[0].type", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void LoadAllRejectsUnknownExitTarget()
	{
		Write("chamber.json", ValidChamber.Replace("\"target\": \"test-chamber\"", "\"target\": \"missing\""));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.LoadAll(_directory));

		Assert.Contains("missing", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void LoadAllRejectsDuplicateIds()
	{
		Write("a.json", ValidChamber);
		Write("b.json", ValidChamber);

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.LoadAll(_directory));

		Assert.Contains("duplicated", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void SampleLevelsLoad()
	{
		var directory = Path.Combine(AppContext.BaseDirectory, "Assets", "levels");

		var chambers = ChamberLoader.LoadAll(directory);

		Assert.Equal(2, chambers.Count);
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-01");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-02");
	}

	private string Write(string name, string json)
	{
		var path = Path.Combine(_directory, name);
		File.WriteAllText(path, json);
		return path;
	}

	public void Dispose()
	{
		if (Directory.Exists(_directory))
		{
			Directory.Delete(_directory, true);
		}

		GC.SuppressFinalize(this);
	}
}
