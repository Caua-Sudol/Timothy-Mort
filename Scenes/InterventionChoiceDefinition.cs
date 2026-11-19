using System;

namespace DontLikePoetry;

public enum SolutionQuality
{
    INADEQUATE,
    PARTIAL,
    ADEQUATE
}

public enum SolutionChoiceRound
{
    INITIAL,
    REVISION
}

// Values are stored in the session data: keep the numbers of the remaining members.
// Retired members (old prototypes) were removed on 2026-11-19; their numbers stay unused.
[Flags]
public enum SolutionVisualModule : long
{
    NONE = 0,
    GELATIN_COLOR = 2,
    GELATIN_JELLY_MOTION = 2048,
    DAMAGE_KNOCKBACK = 1048576,
    INVULNERABILITY_BLINK = 4194304,
    GELATIN_TERRAIN_MARK = 68719476736L,
    // Phase 2 solutions (effects drawn in Ckp 4).
    DAMAGE_HEART_CRACK = 137438953472L,
    DAMAGE_SCREEN_SHAKE = 274877906944L,
    // Phase 3 solutions for the impostor gelatins and rocks.
    IMPOSTOR_DIFFERENTIATE = 549755813888L,
    IMPOSTOR_RESTORE = 1099511627776L,
    // Animar: only the impostors move (the mini-jump stretches up, the impostor rock gives off red embers).
    IMPOSTOR_ANIMATE = 2199023255552L
}

public sealed class InterventionSolutionDefinition
{
    public string Id { get; }
    public string Title { get; }
    public string Description { get; }
    public SolutionQuality Quality { get; }
    public SolutionChoiceRound ChoiceRound { get; }
    public SolutionVisualModule VisualModules { get; }

    public bool OpensExitDoor
    {
        get { return Quality != SolutionQuality.INADEQUATE; }
    }

    public InterventionSolutionDefinition(
        string id,
        string title,
        string description,
        SolutionQuality quality,
        SolutionChoiceRound choiceRound,
        SolutionVisualModule visualModules)
    {
        Id = id;
        Title = title;
        Description = description;
        Quality = quality;
        ChoiceRound = choiceRound;
        VisualModules = visualModules;
    }
}

public sealed class PresentedSolutionDefinition
{
    public string SolutionId { get; }
    public int Position { get; }

    public PresentedSolutionDefinition(string solutionId, int position)
    {
        SolutionId = solutionId;
        Position = position;
    }
}

public sealed class PresentedJustificationDefinition
{
    public string JustificationId { get; }
    public int Position { get; }

    public PresentedJustificationDefinition(string justificationId, int position)
    {
        JustificationId = justificationId;
        Position = position;
    }
}

public sealed class InterventionJustificationDefinition
{
    public string Id { get; }
    public string Text { get; }

    public InterventionJustificationDefinition(string id, string text)
    {
        Id = id;
        Text = text;
    }
}
