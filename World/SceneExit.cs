using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public sealed class SceneExit
{
    public Rectangle Bounds { get; }
    public string DestinationScenarioId { get; }
    public string DestinationSpawnId { get; }

    public SceneExit(
        Rectangle bounds,
        string destinationScenarioId,
        string destinationSpawnId)
    {
        Bounds = bounds;
        DestinationScenarioId = destinationScenarioId;
        DestinationSpawnId = destinationSpawnId;
    }
}
