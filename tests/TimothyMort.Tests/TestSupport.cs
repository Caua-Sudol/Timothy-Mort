using System.Text.Json;
using DontLikePoetry;
using Xunit;

// Some tests change static state (TutorialControls.Current), so they run one at a time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TimothyMort.Tests;

internal static class TestSupport
{
    public static readonly PhaseDefinition[] Phases =
    {
        PhaseCatalog.AffordanceVisibility,
        PhaseCatalog.DamageFeedback,
        PhaseCatalog.ConceptualModelsConstraints
    };

    public static IEnumerable<object[]> PhaseIds() =>
        Phases.Select(phase => new object[] { phase.Id });

    public static PhaseDefinition Phase(string id) => Phases.Single(phase => phase.Id == id);

    // The phase's three solutions, in a fixed order (by id) so tests are repeatable.
    public static IReadOnlyList<InterventionSolutionDefinition> Solutions(PhaseDefinition phase) =>
        phase.ChoiceCatalog.CreateInitialSolutionPresentation(new Random(0))
            .Select(entry => phase.ChoiceCatalog.GetSolution(entry.SolutionId))
            .OrderBy(solution => solution.Id, StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyList<InterventionJustificationDefinition> Justifications(
        PhaseDefinition phase) =>
        phase.ChoiceCatalog.CreateInitialJustificationPresentation(new Random(0))
            .Select(entry => phase.ChoiceCatalog.GetJustification(entry.JustificationId))
            .OrderBy(justification => justification.Id, StringComparer.Ordinal)
            .ToList();

    private static string _repositoryRoot;

    // Folder that holds DontLikePoetry.csproj, found by walking up from the test output.
    public static string RepositoryRoot
    {
        get
        {
            if (_repositoryRoot != null) return _repositoryRoot;
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null &&
                   !File.Exists(Path.Combine(directory.FullName, "DontLikePoetry.csproj")))
                directory = directory.Parent;
            _repositoryRoot = directory?.FullName ??
                throw new InvalidOperationException("DontLikePoetry.csproj não foi encontrado.");
            return _repositoryRoot;
        }
    }
}

// Reads the .ldtk straight from the source tree, as JSON, without the game's renderer.
internal sealed class LdtkProject
{
    public const string CollisionLayer = "Colisao";

    private readonly JsonElement _root;

    public string Directory { get; }

    private LdtkProject(JsonElement root, string directory)
    {
        _root = root;
        Directory = directory;
    }

    public static LdtkProject Load(string relativePath)
    {
        string path = Path.Combine(TestSupport.RepositoryRoot, relativePath);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        return new LdtkProject(document.RootElement.Clone(), Path.GetDirectoryName(path));
    }

    public IEnumerable<JsonElement> Levels => _root.GetProperty("levels").EnumerateArray();

    public IEnumerable<JsonElement> Tilesets =>
        _root.GetProperty("defs").GetProperty("tilesets").EnumerateArray();

    public IEnumerable<JsonElement> LayerDefinitions =>
        _root.GetProperty("defs").GetProperty("layers").EnumerateArray();

    public bool HasLevel(string identifier) =>
        Levels.Any(level => level.GetProperty("identifier").GetString() == identifier);

    public JsonElement Level(string identifier) =>
        Levels.Single(level => level.GetProperty("identifier").GetString() == identifier);

    public static IEnumerable<JsonElement> Layers(JsonElement level) =>
        level.GetProperty("layerInstances").EnumerateArray();

    public static IEnumerable<JsonElement> Entities(JsonElement level) =>
        Layers(level)
            .Where(layer => layer.TryGetProperty("entityInstances", out _))
            .SelectMany(layer => layer.GetProperty("entityInstances").EnumerateArray());

    public static int Count(JsonElement level, string entity) =>
        Entities(level).Count(e => e.GetProperty("__identifier").GetString() == entity);

    public static CollisionGrid Collision(JsonElement level)
    {
        JsonElement layer = Layers(level)
            .Single(l => l.GetProperty("__identifier").GetString() == CollisionLayer);
        return new CollisionGrid(
            layer.GetProperty("intGridCsv").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
            layer.GetProperty("__cWid").GetInt32(),
            layer.GetProperty("__cHei").GetInt32(),
            layer.GetProperty("__gridSize").GetInt32());
    }

    public int TileSize(int tilesetUid) =>
        Tilesets.Single(t => t.GetProperty("uid").GetInt32() == tilesetUid)
            .GetProperty("tileGridSize").GetInt32();
}

internal sealed record CollisionGrid(int[] Values, int Width, int Height, int CellSize)
{
    // Outside the room counts as empty.
    public int At(int cellX, int cellY) =>
        cellX < 0 || cellY < 0 || cellX >= Width || cellY >= Height
            ? 0
            : Values[cellY * Width + cellX];

    public int AtPixel(double x, double y) =>
        At((int)Math.Floor(x / CellSize), (int)Math.Floor(y / CellSize));

    public IEnumerable<(int X, int Y)> CellsWith(int value)
    {
        for (int index = 0; index < Values.Length; index++)
            if (Values[index] == value)
                yield return (index % Width, index / Width);
    }
}
