using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
    private const int TextPadding = 32;

    private readonly SpriteFont _solutionTitleFont;
    private readonly SpriteFont _solutionBodyFont;
    private Texture2D _solutionFrame;
    private Texture2D _solutionCard;
    private Texture2D _selectedSolutionCard;
    private Texture2D _justificationCard;
    private Texture2D _selectedJustificationCard;
    private static readonly float[] JustificationColumns = { 249.3f, 1007.3f };
    private static readonly float[] JustificationRows = { 266.7f, 460.0f, 653.3f };
    private static readonly float[] SolutionCardPositions = { 227.3f, 736.7f, 1243.3f };
    private static readonly Color PaperTextColor = new Color(37, 40, 43);
    private static readonly Color SolutionTextColor = new Color(216, 208, 187);
    private IReadOnlyList<InterventionSolutionDefinition> _solutions;
    private IReadOnlyList<InterventionJustificationDefinition> _justifications;
    private int _selectedIndex;
    private InterventionSolutionDefinition _completedSolution;
    private InterventionJustificationDefinition _completedJustification;
    private bool _continueTestingRequested;
    private string _justificationQuestion;
    private string _solutionQuestion;
    private ChoiceStep _choiceStep;

    // Extra card after the justifications when leaving the intervention room.
    // Tinted red like the sword's smoke, so it reads as "go back to the sword".
    private const string ContinueTestingText = "Continuar testando soluções";
    private static readonly Color ContinueTestingTint = new Color(255, 150, 140);

    private enum ChoiceStep
    {
        SOLUTION,
        JUSTIFICATION
    }

    public InterventionChoiceMenu(SpriteFont font, SpriteFont titleFont, SpriteFont bodyFont)
    {
        _solutionTitleFont = titleFont;
        _solutionBodyFont = bodyFont;
    }

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
    {
        _solutionFrame = content.Load<Texture2D>("ui/frame_cards");
        _solutionCard = content.Load<Texture2D>("ui/card_solution_no_selected");
        _selectedSolutionCard = content.Load<Texture2D>("ui/card_solution_selected");
        _justificationCard = content.Load<Texture2D>("ui/card_intervention_no_selected");
        _selectedJustificationCard = content.Load<Texture2D>("ui/card_intervention_selected");
    }

    // Sword in the intervention room: pick a solution to try (no justification here).
    public void OpenSolutions(IReadOnlyList<InterventionSolutionDefinition> solutions,
        string solutionQuestion = null)
    {
        Open(solutions, Array.Empty<InterventionJustificationDefinition>(),
            solutionQuestion, null, ChoiceStep.SOLUTION);
    }

    // Leaving the intervention room: the justifications plus "keep testing".
    public void OpenExitJustification(
        IReadOnlyList<InterventionJustificationDefinition> justifications,
        string justificationQuestion)
    {
        Open(Array.Empty<InterventionSolutionDefinition>(), justifications,
            null, justificationQuestion, ChoiceStep.JUSTIFICATION);
    }

    private void Open(
        IReadOnlyList<InterventionSolutionDefinition> solutions,
        IReadOnlyList<InterventionJustificationDefinition> justifications,
        string solutionQuestion,
        string justificationQuestion,
        ChoiceStep step)
    {
        _solutions = solutions;
        _justifications = justifications;
        _selectedIndex = 0;
        _completedSolution = null;
        _completedJustification = null;
        _continueTestingRequested = false;
        _solutionQuestion = solutionQuestion;
        _justificationQuestion = justificationQuestion;
        _choiceStep = step;
    }

    private bool IsOpen => _choiceStep == ChoiceStep.SOLUTION
        ? _solutions is { Count: > 0 }
        : _justifications is { Count: > 0 };

    private int JustificationOptionCount => _justifications.Count + 1;

    public void Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        if (!IsOpen || _completedSolution != null || _completedJustification != null ||
            _continueTestingRequested)
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

    public bool TryTakeCompletedSolution(out InterventionSolutionDefinition solution)
    {
        solution = _completedSolution;
        if (solution == null) return false;
        _completedSolution = null;
        return true;
    }

    public bool TryTakeExitJustification(out InterventionJustificationDefinition justification)
    {
        justification = _completedJustification;
        if (justification == null) return false;
        _completedJustification = null;
        return true;
    }

    public bool TryTakeContinueTesting()
    {
        if (!_continueTestingRequested) return false;
        _continueTestingRequested = false;
        return true;
    }

    private void UpdateSolutionSelection(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState)
    {
        if (MenuInput.Left(keyboardState, previousKeyboardState) ||
            MenuInput.Up(keyboardState, previousKeyboardState))
        {
            _selectedIndex = (_selectedIndex - 1 + _solutions.Count) % _solutions.Count;
        }
        else if (MenuInput.Right(keyboardState, previousKeyboardState) ||
            MenuInput.Down(keyboardState, previousKeyboardState))
        {
            _selectedIndex = (_selectedIndex + 1) % _solutions.Count;
        }

        bool clicked = false;
        for (int index = 0; index < _solutions.Count && !clicked; index++)
        {
            Vector2 position = GetSolutionCardPosition(index);
            clicked = UiPointer.UpdateOption(
                new Rectangle((int)position.X, (int)position.Y, CardWidth, CardHeight),
                index, ref _selectedIndex);
        }

        if (clicked || MenuInput.Confirm(keyboardState, previousKeyboardState))
        {
            _completedSolution = _solutions[_selectedIndex];
        }
    }

    private void UpdateJustificationSelection(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState)
    {
        const int columns = 2;
        int optionCount = JustificationOptionCount;
        int row = _selectedIndex / columns;
        int column = _selectedIndex % columns;
        int rowCount = (optionCount + columns - 1) / columns;

        if (MenuInput.Left(keyboardState, previousKeyboardState))
        {
            column = (column - 1 + columns) % columns;
        }
        else if (MenuInput.Right(keyboardState, previousKeyboardState))
        {
            column = (column + 1) % columns;
        }
        else if (MenuInput.Up(keyboardState, previousKeyboardState))
        {
            row = (row - 1 + rowCount) % rowCount;
        }
        else if (MenuInput.Down(keyboardState, previousKeyboardState))
        {
            row = (row + 1) % rowCount;
        }

        int candidateIndex = row * columns + column;
        if (candidateIndex < optionCount)
        {
            _selectedIndex = candidateIndex;
        }

        bool clicked = false;
        for (int index = 0; index < optionCount && !clicked; index++)
        {
            Vector2 position = GetJustificationCardPosition(index, optionCount);
            clicked = UiPointer.UpdateOption(
                new Rectangle((int)position.X, (int)position.Y,
                    JustificationCardWidth, JustificationCardHeight),
                index, ref _selectedIndex);
        }

        if (clicked || MenuInput.Confirm(keyboardState, previousKeyboardState))
        {
            // The last card is "keep testing": close without a justification.
            if (_selectedIndex == _justifications.Count)
                _continueTestingRequested = true;
            else
                _completedJustification = _justifications[_selectedIndex];
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen)
        {
            return;
        }

        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);

        var panel = new Rectangle(
            PanelMargin,
            PanelMargin,
            LogicalWidth - PanelMargin * 2,
            LogicalHeight - PanelMargin * 2);
        spriteBatch.Draw(_solutionFrame, panel, Color.White);

        if (_choiceStep == ChoiceStep.SOLUTION)
        {
            DrawSolutionStep(spriteBatch);
        }
        else
        {
            DrawJustificationStep(spriteBatch);
        }

        string footer = _choiceStep == ChoiceStep.SOLUTION
            ? "A / D ou setas para navegar    Enter para escolher"
            : "WASD ou setas para navegar    Enter para confirmar";
        if (_choiceStep == ChoiceStep.SOLUTION)
        {
            DrawFittedSolutionText(spriteBatch, _solutionBodyFont, footer,
                new Vector2(488.7f, 833.3f), new Vector2(994.7f, 53.3f),
                PaperTextColor, 1.0f, true);
        }
        else
        {
            DrawFittedSolutionText(spriteBatch, _solutionBodyFont, footer,
                new Vector2(488.7f, 841.3f), new Vector2(994.7f, 45.3f),
                PaperTextColor, 1.0f, true);
        }

        spriteBatch.End();
    }

    private void DrawSolutionStep(SpriteBatch spriteBatch)
    {
        string question = _solutionQuestion ??
            "Que intervenção você aplicaria para resolver esse problema?";
        DrawFittedSolutionText(spriteBatch, _solutionTitleFont, question,
            new Vector2(248.7f, 185.3f), new Vector2(1456.0f, 82.7f),
            PaperTextColor, 0.55f, true);

        for (int index = 0; index < _solutions.Count; index++)
            DrawSolutionCard(spriteBatch, GetSolutionCardPosition(index), _solutions[index], index);
    }

    private Vector2 GetSolutionCardPosition(int index)
    {
        // The three-card layout follows the exact Affinity positions.
        // Fewer options stay centered.
        float cardsWidth = _solutions.Count * CardWidth + (_solutions.Count - 1) * CardSpacing;
        float cardsLeft = (LogicalWidth - cardsWidth) / 2.0f;
        float x = _solutions.Count == 3
            ? SolutionCardPositions[index]
            : cardsLeft + index * (CardWidth + CardSpacing);
        return new Vector2(x, CardTop);
    }

    private static Vector2 GetJustificationCardPosition(int index, int optionCount)
    {
        const int columns = 2;
        // Up to four cards use two rows, centred on the three-row layout.
        float rowOffset = optionCount <= 4
            ? (JustificationRows[1] - JustificationRows[0]) / 2.0f
            : 0.0f;
        return new Vector2(JustificationColumns[index % columns],
            JustificationRows[index / columns] + rowOffset);
    }

    private void DrawJustificationStep(SpriteBatch spriteBatch)
    {
        DrawFittedSolutionText(spriteBatch, _solutionTitleFont, _justificationQuestion,
            new Vector2(249.3f, 172.0f), new Vector2(1456.0f, 72.0f),
            PaperTextColor, 0.55f, true);

        int optionCount = JustificationOptionCount;
        for (int index = 0; index < _justifications.Count; index++)
            DrawJustificationCard(spriteBatch, GetJustificationCardPosition(index, optionCount),
                _justifications[index].Text, index, Color.White);
        DrawJustificationCard(spriteBatch, GetJustificationCardPosition(_justifications.Count, optionCount),
            ContinueTestingText, _justifications.Count, ContinueTestingTint);
    }

    private void DrawSolutionCard(
        SpriteBatch spriteBatch,
        Vector2 position,
        InterventionSolutionDefinition solution,
        int index)
    {
        Texture2D texture = index == _selectedIndex ? _selectedSolutionCard : _solutionCard;
        spriteBatch.Draw(texture, position, null, Color.White, 0.0f, Vector2.Zero,
            new Vector2((float)CardWidth / texture.Width, (float)CardHeight / texture.Height),
            SpriteEffects.None, 0.0f);

        // Keep the heading above the painted divider and the description below it.
        DrawFittedSolutionText(spriteBatch, _solutionTitleFont, solution.Title,
            position + new Vector2(TextPadding, 24.0f),
            new Vector2(CardWidth - TextPadding * 2, 72.0f),
            SolutionTextColor, 0.42f, true);
        DrawFittedSolutionText(spriteBatch, _solutionBodyFont, solution.Description,
            position + new Vector2(TextPadding, 164.0f),
            new Vector2(CardWidth - TextPadding * 2, CardHeight - 164.0f - TextPadding),
            SolutionTextColor, 0.9f, false);
    }

    private static void DrawFittedSolutionText(
        SpriteBatch spriteBatch, SpriteFont font, string text,
        Vector2 position, Vector2 bounds, Color color, float maximumScale, bool centered)
    {
        float scale = maximumScale;
        List<string> lines;
        float width;
        float height;
        while (true)
        {
            lines = new List<string>();
            width = 0.0f;
            foreach (string paragraph in text.Replace("\r\n", "\n").Split('\n'))
            {
                string line = string.Empty;
                foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    string candidate = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && font.MeasureString(candidate).X * scale > bounds.X)
                    {
                        lines.Add(line);
                        line = word;
                    }
                    else
                    {
                        line = candidate;
                    }
                }
                lines.Add(line);
            }
            foreach (string line in lines)
                width = Math.Max(width, font.MeasureString(line).X * scale);
            height = (Math.Max(0, lines.Count - 1) * (font.LineSpacing + 4.0f)
                + font.MeasureString(lines[lines.Count - 1]).Y) * scale;
            if (width <= bounds.X && height <= bounds.Y)
                break;
            scale *= 0.95f;
        }

        float y = position.Y + (centered ? (bounds.Y - height) / 2.0f : 0.0f);
        foreach (string line in lines)
        {
            float x = position.X + (centered
                ? (bounds.X - font.MeasureString(line).X * scale) / 2.0f : 0.0f);
            spriteBatch.DrawString(font, line, new Vector2(x, y), color,
                0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
            y += (font.LineSpacing + 4.0f) * scale;
        }
    }

    private void DrawJustificationCard(
        SpriteBatch spriteBatch,
        Vector2 position,
        string text,
        int index,
        Color tint)
    {
        Texture2D texture = index == _selectedIndex ? _selectedJustificationCard : _justificationCard;
        spriteBatch.Draw(texture, position, null, tint, 0.0f, Vector2.Zero,
            new Vector2((float)JustificationCardWidth / texture.Width,
                (float)JustificationCardHeight / texture.Height),
            SpriteEffects.None, 0.0f);
        DrawFittedSolutionText(spriteBatch, _solutionBodyFont, text,
            position + new Vector2(TextPadding),
            new Vector2(JustificationCardWidth - TextPadding * 2,
                JustificationCardHeight - TextPadding * 2),
            SolutionTextColor, 0.9f, true);
    }
}
