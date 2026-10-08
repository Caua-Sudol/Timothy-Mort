using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum PhaseSelectOption
{
    NONE = 0,
    AFFORDANCE = 1,
    PROP_VISUAL = 2,
    FEEDBACK = 3,
    CONCEPTUAL_MODELS = 4,
    SCALE_TEST = 5
}

public sealed class PhaseSelectMenu
{
    private const int CardWidth = 540;
    private const int CardHeight = 368;
    private const int CardSpacing = 48;
    private const int MainPhaseCount = 3;
    private const string TitleText = "Selecionar fase";
    private const string NavigationText = "A/D: fases    S/baixo: teste    W/cima: fases    Enter: selecionar    Esc: voltar";
    private const int TestButtonHeight = 112;
    private const int TestButtonGap = 48;

    private static readonly string[] PhaseTitles =
    {
        "Affordance\ne visibilidade",
        "Feedback",
        "Modelos conceituais\ne restrições"
    };

    private static readonly string[] PhaseIds =
    {
        ScenarioCatalog.AffordanceVisibilityScenarioId,
        ScenarioCatalog.DamageFeedbackScenarioId,
        ScenarioCatalog.ConceptualModelsConstraintsScenarioId
    };

    private readonly SpriteFont _font;
    private readonly SpriteFont _titleFont;
    private static readonly Color BackgroundColor = new Color(21, 25, 31);
    private static readonly Color PrimaryColor = new Color(216, 208, 187);
    private static readonly Color SecondaryColor = new Color(181, 173, 161);
    private static readonly Color CardTextColor = new Color(37, 40, 43);
    private readonly Vector2 _center;
    private readonly PhaseProgress _progress;
    private Texture2D _selectedCard;
    private Texture2D _unselectedCard;
    private Texture2D _openLock, _closedLock, _check;
    private SymbolRain _symbols;
    private int _selectedPhaseIndex;
    private bool _testRoomSelected;
    private string _sessionCode;

    public PhaseSelectMenu(SpriteFont titleFont, SpriteFont font, Vector2 center, PhaseProgress progress)
    {
        _titleFont = titleFont;
        _font = font;
        _center = center;
        _progress = progress;
    }

    public void LoadContent(ContentManager content)
    {
        _selectedCard = content.Load<Texture2D>("ui/card_selected");
        _unselectedCard = content.Load<Texture2D>("ui/card_unselected");
        _openLock = content.Load<Texture2D>("ui/icons/cadeado_aberto");
        _closedLock = content.Load<Texture2D>("ui/icons/cadeado");
        _check = content.Load<Texture2D>("ui/icons/check");
        _symbols = new SymbolRain(content.Load<Texture2D>("ui/icons/icons"), _center * 2, 90, 0.5f);
    }

    public void UpdateAnimation(double deltaTime) => _symbols.Update(deltaTime);

    public void SetSessionCode(string sessionCode)
    {
        _sessionCode = sessionCode;
    }

    public PhaseSelectOption Update(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState)
    {
        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.A) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.Left))
        {
            _testRoomSelected = false;
            _selectedPhaseIndex =
                (_selectedPhaseIndex + PhaseTitles.Length - 1) % PhaseTitles.Length;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.D) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.Right))
        {
            _testRoomSelected = false;
            _selectedPhaseIndex = (_selectedPhaseIndex + 1) % PhaseTitles.Length;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.S) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.Down))
            _testRoomSelected = true;
        else if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.W) ||
                 KeyWasPressed(keyboardState, previousKeyboardState, Keys.Up))
            _testRoomSelected = false;

        if (_testRoomSelected && KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter))
            return PhaseSelectOption.SCALE_TEST;

        if (!_testRoomSelected && KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter) &&
            IsUnlocked(_selectedPhaseIndex))
        {
            return _selectedPhaseIndex switch
            {
                0 => PhaseSelectOption.AFFORDANCE,
                1 => PhaseSelectOption.FEEDBACK,
                2 => PhaseSelectOption.CONCEPTUAL_MODELS,
                _ => PhaseSelectOption.NONE
            };
        }

        return PhaseSelectOption.NONE;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(BackgroundColor);
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _symbols.Draw(spriteBatch);

        if (!string.IsNullOrEmpty(_sessionCode))
        {
            DrawCenteredText(
                spriteBatch,
                $"Sessão: {_sessionCode}",
                new Vector2(_center.X, 120.0f),
                SecondaryColor, scale: 0.75f);
        }

        DrawCenteredText(spriteBatch, TitleText, new Vector2(_center.X, 190.0f), PrimaryColor, _titleFont);

        int totalWidth = MainPhaseCount * CardWidth +
                         (MainPhaseCount - 1) * CardSpacing;
        int startX = (int)_center.X - totalWidth / 2;
        int cardY = (int)_center.Y - CardHeight / 2;

        for (int index = 0; index < PhaseTitles.Length; index++)
        {
            var cardRectangle = new Rectangle(
                startX + index * (CardWidth + CardSpacing),
                cardY,
                CardWidth,
                CardHeight);
            bool isCompleted = _progress.IsCompleted(PhaseIds[index]);
            bool isUnlocked = IsUnlocked(index);
            bool isSelected = !_testRoomSelected && index == _selectedPhaseIndex;
            Color textColor = CardTextColor;
            spriteBatch.Draw(isSelected ? _selectedCard : _unselectedCard, cardRectangle, Color.White);
            DrawCenteredText(
                spriteBatch,
                PhaseTitles[index],
                cardRectangle.Center.ToVector2() - new Vector2(0.0f, 24.0f),
                textColor,
                _titleFont,
                0.45f);

            Vector2 statusPosition = cardRectangle.Center.ToVector2() +
                new Vector2(0.0f, 105.0f);

            Texture2D statusIcon = isCompleted ? _check : isUnlocked ? _openLock : _closedLock;
            const int iconSize = 64;
            spriteBatch.Draw(statusIcon,
                new Rectangle((int)statusPosition.X - iconSize / 2,
                    (int)statusPosition.Y - iconSize / 2, iconSize, iconSize), Color.White);
        }

        var testButton = new Rectangle(
            (int)_center.X - CardWidth / 2,
            cardY + CardHeight + TestButtonGap,
            CardWidth,
            TestButtonHeight);
        spriteBatch.Draw(_testRoomSelected ? _selectedCard : _unselectedCard, testButton, Color.White);
        DrawCenteredText(spriteBatch, "Sala de teste / composição",
            testButton.Center.ToVector2(), CardTextColor, _font, 0.75f);

        DrawCenteredText(spriteBatch, NavigationText, new Vector2(_center.X, 960.0f), SecondaryColor, scale: 0.75f);
        spriteBatch.End();
    }

    private bool IsUnlocked(int index)
    {
        return !_progress.IsCompleted(PhaseIds[index]) &&
            _progress.IsUnlocked(PhaseIds[index]);
    }

    private void DrawCenteredText(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        Color color,
        SpriteFont font = null,
        float scale = 1.0f)
    {
        font ??= _font;
        string[] lines = text.Split('\n');
        float lineHeight = font.LineSpacing * scale;
        float y = position.Y - lines.Length * lineHeight / 2.0f;
        foreach (string line in lines)
        {
            float width = font.MeasureString(line).X * scale;
            spriteBatch.DrawString(font, line, new Vector2(position.X - width / 2.0f, y),
                color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
            y += lineHeight;
        }
    }

    private static bool KeyWasPressed(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }
}
