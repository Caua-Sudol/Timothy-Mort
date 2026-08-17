using System;
using System.Linq;
using TiledSharp;

namespace DontLikePoetry;

public static class TmxMapValidator
{
    private const string PlatformLayerName = "mapaGeral";
    private const string PlayerSpawnLayerName = "spawnJogador";

    public static bool TryLoad(
        string mapPath,
        out TmxMap map,
        out string errorMessage)
    {
        try
        {
            map = new TmxMap(mapPath);
        }
        catch (Exception)
        {
            map = null;
            errorMessage = "Could not read the TMX file.";
            return false;
        }

        if (!map.ObjectGroups.Any(group => group.Name == PlatformLayerName))
        {
            errorMessage = "Required layer 'mapaGeral' is missing.";
            return false;
        }

        var playerSpawnLayer = map.ObjectGroups.FirstOrDefault(
            group => group.Name == PlayerSpawnLayerName);

        if (playerSpawnLayer == null)
        {
            errorMessage = "Required layer 'spawnJogador' is missing.";
            return false;
        }

        if (!playerSpawnLayer.Objects.Any(
                spawn => IsValidPosition(spawn.X, spawn.Y)))
        {
            errorMessage = "Layer 'spawnJogador' has no valid spawn.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    internal static bool IsValidPosition(double x, double y)
    {
        return !double.IsNaN(x) &&
            !double.IsInfinity(x) &&
            !double.IsNaN(y) &&
            !double.IsInfinity(y) &&
            x >= -float.MaxValue &&
            x <= float.MaxValue &&
            y >= -float.MaxValue &&
            y <= float.MaxValue;
    }
}
