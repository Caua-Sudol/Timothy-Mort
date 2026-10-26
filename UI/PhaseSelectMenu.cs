using System;
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
    private const string NavigationText = "A/D ou setas: fases    S/baixo: teste    W/cima: fases    Enter: selecionar    Esc: voltar";
    private const string NavigationTextWithoutTestRoom = "A/D ou setas: fases    Enter: selecionar    Esc: voltar";
    private const int TestButtonHeight = 112;
    private const int TestButtonGap = 48;
    private const double ShakeSeconds = 0.3;
    private const double ShakeFrequency = 70.0;
    private const float ShakeAmplitude = 14.0f;
    // Card layout, measured from the card center.
    private const float CardTitleOffsetY = -50.0f;
    private const float BetaNoticeOffsetY = 30.0f;
    private const float StatusIconOffsetY = 110.0f;
    private const int StatusIconSize = 96;

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
    private static readonly Color LockedCardTint = new Color(120, 120, 120);
    private static readonly Color LockedTextColor = new Color(70, 72, 75);
    private readonly Vector2 _center;
    private readonly PhaseProgress _progress;
    private Texture2D _selectedCard;
    private Texture2D _unselectedCard;
    private Texture2D _openLock, _closedLock, _check;
    private SymbolRain _symbols;
    private int _selectedPhaseIndex;
    private bool _testRoomSelected;
    private int _shakeIndex = -1;
    private double _shakeTimer;
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

    public void UpdateAnimation(double deltaTime)
    {
        _symbols.Update(deltaTime);
        _shakeTimer = Math.Max(0.0, _shakeTimer - deltaTime);
    }

    public void SetSessionCode(string sessionCode)
    {
        _sessionCode = sessionCode;
    }

    public PhaseSelectOption Update(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState)
    {
        if (MenuInput.Left(keyboardState, previousKeyboardState))
        {
            _testRoomSelected = false;
            _selectedPhaseIndex =
                (_selectedPhaseIndex + PhaseTitles.Length - 1) % PhaseTitles.Length;
        }

        if (MenuInput.Right(keyboardState, previousKeyboardState))
        {
            _testRoomSelected = false;
            _selectedPhaseIndex = (_selectedPhaseIndex + 1) % PhaseTitles.Length;
        }

        if (!ResearchStudyConfiguration.DevToolsEnabled)
            _testRoomSelected = false;
        else if (MenuInput.Down(keyboardState, previousKeyboardState))
            _testRoomSelected = true;
        else if (MenuInput.Up(keyboardState, previousKeyboardState))
            _testRoomSelected = false;

        bool cardClicked = false;
        for (int index = 0; index < PhaseTitles.Length && !cardClicked; index++)
        {
            int previousIndex = _selectedPhaseIndex;
            cardClicked = UiPointer.UpdateOption(GetCardBounds(index), index, ref _selectedPhaseIndex);
            if (cardClicked || _selectedPhaseIndex != previousIndex)
                _testRoomSelected = false;
        }

        bool testRoomClicked = false;
        if (ResearchStudyConfiguration.DevToolsEnabled &&
            GetTestButtonBounds().Contains(UiPointer.Position) &&
            (UiPointer.Moved || UiPointer.LeftClicked))
        {
            _testRoomSelected = true;
            testRoomClicked = UiPointer.LeftClicked;
        }

        bool confirmed = MenuInput.Confirm(keyboardState, previousKeyboardState);
        if (testRoomClicked || _testRoomSelected && confirmed)
            return PhaseSelectOption.SCALE_TEST;

        if (!_testRoomSelected && (confirmed || cardClicked) && IsLocked(_selectedPhaseIndex))
        {
            // Shake the grey card so the player sees it cannot be opened yet.
            _shakeIndex = _selectedPhaseIndex;
            _shakeTimer = ShakeSeconds;
        }

        if (!_testRoomSelected && (confirmed || cardClicked) &&
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

        for (int index = 0; index < PhaseTitles.Length; index++)
        {
            Rectangle cardRectangle = GetCardBounds(index);
            if (index == _shakeIndex && _shakeTimer > 0)
            {
                cardRectangle.X += (int)(Math.Sin(_shakeTimer * ShakeFrequency) *
                    ShakeAmplitude * (_shakeTimer / ShakeSeconds));
            }

            bool isCompleted = _progress.IsCompleted(PhaseIds[index]);
            bool isUnlocked = IsUnlocked(index);
            bool isSelected = !_testRoomSelected && index == _selectedPhaseIndex;
            // Locked phases are greyed out so they read as unavailable at a glance.
            bool isLocked = IsLocked(index);
            Color cardTint = isLocked ? LockedCardTint : Color.White;
            Color textColor = isLocked ? LockedTextColor : CardTextColor;
            spriteBatch.Draw(isSelected ? _selectedCard : _unselectedCard, cardRectangle, cardTint);
            DrawCenteredText(
                spriteBatch,
                PhaseTitles[index],
                cardRectangle.Center.ToVector2() + new Vector2(0.0f, CardTitleOffsetY),
                textColor,
                _titleFont,
                0.45f);

            Vector2 statusPosition = cardRectangle.Center.ToVector2() +
                new Vector2(0.0f, StatusIconOffsetY);

            if (ResearchStudyConfiguration.BetaAffordanceOnly && index > 0)
            {
                DrawCenteredText(spriteBatch, "Indisponível na beta",
                    cardRectangle.Center.ToVector2() + new Vector2(0.0f, BetaNoticeOffsetY),
                    textColor, _font, 0.6f);
            }

            Texture2D statusIcon = isCompleted ? _check : isUnlocked ? _openLock : _closedLock;
            var iconRectangle = new Rectangle((int)statusPosition.X - StatusIconSize / 2,
                (int)statusPosition.Y - StatusIconSize / 2, StatusIconSize, StatusIconSize);
            spriteBatch.Draw(statusIcon, iconRectangle, cardTint);
        }

        if (ResearchStudyConfiguration.DevToolsEnabled)
        {
            Rectangle testButton = GetTestButtonBounds();
            spriteBatch.Draw(_testRoomSelected ? _selectedCard : _unselectedCard, testButton, Color.White);
            DrawCenteredText(spriteBatch, "Sala de teste / composição",
                testButton.Center.ToVector2(), CardTextColor, _font, 0.75f);
        }

        DrawCenteredText(spriteBatch,
            ResearchStudyConfiguration.DevToolsEnabled ? NavigationText : NavigationTextWithoutTestRoom,
            new Vector2(_center.X, 960.0f), SecondaryColor, scale: 0.75f);
        spriteBatch.End();
    }

    private bool IsUnlocked(int index)
    {
        return !_progress.IsCompleted(PhaseIds[index]) &&
            _progress.IsUnlocked(PhaseIds[index]);
    }

    private bool IsLocked(int index) =>
        !_progress.IsCompleted(PhaseIds[index]) && !IsUnlocked(index);

    private Rectangle GetCardBounds(int index)
    {
        int totalWidth = MainPhaseCount * CardWidth +
                         (MainPhaseCount - 1) * CardSpacing;
        int startX = (int)_center.X - totalWidth / 2;
        int cardY = (int)_center.Y - CardHeight / 2;
        return new Rectangle(startX + index * (CardWidth + CardSpacing), cardY, CardWidth, CardHeight);
    }

    private Rectangle GetTestButtonBounds() => new Rectangle(
        (int)_center.X - CardWidth / 2,
        (int)_center.Y - CardHeight / 2 + CardHeight + TestButtonGap,
        CardWidth,
        TestButtonHeight);

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
}
