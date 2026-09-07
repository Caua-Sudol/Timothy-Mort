using System;

namespace DontLikePoetry;

public static class PhaseResultTextCatalog
{
    public static string GetExplanation(string phaseId, SolutionQuality? firstQuality)
    {
        if (!firstQuality.HasValue)
        {
            return "A fase foi concluída sem uma solução escolhida. " +
                "Não há primeira hipótese para classificar.";
        }

        string assessment = firstQuality.Value switch
        {
            SolutionQuality.INADEQUATE => "A primeira intervenção não resolveu o problema central desta fase. ",
            SolutionQuality.PARTIAL => "A primeira intervenção resolveu apenas parte do problema desta fase. ",
            SolutionQuality.ADEQUATE => "A primeira intervenção contemplou o problema central desta fase. ",
            _ => throw new ArgumentOutOfRangeException(nameof(firstQuality))
        };

        string objective = phaseId switch
        {
            ScenarioCatalog.AffordanceVisibilityScenarioId =>
                "O objetivo é permitir reconhecer as superfícies especiais e compreender sua relação " +
                "com a recuperação ou disponibilidade do impulso.",
            ScenarioCatalog.DamageFeedbackScenarioId =>
                "O objetivo é comunicar o impacto do dano e o período de proteção temporária.",
            ScenarioCatalog.ConceptualModelsConstraintsScenarioId =>
                "O objetivo é alinhar aparência e comportamento de gelatinas e espinhos, " +
                "permitindo prever seus efeitos na travessia.",
            _ => throw new ArgumentException("A fase não possui explicação de resultado.", nameof(phaseId))
        };

        return assessment + objective;
    }
}
