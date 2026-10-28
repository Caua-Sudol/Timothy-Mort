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
    private readonly SolutionVisualModule? _baseVisualModulesOverride;
    private bool _appliedSolutionAllowsExit;
    private bool _deathRegisteredInCurrentAttempt;
    private IReadOnlyList<PresentedSolutionDefinition> _initialSolutionPresentation =
        Array.Empty<PresentedSolutionDefinition>();
    private IReadOnlyList<PresentedJustificationDefinition> _initialJustificationPresentation =
        Array.Empty<PresentedJustificationDefinition>();

    public string PhaseId { get; }
    public string CurrentScenarioId { get; private set; }
    public PhaseDefinition Definition => _definition;
    // Room the player came from before entering the intervention room (room-format phases).
    public string InterventionReturnScenarioId { get; private set; }
    public int DeathCount { get; private set; }
    public int TotalDeathCount { get; private set; }
    public double ElapsedSeconds { get; private set; }
    public int CurrentHealth { get; private set; }
    public double DamageInvulnerabilityRemainingSeconds { get; private set; }
    public string FirstSolutionId { get; private set; }
    public string FirstJustificationId { get; private set; }
    public string AppliedSolutionId { get; private set; }
    public PedagogicalDemonstrationData Demonstration { get; private set; }

    public void RegisterDemonstration()
    {
        if (CurrentScenarioId != _definition.LastRoomScenarioId || Demonstration != null)
            return;
        Demonstration = new PedagogicalDemonstrationData
        {
            ScenarioId = CurrentScenarioId,
            StartedUtc = DateTimeOffset.UtcNow.ToString("O"),
            PlayerSolutionIdAtStart = AppliedSolutionId,
            DemonstratedVisualModules = (long)_definition.DemonstrationModules,
            ActiveSecondsAtStart = ElapsedSeconds,
            TotalDeathsAtStart = TotalDeathCount
        };
    }
    public int SolutionSelectionCount { get; private set; }
    public bool HasRevision { get; private set; }
    // Room format: the choice can be changed freely inside the intervention room
    // and becomes final when the player leaves it with a solution applied.
    public bool ChoiceLocked { get; private set; }
    // Feedback: Mort's balloon about the unsignalled damage plays once per run.
    private bool _damageCalloutShown;
    public bool VerticalRouteRevealed { get; private set; }
    public bool ExitDoorIsOpen
    {
        get
        {
            return _appliedSolutionAllowsExit ||
                CurrentScenarioId == _definition.ProblemScenarioId &&
                _definition.HealthAllowsExit(CurrentHealth);
        }
    }

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
        get
        {
            return HasFirstChoice &&
                (PhaseId == ScenarioCatalog.AffordanceVisibilityScenarioId || !ExitDoorIsOpen) &&
                ChoiceCatalog.AvailableRevisionSolutions.Count > 0;
        }
    }

    public bool InterventionEntranceIsActive
    {
        get { return !_appliedSolutionAllowsExit; }
    }

    public bool AppliedSolutionUsesModule(SolutionVisualModule module)
    {
        return (AppliedVisualModules & module) == module;
    }

    public SolutionVisualModule AppliedVisualModules
    {
        get
        {
            SolutionVisualModule modules =
                _baseVisualModulesOverride ?? _definition.BaseVisualModules;

            if (!_definition.AppliesSolutionVisualModules ||
                string.IsNullOrEmpty(AppliedSolutionId))
            {
                return modules;
            }

            return modules | _definition.ChoiceCatalog
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
        string initialScenarioId,
        SolutionVisualModule? baseVisualModulesOverride = null)
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
        InterventionReturnScenarioId = definition.ProblemScenarioId;
        _deathCountingScenarioId = definition.ProblemScenarioId;
        _baseVisualModulesOverride = baseVisualModulesOverride;
        RestoreDamageState();
    }

    public void EnterScenario(string scenarioId)
    {
        // Leaving the intervention room refills the hearts, as a help to the player.
        if (CurrentScenarioId == _definition.InterventionScenarioId &&
            scenarioId != _definition.InterventionScenarioId)
        {
            RestoreDamageState();
        }
        if (scenarioId == _definition.InterventionScenarioId &&
            _definition.GetRoomIndex(CurrentScenarioId) >= 0)
        {
            InterventionReturnScenarioId = CurrentScenarioId;
        }
        CurrentScenarioId = scenarioId;
        if (_definition.UsesRoomFormat)
        {
            // A new room sets a checkpoint, but does not refill the five hearts.
            _deathRegisteredInCurrentAttempt = false;
            DamageInvulnerabilityRemainingSeconds = 0;
        }
        else
        {
            BeginAttempt();
        }
    }

    public bool TryTakeDamageCallout()
    {
        if (_damageCalloutShown) return false;
        _damageCalloutShown = true;
        return true;
    }

    public void LockChoice()
    {
        if (HasFirstChoice) ChoiceLocked = true;
    }

    public void RegisterRoomRespawn(bool lostHeartFromFall)
    {
        if (!_definition.UsesRoomFormat) return;
        if (lostHeartFromFall)
            CurrentHealth = Math.Max(0, CurrentHealth - 1);
        RegisterDeath();
        if (CurrentHealth > 0)
            _deathRegisteredInCurrentAttempt = false;
        DamageInvulnerabilityRemainingSeconds = _definition.DamageInvulnerabilitySeconds;
    }

    public void BeginAttempt()
    {
        _deathRegisteredInCurrentAttempt = false;
        RestoreDamageState();
    }

    public void Reset(string initialScenarioId)
    {
        InterventionReturnScenarioId = _definition.ProblemScenarioId;
        CurrentScenarioId = initialScenarioId;
        DeathCount = 0;
        TotalDeathCount = 0;
        ElapsedSeconds = 0.0;
        FirstSolutionId = null;
        FirstJustificationId = null;
        AppliedSolutionId = null;
        Demonstration = null;
        SolutionSelectionCount = 0;
        HasRevision = false;
        ChoiceLocked = false;
        _damageCalloutShown = false;
        VerticalRouteRevealed = false;
        _appliedSolutionAllowsExit = false;
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
        if (_deathRegisteredInCurrentAttempt)
        {
            return;
        }

        TotalDeathCount++;
        if (Demonstration != null) Demonstration.DeathsAfterExposure++;
        if (CurrentScenarioId == _deathCountingScenarioId)
        {
            DeathCount++;
        }

        _deathRegisteredInCurrentAttempt = true;
    }

    public void AccumulateActiveTime(double deltaTime)
    {
        if (!double.IsFinite(deltaTime) || deltaTime < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime));
        }

        ElapsedSeconds += deltaTime;
        if (Demonstration != null) Demonstration.ActiveSecondsAfterExposure += deltaTime;
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

    public bool TryApplyDamage(int amount = 1)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (!HasDamageSystem || CurrentHealth <= 0 ||
            DamageInvulnerabilityRemainingSeconds > 0.0)
        {
            return false;
        }

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
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

        bool canChangeChoice = _definition.UsesRoomFormat && !ChoiceLocked;
        if (HasFirstChoice && !canChangeChoice)
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

        // A change inside the intervention replaces the choice; the count keeps every pick.
        SolutionSelectionCount = HasFirstChoice ? SolutionSelectionCount + 1 : 1;
        FirstSolutionId = solution.Id;
        FirstJustificationId = justification.Id;
        AppliedSolutionId = solution.Id;
        VerticalRouteRevealed = true;
        _appliedSolutionAllowsExit = solution.OpensExitDoor;
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

        if (HasRevision && PhaseId != ScenarioCatalog.AffordanceVisibilityScenarioId)
        {
            throw new InvalidOperationException("A revisao da fase ja foi registrada.");
        }

        bool isAffordanceExperiment = PhaseId == ScenarioCatalog.AffordanceVisibilityScenarioId;
        if (isAffordanceExperiment)
        {
            // Accept only one of this phase's three alternatives; preserve the first hypothesis.
            solution = ChoiceCatalog.GetSolution(solution.Id);
        }

        if (!isAffordanceExperiment &&
            (solution.ChoiceRound != SolutionChoiceRound.REVISION ||
             solution.Quality == SolutionQuality.INADEQUATE))
        {
            throw new InvalidOperationException("A revisao deve usar uma solucao parcial ou adequada.");
        }

        AppliedSolutionId = solution.Id;
        SolutionSelectionCount++;
        HasRevision = true;
        VerticalRouteRevealed = true;
        _appliedSolutionAllowsExit = solution.OpensExitDoor;
    }
}
