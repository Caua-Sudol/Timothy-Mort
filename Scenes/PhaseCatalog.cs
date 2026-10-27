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
                ScenarioCatalog.AffordanceRoom2ScenarioId,
                ScenarioCatalog.AffordanceRoom3ScenarioId,
                ScenarioCatalog.AffordanceInterventionScenarioId
            },
            AffordanceChoiceCatalogAdapter.Instance,
            true,
            5,
            1,
            SolutionVisualModule.DAMAGE_KNOCKBACK |
            SolutionVisualModule.INVULNERABILITY_BLINK,
            roomScenarioIds: new[]
            {
                ScenarioCatalog.AffordanceVisibilityScenarioId,
                ScenarioCatalog.AffordanceRoom2ScenarioId,
                ScenarioCatalog.AffordanceRoom3ScenarioId
            },
            interventionScenarioId: ScenarioCatalog.AffordanceInterventionScenarioId,
            bestSolutionId: AffordanceChoiceCatalog.AppearanceSolutionId,
            demonstrationModules: SolutionVisualModule.GELATIN_COLOR |
                                  SolutionVisualModule.GELATIN_JELLY_MOTION);

    public static PhaseDefinition DamageFeedback { get; } =
        new PhaseDefinition(
            ScenarioCatalog.DamageFeedbackScenarioId,
            ScenarioCatalog.DamageFeedbackScenarioId,
            new[]
            {
                ScenarioCatalog.DamageFeedbackScenarioId,
                ScenarioCatalog.DamageFeedbackRoom2ScenarioId,
                ScenarioCatalog.DamageFeedbackRoom3ScenarioId,
                ScenarioCatalog.DamageFeedbackInterventionScenarioId
            },
            FeedbackDamageChoiceCatalog.Instance,
            true,
            DamageFeedbackSettings.MaximumHealth,
            DamageFeedbackSettings.InvulnerabilityDurationSeconds,
            roomScenarioIds: new[]
            {
                ScenarioCatalog.DamageFeedbackScenarioId,
                ScenarioCatalog.DamageFeedbackRoom2ScenarioId,
                ScenarioCatalog.DamageFeedbackRoom3ScenarioId
            },
            interventionScenarioId: ScenarioCatalog.DamageFeedbackInterventionScenarioId);

    public static PhaseDefinition ConceptualModelsConstraints { get; } =
        new PhaseDefinition(
            ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
            ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
            new[]
            {
                ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
                ScenarioCatalog.ConceptualModelsConstraintsRoom2ScenarioId,
                ScenarioCatalog.ConceptualModelsConstraintsRoom3ScenarioId,
                ScenarioCatalog.ConceptualModelsConstraintsInterventionScenarioId
            },
            ConceptualModelsConstraintsChoiceCatalog.Instance,
            true,
            ConceptualModelsConstraintsSettings.MaximumHealth,
            ConceptualModelsConstraintsSettings.InvulnerabilityDurationSeconds,
            ConceptualModelsConstraintsSettings.BaseVisualModules,
            ConceptualModelsConstraintsSettings.ExitHealthThreshold,
            roomScenarioIds: new[]
            {
                ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
                ScenarioCatalog.ConceptualModelsConstraintsRoom2ScenarioId,
                ScenarioCatalog.ConceptualModelsConstraintsRoom3ScenarioId
            },
            interventionScenarioId: ScenarioCatalog.ConceptualModelsConstraintsInterventionScenarioId);

    private static readonly IReadOnlyList<PhaseDefinition> Phases =
        new[]
        {
            AffordanceVisibility,
            DamageFeedback,
            ConceptualModelsConstraints
        };

    // True for a challenge room of a room-format phase (not the intervention room).
    public static bool IsChallengeRoom(string scenarioId) =>
        GetForScenario(scenarioId)?.GetRoomIndex(scenarioId) >= 0;

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
