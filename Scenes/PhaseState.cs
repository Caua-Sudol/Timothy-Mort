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

    private readonly string _deathCountingScenarioId;
    private bool _deathRegisteredInCurrentAttempt;
    private IReadOnlyList<PresentedSolutionDefinition> _initialSolutionPresentation =
        Array.Empty<PresentedSolutionDefinition>();

    public string PhaseId { get; }
    public string CurrentScenarioId { get; private set; }
    public int FallDeathCount { get; private set; }
    public string FirstSolutionId { get; private set; }
    public string FirstJustificationId { get; private set; }
    public string AppliedSolutionId { get; private set; }
    public int SolutionSelectionCount { get; private set; }
    public bool HasRevision { get; private set; }
    public bool VerticalRouteRevealed { get; private set; }
    public bool ExitDoorIsOpen { get; private set; }

    public IReadOnlyList<PresentedSolutionDefinition> InitialSolutionPresentation
    {
        get { return _initialSolutionPresentation; }
    }

    public bool HasInitialSolutionPresentation
    {
        get { return _initialSolutionPresentation.Count > 0; }
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
        if (string.IsNullOrEmpty(AppliedSolutionId))
        {
            return false;
        }

        InterventionSolutionDefinition solution =
            AffordanceChoiceCatalog.GetSolution(AppliedSolutionId);

        return (solution.VisualModules & module) == module;
    }

    public InterventionEntrancePlacement InterventionEntrancePlacement
    {
        get
        {
            return FallDeathCount >= EarlyEntranceDeathThreshold
                ? InterventionEntrancePlacement.EARLY
                : InterventionEntrancePlacement.LATE;
        }
    }

    public PhaseState(
        string phaseId,
        string initialScenarioId,
        string deathCountingScenarioId)
    {
        PhaseId = phaseId;
        CurrentScenarioId = initialScenarioId;
        _deathCountingScenarioId = deathCountingScenarioId;
    }

    public void EnterScenario(string scenarioId)
    {
        CurrentScenarioId = scenarioId;
        BeginAttempt();
    }

    public void BeginAttempt()
    {
        _deathRegisteredInCurrentAttempt = false;
    }

    public void Reset(string initialScenarioId)
    {
        CurrentScenarioId = initialScenarioId;
        FallDeathCount = 0;
        FirstSolutionId = null;
        FirstJustificationId = null;
        AppliedSolutionId = null;
        SolutionSelectionCount = 0;
        HasRevision = false;
        VerticalRouteRevealed = false;
        ExitDoorIsOpen = false;
        _deathRegisteredInCurrentAttempt = false;
        _initialSolutionPresentation = Array.Empty<PresentedSolutionDefinition>();
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

    public void RegisterFallDeath()
    {
        if (CurrentScenarioId != _deathCountingScenarioId ||
            _deathRegisteredInCurrentAttempt)
        {
            return;
        }

        FallDeathCount++;
        _deathRegisteredInCurrentAttempt = true;
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

        FirstSolutionId = solution.Id;
        FirstJustificationId = justification.Id;
        AppliedSolutionId = solution.Id;
        SolutionSelectionCount = 1;
        VerticalRouteRevealed = true;
        ExitDoorIsOpen = solution.OpensExitDoor;
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
