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
        // Two blocks so each keeps a readable size: summary on the left, how to send on
        // the right. The height stops just above the navigation line; short blocks may grow.
        _presentation.SetBodyArea(0.08f, 0.38f, 0.47f);
        _presentation.SetSecondColumn(0.52f, 0.42f);
        _presentation.SetMaximumBodyScale(1.2f);
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
        // The MEEGA+ step is part of the instructions; the status line starts empty.
        _formStatus = null;
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
        // Left block: what was played.
        var summary = new StringBuilder();
        summary.AppendLine("RESUMO");
        summary.AppendLine($"Sessão: {_session.PublicCode}");
        foreach (PhaseCompletionData phase in _session.Phases)
        {
            long totalSeconds = (long)Math.Floor(phase.ActiveSeconds);
            string title = PhaseIntroductionCatalog.Get(phase.PhaseId).Title;
            summary.AppendLine();
            summary.AppendLine(title);
            summary.AppendLine(
                $"{GetSolutionTitle(phase)} · {totalSeconds / 60:00}:{totalSeconds % 60:00} · " +
                $"{phase.TotalDeaths} mortes");
        }

        // Right block: how to send the data.
        var text = new StringBuilder();
        text.AppendLine("COMO ENVIAR");
        if (!string.IsNullOrEmpty(_exportPath))
        {
            text.AppendLine("1. Seus dados foram salvos em:");
            // The folder path is broken at its separators so it does not shrink the block.
            text.AppendLine(BreakPath(Path.GetDirectoryName(_exportPath)));
            text.AppendLine(Path.GetFileName(_exportPath));
            text.AppendLine();
            text.AppendLine("2. Aperte O para abrir a pasta.");
            text.AppendLine();
            text.AppendLine("3. Envie o arquivo para");
            text.AppendLine(ResearchStudyConfiguration.ResearchEmail);
        }
        else
        {
            text.AppendLine(_exportError ?? "A exportação local não foi concluída.");
        }

        // Only when the form is configured (student builds).
        if (ResearchStudyConfiguration.IsMeegaFormConfigured)
        {
            text.AppendLine();
            text.AppendLine(string.IsNullOrEmpty(_exportPath)
                ? "Aperte F para responder o questionário."
                : "4. Aperte F para responder o questionário.");
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
        _presentation.Open("PESQUISA CONCLUÍDA", summary.ToString(), navigation, text.ToString());
        // Session data and export status are shown at once, without typing.
        _presentation.CompleteTyping();
    }

    // Splits a long folder path into lines of about 32 characters, always after a separator,
    // so the path stays complete but does not force the whole block to a small size.
    private static string BreakPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        const int lineLength = 32;
        var result = new StringBuilder();
        int lineStart = 0;
        for (int index = 0; index < path.Length; index++)
        {
            bool separator = path[index] == Path.DirectorySeparatorChar ||
                             path[index] == Path.AltDirectorySeparatorChar;
            if (separator && index - lineStart >= lineLength)
            {
                result.Append(path, lineStart, index + 1 - lineStart).Append('\n');
                lineStart = index + 1;
            }
        }
        result.Append(path, lineStart, path.Length - lineStart);
        return result.ToString();
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
