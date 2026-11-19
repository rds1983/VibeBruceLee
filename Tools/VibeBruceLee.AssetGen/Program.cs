using VibeBruceLee.AssetGen;

var check = args.Length > 0 && args[0] == "--check";
var argumentOffset = check ? 1 : 0;

if (args.Length > argumentOffset + 1)
{
	Console.Error.WriteLine("Usage: dotnet run --project Tools/VibeBruceLee.AssetGen -- [--check] [asset-root]");
	return 1;
}

var outputRoot = args.Length > argumentOffset
	? args[argumentOffset]
	: Path.Combine(Directory.GetCurrentDirectory(), "VibeBruceLee", "Assets");

if (check)
{
	var errors = AssetGenerator.Verify(outputRoot);
	if (errors.Count > 0)
	{
		foreach (var error in errors)
		{
			Console.Error.WriteLine(error);
		}

		return 1;
	}

	Console.WriteLine("Assets match the generator.");
	return 0;
}

AssetGenerator.Generate(outputRoot);
Console.WriteLine($"Generated assets in '{Path.GetFullPath(outputRoot)}'.");
return 0;
