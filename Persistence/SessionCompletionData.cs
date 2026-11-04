using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class SessionCompletionData
{
    public string SessionId { get; set; }
    public string PublicCode { get; set; }
    public string Status { get; set; }
    public string CreatedUtc { get; set; }
    public string CompletedUtc { get; set; }
    public string GameVersion { get; set; }
    public List<PhaseCompletionData> Phases { get; set; } = new();
    // Kept during play (not only at phase completion), so incomplete sessions still show
    // where the player was, how long each room took and what was tried.
    public SessionActivityData Activity { get; set; }
}

public sealed class SessionActivityData
{
    public string GameVersion { get; set; }
    public string UpdatedUtc { get; set; }
    // Last room entered: where an incomplete session stopped.
    public string LastPhaseId { get; set; }
    public string LastScenarioId { get; set; }
    public bool TutorialCompleted { get; set; }
    // Per room (the tutorial has no phase): entries, active time and deaths.
    public List<RoomActivityData> Rooms { get; set; } = new();
    // Control scheme shown in the tutorial: the initial one and every change.
    public List<ControlSchemeChangeData> ControlSchemes { get; set; } = new();
    // Intervention room, in order: solutions tried, "keep testing" and the justification.
    public List<InterventionEventData> InterventionEvents { get; set; } = new();
}

public sealed class RoomActivityData
{
    public string PhaseId { get; set; }
    public string ScenarioId { get; set; }
    public int Entries { get; set; }
    public double ActiveSeconds { get; set; }
    public int Deaths { get; set; }
}

public sealed class ControlSchemeChangeData
{
    public string Utc { get; set; }
    public string Scheme { get; set; }
}

public sealed class InterventionEventData
{
    public string Utc { get; set; }
    public string PhaseId { get; set; }
    // "solucao", "continuar_testando" or "justificativa".
    public string Kind { get; set; }
    public string ChoiceId { get; set; }
}

public sealed class PhaseCompletionData
{
    public string PhaseId { get; set; }
    public string CompletedUtc { get; set; }
    public double ActiveSeconds { get; set; }
    public int TotalDeaths { get; set; }
    public string FirstSolutionId { get; set; }
    public string FirstSolutionQuality { get; set; }
    public int? FirstSolutionPosition { get; set; }
    public string FirstJustificationId { get; set; }
    public int? FirstJustificationPosition { get; set; }
    public bool HadRevision { get; set; }
    public int SelectionCount { get; set; }
    public string FinalSolutionId { get; set; }
    public string FinalSolutionQuality { get; set; }
    public long AppliedVisualModules { get; set; }
    public PedagogicalDemonstrationData Demonstration { get; set; }
    public List<PresentedChoiceData> PresentedSolutions { get; set; } = new();
    public List<PresentedChoiceData> PresentedJustifications { get; set; } = new();
}

public sealed class PresentedChoiceData
{
    public int Position { get; set; }
    public string ChoiceId { get; set; }
}

public sealed class PedagogicalDemonstrationData
{
    public string ScenarioId { get; set; }
    public string StartedUtc { get; set; }
    public string PlayerSolutionIdAtStart { get; set; }
    public long DemonstratedVisualModules { get; set; }
    public double ActiveSecondsAtStart { get; set; }
    public int TotalDeathsAtStart { get; set; }
    // Exposure boundary, including later returns to other rooms; not time spent only in the demo room.
    public double ActiveSecondsAfterExposure { get; set; }
    public int DeathsAfterExposure { get; set; }
}
