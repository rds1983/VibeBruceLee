using AssetManagementBase;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBruceLee.Rendering;

/// <summary>
/// Temporary scene that proves XNAssets texture loading and FontStashSharp text.
/// Replaced by the real screen stack in T-010.
/// </summary>
public sealed class AssetPipelineTestScene : IDisposable
{
	private static readonly Color SpectrumYellow = new(255, 216, 0);
	private static readonly Color SpectrumCyan = new(0, 216, 216);
	private static readonly Color MutedText = new(160, 160, 160);

	private readonly Texture2D _testCard;
	private readonly FontSystem _fontSystem;

	public AssetPipelineTestScene(AssetManager assetManager, GraphicsDevice graphicsDevice)
	{
		ArgumentNullException.ThrowIfNull(assetManager);
		ArgumentNullException.ThrowIfNull(graphicsDevice);

		_testCard = assetManager.LoadTexture2D(graphicsDevice, "art/test-card.png");
		_fontSystem = assetManager.LoadFontSystem("fonts/press-start-2p-regular.ttf");
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		ArgumentNullException.ThrowIfNull(spriteBatch);

		spriteBatch.Draw(
			_testCard,
			new Rectangle(48, 96, _testCard.Width * 6, _testCard.Height * 6),
			Color.White);

		spriteBatch.DrawString(_fontSystem.GetFont(48), "VibeBruceLee", new Vector2(480, 96), SpectrumYellow);
		spriteBatch.DrawString(_fontSystem.GetFont(24), "Asset pipeline online", new Vector2(480, 176), Color.White);
		spriteBatch.DrawString(_fontSystem.GetFont(16), "XNAssets texture + FontStashSharp text", new Vector2(480, 224), SpectrumCyan);
		spriteBatch.DrawString(_fontSystem.GetFont(16), "art/test-card.png", new Vector2(480, 256), MutedText);
		spriteBatch.DrawString(_fontSystem.GetFont(16), "Esc to quit", new Vector2(48, 880), MutedText);
	}

	public void Dispose()
	{
		_testCard.Dispose();
		_fontSystem.Dispose();
		GC.SuppressFinalize(this);
	}
}
