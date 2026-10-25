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
    private List<(Rectangle Bounds, Rectangle Source, SpriteEffects Flip, float Alpha, int TilesetUid, SolutionVisualModule Solution, int LayerOrder)> _ldtkTiles = [];
    private Dictionary<int, string> _ldtkTilesetPaths = [];
    private Dictionary<int, Texture2D> _ldtkTextures = [];
    private GraphicsDevice _ldtkGraphicsDevice;
    private LdtkTextureCache _ldtkTextureCache;

    internal void UseLdtkTextureCache(LdtkTextureCache cache) => _ldtkTextureCache = cache;

    // Sala_intervencao art: full 1920x1080 layers, back to front, drawn to cover the room.
    // Collision still comes only from the LDtk Colisao layer.
    private const string InterventionLevelId = "Sala_intervencao";
    private const string InterventionSceneryDirectory = "../../assets/eureka";
    private static readonly string[] InterventionSceneryBehindPlayer =
        { "Intervencao_bg_geral.png", "fumaca_frente.png", "Intervencao_plataformas_fundo.png" };
    private static readonly string[] InterventionSceneryInFrontOfPlayer =
        { "Intervencao_plataformas_fg.png", "fg_fumaca_frente.png" };
    private List<Texture2D> _sceneryBehindPlayer = [];
    private List<Texture2D> _sceneryInFrontOfPlayer = [];

    public void ReleaseLdtkTextures()
    {
        if (_ldtkTextureCache == null)
            foreach (Texture2D texture in _ldtkTextures.Values) texture.Dispose();
        _ldtkTextures.Clear();
        // Scenery is not released here: the shared cache owns and disposes it.
    }

    private void LoadInterventionScenery(GraphicsDevice graphicsDevice, string ldtkDirectory)
    {
        if (_ldtkLevelId != InterventionLevelId || _ldtkTextureCache == null) return;
        List<Texture2D> Load(string[] files) => files
            .Select(file => _ldtkTextureCache.GetOrLoad(graphicsDevice, Path.GetFullPath(
                Path.Combine(ldtkDirectory, InterventionSceneryDirectory, file))))
            .ToList();
        _sceneryBehindPlayer = Load(InterventionSceneryBehindPlayer);
        _sceneryInFrontOfPlayer = Load(InterventionSceneryInFrontOfPlayer);
    }

    public bool HasLdtkScenery => _sceneryBehindPlayer.Count + _sceneryInFrontOfPlayer.Count > 0;

    public void DrawLdtkScenery(SpriteBatch batch, bool inFrontOfPlayer)
    {
        foreach (Texture2D texture in inFrontOfPlayer ? _sceneryInFrontOfPlayer : _sceneryBehindPlayer)
        {
            // Same as LDtk's "Cover" background: fill the room, keep proportions, center the crop.
            float scale = Math.Max(WidthInPixels / (float)texture.Width,
                HeightInPixels / (float)texture.Height);
            var position = new Vector2(
                (WidthInPixels - texture.Width * scale) / 2f,
                (HeightInPixels - texture.Height * scale) / 2f);
            batch.Draw(texture, position, null, Color.White, 0f, Vector2.Zero, scale,
                SpriteEffects.None, 0f);
        }
    }

    private void LoadLdtkTextures(GraphicsDevice graphicsDevice)
    {
        string directory = Path.GetDirectoryName(ResolveLdtkPath());
        foreach (int uid in _ldtkTiles.Select(tile => tile.TilesetUid).Distinct())
        {
            string path = Path.GetFullPath(Path.Combine(directory, _ldtkTilesetPaths[uid]));
            Texture2D texture = _ldtkTextureCache != null
                ? _ldtkTextureCache.GetOrLoad(graphicsDevice, path)
                : LdtkTextureCache.LoadTexture(graphicsDevice, path);
            _ldtkTextures.Add(uid, texture);
        }
        LoadInterventionScenery(graphicsDevice, directory);
        foreach (var tile in _ldtkTiles)
        {
            Texture2D texture = _ldtkTextures[tile.TilesetUid];
            if (tile.Source.X < 0 || tile.Source.Y < 0 ||
                tile.Source.Right > texture.Width || tile.Source.Bottom > texture.Height)
                throw new InvalidDataException(
                    $"Recorte fora do PNG do tileset {tile.TilesetUid}: {_ldtkTilesetPaths[tile.TilesetUid]}");
        }
    }

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
        using var loading = new LoadDiagnostics($"LDtk {_ldtkLevelId}");
        var candidate = new LevelMap(_mapPath, _ldtkLevelId);
        // Initial room entry can reuse the cache. Ctrl+R loads independent replacements,
        // keeping the current room's textures valid if reading or validation fails.
        if (_ldtkTextures.Count == 0)
            candidate._ldtkTextureCache = _ldtkTextureCache;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(ResolveLdtkPath()));
            loading.Mark("ler e interpretar JSON");
            candidate.LoadLdtk(document.RootElement);
            loading.Mark("montar entidades, colisoes e tiles");
            if (_ldtkGraphicsDevice != null)
                candidate.LoadLdtkTextures(_ldtkGraphicsDevice);
            loading.Mark("carregar e validar tilesets");
            if (_ldtkTextureCache != null && candidate._ldtkTextureCache == null)
            {
                string directory = Path.GetDirectoryName(ResolveLdtkPath());
                var replacements = candidate._ldtkTextures.ToDictionary(
                    entry => Path.GetFullPath(Path.Combine(directory, candidate._ldtkTilesetPaths[entry.Key])),
                    entry => entry.Value);
                ApplyObjectState(candidate);
                ReleaseLdtkTextures();
                foreach (var entry in replacements)
                    _ldtkTextureCache.Replace(entry.Key, entry.Value);
                candidate._ldtkTextureCache = _ldtkTextureCache;
            }
            else
            {
                ApplyObjectState(candidate);
                ReleaseLdtkTextures();
            }
            _ldtkTiles = candidate._ldtkTiles;
            _ldtkBackgroundLayers.Clear();
            _ldtkBackgroundLayers.UnionWith(candidate._ldtkBackgroundLayers);
            _ldtkTilesetPaths = candidate._ldtkTilesetPaths;
            _ldtkTextures = candidate._ldtkTextures;
            // Only the first load reads scenery (through the cache); Ctrl+R keeps it.
            if (candidate.HasLdtkScenery)
            {
                _sceneryBehindPlayer = candidate._sceneryBehindPlayer;
                _sceneryInFrontOfPlayer = candidate._sceneryInFrontOfPlayer;
            }
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            candidate.ReleaseLdtkTextures();
            error = "LDtk: " + ex.Message;
            return false;
        }
    }

    private void LoadLdtk(JsonElement root)
    {
        JsonElement level = _ldtkLevelId == null ? root.GetProperty("levels")[0] :
            root.GetProperty("levels").EnumerateArray().First(l =>
                l.GetProperty("identifier").GetString() == _ldtkLevelId);
        string levelId = level.GetProperty("identifier").GetString();
        string nextRoom = levelId == "AffordanceTela1" ? ScenarioCatalog.AffordanceRoom2ScenarioId :
            levelId == "AffordanceTela2" ? ScenarioCatalog.AffordanceRoom3ScenarioId :
            levelId == "Sala_intervencao" ? ScenarioCatalog.AffordanceVisibilityScenarioId :
            ScenarioCatalog.InitialScenarioId;
        string previousRoom = levelId == "AffordanceTela3" ? ScenarioCatalog.AffordanceRoom2ScenarioId :
            ScenarioCatalog.AffordanceVisibilityScenarioId;
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
                    if (kind.Value.Equals("espinhos", StringComparison.OrdinalIgnoreCase))
                    {
                        _damageVolumes.AddRange(MergeLdtkCells(cells, columns, size, kind.Key, ox, oy)
                            .Select(bounds => new DamageVolume(bounds)));
                        continue;
                    }
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
                        _sceneExits.Add(new SceneExit(bounds, nextRoom,
                            levelId == "Sala_intervencao" ? "retorno_intervencao" : "spawn_inicial",
                            levelId == "AffordanceTela3")); break;
                    case "RoomBack":
                        _sceneExits.Add(new SceneExit(bounds, previousRoom, "retorno_tela", false)); break;
                    case "InterventionSword":
                        _interventionButtons.Add(bounds); break;
                }
            }
        }
        if (!PlayerSpawn.HasValue)
            throw new InvalidDataException("Adicione a entidade PlayerSpawn ao nivel.");
        // LDtk stores the topmost layer first; render bottom to top.
        int layerOrder = 0;
        foreach (var layer in layers.Reverse())
        {
            layerOrder++;
            if (string.Equals(layer.GetProperty("__identifier").GetString(), "Casa_BG", StringComparison.OrdinalIgnoreCase))
                _ldtkBackgroundLayers.Add(layerOrder);
            SolutionVisualModule solution = GetLdtkLayerSolution(layer.GetProperty("__identifier").GetString());
            // Solution layers can be hidden in the editor while composing the base terrain.
            if (solution == SolutionVisualModule.NONE && !layer.GetProperty("visible").GetBoolean()) continue;
            var uid = layer.GetProperty("__tilesetDefUid");
            if (uid.ValueKind == JsonValueKind.Null) continue;
            int tilesetUid = uid.GetInt32();
            var tileset = tilesets[tilesetUid];
            string path = tileset.GetProperty("relPath").GetString();
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidDataException($"Tileset {tilesetUid} sem caminho de imagem.");
            _ldtkTilesetPaths[tilesetUid] = path;
            int size = tileset.GetProperty("tileGridSize").GetInt32();
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
                    effects, alpha * tile.GetProperty("a").GetSingle(), tilesetUid, solution, layerOrder));
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

    private static SolutionVisualModule GetLdtkLayerSolution(string name)
    {
        if (string.Equals(name, "solucao_gelatina", StringComparison.OrdinalIgnoreCase))
            return SolutionVisualModule.GELATIN_COLOR;
        if (string.Equals(name, "solucao_mark", StringComparison.OrdinalIgnoreCase))
            return SolutionVisualModule.GELATIN_TERRAIN_MARK;
        return SolutionVisualModule.NONE;
    }

    public void DrawLdtkBackground(SpriteBatch batch, SolutionVisualModule modules)
    {
        if (IsLdtk) DrawLdtk(batch, modules, true);
    }

    private readonly HashSet<int> _ldtkBackgroundLayers = [];

    private void DrawLdtk(SpriteBatch batch, SolutionVisualModule modules, bool background = false)
    {
        // Replace only covered terrain cells; unpainted cells retain their original art.
        var replacements = new Dictionary<Rectangle, int>();
        foreach (var tile in _ldtkTiles)
            if (tile.Solution != SolutionVisualModule.NONE && modules.HasFlag(tile.Solution))
                replacements[tile.Bounds] = Math.Max(
                    replacements.GetValueOrDefault(tile.Bounds), tile.LayerOrder);

        foreach (var tile in _ldtkTiles)
        {
            if (_ldtkBackgroundLayers.Contains(tile.LayerOrder) != background) continue;
            if (tile.Solution != SolutionVisualModule.NONE && !modules.HasFlag(tile.Solution))
                continue;
            var gelatin = _gelatins.FirstOrDefault(g => g.Contains(tile.Bounds.Center));
            if (tile.Solution == SolutionVisualModule.NONE && gelatin.Width > 0 &&
                replacements.TryGetValue(tile.Bounds, out int replacementLayer) &&
                tile.LayerOrder < replacementLayer)
                continue;
            Color tint = Color.White;
            Rectangle destination = tile.Bounds;
            if (gelatin.Width > 0 && modules.HasFlag(SolutionVisualModule.GELATIN_JELLY_MOTION))
            {
                Rectangle animated = GetGelatinVisualBounds(gelatin, modules);
                // Transform shared edges relative to the whole volume so adjacent tiles
                // and correction layers deform together instead of opening seams.
                int MapX(int x) => animated.X + (int)Math.Round(
                    (x - gelatin.X) * (double)animated.Width / gelatin.Width);
                int MapY(int y) => animated.Y + (int)Math.Round(
                    (y - gelatin.Y) * (double)animated.Height / gelatin.Height);
                int left = MapX(tile.Bounds.Left);
                int top = MapY(tile.Bounds.Top);
                destination = new Rectangle(left, top,
                    MapX(tile.Bounds.Right) - left, MapY(tile.Bounds.Bottom) - top);
            }
            batch.Draw(_ldtkTextures[tile.TilesetUid], destination, tile.Source, tint * tile.Alpha,
                0, Vector2.Zero, tile.Flip, 0);
        }
    }
}
