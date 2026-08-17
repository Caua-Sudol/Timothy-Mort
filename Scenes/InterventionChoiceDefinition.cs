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

[Flags]
public enum SolutionVisualModule
{
    NONE = 0,
    PATH_INDICATOR = 1,
    GELATIN_COLOR = 2,
    GELATIN_TEXTURE = 4,
    GELATIN_GLOW = 8,
    PLAYER_DASH_RECOVERY_COLOR = 16,
    DESTINATION_HIGHLIGHT = 32
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
