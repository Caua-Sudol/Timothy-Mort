using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum StudyCompletionAction
{
    NONE = 0,
    OPEN_FORM = 1,
    RETRY_EXPORT = 2,
    RETURN_TO_MENU = 3
}

public sealed class StudyCompletionScreen
{
    private readonly PhaseIntroductionScreen _presentation;
    private SessionCompletionData _session;
    private string _exportPath;
    private string _exportError;
    private string _formStatus;

    public StudyCompletionScreen(SpriteFont font, Vector2 dimensions)
    {
        _presentation = new PhaseIntroductionScreen(font, dimensions);
    }

    public void Open(
        SessionCompletionData session,
        string exportPath,
        string exportError)
    {
        _session = session;
        _exportPath = exportPath;
        _exportError = exportError;
        _formStatus = ResearchStudyConfiguration.IsMeegaFormConfigured
            ? "Questionário MEEGA+ pronto para abrir."
            : "Questionário MEEGA+ ainda não configurado.";
        RefreshPresentation();
    }

    public void SetExportResult(string exportPath, string exportError)
    {
        _exportPath = exportPath;
        _exportError = exportError;
        RefreshPresentation();
    }

    public void SetFormStatus(string formStatus)
    {
        _formStatus = formStatus;
        RefreshPresentation();
    }

    public StudyCompletionAction Update(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        double deltaTime)
    {
        _presentation.Update(deltaTime);

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.Escape))
        {
            return StudyCompletionAction.RETURN_TO_MENU;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.R))
        {
            return StudyCompletionAction.RETRY_EXPORT;
        }

        if (ResearchStudyConfiguration.IsMeegaFormConfigured &&
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.F))
        {
            return StudyCompletionAction.OPEN_FORM;
        }

        return StudyCompletionAction.NONE;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        _presentation.Draw(spriteBatch, pixel);
    }

    private void RefreshPresentation()
    {
        var text = new StringBuilder();
        text.AppendLine($"Sessão: {_session.PublicCode}");
        text.AppendLine();

        foreach (PhaseCompletionData phase in _session.Phases)
        {
            long totalSeconds = (long)Math.Floor(phase.ActiveSeconds);
            string title = PhaseIntroductionCatalog.Get(phase.PhaseId).Title;
            text.AppendLine(title);
            text.AppendLine(
                $"  Primeira: {GetQualityText(phase.FirstSolutionQuality)} | " +
                $"Final: {GetQualityText(phase.FinalSolutionQuality)} | " +
                $"Tempo: {totalSeconds / 60:00}:{totalSeconds % 60:00} | " +
                $"Mortes: {phase.TotalDeaths}");
        }

        text.AppendLine();
        if (!string.IsNullOrEmpty(_exportPath))
        {
            text.AppendLine("Dados exportados localmente:");
            text.AppendLine(_exportPath);
        }
        else
        {
            text.AppendLine(_exportError ?? "A exportação local não foi concluída.");
        }

        text.AppendLine();
        text.Append(_formStatus);

        string navigation = ResearchStudyConfiguration.IsMeegaFormConfigured
            ? "F: abrir MEEGA+    R: exportar novamente    Enter: voltar ao menu"
            : "R: exportar novamente    Enter: voltar ao menu";
        _presentation.Open("PESQUISA CONCLUÍDA", text.ToString(), navigation);
    }

    private static string GetQualityText(string quality)
    {
        return quality switch
        {
            nameof(SolutionQuality.INADEQUATE) => "I",
            nameof(SolutionQuality.PARTIAL) => "P",
            nameof(SolutionQuality.ADEQUATE) => "A",
            _ => "-"
        };
    }

    private static bool KeyWasPressed(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }
}
