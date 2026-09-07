using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class PhaseResultScreen
{
    private readonly PhaseIntroductionScreen _presentation;

    public PhaseResultScreen(SpriteFont font, Vector2 dimensions)
    {
        _presentation = new PhaseIntroductionScreen(font, dimensions);
    }

    public void Open(PhaseState state)
    {
        InterventionSolutionDefinition firstSolution = state.HasFirstChoice
            ? state.ChoiceCatalog.GetSolution(state.FirstSolutionId)
            : null;
        InterventionSolutionDefinition finalSolution = string.IsNullOrEmpty(state.AppliedSolutionId)
            ? null
            : state.ChoiceCatalog.GetSolution(state.AppliedSolutionId);

        long totalSeconds = (long)Math.Floor(state.ElapsedSeconds);
        var text = new StringBuilder();
        text.AppendLine(PhaseIntroductionCatalog.Get(state.PhaseId).Title);
        text.AppendLine();
        text.AppendLine($"Primeira hipótese: {GetQualityText(firstSolution?.Quality)}");
        text.AppendLine($"Primeira escolha: {GetSolutionText(firstSolution)}");
        text.AppendLine($"Revisão: {(state.HasRevision ? "Realizada" : "Não realizada")}");
        text.AppendLine($"Solução final: {GetSolutionText(finalSolution)}");
        text.AppendLine($"Qualidade final: {GetQualityText(finalSolution?.Quality)}");
        text.AppendLine();
        text.AppendLine($"Tempo da fase: {totalSeconds / 60:00}:{totalSeconds % 60:00}");
        text.AppendLine($"Mortes: {state.TotalDeathCount}");
        text.AppendLine();
        text.AppendLine("Sobre a primeira escolha:");
        text.Append(PhaseResultTextCatalog.GetExplanation(state.PhaseId, firstSolution?.Quality));

        // Capture the result now so time and counts remain fixed while this screen is open.
        _presentation.Open(
            "FASE CONCLUÍDA", text.ToString(), "Enter ou Esc para voltar ao seletor");
    }

    public void Update(double deltaTime)
    {
        _presentation.Update(deltaTime);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        _presentation.Draw(spriteBatch, pixel);
    }

    private static string GetSolutionText(InterventionSolutionDefinition solution)
    {
        return solution == null ? "Nenhuma" : $"{solution.Id} - {solution.Title}";
    }

    private static string GetQualityText(SolutionQuality? quality)
    {
        return quality switch
        {
            SolutionQuality.INADEQUATE => "Inadequada (I)",
            SolutionQuality.PARTIAL => "Parcial (P)",
            SolutionQuality.ADEQUATE => "Adequada (A)",
            _ => "Sem escolha"
        };
    }
}
