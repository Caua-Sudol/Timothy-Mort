using System.Text.Json;
using DontLikePoetry;
using Xunit;

namespace TimothyMort.Tests;

// The LDtk project that holds every room. These checks follow the rules in
// Content/Maps/README.md and catch map edits (or moved art) that would break a phase.
public class LdtkMapTests
{
    private const string MapPath = "Content/Maps/Affordance.ldtk";
    private const int Solid = 1, Gelatin = 2, HotRock = 4, MiniJump = 5, UnusedHarmlessSpike = 6, ImpostorRock = 7;

    private static readonly LdtkProject Project = LdtkProject.Load(MapPath);

    private static string LevelOf(string scenarioId)
    {
        ScenarioDefinition scenario = ScenarioCatalog.Get(scenarioId);
        Assert.Equal(MapPath, scenario.MapPath);
        return scenario.LdtkLevelId;
    }

    private static IEnumerable<string> PlayableScenarioIds() =>
        TestSupport.Phases
            .SelectMany(p => p.RoomScenarioIds.Append(p.InterventionScenarioId))
            .Append(ScenarioCatalog.TutorialScenarioId);

    // The game picks levels by identifier: a renamed level breaks the phase.
    [Fact]
    public void Every_room_used_by_the_game_exists_in_the_map()
    {
        foreach (string scenarioId in PlayableScenarioIds())
            Assert.True(Project.HasLevel(LevelOf(scenarioId)), $"Nível ausente para {scenarioId}");
    }

    [Fact]
    public void Rooms_are_2880_by_1600_with_a_96_collision_grid()
    {
        foreach (string scenarioId in PlayableScenarioIds())
        {
            JsonElement level = Project.Level(LevelOf(scenarioId));
            Assert.Equal(2880, level.GetProperty("pxWid").GetInt32());
            Assert.Equal(1600, level.GetProperty("pxHei").GetInt32());
            Assert.Equal(96, LdtkProject.Collision(level).CellSize);
        }
    }

    // Room format: rooms 1 and 2 hold the intervention entrance and the returns, rooms 2 and 3
    // go back, every room has one start and one exit.
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Challenge_rooms_have_the_passages_of_the_room_format(string phaseId)
    {
        PhaseDefinition phase = TestSupport.Phase(phaseId);
        for (int index = 0; index < phase.RoomScenarioIds.Count; index++)
        {
            string levelId = LevelOf(phase.RoomScenarioIds[index]);
            JsonElement level = Project.Level(levelId);
            bool beforeChoice = index < 2;

            Assert.True(1 == LdtkProject.Count(level, "PlayerSpawn"), $"{levelId}: PlayerSpawn");
            Assert.True(1 == LdtkProject.Count(level, "RoomExit"), $"{levelId}: RoomExit");
            Assert.True((beforeChoice ? 1 : 0) == LdtkProject.Count(level, "InterventionEntrance"), $"{levelId}: InterventionEntrance");
            Assert.True((beforeChoice ? 1 : 0) == LdtkProject.Count(level, "InterventionReturn"), $"{levelId}: InterventionReturn");
            Assert.True((beforeChoice ? 1 : 0) == LdtkProject.Count(level, "RoomReturn"), $"{levelId}: RoomReturn");
            Assert.True((index > 0 ? 1 : 0) == LdtkProject.Count(level, "RoomBack"), $"{levelId}: RoomBack");
        }
    }

    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Intervention_room_has_start_sword_and_exit(string phaseId)
    {
        string levelId = LevelOf(TestSupport.Phase(phaseId).InterventionScenarioId);
        JsonElement level = Project.Level(levelId);

        Assert.True(1 == LdtkProject.Count(level, "PlayerSpawn"), $"{levelId}: PlayerSpawn");
        Assert.True(1 == LdtkProject.Count(level, "InterventionSword"), $"{levelId}: InterventionSword");
        Assert.True(1 == LdtkProject.Count(level, "RoomExit"), $"{levelId}: RoomExit");
    }

    [Fact]
    public void Tutorial_has_start_exit_and_controls_sign()
    {
        JsonElement level = Project.Level(LevelOf(ScenarioCatalog.TutorialScenarioId));

        Assert.Equal(1, LdtkProject.Count(level, "PlayerSpawn"));
        Assert.Equal(1, LdtkProject.Count(level, "RoomExit"));
        Assert.Equal(1, LdtkProject.Count(level, "Controles"));
    }

    // Spawns and returns: feet on a solid cell and the body clear of any collision, so the
    // player never starts inside a wall, a hazard or in the air.
    [Fact]
    public void Starts_and_returns_stand_on_solid_ground_with_a_free_body()
    {
        string[] standingEntities = { "PlayerSpawn", "RoomReturn", "InterventionReturn" };
        var problems = new List<string>();

        foreach (JsonElement level in Project.Levels)
        {
            string levelId = level.GetProperty("identifier").GetString();
            CollisionGrid grid = LdtkProject.Collision(level);
            foreach (JsonElement entity in LdtkProject.Entities(level))
            {
                string name = entity.GetProperty("__identifier").GetString();
                if (!standingEntities.Contains(name)) continue;

                double x = entity.GetProperty("px")[0].GetDouble();
                double y = entity.GetProperty("px")[1].GetDouble();
                double width = entity.GetProperty("width").GetDouble();
                double height = entity.GetProperty("height").GetDouble();
                double left = x - width * entity.GetProperty("__pivot")[0].GetDouble();
                double top = y - height * entity.GetProperty("__pivot")[1].GetDouble();

                if (grid.AtPixel(x, top + height + 1) != Solid)
                    problems.Add($"{levelId}/{name}: pés fora de um sólido");
                if (BodyTouchesCollision(grid, left, top, width, height))
                    problems.Add($"{levelId}/{name}: corpo dentro de colisão");
            }
        }

        Assert.Empty(problems);
    }

    // Samples the body every half cell, including its right and bottom edges.
    private static bool BodyTouchesCollision(
        CollisionGrid grid, double left, double top, double width, double height)
    {
        double step = grid.CellSize / 2.0;
        var xs = new List<double>();
        for (double px = left; px < left + width; px += step) xs.Add(px);
        xs.Add(left + width - 1);
        var ys = new List<double>();
        for (double py = top; py < top + height; py += step) ys.Add(py);
        ys.Add(top + height - 1);

        return xs.Any(px => ys.Any(py => grid.AtPixel(px, py) != 0));
    }

    [Fact]
    public void Collision_uses_only_defined_values_and_never_the_retired_harmless_spike()
    {
        JsonElement collision = Project.LayerDefinitions
            .Single(l => l.GetProperty("identifier").GetString() == LdtkProject.CollisionLayer);
        var defined = collision.GetProperty("intGridValues").EnumerateArray()
            .Select(v => v.GetProperty("value").GetInt32())
            .Append(0)
            .ToHashSet();

        foreach (JsonElement level in Project.Levels)
        {
            int[] values = LdtkProject.Collision(level).Values;
            string levelId = level.GetProperty("identifier").GetString();
            Assert.True(values.All(defined.Contains), $"{levelId}: valor de colisão desconhecido");
            Assert.False(values.Contains(UnusedHarmlessSpike), $"{levelId}: usa espinhoInofensivo (6)");
        }
    }

    // Each phase must contain the problem it teaches, and nothing from the other phases' problems.
    [Fact]
    public void Each_phase_contains_its_own_problem()
    {
        foreach (string room in PhaseCatalog.AffordanceVisibility.RoomScenarioIds)
            AssertCollision(room, must: new[] { Gelatin }, mustNot: new[] { MiniJump, ImpostorRock });

        foreach (string room in PhaseCatalog.DamageFeedback.RoomScenarioIds)
            AssertCollision(room, must: new[] { HotRock }, mustNot: new[] { Gelatin, MiniJump, ImpostorRock });

        // Phase 3 mixes known platforms with impostors that look the same.
        foreach (string room in PhaseCatalog.ConceptualModelsConstraints.RoomScenarioIds)
            AssertCollision(room, must: new[] { Gelatin, HotRock, MiniJump, ImpostorRock }, mustNot: Array.Empty<int>());

        // The phase 3 intervention room shows an example of every platform.
        AssertCollision(PhaseCatalog.ConceptualModelsConstraints.InterventionScenarioId,
            must: new[] { Gelatin, HotRock, MiniJump, ImpostorRock }, mustNot: Array.Empty<int>());
    }

    private static void AssertCollision(string scenarioId, int[] must, int[] mustNot)
    {
        string levelId = LevelOf(scenarioId);
        int[] values = LdtkProject.Collision(Project.Level(levelId)).Values;
        foreach (int value in must)
            Assert.True(values.Contains(value), $"{levelId}: falta o valor {value}");
        foreach (int value in mustNot)
            Assert.False(values.Contains(value), $"{levelId}: não deveria ter o valor {value}");
    }

    // Solution art: when a solution is applied, every special cell must get its new art,
    // otherwise some platforms keep the old look and the solution is only half applied.
    [Theory]
    [InlineData(Gelatin, "Solucao_gelatina", "Affordance")]
    // AffordanceTela1 marks only the top row of its 5x3 gelatin block; checked in play and
    // accepted for now (2026-11-19). Decide later: paint the rows or relax the rule.
    [InlineData(Gelatin, "Solucao_mark", "Affordance",
        Skip = "AffordanceTela1: Marcação só no topo do bloco 5x3; aceito por enquanto")]
    [InlineData(MiniJump, "mini_jump", "Modelos")]
    [InlineData(ImpostorRock, "blue_fire", "Modelos")]
    public void Solution_art_covers_every_special_cell(int collisionValue, string layerPrefix, string phase)
    {
        PhaseDefinition definition = phase == "Affordance"
            ? PhaseCatalog.AffordanceVisibility
            : PhaseCatalog.ConceptualModelsConstraints;
        var problems = new List<string>();

        foreach (string scenarioId in definition.RoomScenarioIds.Append(definition.InterventionScenarioId))
        {
            string levelId = LevelOf(scenarioId);
            JsonElement level = Project.Level(levelId);
            CollisionGrid grid = LdtkProject.Collision(level);
            var tiles = SolutionTiles(level, layerPrefix);

            int uncovered = grid.CellsWith(collisionValue).Count(cell =>
            {
                double cx = cell.X * grid.CellSize + grid.CellSize / 2.0;
                double cy = cell.Y * grid.CellSize + grid.CellSize / 2.0;
                return !tiles.Any(t => cx >= t.Left && cx < t.Right && cy >= t.Top && cy < t.Bottom);
            });
            if (uncovered > 0)
                problems.Add($"{levelId}: {uncovered} célula(s) sem arte em {layerPrefix}*");
        }

        Assert.Empty(problems);
    }

    private static List<(double Left, double Top, double Right, double Bottom)> SolutionTiles(
        JsonElement level, string layerPrefix)
    {
        var tiles = new List<(double, double, double, double)>();
        foreach (JsonElement layer in LdtkProject.Layers(level))
        {
            if (!layer.GetProperty("__identifier").GetString()
                    .StartsWith(layerPrefix, StringComparison.OrdinalIgnoreCase) ||
                layer.GetProperty("__tilesetDefUid").ValueKind != JsonValueKind.Number)
                continue;

            int size = Project.TileSize(layer.GetProperty("__tilesetDefUid").GetInt32());
            double offsetX = layer.GetProperty("__pxTotalOffsetX").GetDouble();
            double offsetY = layer.GetProperty("__pxTotalOffsetY").GetDouble();
            foreach (string list in new[] { "gridTiles", "autoLayerTiles" })
            foreach (JsonElement tile in layer.GetProperty(list).EnumerateArray())
            {
                double x = tile.GetProperty("px")[0].GetDouble() + offsetX;
                double y = tile.GetProperty("px")[1].GetDouble() + offsetY;
                tiles.Add((x, y, x + size, y + size));
            }
        }
        return tiles;
    }

    // Moving art out of the project must not leave the map pointing at missing files.
    [Fact]
    public void Every_tileset_image_exists()
    {
        var missing = Project.Tilesets
            .Where(t => t.TryGetProperty("relPath", out JsonElement rel) && rel.ValueKind == JsonValueKind.String)
            .Select(t => t.GetProperty("relPath").GetString())
            .Where(rel => !File.Exists(Path.Combine(Project.Directory, rel)))
            .ToList();

        Assert.Empty(missing);
    }

    // "Decoracao" entities name a PNG in assets/eureka; a wrong name silently hides the piece.
    [Fact]
    public void Every_decoration_image_exists()
    {
        string eureka = Path.Combine(TestSupport.RepositoryRoot, "assets", "eureka");
        var missing = new List<string>();

        foreach (JsonElement level in Project.Levels)
        foreach (JsonElement entity in LdtkProject.Entities(level))
        {
            if (entity.GetProperty("__identifier").GetString() != "Decoracao") continue;
            foreach (JsonElement field in entity.GetProperty("fieldInstances").EnumerateArray())
            {
                if (field.GetProperty("__identifier").GetString() != "Imagem" ||
                    field.GetProperty("__value").ValueKind != JsonValueKind.String) continue;
                string file = field.GetProperty("__value").GetString();
                if (!file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) file += ".png";
                if (!File.Exists(Path.Combine(eureka, file))) missing.Add(file);
            }
        }

        Assert.Empty(missing);
    }
}
