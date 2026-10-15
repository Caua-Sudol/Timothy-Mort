using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public enum ScenarioCameraBehavior
{
    FOLLOW_PLAYER,
    CENTER_MAP
}

public sealed class ScenarioDefinition
{
    public string Id { get; }
    public string MapPath { get; }
    public string LdtkLevelId { get; }
    public Vector2 FallbackPlayerStart { get; }
    public ScenarioCameraBehavior CameraBehavior { get; }
    public float? PlayerScreenHeightRatio { get; }
    public bool SupportsPlayerScalePreview { get; }

    public ScenarioDefinition(
        string id,
        string mapPath,
        Vector2 fallbackPlayerStart,
        string ldtkLevelId = null,
        ScenarioCameraBehavior cameraBehavior = ScenarioCameraBehavior.FOLLOW_PLAYER,
        float? playerScreenHeightRatio = null,
        bool supportsPlayerScalePreview = false)
    {
        Id = id;
        MapPath = mapPath;
        LdtkLevelId = ldtkLevelId;
        FallbackPlayerStart = fallbackPlayerStart;
        CameraBehavior = cameraBehavior;
        PlayerScreenHeightRatio = playerScreenHeightRatio;
        SupportsPlayerScalePreview = supportsPlayerScalePreview;
    }
}
