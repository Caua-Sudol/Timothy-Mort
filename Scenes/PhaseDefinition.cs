using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class PhaseDefinition
{
    private readonly HashSet<string> _scenarioIds;

    public string Id { get; }
    public string ProblemScenarioId { get; }
    public IInterventionChoiceCatalog ChoiceCatalog { get; }
    public bool AppliesSolutionVisualModules { get; }
    public int MaximumHealth { get; }
    public double DamageInvulnerabilitySeconds { get; }
    public SolutionVisualModule BaseVisualModules { get; }
    public int? ExitHealthThreshold { get; }

    public bool HasDamageSystem
    {
        get { return MaximumHealth > 0; }
    }

    public PhaseDefinition(
        string id,
        string problemScenarioId,
        IEnumerable<string> scenarioIds,
        IInterventionChoiceCatalog choiceCatalog,
        bool appliesSolutionVisualModules,
        int maximumHealth = 0,
        double damageInvulnerabilitySeconds = 0.0,
        SolutionVisualModule baseVisualModules = SolutionVisualModule.NONE,
        int? exitHealthThreshold = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A fase precisa de um ID.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(problemScenarioId))
        {
            throw new ArgumentException(
                "A fase precisa de um cenario de problema.",
                nameof(problemScenarioId));
        }

        if (choiceCatalog == null)
        {
            throw new ArgumentNullException(nameof(choiceCatalog));
        }

        if (maximumHealth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumHealth));
        }

        if (maximumHealth > 0 && damageInvulnerabilitySeconds <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damageInvulnerabilitySeconds));
        }

        if (exitHealthThreshold.HasValue &&
            (maximumHealth <= 0 ||
             exitHealthThreshold.Value < 0 ||
             exitHealthThreshold.Value > maximumHealth))
        {
            throw new ArgumentOutOfRangeException(nameof(exitHealthThreshold));
        }

        Id = id;
        ProblemScenarioId = problemScenarioId;
        ChoiceCatalog = choiceCatalog;
        AppliesSolutionVisualModules = appliesSolutionVisualModules;
        MaximumHealth = maximumHealth;
        DamageInvulnerabilitySeconds = damageInvulnerabilitySeconds;
        BaseVisualModules = baseVisualModules;
        ExitHealthThreshold = exitHealthThreshold;
        _scenarioIds = new HashSet<string>(scenarioIds, StringComparer.Ordinal);

        if (!_scenarioIds.Contains(problemScenarioId))
        {
            throw new ArgumentException(
                "O cenario de problema precisa pertencer a fase.",
                nameof(scenarioIds));
        }
    }

    public bool ContainsScenario(string scenarioId)
    {
        return !string.IsNullOrEmpty(scenarioId) &&
            _scenarioIds.Contains(scenarioId);
    }

    public bool HealthAllowsExit(int currentHealth)
    {
        return ExitHealthThreshold.HasValue &&
            currentHealth <= ExitHealthThreshold.Value;
    }
}
