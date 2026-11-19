using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class PhaseProgress
{
    private readonly HashSet<string> _completedPhaseIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _finalSolutionIds =
        new(StringComparer.Ordinal);

    public bool IsCompleted(string phaseId)
    {
        return _completedPhaseIds.Contains(phaseId);
    }

    public void Clear()
    {
        _completedPhaseIds.Clear();
        _finalSolutionIds.Clear();
    }

    public void RestoreCompletedPhase(
        string phaseId,
        string finalSolutionId,
        SolutionVisualModule finalSolutionVisualModules)
    {
        _completedPhaseIds.Add(phaseId);

        if (!string.IsNullOrEmpty(finalSolutionId))
            _finalSolutionIds[phaseId] = finalSolutionId;
    }

    public bool IsUnlocked(string phaseId)
    {
        return phaseId switch
        {
            ScenarioCatalog.AffordanceVisibilityScenarioId => true,
            // Debug builds open every phase so the new rooms can be tested out of order.
            _ when ResearchStudyConfiguration.DevToolsEnabled => true,
            ScenarioCatalog.DamageFeedbackScenarioId =>
                !ResearchStudyConfiguration.BetaAffordanceOnly &&
                IsCompleted(ScenarioCatalog.AffordanceVisibilityScenarioId),
            ScenarioCatalog.ConceptualModelsConstraintsScenarioId =>
                !ResearchStudyConfiguration.BetaAffordanceOnly &&
                IsCompleted(ScenarioCatalog.DamageFeedbackScenarioId),
            _ => false
        };
    }

    public string GetFinalSolutionId(string phaseId)
    {
        return _finalSolutionIds.TryGetValue(phaseId, out string solutionId)
            ? solutionId
            : null;
    }

    public bool TryCompletePhase(
        string phaseId,
        string finalSolutionId,
        SolutionVisualModule finalSolutionVisualModules)
    {
        if (!IsUnlocked(phaseId))
        {
            return false;
        }

        if (string.IsNullOrEmpty(finalSolutionId))
            _finalSolutionIds.Remove(phaseId);
        else
            _finalSolutionIds[phaseId] = finalSolutionId;

        return _completedPhaseIds.Add(phaseId);
    }
}
