using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class PhaseProgress
{
    private readonly HashSet<string> _completedPhaseIds = new(StringComparer.Ordinal);

    public bool IsCompleted(string phaseId)
    {
        return _completedPhaseIds.Contains(phaseId);
    }

    public bool IsUnlocked(string phaseId)
    {
        return phaseId switch
        {
            ScenarioCatalog.AffordanceVisibilityScenarioId => true,
            ScenarioCatalog.DamageFeedbackScenarioId =>
                IsCompleted(ScenarioCatalog.AffordanceVisibilityScenarioId),
            ScenarioCatalog.ConceptualModelsConstraintsScenarioId =>
                IsCompleted(ScenarioCatalog.DamageFeedbackScenarioId),
            _ => false
        };
    }

    public bool TryCompletePhase(string phaseId)
    {
        return IsUnlocked(phaseId) && _completedPhaseIds.Add(phaseId);
    }
}
