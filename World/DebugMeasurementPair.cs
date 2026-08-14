using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public sealed class DebugMeasurementPair
{
    public string PairId { get; }
    public string Action { get; }
    public Vector2 Origin { get; }
    public Vector2 Destination { get; }

    public DebugMeasurementPair(
        string pairId,
        string action,
        Vector2 origin,
        Vector2 destination)
    {
        PairId = pairId;
        Action = action;
        Origin = origin;
        Destination = destination;
    }
}
