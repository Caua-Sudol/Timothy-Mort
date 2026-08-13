using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public sealed class InterventionEntrance
{
    public Rectangle Bounds { get; }
    public string DestinationScenarioId { get; }
    public string DestinationSpawnId { get; }
    public InterventionEntrancePlacement Placement { get; }

    public InterventionEntrance(
        Rectangle bounds,
        string destinationScenarioId,
        string destinationSpawnId,
        InterventionEntrancePlacement placement)
    {
        Bounds = bounds;
        DestinationScenarioId = destinationScenarioId;
        DestinationSpawnId = destinationSpawnId;
        Placement = placement;
    }
}
