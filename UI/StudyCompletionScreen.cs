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
    RETURN_TO_MENU = 3,
    OPEN_EXPORT_FOLDER = 4
}

public sealed class StudyCompletionScreen
{
    private readonly PhaseIntroductionScreen _presentation;
    private SessionCompletionData _session;
    private string _exportPath;
    private string _exportError;
    private string _formStatus;

    public string ExportPath => _exportPath;

    public StudyCompletionScreen(SpriteFont titleFont, SpriteFont font, Vector2 dimensions, Texture2D symbols)
    {
        _presentation = new PhaseIntroductionScreen(titleFont, font, dimensions, symbols);
    }

    public void Open(
        SessionCompletionData session,
        string exportPath,
        string exportError)
    {
        _session = session;
        _exportPath = exportPath;
        _exportError = exportError;
        // While the MEEGA+ form is not configured (expert beta), the screen does not mention it.
        _formStatus = ResearchStudyConfiguration.IsMeegaFormConfigured
            ? "Questionário MEEGA+ pronto para abrir."
            : null;
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

        if (!string.IsNullOrEmpty(_exportPath) &&
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.O))
        {
            return StudyCompletionAction.OPEN_EXPORT_FOLDER;
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
            text.AppendLine("Envie este arquivo ao pesquisador (O abre a pasta):");
            text.AppendLine(_exportPath);
        }
        else
        {
            text.AppendLine(_exportError ?? "A exportação local não foi concluída.");
        }

        if (!string.IsNullOrEmpty(_formStatus))
        {
            text.AppendLine();
            text.Append(_formStatus);
        }

        string navigation =
            (ResearchStudyConfiguration.IsMeegaFormConfigured ? "F: abrir MEEGA+    " : "") +
            (!string.IsNullOrEmpty(_exportPath) ? "O: abrir pasta    " : "") +
            "R: exportar novamente    Enter: voltar ao menu";
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
