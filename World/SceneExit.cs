using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public sealed class SceneExit
{
    public Rectangle Bounds { get; }
    public string DestinationScenarioId { get; }

    public SceneExit(Rectangle bounds, string destinationScenarioId)
    {
        Bounds = bounds;
        DestinationScenarioId = destinationScenarioId;
    }
}
