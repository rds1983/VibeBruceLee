using VibeBruceLee;

var fullscreen = args.Contains("--fullscreen", StringComparer.Ordinal);
var debugOverlay = args.Contains("--debug-overlay", StringComparer.Ordinal);

using var game = new BruceLeeGame(fullscreen, debugOverlay);
game.Run();
