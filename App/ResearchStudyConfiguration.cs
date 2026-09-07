using System;
using System.Diagnostics;

namespace DontLikePoetry;

public static class ResearchStudyConfiguration
{
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
