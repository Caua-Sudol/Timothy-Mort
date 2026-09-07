using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class PhaseProgress
{
    private readonly HashSet<string> _completedPhaseIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _finalSolutionIds =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, SolutionVisualModule> _finalSolutionVisualModules =
        new(StringComparer.Ordinal);

    public bool IsCompleted(string phaseId)
    {
        return _completedPhaseIds.Contains(phaseId);
    }

    public void Clear()
    {
        _completedPhaseIds.Clear();
        _finalSolutionIds.Clear();
        _finalSolutionVisualModules.Clear();
    }

    public void RestoreCompletedPhase(
        string phaseId,
        string finalSolutionId,
        SolutionVisualModule finalSolutionVisualModules)
    {
        _completedPhaseIds.Add(phaseId);

        if (!string.IsNullOrEmpty(finalSolutionId))
        {
            _finalSolutionIds[phaseId] = finalSolutionId;
            _finalSolutionVisualModules[phaseId] = finalSolutionVisualModules;
        }
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

    public string GetFinalSolutionId(string phaseId)
    {
        return _finalSolutionIds.TryGetValue(phaseId, out string solutionId)
            ? solutionId
            : null;
    }

    public SolutionVisualModule GetTransferredVisualModules()
    {
        return GetFinalSolutionVisualModules(
                   ScenarioCatalog.AffordanceVisibilityScenarioId) |
               GetFinalSolutionVisualModules(
                   ScenarioCatalog.DamageFeedbackScenarioId);
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
        {
            _finalSolutionIds.Remove(phaseId);
            _finalSolutionVisualModules.Remove(phaseId);
        }
        else
        {
            _finalSolutionIds[phaseId] = finalSolutionId;
            _finalSolutionVisualModules[phaseId] = finalSolutionVisualModules;
        }

        return _completedPhaseIds.Add(phaseId);
    }

    private SolutionVisualModule GetFinalSolutionVisualModules(string phaseId)
    {
        return _finalSolutionVisualModules.TryGetValue(
            phaseId,
            out SolutionVisualModule visualModules)
            ? visualModules
            : SolutionVisualModule.NONE;
    }
}
