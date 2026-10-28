using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum ControlSchemeMenuResult
{
    NONE = 0,
    CLOSED = 1,
    CONTINUE = 2,
    TEST_MORE = 3
}

// Tutorial overlay: choose which keys the tutorial shows, or confirm leaving the tutorial.
public sealed class ControlSchemeMenu
{
    private const float OptionSpacing = 170.0f;
    private const int OptionWidth = 900;
    private const int SelectorHeight = 96;
    private const float OptionTextScale = 1.2f;
    private static readonly Color PrimaryColor = new Color(216, 208, 187);
    private static readonly Color SecondaryColor = new Color(181, 173, 161);

    private static readonly ControlScheme[] Schemes =
        { ControlScheme.SHIFT, ControlScheme.ARROWS, ControlScheme.RIGHT_HAND };
    private static readonly string[] ConfirmOptions = { "Sim, seguir", "Testar mais" };

    private readonly SpriteFont _titleFont;
    private readonly SpriteFont _font;
    private readonly Texture2D _selector;
    private readonly Vector2 _center;
    private bool _confirmingExit;
    private int _selectedIndex;

    public ControlSchemeMenu(SpriteFont titleFont, SpriteFont font, Texture2D selector, Vector2 center)
    {
        _titleFont = titleFont;
        _font = font;
        _selector = selector;
        _center = center;
    }

    public void OpenChoice()
    {
        _confirmingExit = false;
        _selectedIndex = Array.IndexOf(Schemes, TutorialControls.Current);
    }

    public void OpenExitConfirmation()
    {
        _confirmingExit = true;
        _selectedIndex = 0;
    }

    private int OptionCount => _confirmingExit ? ConfirmOptions.Length : Schemes.Length;

    public ControlSchemeMenuResult Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        bool clicked = false;
        for (int index = 0; index < OptionCount && !clicked; index++)
            clicked = UiPointer.UpdateOption(GetOptionBounds(index), index, ref _selectedIndex);

        if (MenuInput.Up(keyboardState, previousKeyboardState))
            _selectedIndex = (_selectedIndex - 1 + OptionCount) % OptionCount;
        else if (MenuInput.Down(keyboardState, previousKeyboardState))
            _selectedIndex = (_selectedIndex + 1) % OptionCount;

        bool escape = keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape);
        if (escape)
            return _confirmingExit ? ControlSchemeMenuResult.TEST_MORE : ControlSchemeMenuResult.CLOSED;

        if (!clicked && !MenuInput.Confirm(keyboardState, previousKeyboardState))
            return ControlSchemeMenuResult.NONE;

        if (_confirmingExit)
            return _selectedIndex == 0 ? ControlSchemeMenuResult.CONTINUE : ControlSchemeMenuResult.TEST_MORE;

        TutorialControls.Current = Schemes[_selectedIndex];
        return ControlSchemeMenuResult.CLOSED;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D fadeTexture)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, (int)(_center.X * 2), (int)(_center.Y * 2)),
            Color.Black * 0.7f);

        string title = _confirmingExit ? "Seguir com este controle?" : "Como você prefere jogar?";
        string subtitle = _confirmingExit
            ? TutorialControls.Describe(TutorialControls.Current)
            : "Todas as teclas funcionam. Dá para trocar depois na placa Controles.";
        DrawCentered(spriteBatch, _titleFont, title, _center - new Vector2(0, 380), PrimaryColor);
        DrawCentered(spriteBatch, _font, subtitle, _center - new Vector2(0, 270), SecondaryColor, 0.8f);

        for (int index = 0; index < OptionCount; index++)
        {
            string text = _confirmingExit ? ConfirmOptions[index] : TutorialControls.Describe(Schemes[index]);
            Vector2 position = GetOptionPosition(index);
            bool selected = index == _selectedIndex;
            if (selected)
            {
                spriteBatch.Draw(_selector, new Rectangle((int)(position.X - OptionWidth / 2f),
                    (int)(position.Y - SelectorHeight / 2f), OptionWidth, SelectorHeight), Color.White);
            }
            DrawCentered(spriteBatch, _font, text, position, selected ? PrimaryColor : SecondaryColor,
                OptionTextScale);
        }
        spriteBatch.End();
    }

    private Vector2 GetOptionPosition(int index) =>
        _center + new Vector2(0, 40 + (index - (OptionCount - 1) / 2.0f) * OptionSpacing);

    private Rectangle GetOptionBounds(int index)
    {
        Vector2 position = GetOptionPosition(index);
        return new Rectangle((int)(position.X - OptionWidth / 2f), (int)(position.Y - OptionSpacing / 2f),
            OptionWidth, (int)OptionSpacing);
    }

    private static void DrawCentered(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position,
        Color color, float scale = 1f)
    {
        Vector2 size = font.MeasureString(text) * scale;
        spriteBatch.DrawString(font, text, position - size / 2f, color, 0f, Vector2.Zero, scale,
            SpriteEffects.None, 0f);
    }
}
