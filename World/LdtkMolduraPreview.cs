using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Reads the scale-test solids and auto-tiles from Colisao, with manual Correcoes tiles on top.
internal sealed class LdtkMolduraPreview
{
    private readonly Texture2D _terrain;
    private readonly List<(Rectangle Destination, Rectangle Source)> _tiles = [];
    public IReadOnlyList<Rectangle> FrameColliders { get; }
    public IReadOnlyList<Rectangle> GelatinVolumes => _gelatins;
    private readonly List<Rectangle> _gelatins = [];

    public LdtkMolduraPreview(ContentManager content, string projectPath)
    {
        _terrain = content.Load<Texture2D>("visual/teste_ldtk_terrain");
        FrameColliders = Load(projectPath, _tiles, _gelatins);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach ((Rectangle destination, Rectangle source) in _tiles)
            spriteBatch.Draw(_terrain, destination, source, Color.White);
    }

    private static IReadOnlyList<Rectangle> Load(
        string projectPath,
        List<(Rectangle Destination, Rectangle Source)> tiles,
        List<Rectangle> gelatins)
    {
        string path = Path.Combine(AppContext.BaseDirectory, projectPath);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement level = document.RootElement.GetProperty("levels")[0];
        int gelatinValue = -1;
        foreach (JsonElement definition in document.RootElement.GetProperty("defs").GetProperty("layers").EnumerateArray())
        {
            if (definition.GetProperty("identifier").GetString() != "Colisao") continue;
            foreach (JsonElement value in definition.GetProperty("intGridValues").EnumerateArray())
                if (string.Equals(value.GetProperty("identifier").GetString(), "Gelatina", StringComparison.OrdinalIgnoreCase))
                    gelatinValue = value.GetProperty("value").GetInt32();
        }
        foreach (JsonElement layer in level.GetProperty("layerInstances").EnumerateArray())
        {
            if (layer.GetProperty("__identifier").GetString() != "Colisao")
                continue;

            int columns = layer.GetProperty("__cWid").GetInt32();
            int tileSize = layer.GetProperty("__gridSize").GetInt32();
            List<Rectangle> colliders = [];
            JsonElement values = layer.GetProperty("intGridCsv");
            for (int index = 0; index < values.GetArrayLength(); index++)
            {
                if (values[index].GetInt32() == 0)
                    continue;

                int x = index % columns;
                int y = index / columns;
                if (values[index].GetInt32() == gelatinValue)
                    continue;
                colliders.Add(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize));
            }

            // Merge rectangular patches so crossing a tile seam does not recharge dash again.
            var visited = new bool[values.GetArrayLength()];
            for (int index = 0; index < visited.Length; index++)
            {
                if (visited[index] || values[index].GetInt32() != gelatinValue) continue;
                int x = index % columns, y = index / columns;
                int width = 1, height = 1;
                while (x + width < columns && !visited[index + width] &&
                       values[index + width].GetInt32() == gelatinValue) width++;
                while ((y + height) * columns + x + width <= visited.Length)
                {
                    bool matches = true;
                    for (int dx = 0; dx < width; dx++)
                    {
                        int next = (y + height) * columns + x + dx;
                        if (visited[next] || values[next].GetInt32() != gelatinValue) matches = false;
                    }
                    if (!matches) break;
                    height++;
                }
                for (int dy = 0; dy < height; dy++)
                for (int dx = 0; dx < width; dx++)
                    visited[(y + dy) * columns + x + dx] = true;
                gelatins.Add(new Rectangle(x * tileSize, y * tileSize, width * tileSize, height * tileSize));
            }

            foreach (JsonElement tile in layer.GetProperty("autoLayerTiles").EnumerateArray())
            {
                JsonElement position = tile.GetProperty("px");
                JsonElement source = tile.GetProperty("src");
                tiles.Add((
                    new Rectangle(position[0].GetInt32(), position[1].GetInt32(), tileSize, tileSize),
                    new Rectangle(source[0].GetInt32(), source[1].GetInt32(), tileSize, tileSize)));
            }

            // Correcoes is visual only and must be drawn after the automatic base.
            foreach (JsonElement correctionLayer in level.GetProperty("layerInstances").EnumerateArray())
            {
                if (correctionLayer.GetProperty("__identifier").GetString() != "Correcoes" ||
                    !correctionLayer.GetProperty("visible").GetBoolean())
                    continue;

                int correctionTileSize = correctionLayer.GetProperty("__gridSize").GetInt32();
                foreach (JsonElement tile in correctionLayer.GetProperty("gridTiles").EnumerateArray())
                {
                    JsonElement position = tile.GetProperty("px");
                    JsonElement source = tile.GetProperty("src");
                    tiles.Add((
                        new Rectangle(position[0].GetInt32(), position[1].GetInt32(),
                            correctionTileSize, correctionTileSize),
                        new Rectangle(source[0].GetInt32(), source[1].GetInt32(),
                            correctionTileSize, correctionTileSize)));
                }
            }

            return colliders;
        }

        throw new InvalidDataException("A camada Colisao não foi encontrada no teste LDtk.");
    }
}
