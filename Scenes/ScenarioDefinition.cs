using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public sealed class ScenarioDefinition
{
    public string Id { get; }
    public string MapPath { get; }
    public string LdtkLevelId { get; }
    public Vector2 FallbackPlayerStart { get; }

    public ScenarioDefinition(string id, string mapPath, Vector2 fallbackPlayerStart, string ldtkLevelId = null)
    {
        Id = id;
        MapPath = mapPath;
        LdtkLevelId = ldtkLevelId;
        FallbackPlayerStart = fallbackPlayerStart;
    }
}
