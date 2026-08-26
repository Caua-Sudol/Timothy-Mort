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
            FeedbackDamageChoiceCatalog.Instance,
            true,
            DamageFeedbackSettings.MaximumHealth,
            DamageFeedbackSettings.InvulnerabilityDurationSeconds);

    public static PhaseDefinition ConceptualModelsConstraints { get; } =
        new PhaseDefinition(
            ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
            ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
            new[]
            {
                ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
                ScenarioCatalog.ConceptualModelsConstraintsInterventionScenarioId
            },
            ConceptualModelsConstraintsChoiceCatalog.Instance,
            false,
            ConceptualModelsConstraintsSettings.MaximumHealth,
            ConceptualModelsConstraintsSettings.InvulnerabilityDurationSeconds,
            ConceptualModelsConstraintsSettings.BaseVisualModules,
            ConceptualModelsConstraintsSettings.ExitHealthThreshold);

    private static readonly IReadOnlyList<PhaseDefinition> Phases =
        new[]
        {
            AffordanceVisibility,
            DamageFeedback,
            ConceptualModelsConstraints
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
