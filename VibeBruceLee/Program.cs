using VibeBruceLee;

using var game = new BruceLeeGame(args.Contains("--fullscreen", StringComparer.Ordinal));
game.Run();
