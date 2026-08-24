using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public enum InterventionEntrancePlacement
{
    LATE,
    EARLY
}

public sealed class PhaseState
{
    private const int EarlyEntranceDeathThreshold = 2;

    private readonly PhaseDefinition _definition;
    private readonly string _deathCountingScenarioId;
    private bool _deathRegisteredInCurrentAttempt;
    private IReadOnlyList<PresentedSolutionDefinition> _initialSolutionPresentation =
        Array.Empty<PresentedSolutionDefinition>();
    private IReadOnlyList<PresentedJustificationDefinition> _initialJustificationPresentation =
        Array.Empty<PresentedJustificationDefinition>();

    public string PhaseId { get; }
    public string CurrentScenarioId { get; private set; }
    public int DeathCount { get; private set; }
    public int CurrentHealth { get; private set; }
    public double DamageInvulnerabilityRemainingSeconds { get; private set; }
    public string FirstSolutionId { get; private set; }
    public string FirstJustificationId { get; private set; }
    public string AppliedSolutionId { get; private set; }
    public int SolutionSelectionCount { get; private set; }
    public bool HasRevision { get; private set; }
    public bool VerticalRouteRevealed { get; private set; }
    public bool ExitDoorIsOpen { get; private set; }

    public int MaximumHealth
    {
        get { return _definition.MaximumHealth; }
    }

    public bool HasDamageSystem
    {
        get { return _definition.HasDamageSystem; }
    }

    public IReadOnlyList<PresentedSolutionDefinition> InitialSolutionPresentation
    {
        get { return _initialSolutionPresentation; }
    }

    public bool HasInitialSolutionPresentation
    {
        get { return _initialSolutionPresentation.Count > 0; }
    }

    public IReadOnlyList<PresentedJustificationDefinition> InitialJustificationPresentation
    {
        get { return _initialJustificationPresentation; }
    }

    public bool HasInitialJustificationPresentation
    {
        get { return _initialJustificationPresentation.Count > 0; }
    }

    public bool HasFirstChoice
    {
        get { return !string.IsNullOrEmpty(FirstSolutionId); }
    }

    public bool NeedsSolutionRevision
    {
        get { return HasFirstChoice && !ExitDoorIsOpen; }
    }

    public bool InterventionEntranceIsActive
    {
        get { return !ExitDoorIsOpen; }
    }

    public bool AppliedSolutionUsesModule(SolutionVisualModule module)
    {
        if (!_definition.AppliesSolutionVisualModules ||
            string.IsNullOrEmpty(AppliedSolutionId))
        {
            return false;
        }

        InterventionSolutionDefinition solution =
            _definition.ChoiceCatalog.GetSolution(AppliedSolutionId);

        return (solution.VisualModules & module) == module;
    }

    public SolutionVisualModule AppliedVisualModules
    {
        get
        {
            if (!_definition.AppliesSolutionVisualModules ||
                string.IsNullOrEmpty(AppliedSolutionId))
            {
                return SolutionVisualModule.NONE;
            }

            return _definition.ChoiceCatalog
                .GetSolution(AppliedSolutionId)
                .VisualModules;
        }
    }

    public IInterventionChoiceCatalog ChoiceCatalog
    {
        get { return _definition.ChoiceCatalog; }
    }

    public InterventionEntrancePlacement InterventionEntrancePlacement
    {
        get
        {
            return DeathCount >= EarlyEntranceDeathThreshold
                ? InterventionEntrancePlacement.EARLY
                : InterventionEntrancePlacement.LATE;
        }
    }

    public PhaseState(
        PhaseDefinition definition,
        string initialScenarioId)
    {
        _definition = definition ??
            throw new ArgumentNullException(nameof(definition));

        if (!definition.ContainsScenario(initialScenarioId))
        {
            throw new ArgumentException(
                "O cenario inicial nao pertence a fase.",
                nameof(initialScenarioId));
        }

        PhaseId = definition.Id;
        CurrentScenarioId = initialScenarioId;
        _deathCountingScenarioId = definition.ProblemScenarioId;
        RestoreDamageState();
    }

    public void EnterScenario(string scenarioId)
    {
        CurrentScenarioId = scenarioId;
        BeginAttempt();
    }

    public void BeginAttempt()
    {
        _deathRegisteredInCurrentAttempt = false;
        RestoreDamageState();
    }

    public void Reset(string initialScenarioId)
    {
        CurrentScenarioId = initialScenarioId;
        DeathCount = 0;
        FirstSolutionId = null;
        FirstJustificationId = null;
        AppliedSolutionId = null;
        SolutionSelectionCount = 0;
        HasRevision = false;
        VerticalRouteRevealed = false;
        ExitDoorIsOpen = false;
        _deathRegisteredInCurrentAttempt = false;
        _initialSolutionPresentation = Array.Empty<PresentedSolutionDefinition>();
        _initialJustificationPresentation =
            Array.Empty<PresentedJustificationDefinition>();
        RestoreDamageState();
    }

    public void RegisterInitialSolutionPresentation(
        IReadOnlyList<PresentedSolutionDefinition> presentation)
    {
        if (presentation == null)
        {
            throw new ArgumentNullException(nameof(presentation));
        }

        if (HasInitialSolutionPresentation)
        {
            throw new InvalidOperationException(
                "A apresentacao inicial da fase ja foi registrada.");
        }

        if (presentation.Count == 0)
        {
            throw new InvalidOperationException(
                "A apresentacao inicial deve possuir ao menos uma solucao.");
        }

        var entriesByPosition =
            new PresentedSolutionDefinition[presentation.Count];
        var registeredSolutionIds =
            new HashSet<string>(StringComparer.Ordinal);

        foreach (PresentedSolutionDefinition entry in presentation)
        {
            if (entry == null || string.IsNullOrEmpty(entry.SolutionId))
            {
                throw new InvalidOperationException(
                    "A apresentacao inicial possui uma solucao sem ID.");
            }

            if (entry.Position < 1 || entry.Position > presentation.Count)
            {
                throw new InvalidOperationException(
                    "As posicoes da apresentacao inicial devem formar o intervalo 1..N.");
            }

            if (!registeredSolutionIds.Add(entry.SolutionId))
            {
                throw new InvalidOperationException(
                    $"A solucao {entry.SolutionId} foi apresentada mais de uma vez.");
            }

            int positionIndex = entry.Position - 1;

            if (entriesByPosition[positionIndex] != null)
            {
                throw new InvalidOperationException(
                    $"A posicao {entry.Position} foi registrada mais de uma vez.");
            }

            entriesByPosition[positionIndex] = new PresentedSolutionDefinition(
                entry.SolutionId,
                entry.Position);
        }

        _initialSolutionPresentation = Array.AsReadOnly(entriesByPosition);
    }

    public void RegisterInitialJustificationPresentation(
        IReadOnlyList<PresentedJustificationDefinition> presentation)
    {
        if (presentation == null)
        {
            throw new ArgumentNullException(nameof(presentation));
        }

        if (HasInitialJustificationPresentation)
        {
            throw new InvalidOperationException(
                "A apresentacao inicial de justificativas ja foi registrada.");
        }

        if (presentation.Count == 0)
        {
            throw new InvalidOperationException(
                "A apresentacao inicial deve possuir ao menos uma justificativa.");
        }

        var entriesByPosition =
            new PresentedJustificationDefinition[presentation.Count];
        var registeredJustificationIds =
            new HashSet<string>(StringComparer.Ordinal);

        foreach (PresentedJustificationDefinition entry in presentation)
        {
            if (entry == null || string.IsNullOrEmpty(entry.JustificationId))
            {
                throw new InvalidOperationException(
                    "A apresentacao inicial possui uma justificativa sem ID.");
            }

            if (entry.Position < 1 || entry.Position > presentation.Count)
            {
                throw new InvalidOperationException(
                    "As posicoes da apresentacao inicial devem formar o intervalo 1..N.");
            }

            if (!registeredJustificationIds.Add(entry.JustificationId))
            {
                throw new InvalidOperationException(
                    $"A justificativa {entry.JustificationId} foi apresentada mais de uma vez.");
            }

            int positionIndex = entry.Position - 1;

            if (entriesByPosition[positionIndex] != null)
            {
                throw new InvalidOperationException(
                    $"A posicao {entry.Position} foi registrada mais de uma vez.");
            }

            entriesByPosition[positionIndex] =
                new PresentedJustificationDefinition(
                    entry.JustificationId,
                    entry.Position);
        }

        _initialJustificationPresentation = Array.AsReadOnly(entriesByPosition);
    }

    public void RegisterDeath()
    {
        if (CurrentScenarioId != _deathCountingScenarioId ||
            _deathRegisteredInCurrentAttempt)
        {
            return;
        }

        DeathCount++;
        _deathRegisteredInCurrentAttempt = true;
    }

    public void UpdateDamageInvulnerability(double deltaTime)
    {
        if (DamageInvulnerabilityRemainingSeconds <= 0.0)
        {
            return;
        }

        DamageInvulnerabilityRemainingSeconds = Math.Max(
            0.0,
            DamageInvulnerabilityRemainingSeconds - deltaTime);
    }

    public bool TryApplyDamage()
    {
        if (!HasDamageSystem || CurrentHealth <= 0 ||
            DamageInvulnerabilityRemainingSeconds > 0.0)
        {
            return false;
        }

        CurrentHealth--;
        DamageInvulnerabilityRemainingSeconds =
            _definition.DamageInvulnerabilitySeconds;
        return true;
    }

    private void RestoreDamageState()
    {
        CurrentHealth = MaximumHealth;
        DamageInvulnerabilityRemainingSeconds = 0.0;
    }

    public void ApplyFirstChoice(
        InterventionSolutionDefinition solution,
        InterventionJustificationDefinition justification)
    {
        if (solution == null)
        {
            throw new ArgumentNullException(nameof(solution));
        }

        if (justification == null)
        {
            throw new ArgumentNullException(nameof(justification));
        }

        if (HasFirstChoice)
        {
            throw new InvalidOperationException("A primeira escolha da fase ja foi registrada.");
        }

        if (solution.ChoiceRound != SolutionChoiceRound.INITIAL)
        {
            throw new InvalidOperationException("A primeira escolha deve usar uma solucao inicial.");
        }

        if (!HasInitialJustificationPresentation)
        {
            throw new InvalidOperationException(
                "A apresentacao inicial de justificativas ainda nao foi registrada.");
        }

        if (!InitialJustificationPresentationContains(justification.Id))
        {
            throw new InvalidOperationException(
                $"A justificativa {justification.Id} nao pertence a apresentacao inicial da fase.");
        }

        FirstSolutionId = solution.Id;
        FirstJustificationId = justification.Id;
        AppliedSolutionId = solution.Id;
        SolutionSelectionCount = 1;
        VerticalRouteRevealed = true;
        ExitDoorIsOpen = solution.OpensExitDoor;
    }

    private bool InitialJustificationPresentationContains(string justificationId)
    {
        foreach (PresentedJustificationDefinition entry in _initialJustificationPresentation)
        {
            if (entry.JustificationId == justificationId)
            {
                return true;
            }
        }

        return false;
    }

    public void ApplyRevision(InterventionSolutionDefinition solution)
    {
        if (solution == null)
        {
            throw new ArgumentNullException(nameof(solution));
        }

        if (!HasFirstChoice || !NeedsSolutionRevision)
        {
            throw new InvalidOperationException("A fase nao esta aguardando revisao de solucao.");
        }

        if (HasRevision)
        {
            throw new InvalidOperationException("A revisao da fase ja foi registrada.");
        }

        if (solution.ChoiceRound != SolutionChoiceRound.REVISION ||
            solution.Quality == SolutionQuality.INADEQUATE)
        {
            throw new InvalidOperationException("A revisao deve usar uma solucao parcial ou adequada.");
        }

        AppliedSolutionId = solution.Id;
        SolutionSelectionCount++;
        HasRevision = true;
        VerticalRouteRevealed = true;
        ExitDoorIsOpen = solution.OpensExitDoor;
    }
}
