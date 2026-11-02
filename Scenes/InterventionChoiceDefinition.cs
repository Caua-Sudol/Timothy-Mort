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
public enum SolutionVisualModule : long
{
    NONE = 0,
    PATH_INDICATOR = 1,
    GELATIN_COLOR = 2,
    GELATIN_TEXTURE = 4,
    GELATIN_GLOW = 8,
    PLAYER_DASH_RECOVERY_COLOR = 16,
    DESTINATION_HIGHLIGHT = 32,
    PLATFORM_DECORATION = 64,
    PLAYER_DASH_TRAIL = 128,
    PLATFORM_OUTLINE = 256,
    PLAYER_SURFACE_CONTACT_FLASH = 512,
    GELATIN_GLOW_PULSE = 1024,
    GELATIN_JELLY_MOTION = 2048,
    GELATIN_DASH_SYMBOL = 4096,
    PLAYER_DASH_RECHARGE_SYMBOL = 8192,
    PLAYER_DASH_ENERGY_ABSORPTION = 16384,
    GELATIN_DIRECTIONAL_ANIMATION = 32768,
    PLAYER_DASH_AVAILABLE_OUTLINE = 65536,
    DAMAGE_HUD_EMPHASIS = 131072,
    DANGER_PLATFORM_HIGHLIGHT = 262144,
    SAFE_ROUTE = 524288,
    DAMAGE_KNOCKBACK = 1048576,
    DAMAGE_PARTICLES = 2097152,
    INVULNERABILITY_BLINK = 4194304,
    INVULNERABILITY_OUTLINE = 8388608,
    DAMAGE_IMPACT_MARK = 16777216,
    INVULNERABILITY_OUTLINE_PULSE = 33554432,
    DAMAGE_COMPRESSION = 67108864,
    INVULNERABILITY_TRANSPARENCY = 134217728,
    INVULNERABILITY_SILHOUETTE = 268435456,
    CONCEPTUAL_RULE_REMINDERS = 536870912,
    RESTORE_TRAMPOLINE_GELATIN = 1073741824L,
    RESTORE_HARMLESS_SPIKE_DAMAGE = 2147483648L,
    DISTINCT_TRAMPOLINE_GELATIN = 4294967296L,
    TRAMPOLINE_DASH_EQUIVALENT = 8589934592L,
    DISTINCT_HARMLESS_SPIKES = 17179869184L,
    HARMLESS_SPIKE_DOUBLE_DAMAGE = 34359738368L,
    GELATIN_TERRAIN_MARK = 68719476736L,
    // Phase 2 solutions (effects drawn in Ckp 4).
    DAMAGE_HEART_CRACK = 137438953472L,
    DAMAGE_SCREEN_SHAKE = 274877906944L,
    // Phase 3 solutions for the impostor gelatins and rocks (effects in Ckp 10).
    IMPOSTOR_DIFFERENTIATE = 549755813888L,
    IMPOSTOR_RESTORE = 1099511627776L,
    IMPOSTOR_NEUTRALIZE = 2199023255552L
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
