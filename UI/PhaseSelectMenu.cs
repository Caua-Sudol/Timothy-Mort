using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum PhaseSelectOption
{
    NONE = 0,
    AFFORDANCE = 1,
    PROP_VISUAL = 2,
    FEEDBACK = 3,
    CONCEPTUAL_MODELS = 4
}

public sealed class PhaseSelectMenu
{
    private const int CardWidth = 480;
    private const int CardHeight = 280;
    private const int CardSpacing = 48;
    private const int BorderThickness = 4;
    private const int MainPhaseCount = 3;
    private const int VisualPrototypeIndex = 3;
    private const string TitleText = "Selecionar fase";
    private const string TutorialText = "Tutorial - em breve";
    private const string NavigationText = "A/D ou setas para navegar    Enter para selecionar    Esc para voltar";

    private static readonly string[] PhaseTitles =
    {
        "1. Affordance e visibilidade",
        "2. Feedback de dano",
        "3. Modelos conceituais e restrições",
        "Protótipo visual"
    };

    private static readonly string[] PhaseIds =
    {
        ScenarioCatalog.AffordanceVisibilityScenarioId,
        ScenarioCatalog.DamageFeedbackScenarioId,
        ScenarioCatalog.ConceptualModelsConstraintsScenarioId
    };

    private readonly SpriteFont _font;
    private readonly Vector2 _center;
    private readonly PhaseProgress _progress;
    private Texture2D _pixel;
    private int _selectedPhaseIndex;
    private string _sessionCode;

    public PhaseSelectMenu(SpriteFont font, Vector2 center, PhaseProgress progress)
    {
        _font = font;
        _center = center;
        _progress = progress;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

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
            _selectedPhaseIndex =
                (_selectedPhaseIndex + PhaseTitles.Length - 1) % PhaseTitles.Length;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.D) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.Right))
        {
            _selectedPhaseIndex = (_selectedPhaseIndex + 1) % PhaseTitles.Length;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter) &&
            IsUnlocked(_selectedPhaseIndex))
        {
            return _selectedPhaseIndex switch
            {
                0 => PhaseSelectOption.AFFORDANCE,
                1 => PhaseSelectOption.FEEDBACK,
                2 => PhaseSelectOption.CONCEPTUAL_MODELS,
                VisualPrototypeIndex => PhaseSelectOption.PROP_VISUAL,
                _ => PhaseSelectOption.NONE
            };
        }

        return PhaseSelectOption.NONE;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();

        if (!string.IsNullOrEmpty(_sessionCode))
        {
            DrawCenteredText(
                spriteBatch,
                $"Sessão: {_sessionCode}",
                new Vector2(_center.X, 120.0f),
                Color.LightGray);
        }

        DrawCenteredText(spriteBatch, TitleText, new Vector2(_center.X, 180.0f), Color.White);
        DrawCenteredText(spriteBatch, TutorialText, new Vector2(_center.X, 250.0f), Color.Gray);

        int totalWidth = MainPhaseCount * CardWidth +
                         (MainPhaseCount - 1) * CardSpacing;
        int startX = (int)_center.X - totalWidth / 2;
        int cardY = (int)_center.Y - CardHeight / 2;

        for (int index = 0; index < PhaseTitles.Length; index++)
        {
            bool isVisualPrototype = index == VisualPrototypeIndex;
            var cardRectangle = isVisualPrototype
                ? new Rectangle((int)_center.X - CardWidth / 2, cardY + CardHeight + CardSpacing, CardWidth, 120)
                : new Rectangle(
                startX + index * (CardWidth + CardSpacing),
                cardY,
                CardWidth,
                CardHeight);
            bool isCompleted = !isVisualPrototype &&
                _progress.IsCompleted(PhaseIds[index]);
            bool isUnlocked = IsUnlocked(index);
            bool isSelected = index == _selectedPhaseIndex;
            Color borderColor = isSelected ? Color.Yellow : Color.DimGray;
            Color textColor = isCompleted
                ? Color.LightGreen
                : isUnlocked ? Color.White : Color.Gray;

            DrawBorder(spriteBatch, cardRectangle, borderColor);
            DrawCenteredText(
                spriteBatch,
                PhaseTitles[index],
                cardRectangle.Center.ToVector2() - new Vector2(0.0f, 24.0f),
                textColor);

            if (isCompleted)
            {
                DrawCenteredText(
                    spriteBatch,
                    "Concluída",
                    cardRectangle.Center.ToVector2() + new Vector2(0.0f, 32.0f),
                    Color.LightGreen);
            }
            else if (!isUnlocked)
            {
                DrawCenteredText(
                    spriteBatch,
                    "Bloqueada",
                    cardRectangle.Center.ToVector2() + new Vector2(0.0f, 32.0f),
                    Color.Gray);
            }
            else if (isVisualPrototype)
            {
                DrawCenteredText(
                    spriteBatch,
                    "Área de testes",
                    cardRectangle.Center.ToVector2() + new Vector2(0.0f, 24.0f),
                    Color.LightGray);
            }
            else
            {
                DrawCenteredText(
                    spriteBatch,
                    "Disponível",
                    cardRectangle.Center.ToVector2() + new Vector2(0.0f, 32.0f),
                    Color.LightGray);
            }
        }

        DrawCenteredText(spriteBatch, NavigationText, new Vector2(_center.X, 900.0f), Color.LightGray);
        spriteBatch.End();
    }

    private bool IsUnlocked(int index)
    {
        return index == VisualPrototypeIndex ||
            !_progress.IsCompleted(PhaseIds[index]) &&
            _progress.IsUnlocked(PhaseIds[index]);
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, BorderThickness), color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(rectangle.X, rectangle.Bottom - BorderThickness, rectangle.Width, BorderThickness),
            color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Y, BorderThickness, rectangle.Height), color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(rectangle.Right - BorderThickness, rectangle.Y, BorderThickness, rectangle.Height),
            color);
    }

    private void DrawCenteredText(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        Color color)
    {
        Vector2 textSize = _font.MeasureString(text);
        spriteBatch.DrawString(_font, text, position - textSize / 2.0f, color);
    }

    private static bool KeyWasPressed(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }
}
