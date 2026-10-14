using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class SessionCompletionData
{
    public string SessionId { get; set; }
    public string PublicCode { get; set; }
    public string Status { get; set; }
    public string CreatedUtc { get; set; }
    public string CompletedUtc { get; set; }
    public List<PhaseCompletionData> Phases { get; set; } = new();
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
