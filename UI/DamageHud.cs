using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class DamageHud
{
    private const int Margin = 32;
    private Texture2D _heart;

    public void LoadContent(ContentManager content)
    {
        _heart = content.Load<Texture2D>("ui/icons/coracao");
    }

    public void Draw(SpriteBatch spriteBatch, int currentHealth, int maximumHealth,
        SolutionVisualModule visualModules)
    {
        if (_heart == null || maximumHealth <= 0) return;
        bool emphasizeHud = visualModules.HasFlag(SolutionVisualModule.DAMAGE_HUD_EMPHASIS);
        int size = emphasizeHud ? 56 : 42;
        int spacing = emphasizeHud ? 16 : 12;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        for (int index = 0; index < maximumHealth; index++)
        {
            Color color = index < currentHealth ? Color.White : Color.White * 0.22f;
            spriteBatch.Draw(_heart,
                new Rectangle(Margin + index * (size + spacing), Margin, size, size), color);
        }
        spriteBatch.End();
    }
}
