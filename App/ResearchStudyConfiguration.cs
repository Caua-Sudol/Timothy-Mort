using System;
using System.Diagnostics;
using System.IO;

namespace DontLikePoetry;

public static class ResearchStudyConfiguration
{
    // True: only Affordance is playable, and finishing it completes the study.
    // False: each phase unlocks after the previous one (Affordance → Feedback → Conceptual
    // models), and finishing Conceptual models completes the study.
    public const bool BetaAffordanceOnly = false;

    // Written in every session file, to compare results between builds. Update before each build.
    public const string GameVersion = "2026.11.04-estudantes";

    // Address shown on the final screen for sending the session file.
    public const string ResearchEmail = "timothy.mort.game@gmail.com";

    // Developer tools (test room, Ctrl+P, Ctrl+R, 1-4 player scale) exist only in Debug builds
    // (Rider / dotnet run). Release builds (dotnet publish -c Release) hide them automatically.
#if DEBUG
    public static bool DevToolsEnabled => true;
#else
    public static bool DevToolsEnabled => false;
#endif

    // Configure these values after creating the final Google Form.
    // Example field: entry.123456789
    private const string MeegaFormUrl = "";
    private const string SessionFieldName = "";

    public static bool IsMeegaFormConfigured
    {
        get
        {
            return Uri.TryCreate(MeegaFormUrl, UriKind.Absolute, out _) &&
                !string.IsNullOrWhiteSpace(SessionFieldName);
        }
    }

    // Opens the folder in Explorer (Windows) or the default file manager (Linux, via xdg-open).
    public static bool TryOpenExportFolder(string exportPath, out string errorMessage)
    {
        try
        {
            string folder = Path.GetDirectoryName(exportPath);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                errorMessage = "A pasta dos dados não foi encontrada.";
                return false;
            }

            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            errorMessage = null;
            return true;
        }
        catch (Exception)
        {
            errorMessage = "Não foi possível abrir a pasta. Use o caminho mostrado acima.";
            return false;
        }
    }

    public static bool TryOpenMeegaForm(string sessionId, out string errorMessage)
    {
        if (!IsMeegaFormConfigured)
        {
            errorMessage = "O formulário MEEGA+ ainda não foi configurado.";
            return false;
        }

        try
        {
            string separator = MeegaFormUrl.Contains('?') ? "&" : "?";
            string url = MeegaFormUrl + separator + "usp=pp_url&" +
                Uri.EscapeDataString(SessionFieldName) + "=" +
                Uri.EscapeDataString(sessionId);
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
            errorMessage = null;
            return true;
        }
        catch (Exception)
        {
            errorMessage = "Não foi possível abrir o formulário.";
            return false;
        }
    }
}
