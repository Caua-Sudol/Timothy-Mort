using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public partial class LevelMap
{
    public bool IsLdtk => _mapPath.EndsWith(".ldtk", StringComparison.OrdinalIgnoreCase);
    private List<(Rectangle Bounds, Rectangle Source, SpriteEffects Flip, float Alpha)> _ldtkTiles = [];
    private Texture2D _ldtkAtlas;
    public void SetLdtkAtlas(Texture2D atlas) => _ldtkAtlas = atlas;

    private string ResolveLdtkPath()
    {
        // Prefer the editable project file during development, including Ctrl+R.
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "DontLikePoetry.csproj")))
                return Path.Combine(directory.FullName, _mapPath);
        return Path.Combine(AppContext.BaseDirectory, _mapPath);
    }

    private bool TryReloadLdtk(out string error)
    {
        try
        {
            var candidate = new LevelMap(_mapPath);
            using var document = JsonDocument.Parse(File.ReadAllText(ResolveLdtkPath()));
            candidate.LoadLdtk(document.RootElement);
            ApplyObjectState(candidate);
            _ldtkTiles = candidate._ldtkTiles;
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = "LDtk: " + ex.Message;
            return false;
        }
    }

    private void LoadLdtk(JsonElement root)
    {
        JsonElement level = root.GetProperty("levels")[0];
        WidthInPixels = level.GetProperty("pxWid").GetInt32();
        HeightInPixels = level.GetProperty("pxHei").GetInt32();
        var definitions = root.GetProperty("defs").GetProperty("layers").EnumerateArray()
            .ToDictionary(d => d.GetProperty("uid").GetInt32());
        var tilesets = root.GetProperty("defs").GetProperty("tilesets").EnumerateArray()
            .ToDictionary(d => d.GetProperty("uid").GetInt32());
        var layers = level.GetProperty("layerInstances").EnumerateArray().ToArray();
        foreach (var layer in layers)
        {
            string name = layer.GetProperty("__identifier").GetString();
            int size = layer.GetProperty("__gridSize").GetInt32();
            int ox = layer.GetProperty("__pxTotalOffsetX").GetInt32();
            int oy = layer.GetProperty("__pxTotalOffsetY").GetInt32();
            if (name == "Colisao" || name.Equals("Gelatina", StringComparison.OrdinalIgnoreCase))
            {
                var definition = definitions[layer.GetProperty("layerDefUid").GetInt32()];
                var kinds = definition.GetProperty("intGridValues").EnumerateArray()
                    .ToDictionary(v => v.GetProperty("value").GetInt32(),
                        v => v.GetProperty("identifier").GetString() ?? "");
                int[] cells = layer.GetProperty("intGridCsv").EnumerateArray().Select(v => v.GetInt32()).ToArray();
                int columns = layer.GetProperty("__cWid").GetInt32();
                foreach (var kind in kinds)
                {
                    bool gelatin = name.Equals("Gelatina", StringComparison.OrdinalIgnoreCase) ||
                        kind.Value.Equals("gelatina", StringComparison.OrdinalIgnoreCase);
                    if (!gelatin && !kind.Value.Equals("solido", StringComparison.OrdinalIgnoreCase)) continue;
                    var target = gelatin ? _gelatins : _platforms;
                    target.AddRange(MergeLdtkCells(cells, columns, size, kind.Key, ox, oy));
                }
            }
            foreach (var entity in layer.GetProperty("entityInstances").EnumerateArray())
            {
                var px = entity.GetProperty("px");
                var pivot = entity.GetProperty("__pivot");
                int width = entity.GetProperty("width").GetInt32(), height = entity.GetProperty("height").GetInt32();
                int x = px[0].GetInt32() + ox, y = px[1].GetInt32() + oy;
                var bounds = new Rectangle(x - (int)(pivot[0].GetSingle() * width),
                    y - (int)(pivot[1].GetSingle() * height), width, height);
                // Spawn entity pivot is the player's feet; Scene uses a legacy 16px spawn marker.
                Vector2 spawn = new(x - 8, y - 16);
                switch (entity.GetProperty("__identifier").GetString())
                {
                    case "PlayerSpawn":
                        PlayerSpawn = spawn; _playerSpawns["spawn_inicial"] = spawn; break;
                    case "InterventionReturn":
                        _playerSpawns["retorno_intervencao"] = spawn; break;
                    case "RoomReturn":
                        _playerSpawns["retorno_tela"] = spawn; break;
                    case "InterventionEntrance":
                        _interventionEntrances.Add(new InterventionEntrance(bounds,
                            ScenarioCatalog.AffordanceInterventionScenarioId, "spawn_inicial",
                            InterventionEntrancePlacement.LATE)); break;
                    case "RoomExit":
                        _sceneExits.Add(new SceneExit(bounds, ScenarioCatalog.AffordanceRoom2ScenarioId,
                            "spawn_inicial", false)); break;
                }
            }
        }
        if (!PlayerSpawn.HasValue)
            throw new InvalidDataException("Adicione a entidade PlayerSpawn ao nivel.");
        // LDtk stores the topmost layer first; render bottom to top.
        foreach (var layer in layers.Reverse())
        {
            if (!layer.GetProperty("visible").GetBoolean()) continue;
            var uid = layer.GetProperty("__tilesetDefUid");
            if (uid.ValueKind == JsonValueKind.Null) continue;
            int size = tilesets[uid.GetInt32()].GetProperty("tileGridSize").GetInt32();
            int ox = layer.GetProperty("__pxTotalOffsetX").GetInt32();
            int oy = layer.GetProperty("__pxTotalOffsetY").GetInt32();
            float alpha = layer.GetProperty("__opacity").GetSingle();
            foreach (string field in new[] { "autoLayerTiles", "gridTiles" })
            foreach (var tile in layer.GetProperty(field).EnumerateArray())
            {
                var px = tile.GetProperty("px"); var src = tile.GetProperty("src");
                int flip = tile.GetProperty("f").GetInt32();
                SpriteEffects effects = SpriteEffects.None;
                if ((flip & 1) != 0) effects |= SpriteEffects.FlipHorizontally;
                if ((flip & 2) != 0) effects |= SpriteEffects.FlipVertically;
                _ldtkTiles.Add((new Rectangle(px[0].GetInt32() + ox, px[1].GetInt32() + oy, size, size),
                    new Rectangle(src[0].GetInt32(), src[1].GetInt32(), size, size),
                    effects, alpha * tile.GetProperty("a").GetSingle()));
            }
        }
    }

    private static IEnumerable<Rectangle> MergeLdtkCells(int[] cells, int columns, int size, int value, int ox, int oy)
    {
        var used = new bool[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            if (used[i] || cells[i] != value) continue;
            int x = i % columns, y = i / columns, w = 1, h = 1;
            while (x + w < columns && i + w < cells.Length && !used[i + w] && cells[i + w] == value) w++;
            while ((y + h) * columns + x + w <= cells.Length)
            {
                bool match = true;
                for (int dx = 0; dx < w; dx++)
                {
                    int next = (y + h) * columns + x + dx;
                    if (used[next] || cells[next] != value) match = false;
                }
                if (!match) break;
                h++;
            }
            for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++) used[(y + dy) * columns + x + dx] = true;
            yield return new Rectangle(x * size + ox, y * size + oy, w * size, h * size);
        }
    }

    private void DrawLdtk(SpriteBatch batch, SolutionVisualModule modules)
    {
        foreach (var tile in _ldtkTiles)
        {
            var gelatin = _gelatins.FirstOrDefault(g => g.Contains(tile.Bounds.Center));
            Color tint = gelatin.Width > 0 && modules.HasFlag(SolutionVisualModule.GELATIN_COLOR)
                ? Color.DeepSkyBlue : Color.White;
            batch.Draw(_ldtkAtlas, tile.Bounds, tile.Source, tint * tile.Alpha,
                0, Vector2.Zero, tile.Flip, 0);
        }
    }
}
