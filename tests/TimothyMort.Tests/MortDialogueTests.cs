using DontLikePoetry;
using Xunit;

namespace TimothyMort.Tests;

// Mort's lines and the phase introductions: every situation must have its own text.
public class MortDialogueTests
{
    private const string UnknownId = "__solucao_desconhecida__";

    // Without a specific line, Mort falls back to a generic one; no real solution may get it.
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Every_solution_has_its_own_comment_after_being_applied(string phaseId)
    {
        string fallback = MortDialogueCatalog.AfterChoice(UnknownId);
        var comments = TestSupport.Solutions(TestSupport.Phase(phaseId))
            .Select(solution => MortDialogueCatalog.AfterChoice(solution.Id))
            .ToList();

        Assert.All(comments, comment =>
        {
            Assert.False(string.IsNullOrWhiteSpace(comment));
            Assert.NotEqual(fallback, comment);
        });
        Assert.Equal(3, comments.Distinct().Count());
    }

    [Fact]
    public void Combined_demonstration_has_a_specific_conclusion_for_each_best_solution()
    {
        string fallback = MortDialogueCatalog.Conclusion(UnknownId);

        foreach (PhaseDefinition phase in TestSupport.Phases.Where(p => p.BestSolutionId != null))
            Assert.NotEqual(fallback, MortDialogueCatalog.Conclusion(phase.BestSolutionId));
    }

    // Phase 3 texts stay neutral ("plataformas especiais"): naming the material would explain
    // the impostor before the player discovers it.
    [Fact]
    public void Conceptual_models_lines_do_not_name_the_material()
    {
        var lines = new List<string>
        {
            MortDialogueCatalog.ConceptTrampolineCallout,
            MortDialogueCatalog.ConceptImpostorRockCallout
        };
        lines.AddRange(TestSupport.Solutions(PhaseCatalog.ConceptualModelsConstraints)
            .Select(solution => MortDialogueCatalog.AfterChoice(solution.Id)));

        Assert.All(lines, line => Assert.DoesNotContain("gelatin", line, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Every_phase_has_an_introduction(string phaseId)
    {
        (string title, string text) = PhaseIntroductionCatalog.Get(phaseId);

        Assert.False(string.IsNullOrWhiteSpace(title));
        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    [Fact]
    public void Phase_titles_are_distinct()
    {
        var titles = TestSupport.Phases.Select(p => PhaseIntroductionCatalog.Get(p.Id).Title).ToList();

        Assert.Equal(titles.Count, titles.Distinct().Count());
    }
}
