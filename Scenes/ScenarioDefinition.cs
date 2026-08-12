using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public sealed class ScenarioDefinition
{
    public string Id { get; }
    public string MapPath { get; }
    public Vector2 FallbackPlayerStart { get; }

    public ScenarioDefinition(string id, string mapPath, Vector2 fallbackPlayerStart)
    {
        Id = id;
        MapPath = mapPath;
        FallbackPlayerStart = fallbackPlayerStart;
    }
}
