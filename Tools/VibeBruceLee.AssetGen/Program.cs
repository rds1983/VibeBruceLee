using VibeBruceLee.AssetGen;

if (args.Length > 1)
{
	Console.Error.WriteLine("Usage: dotnet run --project Tools/VibeBruceLee.AssetGen -- [asset-root]");
	return 1;
}

var outputRoot = args.Length == 1
	? args[0]
	: Path.Combine(Directory.GetCurrentDirectory(), "VibeBruceLee", "Assets");

AssetGenerator.Generate(outputRoot);
Console.WriteLine($"Generated assets in '{Path.GetFullPath(outputRoot)}'.");
return 0;
