using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public sealed class InterventionChoiceMenu
{
    private const int LogicalWidth = 1920;
    private const int LogicalHeight = 1080;
    private const int PanelMargin = 96;
    private const int CardWidth = 480;
    private const int CardHeight = 480;
    private const int CardSpacing = 48;
    private const int CardTop = 300;
    private const int JustificationCardWidth = 720;
    private const int JustificationCardHeight = 160;
    private const int JustificationColumnSpacing = 48;
    private const int JustificationRowSpacing = 32;
    private const int JustificationTop = 280;
    private const float TextScale = 1.5f;
    private const float TitleScale = 2.0f;
    private const int TextPadding = 32;

    private readonly SpriteFont _font;
    private IReadOnlyList<InterventionSolutionDefinition> _solutions;
    private IReadOnlyList<InterventionJustificationDefinition> _justifications;
    private Texture2D _pixel;
    private int _selectedIndex;
    private InterventionSolutionDefinition _pendingSolution;
    private InterventionSolutionDefinition _completedSolution;
    private InterventionJustificationDefinition _completedJustification;
    private ChoiceStep _choiceStep;
    private bool _requiresJustification;

    private enum ChoiceStep
    {
        SOLUTION,
        JUSTIFICATION
    }

    public InterventionChoiceMenu(SpriteFont font)
    {
        _font = font;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void OpenInitial(IReadOnlyList<InterventionSolutionDefinition> solutions)
    {
        Open(solutions, true);
    }

    public void OpenRevision(IReadOnlyList<InterventionSolutionDefinition> solutions)
    {
        Open(solutions, false);
    }

    private void Open(
        IReadOnlyList<InterventionSolutionDefinition> solutions,
        bool requiresJustification)
    {
        _solutions = solutions;
        _justifications = AffordanceChoiceCatalog.Justifications;
        _selectedIndex = 0;
        _pendingSolution = null;
        _completedSolution = null;
        _completedJustification = null;
        _choiceStep = ChoiceStep.SOLUTION;
        _requiresJustification = requiresJustification;
    }

    public void Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        if (_solutions == null || _solutions.Count == 0 ||
            _completedSolution != null)
        {
            return;
        }

        if (_choiceStep == ChoiceStep.SOLUTION)
        {
            UpdateSolutionSelection(keyboardState, previousKeyboardState);
            return;
        }

        UpdateJustificationSelection(keyboardState, previousKeyboardState);
    }

    public bool TryTakeCompletedFirstChoice(
        out InterventionSolutionDefinition solution,
        out InterventionJustificationDefinition justification)
    {
        solution = _completedSolution;
        justification = _completedJustification;

        if (solution == null || justification == null || !_requiresJustification)
        {
            return false;
        }

        _completedSolution = null;
        _completedJustification = null;
        return true;
    }

    public bool TryTakeCompletedRevision(out InterventionSolutionDefinition solution)
    {
        solution = _completedSolution;

        if (solution == null || _requiresJustification || _completedJustification != null)
        {
            return false;
        }

        _completedSolution = null;
        return true;
    }

    private void UpdateSolutionSelection(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState)
    {
        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.A) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.W))
        {
            _selectedIndex = (_selectedIndex - 1 + _solutions.Count) % _solutions.Count;
        }
        else if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.D) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.S))
        {
            _selectedIndex = (_selectedIndex + 1) % _solutions.Count;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter))
        {
            _pendingSolution = _solutions[_selectedIndex];

            if (_requiresJustification)
            {
                _choiceStep = ChoiceStep.JUSTIFICATION;
                _selectedIndex = 0;
            }
            else
            {
                _completedSolution = _pendingSolution;
            }
        }
    }

    private void UpdateJustificationSelection(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState)
    {
        const int columns = 2;
        int row = _selectedIndex / columns;
        int column = _selectedIndex % columns;
        int rowCount = (_justifications.Count + columns - 1) / columns;

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.A))
        {
            column = (column - 1 + columns) % columns;
        }
        else if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.D))
        {
            column = (column + 1) % columns;
        }
        else if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.W))
        {
            row = (row - 1 + rowCount) % rowCount;
        }
        else if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.S))
        {
            row = (row + 1) % rowCount;
        }

        int candidateIndex = row * columns + column;
        if (candidateIndex < _justifications.Count)
        {
            _selectedIndex = candidateIndex;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter))
        {
            _completedSolution = _pendingSolution;
            _completedJustification = _justifications[_selectedIndex];
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_solutions == null || _solutions.Count == 0)
        {
            return;
        }

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var panel = new Rectangle(
            PanelMargin,
            PanelMargin,
            LogicalWidth - PanelMargin * 2,
            LogicalHeight - PanelMargin * 2);
        spriteBatch.Draw(_pixel, panel, new Color(20, 22, 32) * 0.97f);
        DrawOutline(spriteBatch, panel, Color.White, 4);

        if (_choiceStep == ChoiceStep.SOLUTION)
        {
            DrawSolutionStep(spriteBatch);
        }
        else
        {
            DrawJustificationStep(spriteBatch);
        }

        string footer = _choiceStep == ChoiceStep.SOLUTION
            ? "A / D para navegar    Enter para escolher"
            : "W / A / S / D para navegar    Enter para confirmar";
        DrawCenteredText(
            spriteBatch,
            footer,
            new Vector2(LogicalWidth / 2.0f, 900.0f),
            Color.White,
            TextScale);

        spriteBatch.End();
    }

    private void DrawSolutionStep(SpriteBatch spriteBatch)
    {
        DrawCenteredText(
            spriteBatch,
            _requiresJustification ? "Escolha uma solucao" : "Revise a solucao",
            new Vector2(LogicalWidth / 2.0f, 180.0f),
            Color.White,
            TitleScale);

        int cardsWidth = _solutions.Count * CardWidth + (_solutions.Count - 1) * CardSpacing;
        int cardsLeft = (LogicalWidth - cardsWidth) / 2;

        for (int index = 0; index < _solutions.Count; index++)
        {
            var card = new Rectangle(
                cardsLeft + index * (CardWidth + CardSpacing),
                CardTop,
                CardWidth,
                CardHeight);
            DrawSolutionCard(spriteBatch, card, _solutions[index], index);
        }
    }

    private void DrawJustificationStep(SpriteBatch spriteBatch)
    {
        DrawCenteredText(
            spriteBatch,
            AffordanceChoiceCatalog.JustificationQuestion,
            new Vector2(LogicalWidth / 2.0f, 180.0f),
            Color.White,
            TitleScale);

        const int columns = 2;
        int gridWidth = columns * JustificationCardWidth + JustificationColumnSpacing;
        int gridLeft = (LogicalWidth - gridWidth) / 2;

        for (int index = 0; index < _justifications.Count; index++)
        {
            int row = index / columns;
            int column = index % columns;
            var card = new Rectangle(
                gridLeft + column * (JustificationCardWidth + JustificationColumnSpacing),
                JustificationTop + row * (JustificationCardHeight + JustificationRowSpacing),
                JustificationCardWidth,
                JustificationCardHeight);
            DrawJustificationCard(spriteBatch, card, _justifications[index], index);
        }
    }

    private void DrawSolutionCard(
        SpriteBatch spriteBatch,
        Rectangle card,
        InterventionSolutionDefinition solution,
        int index)
    {
        bool selected = index == _selectedIndex;
        Color outlineColor = selected ? Color.Yellow : Color.Gray;

        spriteBatch.Draw(_pixel, card, new Color(45, 48, 64));
        DrawOutline(spriteBatch, card, outlineColor, selected ? 6 : 3);

        var titleArea = new Rectangle(
            card.X + TextPadding,
            card.Y + TextPadding,
            card.Width - TextPadding * 2,
            130);
        DrawWrappedText(spriteBatch, solution.Title, titleArea, Color.White, TextScale);

        var descriptionArea = new Rectangle(
            card.X + TextPadding,
            card.Y + 180,
            card.Width - TextPadding * 2,
            card.Height - 210);
        DrawWrappedText(
            spriteBatch,
            solution.Description,
            descriptionArea,
            Color.LightGray,
            TextScale);
    }

    private void DrawJustificationCard(
        SpriteBatch spriteBatch,
        Rectangle card,
        InterventionJustificationDefinition justification,
        int index)
    {
        bool selected = index == _selectedIndex;
        spriteBatch.Draw(_pixel, card, new Color(45, 48, 64));
        DrawOutline(spriteBatch, card, selected ? Color.Yellow : Color.Gray, selected ? 6 : 3);

        var textArea = new Rectangle(
            card.X + TextPadding,
            card.Y + TextPadding,
            card.Width - TextPadding * 2,
            card.Height - TextPadding * 2);
        DrawWrappedText(spriteBatch, justification.Text, textArea, Color.White, TextScale);
    }

    private void DrawWrappedText(
        SpriteBatch spriteBatch,
        string text,
        Rectangle area,
        Color color,
        float scale)
    {
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string line = string.Empty;
        float y = area.Y;
        float lineHeight = _font.LineSpacing * scale + 8.0f;

        foreach (string word in words)
        {
            string candidate = string.IsNullOrEmpty(line) ? word : line + " " + word;

            if (_font.MeasureString(candidate).X * scale > area.Width &&
                !string.IsNullOrEmpty(line))
            {
                spriteBatch.DrawString(_font, line, new Vector2(area.X, y), color, 0.0f,
                    Vector2.Zero, scale, SpriteEffects.None, 0.0f);
                line = word;
                y += lineHeight;
            }
            else
            {
                line = candidate;
            }
        }

        if (!string.IsNullOrEmpty(line) && y < area.Bottom)
        {
            spriteBatch.DrawString(_font, line, new Vector2(area.X, y), color, 0.0f,
                Vector2.Zero, scale, SpriteEffects.None, 0.0f);
        }
    }

    private void DrawCenteredText(
        SpriteBatch spriteBatch,
        string text,
        Vector2 center,
        Color color,
        float scale)
    {
        Vector2 size = _font.MeasureString(text) * scale;
        spriteBatch.DrawString(
            _font,
            text,
            center - size / 2.0f,
            color,
            0.0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0.0f);
    }

    private void DrawOutline(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, thickness, rectangle.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Right - thickness, rectangle.Top, thickness, rectangle.Height), color);
    }

    private bool KeyWasPressed(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }
}
