using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Where an LDtk room's exits lead, taken from the phase that owns the scenario.
public sealed record LdtkRoomLinks(
    string NextScenarioId,
    string PreviousScenarioId,
    string InterventionScenarioId,
    bool IsInterventionRoom,
    bool IsLastRoom)
{
    public static LdtkRoomLinks For(string scenarioId)
    {
        PhaseDefinition phase = PhaseCatalog.GetForScenario(scenarioId);
        if (phase == null || !phase.UsesRoomFormat)
        {
            return new LdtkRoomLinks(ScenarioCatalog.InitialScenarioId,
                ScenarioCatalog.InitialScenarioId, ScenarioCatalog.AffordanceInterventionScenarioId,
                false, false);
        }

        IReadOnlyList<string> rooms = phase.RoomScenarioIds;
        int index = phase.GetRoomIndex(scenarioId);
        if (index < 0)
        {
            // Intervention room: its exit goes back to the room the player came from
            // (resolved by Scene from PhaseState.InterventionReturnScenarioId).
            return new LdtkRoomLinks(rooms[0], rooms[0], phase.InterventionScenarioId, true, false);
        }

        bool isLast = index == rooms.Count - 1;
        return new LdtkRoomLinks(
            isLast ? ScenarioCatalog.InitialScenarioId : rooms[index + 1],
            index > 0 ? rooms[index - 1] : rooms[0],
            phase.InterventionScenarioId,
            false,
            isLast);
    }
}

public partial class LevelMap
{
    public bool IsLdtk => _mapPath.EndsWith(".ldtk", StringComparison.OrdinalIgnoreCase);
    private LdtkRoomLinks _roomLinks;
    // Phase 2 hot rock: also listed in _platforms (it is solid); damage comes in Scene.
    private List<Rectangle> _hotSurfaces = [];
    public IReadOnlyList<Rectangle> HotSurfaces => _hotSurfaces;
    // Phase 3 impostor rock: looks like a hot rock but costs two hearts. Also in _platforms
    // and _hotSurfaces, so it is solid and hurts like the known one.
    private List<Rectangle> _impostorRocks = [];
    public IReadOnlyList<Rectangle> ImpostorRocks => _impostorRocks;
    public bool IsImpostorRock(Rectangle surface) => _impostorRocks.Contains(surface);
    // Tutorial "Texto" and "Seta" entities, drawn by Scene on the background.
    private List<(Rectangle Bounds, string Text)> _signTexts = [];
    private List<(Rectangle Bounds, string Direction)> _signArrows = [];
    public IReadOnlyList<(Rectangle Bounds, string Text)> SignTexts => _signTexts;
    public IReadOnlyList<(Rectangle Bounds, string Direction)> SignArrows => _signArrows;
    // "Controles" signs (tutorial) and "FalaMort" areas (Mort's short balloon, once per session).
    private List<Rectangle> _controlSigns = [];
    private List<(Rectangle Bounds, string Text, string Id)> _mortCallouts = [];
    public IReadOnlyList<Rectangle> ControlSigns => _controlSigns;
    public IReadOnlyList<(Rectangle Bounds, string Text, string Id)> MortCallouts => _mortCallouts;
    private List<(Rectangle Bounds, Rectangle Source, SpriteEffects Flip, float Alpha, int TilesetUid, SolutionVisualModule Solution, int LayerOrder)> _ldtkTiles = [];
    private Dictionary<int, string> _ldtkTilesetPaths = [];
    private Dictionary<int, Texture2D> _ldtkTextures = [];
    private GraphicsDevice _ldtkGraphicsDevice;
    private LdtkTextureCache _ldtkTextureCache;

    internal void UseLdtkTextureCache(LdtkTextureCache cache) => _ldtkTextureCache = cache;

    // Intervention room art (every phase): one full 1920x1080 background drawn to cover the room.
    // Collision still comes only from the LDtk Colisao layer.
    private const string InterventionBackgroundFile = "../../assets/mg/root/intervencao/fundo_intervencao.png";
    // Decoracao entities keep reading their PNGs from assets/eureka.
    private const string InterventionSceneryDirectory = "../../assets/eureka";
    // Soft dark-blue light rising from the bottom of the intervention room.
    private static readonly Color InterventionGlowColor = new Color(40, 80, 170);
    private const float InterventionGlowMaxAlpha = 0.28f;
    private const float InterventionGlowHeightShare = 0.4f;
    private const int InterventionGlowBands = 48;
    private const double InterventionGlowPeriodSeconds = 6.0;
    private static Texture2D _whitePixel;
    private List<Texture2D> _sceneryBehindPlayer = [];
    private List<Texture2D> _sceneryInFrontOfPlayer = [];

    // "Decoracao" entities: loose PNGs from assets/eureka that float gently in place.
    private const float DecorationFloatPixels = 6f;
    private const float DecorationTiltDegrees = 1.2f;
    private const double DecorationMinPeriodSeconds = 3.5;
    private const double DecorationMaxPeriodSeconds = 5.5;
    // File is relative to the .ldtk folder.
    private List<(Rectangle Bounds, string File, bool InFront)> _decorations = [];
    private List<Texture2D> _decorationTextures = [];

    public void ReleaseLdtkTextures()
    {
        if (_ldtkTextureCache == null)
            foreach (Texture2D texture in _ldtkTextures.Values) texture.Dispose();
        _ldtkTextures.Clear();
        // Scenery is not released here: the shared cache owns and disposes it.
    }

    private void LoadInterventionScenery(GraphicsDevice graphicsDevice, string ldtkDirectory)
    {
        if (_roomLinks?.IsInterventionRoom != true || _ldtkTextureCache == null) return;
        _sceneryBehindPlayer = [_ldtkTextureCache.GetOrLoad(graphicsDevice,
            Path.GetFullPath(Path.Combine(ldtkDirectory, InterventionBackgroundFile)))];
        _sceneryInFrontOfPlayer = [];
        if (_whitePixel == null)
        {
            _whitePixel = new Texture2D(graphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
        }
    }

    private bool HasSceneryLayers => _sceneryBehindPlayer.Count + _sceneryInFrontOfPlayer.Count > 0;
    public bool HasLdtkScenery => HasSceneryLayers || _decorations.Count > 0;

    // Missing PNGs leave a null entry so the decoration is skipped instead of crashing the room.
    private void LoadDecorationTextures(GraphicsDevice graphicsDevice, string ldtkDirectory)
    {
        _decorationTextures = [];
        if (_ldtkTextureCache == null) return;
        foreach (var decoration in _decorations)
        {
            string path = Path.GetFullPath(Path.Combine(ldtkDirectory, decoration.File));
            if (File.Exists(path))
                _decorationTextures.Add(_ldtkTextureCache.GetOrLoad(graphicsDevice, path));
            else
            {
                Console.Error.WriteLine($"Decoracao: PNG nao encontrado: {path}");
                _decorationTextures.Add(null);
            }
        }
    }

    private void DrawDecorations(SpriteBatch batch, bool inFrontOfPlayer)
    {
        for (int index = 0; index < _decorations.Count && index < _decorationTextures.Count; index++)
        {
            var decoration = _decorations[index];
            Texture2D texture = _decorationTextures[index];
            if (texture == null || decoration.InFront != inFrontOfPlayer) continue;
            // Each piece gets its own rhythm so they never move in sync.
            double period = DecorationMinPeriodSeconds +
                (DecorationMaxPeriodSeconds - DecorationMinPeriodSeconds) * ((index * 0.618) % 1.0);
            double phase = _visualEffectTime / period * Math.PI * 2.0 + index * 2.4;
            float offsetY = (float)Math.Sin(phase) * DecorationFloatPixels;
            float tilt = (float)Math.Sin(phase * 0.7 + 1.3) * MathHelper.ToRadians(DecorationTiltDegrees);
            Vector2 center = decoration.Bounds.Center.ToVector2() + new Vector2(0f, offsetY);
            var scale = new Vector2(decoration.Bounds.Width / (float)texture.Width,
                decoration.Bounds.Height / (float)texture.Height);
            batch.Draw(texture, center, null, Color.White, tilt,
                new Vector2(texture.Width / 2f, texture.Height / 2f), scale, SpriteEffects.None, 0f);
        }
    }

    public void DrawLdtkScenery(SpriteBatch batch, bool inFrontOfPlayer)
    {
        // Decorations sit between the player and the full-screen layers on each side.
        if (inFrontOfPlayer) DrawDecorations(batch, true);
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
        if (!inFrontOfPlayer)
        {
            DrawInterventionGlow(batch);
            DrawDecorations(batch, false);
        }
    }

    // Kept faint and slow so the bottom reads as light, not as a path to jump into.
    private void DrawInterventionGlow(SpriteBatch batch)
    {
        if (_roomLinks?.IsInterventionRoom != true || _whitePixel == null) return;
        float breathing = 0.85f + 0.15f *
            (float)Math.Sin(_visualEffectTime / InterventionGlowPeriodSeconds * Math.PI * 2.0);
        float glowHeight = HeightInPixels * InterventionGlowHeightShare;
        float bandHeight = glowHeight / InterventionGlowBands;
        for (int band = 0; band < InterventionGlowBands; band++)
        {
            // Strongest at the bottom edge, fading out smoothly upwards.
            float strength = (band + 1) / (float)InterventionGlowBands;
            float alpha = InterventionGlowMaxAlpha * strength * strength * breathing;
            int top = (int)(HeightInPixels - glowHeight + band * bandHeight);
            int bottom = (int)Math.Ceiling(HeightInPixels - glowHeight + (band + 1) * bandHeight);
            batch.Draw(_whitePixel, new Rectangle(0, top, WidthInPixels, bottom - top),
                InterventionGlowColor * alpha);
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
        LoadDecorationTextures(graphicsDevice, directory);
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
        var candidate = new LevelMap(_mapPath, _ldtkLevelId, _roomLinks);
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
            if (candidate.HasSceneryLayers)
            {
                _sceneryBehindPlayer = candidate._sceneryBehindPlayer;
                _sceneryInFrontOfPlayer = candidate._sceneryInFrontOfPlayer;
            }
            // Decorations follow the edited LDtk on Ctrl+R; their PNGs come from the shared cache.
            _decorations = candidate._decorations;
            if (_ldtkGraphicsDevice != null)
                LoadDecorationTextures(_ldtkGraphicsDevice, Path.GetDirectoryName(ResolveLdtkPath()));
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
        LdtkRoomLinks links = _roomLinks ?? LdtkRoomLinks.For(null);
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
                    // Retired value (old phase 3 harmless spike): ignored, never treated as solid.
                    if (kind.Value.Equals("espinhoInofensivo", StringComparison.OrdinalIgnoreCase))
                        continue;
                    // Phase 3: looks like the gelatin already seen, but bounces the player up.
                    if (kind.Value.Equals("gelatinaTrampolim", StringComparison.OrdinalIgnoreCase))
                    {
                        _trampolineGelatins.AddRange(MergeLdtkCells(cells, columns, size, kind.Key, ox, oy));
                        continue;
                    }
                    // Phase 2: solid ground that hurts while the player touches it.
                    if (kind.Value.Equals("rochaQuente", StringComparison.OrdinalIgnoreCase))
                    {
                        var hotRocks = MergeLdtkCells(cells, columns, size, kind.Key, ox, oy).ToList();
                        _platforms.AddRange(hotRocks);
                        _hotSurfaces.AddRange(hotRocks);
                        continue;
                    }
                    // Phase 3: looks like the hot rock already seen, but costs two hearts.
                    if (kind.Value.Equals("rochaImpostora", StringComparison.OrdinalIgnoreCase))
                    {
                        var impostorRocks = MergeLdtkCells(cells, columns, size, kind.Key, ox, oy).ToList();
                        _platforms.AddRange(impostorRocks);
                        _hotSurfaces.AddRange(impostorRocks);
                        _impostorRocks.AddRange(impostorRocks);
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
                string entityId = entity.GetProperty("__identifier").GetString();
                // "Decor_*" entities: the PNG is the tileset of the entity's editor preview tile.
                if (entityId.StartsWith("Decor_", StringComparison.Ordinal))
                {
                    if (entity.GetProperty("__tile") is { ValueKind: JsonValueKind.Object } tile &&
                        tilesets.TryGetValue(tile.GetProperty("tilesetUid").GetInt32(), out JsonElement tileset))
                    {
                        bool front = GetLdtkField(entity, "Frente") is { ValueKind: JsonValueKind.True };
                        _decorations.Add((bounds, tileset.GetProperty("relPath").GetString(), front));
                    }
                    continue;
                }
                switch (entityId)
                {
                    case "PlayerSpawn":
                        PlayerSpawn = spawn; _playerSpawns["spawn_inicial"] = spawn; break;
                    case "InterventionReturn":
                        _playerSpawns["retorno_intervencao"] = spawn; break;
                    case "RoomReturn":
                        _playerSpawns["retorno_tela"] = spawn; break;
                    case "InterventionEntrance":
                        _interventionEntrances.Add(new InterventionEntrance(bounds,
                            links.InterventionScenarioId, "spawn_inicial",
                            InterventionEntrancePlacement.LATE)); break;
                    case "RoomExit":
                        _sceneExits.Add(new SceneExit(bounds, links.NextScenarioId,
                            links.IsInterventionRoom ? "retorno_intervencao" : "spawn_inicial",
                            links.IsLastRoom)); break;
                    case "RoomBack":
                        _sceneExits.Add(new SceneExit(bounds, links.PreviousScenarioId, "retorno_tela", false)); break;
                    case "InterventionSword":
                        _interventionButtons.Add(bounds); break;
                    case "Texto":
                    {
                        string value = GetLdtkField(entity, "Texto")?.GetString();
                        if (!string.IsNullOrWhiteSpace(value)) _signTexts.Add((bounds, value));
                        break;
                    }
                    case "Seta":
                        _signArrows.Add((bounds, GetLdtkField(entity, "Direcao")?.GetString() ?? "direita"));
                        break;
                    case "Controles":
                        _controlSigns.Add(bounds); break;
                    case "FalaMort":
                    {
                        string value = GetLdtkField(entity, "Texto")?.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                            _mortCallouts.Add((bounds, value, entity.GetProperty("iid").GetString()));
                        break;
                    }
                    case "Decoracao":
                    {
                        // Imagem: PNG name in assets/eureka (".png" optional). Frente: draw in front of the player.
                        string file = GetLdtkField(entity, "Imagem")?.GetString();
                        if (string.IsNullOrWhiteSpace(file)) break;
                        if (!file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) file += ".png";
                        bool inFront = GetLdtkField(entity, "Frente") is { ValueKind: JsonValueKind.True };
                        _decorations.Add((bounds, Path.Combine(InterventionSceneryDirectory, file), inFront));
                        break;
                    }
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

    private static JsonElement? GetLdtkField(JsonElement entity, string identifier)
    {
        foreach (var field in entity.GetProperty("fieldInstances").EnumerateArray())
            if (field.GetProperty("__identifier").GetString() == identifier)
            {
                JsonElement value = field.GetProperty("__value");
                return value.ValueKind == JsonValueKind.Null ? null : value;
            }
        return null;
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
        // Phase 3 (Diferenciar): mini-jump and blue fire art cover the impostors' normal art.
        if (name.StartsWith("mini_jump", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("blue_fire", StringComparison.OrdinalIgnoreCase))
            return SolutionVisualModule.IMPOSTOR_DIFFERENTIATE;
        return SolutionVisualModule.NONE;
    }

    public void DrawLdtkBackground(SpriteBatch batch, SolutionVisualModule modules)
    {
        if (IsLdtk) DrawLdtk(batch, modules, true);
    }

    private readonly HashSet<int> _ldtkBackgroundLayers = [];

    // Hidden base tiles per (active modules, background pass); rebuilt when the tile list changes.
    private readonly Dictionary<(SolutionVisualModule, bool), HashSet<int>> _hiddenLdtkTiles = [];
    private object _hiddenLdtkTilesSource;

    // Solution art replaces covered terrain even when its grid size or layer order differs.
    // A base tile inside gelatin and under a solution tile is hidden, and so is every tile
    // touching it in the same layer that solution art also covers, so a whole rock
    // (including parts without collision) swaps at once. Uncovered pieces stay intact.
    private HashSet<int> GetHiddenLdtkTiles(SolutionVisualModule modules, bool background)
    {
        if (!ReferenceEquals(_hiddenLdtkTilesSource, _ldtkTiles))
        {
            _hiddenLdtkTiles.Clear();
            _hiddenLdtkTilesSource = _ldtkTiles;
        }
        if (_hiddenLdtkTiles.TryGetValue((modules, background), out HashSet<int> hidden))
            return hidden;

        var replacements = new List<Rectangle>();
        foreach (var tile in _ldtkTiles)
            if (tile.Solution != SolutionVisualModule.NONE && modules.HasFlag(tile.Solution) &&
                _ldtkBackgroundLayers.Contains(tile.LayerOrder) == background)
                replacements.Add(tile.Bounds);

        bool IsBase(int index) => _ldtkTiles[index].Solution == SolutionVisualModule.NONE &&
            _ldtkBackgroundLayers.Contains(_ldtkTiles[index].LayerOrder) == background;

        hidden = [];
        var pending = new Queue<int>();
        for (int index = 0; index < _ldtkTiles.Count; index++)
        {
            Rectangle bounds = _ldtkTiles[index].Bounds;
            if (IsBase(index) && (_gelatins.Any(g => g.Contains(bounds.Center)) ||
                                  IsInsideImpostor(bounds.Center)) &&
                replacements.Any(area => area.Contains(bounds)) && hidden.Add(index))
                pending.Enqueue(index);
        }

        while (pending.Count > 0)
        {
            var current = _ldtkTiles[pending.Dequeue()];
            Rectangle touchArea = current.Bounds;
            touchArea.Inflate(1, 1);
            for (int index = 0; index < _ldtkTiles.Count; index++)
                if (!hidden.Contains(index) && IsBase(index) &&
                    _ldtkTiles[index].LayerOrder == current.LayerOrder &&
                    touchArea.Intersects(_ldtkTiles[index].Bounds) &&
                    // Only pieces under solution art: a solid neighbour must stay visible.
                    replacements.Any(area => area.Intersects(_ldtkTiles[index].Bounds)))
                {
                    hidden.Add(index);
                    pending.Enqueue(index);
                }
        }

        _hiddenLdtkTiles[(modules, background)] = hidden;
        return hidden;
    }

    private static readonly Color ImpostorEmberColor = new Color(230, 50, 40);

    private bool IsInsideImpostor(Point point) =>
        _trampolineGelatins.Any(g => g.Contains(point)) ||
        _impostorRocks.Any(r => r.Contains(point));

    private void DrawLdtk(SpriteBatch batch, SolutionVisualModule modules, bool background = false)
    {
        HashSet<int> hidden = GetHiddenLdtkTiles(modules, background);
        bool animate = modules.HasFlag(SolutionVisualModule.IMPOSTOR_ANIMATE);
        for (int index = 0; index < _ldtkTiles.Count; index++)
        {
            var tile = _ldtkTiles[index];
            if (_ldtkBackgroundLayers.Contains(tile.LayerOrder) != background) continue;
            if (tile.Solution != SolutionVisualModule.NONE && !modules.HasFlag(tile.Solution))
                continue;
            if (hidden.Contains(index)) continue;
            var gelatin = _gelatins.FirstOrDefault(g => g.Contains(tile.Bounds.Center));
            var miniJump = animate
                ? _trampolineGelatins.FirstOrDefault(g => g.Contains(tile.Bounds.Center))
                : Rectangle.Empty;
            Rectangle destination = tile.Bounds;
            // Animar (phase 3): only the mini-jump moves (stretches up); the known gelatin stays still.
            if (miniJump.Width > 0)
                destination = MapIntoAnimatedVolume(tile.Bounds, miniJump, GetMiniJumpVisualBounds(miniJump));
            else if (gelatin.Width > 0 && modules.HasFlag(SolutionVisualModule.GELATIN_JELLY_MOTION))
                destination = MapIntoAnimatedVolume(tile.Bounds, gelatin, GetGelatinVisualBounds(gelatin, modules));
            batch.Draw(_ldtkTextures[tile.TilesetUid], destination, tile.Source, Color.White * tile.Alpha,
                0, Vector2.Zero, tile.Flip, 0);
        }
        if (animate && !background) DrawImpostorRockEmbers(batch);
    }

    // Transform shared edges relative to the whole volume so adjacent tiles
    // and correction layers deform together instead of opening seams.
    private static Rectangle MapIntoAnimatedVolume(Rectangle bounds, Rectangle volume, Rectangle animated)
    {
        int MapX(int x) => animated.X + (int)Math.Round(
            (x - volume.X) * (double)animated.Width / volume.Width);
        int MapY(int y) => animated.Y + (int)Math.Round(
            (y - volume.Y) * (double)animated.Height / volume.Height);
        int left = MapX(bounds.Left);
        int top = MapY(bounds.Top);
        return new Rectangle(left, top, MapX(bounds.Right) - left, MapY(bounds.Bottom) - top);
    }

    // Like a spring: stretches up from its base, then a short squash, hinting at the jump.
    private Rectangle GetMiniJumpVisualBounds(Rectangle volume)
    {
        double wave = Math.Sin(_visualEffectTime * 3.0 + volume.X * 0.01);
        double amount = wave > 0 ? wave : wave * 0.35;
        int stretch = (int)Math.Round(amount * Math.Min(18, volume.Height / 4));
        int squeeze = stretch / 3;
        return new Rectangle(volume.X + squeeze, volume.Y - stretch,
            volume.Width - squeeze * 2, volume.Height + stretch);
    }

    // Animar: red embers rising from the impostor rocks only (the known hot rock stays still).
    private void DrawImpostorRockEmbers(SpriteBatch batch)
    {
        foreach (Rectangle rock in _impostorRocks)
        {
            Color color = ImpostorEmberColor;
            int count = Math.Max(1, rock.Width / 32);
            for (int i = 0; i < count; i++)
            {
                // Each ember has its own cycle offset so they rise out of step.
                double cycle = (_visualEffectTime * 0.7 + i * 0.37 + rock.X * 0.013) % 1.0;
                float x = rock.X + (i + 0.5f) * rock.Width / count +
                          (float)Math.Sin((_visualEffectTime + i) * 3.0) * 4f;
                float y = rock.Y - (float)cycle * 40f;
                int size = cycle < 0.5 ? 6 : 4;
                batch.Draw(_platformTexture,
                    new Rectangle((int)x - size / 2, (int)y - size / 2, size, size),
                    color * (float)(1.0 - cycle));
            }
        }
    }
}
