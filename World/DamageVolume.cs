using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public sealed class DamageVolume
{
    public Rectangle Bounds { get; }

    public DamageVolume(Rectangle bounds)
    {
        Bounds = bounds;
    }
}
