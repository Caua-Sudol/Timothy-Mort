using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class PhaseResultScreen
{
    private readonly PhaseIntroductionScreen _presentation;
    private readonly SpriteFont _titleFont;
    private readonly SpriteFont _font;
    private readonly Vector2 _dimensions;
    private string _summary = string.Empty;
    private string _explanation = string.Empty;
    private string _persistenceMessage;
    private float _columnScale = 1.0f;
    private string _explanationHeading = "Sobre sua escolha";
    // Affinity layout, measured from the top-left of a 1920 x 1080 document.
    private float SummaryX => _dimensions.X * (321.3f / 1920.0f);
    private float SummaryWidth => _dimensions.X * (480.0f / 1920.0f);
    private float SummaryHeight => _dimensions.Y * (477.3f / 1080.0f);
    private float ExplanationX => _dimensions.X * (960.0f / 1920.0f);
    private float ExplanationWidth => _dimensions.X * (722.5f / 1920.0f);
    private float ExplanationHeight => _dimensions.Y * (238.7f / 1080.0f);
    private float HeadingY => _dimensions.Y * (409.3f / 1080.0f);
    private float BodyY => HeadingY + (_font.LineSpacing + 24.0f) * _columnScale;
    private float FooterY => _dimensions.Y * 0.83f;

    public PhaseResultScreen(SpriteFont titleFont, SpriteFont font, Vector2 dimensions)
    {
        _presentation = new PhaseIntroductionScreen(titleFont, font, dimensions);
        _titleFont = titleFont;
        _font = font;
        _dimensions = dimensions;
    }

    public void Open(PhaseState state, string persistenceMessage = null)
    {
        InterventionSolutionDefinition firstSolution = state.HasFirstChoice
            ? state.ChoiceCatalog.GetSolution(state.FirstSolutionId)
            : null;
        InterventionSolutionDefinition finalSolution = string.IsNullOrEmpty(state.AppliedSolutionId)
            ? null
            : state.ChoiceCatalog.GetSolution(state.AppliedSolutionId);

        long totalSeconds = (long)Math.Floor(state.ElapsedSeconds);
        var text = new StringBuilder();
        if (state.HasRevision)
        {
            text.AppendLine($"Escolha inicial: {GetSolutionText(firstSolution)}");
            text.AppendLine($"Avaliação: {GetQualityText(firstSolution?.Quality)}");
            text.AppendLine();
            text.AppendLine($"Após revisão: {GetSolutionText(finalSolution)}");
            text.AppendLine($"Avaliação: {GetQualityText(finalSolution?.Quality)}");
        }
        else
        {
            text.AppendLine($"Escolha: {GetSolutionText(finalSolution ?? firstSolution)}");
            text.AppendLine($"Avaliação: {GetQualityText((finalSolution ?? firstSolution)?.Quality)}");
            text.AppendLine("Sem revisão");
        }
        text.AppendLine();
        text.AppendLine($"Tempo da fase: {totalSeconds / 60:00}:{totalSeconds % 60:00}");
        text.AppendLine($"Mortes: {state.TotalDeathCount}");
        _explanationHeading = state.HasRevision ? "Sobre a escolha inicial" : "Sobre sua escolha";
        string explanation = PhaseResultTextCatalog.GetExplanation(state.PhaseId, firstSolution?.Quality);
        if (!state.HasRevision)
        {
            explanation = explanation.Replace("A primeira intervenção", "A intervenção");
        }

        // Capture and fit both columns once, keeping the same type size in each.
        _columnScale = 1.0f;
        for (int attempt = 0; attempt < 50; attempt++)
        {
            _summary = WrapText(text.ToString().TrimEnd(), SummaryWidth / _columnScale);
            _explanation = WrapText(explanation, ExplanationWidth / _columnScale);
            Vector2 summarySize = _font.MeasureString(_summary) * _columnScale;
            Vector2 explanationSize = _font.MeasureString(_explanation) * _columnScale;
            float headingHeight = BodyY - HeadingY;
            if (summarySize.Y + headingHeight <= SummaryHeight &&
                explanationSize.Y + headingHeight <= ExplanationHeight &&
                summarySize.X <= SummaryWidth && explanationSize.X <= ExplanationWidth &&
                _font.MeasureString("Sua tentativa").X * _columnScale <= SummaryWidth &&
                _font.MeasureString(_explanationHeading).X * _columnScale <= ExplanationWidth)
            {
                break;
            }
            _columnScale *= 0.95f;
        }
        _persistenceMessage = persistenceMessage;
        _presentation.Open(string.Empty, string.Empty);
    }

    public void Update(double deltaTime)
    {
        _presentation.Update(deltaTime);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _presentation.DrawBackground(spriteBatch, pixel);
        Color primary = new Color(216, 208, 187);
        Vector2 titlePosition = new Vector2(561.3f / 1920.0f, 150.7f / 1080.0f) * _dimensions;
        Vector2 titleBounds = new Vector2(797.3f / 1920.0f, 126.7f / 1080.0f) * _dimensions;
        Vector2 titleSize = _titleFont.MeasureString("FASE CONCLUÍDA");
        float titleScale = Math.Min(titleBounds.X / titleSize.X, titleBounds.Y / titleSize.Y);
        spriteBatch.DrawString(_titleFont, "FASE CONCLUÍDA", titlePosition, primary,
            0.0f, Vector2.Zero, titleScale, SpriteEffects.None, 0.0f);
        spriteBatch.DrawString(_font, "Sua tentativa", new Vector2(SummaryX, HeadingY), primary,
            0.0f, Vector2.Zero, _columnScale, SpriteEffects.None, 0.0f);
        spriteBatch.DrawString(_font, _explanationHeading, new Vector2(ExplanationX, HeadingY), primary,
            0.0f, Vector2.Zero, _columnScale, SpriteEffects.None, 0.0f);
        spriteBatch.DrawString(_font, _summary,
            new Vector2(SummaryX, BodyY), primary,
            0.0f, Vector2.Zero, _columnScale, SpriteEffects.None, 0.0f);
        spriteBatch.DrawString(_font, _explanation,
            new Vector2(ExplanationX, BodyY), primary,
            0.0f, Vector2.Zero, _columnScale, SpriteEffects.None, 0.0f);
        if (!string.IsNullOrEmpty(_persistenceMessage))
        {
            Color persistenceColor = _persistenceMessage.StartsWith("Não foi possível", StringComparison.Ordinal)
                ? Color.OrangeRed : new Color(166, 155, 174);
            DrawCenteredText(spriteBatch, _font, _persistenceMessage, FooterY / _dimensions.Y, persistenceColor, 0.75f);
        }
        float navigationY = FooterY;
        if (!string.IsNullOrEmpty(_persistenceMessage))
        {
            float messageScale = Math.Min(0.75f,
                _dimensions.X * 0.8f / Math.Max(1.0f, _font.MeasureString(_persistenceMessage).X));
            navigationY += _font.MeasureString(_persistenceMessage).Y * messageScale + 24.0f;
        }
        DrawCenteredText(spriteBatch, _font, "Enter ou Esc para voltar ao seletor",
            navigationY / _dimensions.Y, new Color(181, 173, 161), 0.75f);
        spriteBatch.End();
    }

    private void DrawCenteredText(SpriteBatch batch, SpriteFont font, string text, float y, Color color, float scale)
    {
        Vector2 size = font.MeasureString(text);
        scale = Math.Min(scale, _dimensions.X * 0.8f / Math.Max(1.0f, size.X));
        batch.DrawString(font, text, new Vector2((_dimensions.X - size.X * scale) / 2, _dimensions.Y * y),
            color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
    }

    private string WrapText(string text, float width)
    {
        var result = new StringBuilder();
        foreach (string paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            string line = string.Empty;
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = line.Length == 0 ? word : line + " " + word;
                if (line.Length > 0 && _font.MeasureString(candidate).X > width)
                {
                    result.AppendLine(line);
                    line = word;
                }
                else
                {
                    line = candidate;
                }
            }
            result.AppendLine(line);
        }
        return result.ToString().TrimEnd();
    }

    private static string GetSolutionText(InterventionSolutionDefinition solution)
    {
        return solution == null ? "Nenhuma" : solution.Title;
    }

    private static string GetQualityText(SolutionQuality? quality)
    {
        return quality switch
        {
            SolutionQuality.INADEQUATE => "Inadequada",
            SolutionQuality.PARTIAL => "Parcial",
            SolutionQuality.ADEQUATE => "Adequada",
            _ => "Sem escolha"
        };
    }
}
