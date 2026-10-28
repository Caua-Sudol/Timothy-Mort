using System;
using System.IO;
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
    // Leaving stays locked until the folder was opened (or failed to open).
    private bool _folderOpenAttempted;

    public string ExportPath => _exportPath;

    // Without an exported file there is no folder to open, so leaving is allowed.
    private bool CanLeave => _folderOpenAttempted || string.IsNullOrEmpty(_exportPath);

    public StudyCompletionScreen(SpriteFont titleFont, SpriteFont font, Vector2 dimensions, Texture2D symbols)
    {
        _presentation = new PhaseIntroductionScreen(titleFont, font, dimensions, symbols);
        // Wider and taller than the introduction so the text keeps a readable size;
        // the height stops just above the navigation line.
        _presentation.SetBodyArea(0.10f, 0.80f, 0.47f);
    }

    public void Open(
        SessionCompletionData session,
        string exportPath,
        string exportError)
    {
        _session = session;
        _exportPath = exportPath;
        _exportError = exportError;
        _folderOpenAttempted = false;
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

    // Called after trying to open the folder; success or failure unlocks leaving.
    public void SetFolderStatus(string folderStatus)
    {
        _folderOpenAttempted = true;
        SetFormStatus(folderStatus);
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
            if (CanLeave)
            {
                return StudyCompletionAction.RETURN_TO_MENU;
            }

            SetFormStatus("Abra a pasta antes de sair (tecla O).");
            return StudyCompletionAction.NONE;
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
                $"  Escolha: {GetSolutionTitle(phase)} | " +
                $"Tempo: {totalSeconds / 60:00}:{totalSeconds % 60:00} | " +
                $"Mortes: {phase.TotalDeaths}");
        }

        text.AppendLine();
        if (!string.IsNullOrEmpty(_exportPath))
        {
            // Folder and file name on separate lines, so the long path does not shrink the text.
            text.AppendLine("1. Seus dados foram salvos neste arquivo:");
            text.AppendLine("   Pasta: " + Path.GetDirectoryName(_exportPath));
            text.AppendLine("   Arquivo: " + Path.GetFileName(_exportPath));
            text.AppendLine("2. Aperte O para abrir a pasta.");
            text.AppendLine($"3. Envie o arquivo para {ResearchStudyConfiguration.ResearchEmail}");
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
            "R: exportar novamente" +
            (CanLeave ? "    Enter: voltar ao seletor" : "");
        _presentation.Open("PESQUISA CONCLUÍDA", text.ToString(), navigation);
        // Session data and export status are shown at once, without typing.
        _presentation.CompleteTyping();
    }

    private static string GetSolutionTitle(PhaseCompletionData phase)
    {
        if (string.IsNullOrEmpty(phase.FinalSolutionId))
        {
            return "Nenhuma";
        }

        return PhaseCatalog.GetForScenario(phase.PhaseId)?.ChoiceCatalog
            .GetSolution(phase.FinalSolutionId)?.Title ?? "-";
    }

    private static bool KeyWasPressed(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }
}
