using System.Text.Json;
using DontLikePoetry;
using Xunit;

namespace TimothyMort.Tests;

// Tutorial texts come from LDtk with placeholders ({pular}, {dash}...) filled per control scheme.
public class TutorialControlsTests : IDisposable
{
    private readonly ControlScheme _previousScheme = TutorialControls.Current;

    public void Dispose() => TutorialControls.Current = _previousScheme;

    private static List<string> TutorialTexts()
    {
        LdtkProject project = LdtkProject.Load("Content/Maps/Affordance.ldtk");
        JsonElement level = project.Level(ScenarioCatalog.Get(ScenarioCatalog.TutorialScenarioId).LdtkLevelId);
        return LdtkProject.Entities(level)
            .Where(e => e.GetProperty("__identifier").GetString() == "Texto")
            .SelectMany(e => e.GetProperty("fieldInstances").EnumerateArray())
            .Where(f => f.GetProperty("__value").ValueKind == JsonValueKind.String)
            .Select(f => f.GetProperty("__value").GetString())
            .ToList();
    }

    // A typo in a placeholder would show "{pullar}" to the player in some scheme.
    [Theory]
    [InlineData(ControlScheme.SHIFT)]
    [InlineData(ControlScheme.ARROWS)]
    [InlineData(ControlScheme.RIGHT_HAND)]
    public void Every_tutorial_text_is_fully_filled_for_each_scheme(ControlScheme scheme)
    {
        TutorialControls.Current = scheme;
        List<string> texts = TutorialTexts();
        Assert.NotEmpty(texts);

        foreach (string text in texts)
        {
            string filled = TutorialControls.Fill(text);
            Assert.DoesNotContain("{", filled);
            Assert.DoesNotContain("}", filled);
        }
    }

    // Default scheme shown to new players (decided after the expert test): Space jumps, Shift dashes.
    [Fact]
    public void Keys_are_shown_in_brackets_with_the_default_scheme()
    {
        TutorialControls.Current = ControlScheme.SHIFT;

        Assert.Equal("[Espaço] duas vezes", TutorialControls.Fill("{pular} duas vezes"));
        Assert.Equal("[Shift]", TutorialControls.Fill("{dash}"));
    }

    [Fact]
    public void Schemes_show_different_jump_keys()
    {
        var jumpKeys = Enum.GetValues<ControlScheme>().Select(scheme =>
        {
            TutorialControls.Current = scheme;
            return TutorialControls.Fill("{pular}");
        }).ToList();

        Assert.Equal(jumpKeys.Count, jumpKeys.Distinct().Count());
    }
}
