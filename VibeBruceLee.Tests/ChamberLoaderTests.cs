using VibeBruceLee.Levels;
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
	public void InvalidHazardDirectionIsReported()
	{
		var path = Write("direction.json", ValidChamber.Replace(
			"\"type\": \"mine\"",
			"\"type\": \"mine\", \"directionX\": 0"));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("directionX", exception.Message, StringComparison.Ordinal);
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

		Assert.Equal(20, chambers.Count);
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-01");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-02");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-03");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-04");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-05");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-06");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-07");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-08");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-09");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-10");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-11");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-12");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-13");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-14");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-15");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-16");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-17");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-18");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-19");
		Assert.Contains(chambers, chamber => chamber.Id == "chamber-20");
	}

	[Fact]
	public void ElectrifiedCombAddsLoopHazards()
	{
		var directory = Path.Combine(AppContext.BaseDirectory, "Assets", "levels");
		var chambers = ChamberLoader.LoadAll(directory).ToDictionary(chamber => chamber.Id, StringComparer.Ordinal);

		Assert.Equal(2, chambers["chamber-16"].LoopHazards.Count);
		Assert.Empty(chambers["chamber-01"].LoopHazards);
	}

	[Fact]
	public void SampleChambersFormAContinuousChain()
	{
		var directory = Path.Combine(AppContext.BaseDirectory, "Assets", "levels");
		var chambers = ChamberLoader.LoadAll(directory).ToDictionary(chamber => chamber.Id, StringComparer.Ordinal);

		Assert.Equal("chamber-05", chambers["chamber-04"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-06", chambers["chamber-05"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-07", chambers["chamber-06"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-08", chambers["chamber-07"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-09", chambers["chamber-08"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-10", chambers["chamber-09"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-11", chambers["chamber-10"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-12", chambers["chamber-11"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-13", chambers["chamber-12"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-14", chambers["chamber-13"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-15", chambers["chamber-14"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-16", chambers["chamber-15"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-17", chambers["chamber-16"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-18", chambers["chamber-17"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-19", chambers["chamber-18"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.Equal("chamber-20", chambers["chamber-19"].Exits.MaxBy(exit => exit.X)!.Target);
		Assert.True(chambers["chamber-20"].Boss);
		Assert.NotNull(chambers["chamber-20"].Wizard);
		Assert.Empty(chambers["chamber-20"].Exits);
	}

	[Fact]
	public void BossChamberWithoutExitsLoads()
	{
		var path = Write("boss.json", BossChamber());

		var chamber = ChamberLoader.Load(path);

		Assert.True(chamber.Boss);
		Assert.NotNull(chamber.Wizard);
		Assert.Empty(chamber.Exits);
	}

	[Fact]
	public void NonBossChamberWithoutExitsIsRejected()
	{
		var path = Write("no-exit.json", ValidChamber.Replace(
			"\"exits\": [ { \"x\": 120, \"y\": 184, \"width\": 16, \"height\": 8, \"target\": \"test-chamber\" } ],",
			"\"exits\": [],"));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("exit", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void BossWithoutWizardIsRejected()
	{
		var path = Write("boss-no-wizard.json", BossChamber(includeWizard: false));

		var exception = Assert.Throws<InvalidDataException>(() => ChamberLoader.Load(path));

		Assert.Contains("wizard", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	private static string BossChamber(bool includeWizard = true)
	{
		var json = ValidChamber
			.Replace(
				"\"id\": \"test-chamber\",",
				"\"id\": \"test-chamber\", \"boss\": true,")
			.Replace(
				"\"exits\": [ { \"x\": 120, \"y\": 184, \"width\": 16, \"height\": 8, \"target\": \"test-chamber\" } ],",
				"\"exits\": [],");

		if (includeWizard)
		{
			json = json.Replace(
				"\"exits\": [],",
				"\"exits\": [], \"wizard\": { \"x\": 96, \"y\": 16, \"width\": 32, \"height\": 32, \"buttonX\": 24, \"buttonY\": 112, \"buttonWidth\": 16, \"buttonHeight\": 8 },");
		}

		return json;
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
