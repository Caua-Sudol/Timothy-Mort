using System.Collections.Generic;

namespace DontLikePoetry;

public static class PhaseCatalog
{
    public static PhaseDefinition AffordanceVisibility { get; } =
        new PhaseDefinition(
            ScenarioCatalog.AffordanceVisibilityScenarioId,
            ScenarioCatalog.AffordanceVisibilityScenarioId,
            new[]
            {
                ScenarioCatalog.AffordanceVisibilityScenarioId,
                ScenarioCatalog.AffordanceInterventionScenarioId
            },
            AffordanceChoiceCatalogAdapter.Instance,
            true);

    public static PhaseDefinition DamageFeedback { get; } =
        new PhaseDefinition(
            ScenarioCatalog.DamageFeedbackScenarioId,
            ScenarioCatalog.DamageFeedbackScenarioId,
            new[]
            {
                ScenarioCatalog.DamageFeedbackScenarioId,
                ScenarioCatalog.DamageFeedbackInterventionScenarioId
            },
            AffordanceChoiceCatalogAdapter.Instance,
            false,
            5,
            1.5);

    private static readonly IReadOnlyList<PhaseDefinition> Phases =
        new[]
        {
            AffordanceVisibility,
            DamageFeedback
        };

    public static PhaseDefinition GetForScenario(string scenarioId)
    {
        foreach (PhaseDefinition phase in Phases)
        {
            if (phase.ContainsScenario(scenarioId))
            {
                return phase;
            }
        }

        return null;
    }
}
