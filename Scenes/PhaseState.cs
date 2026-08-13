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

    public string PhaseId { get; }
    public string CurrentScenarioId { get; private set; }
    public int FallDeathCount { get; private set; }

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
}
