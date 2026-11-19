using DontLikePoetry;
using Xunit;

namespace TimothyMort.Tests;

// The intervention menu of each phase: what the player can choose and what is recorded.
public class ChoiceCatalogTests
{
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Presents_three_distinct_initial_solutions_in_positions_1_to_3(string phaseId)
    {
        IInterventionChoiceCatalog catalog = TestSupport.Phase(phaseId).ChoiceCatalog;

        IReadOnlyList<PresentedSolutionDefinition> presentation =
            catalog.CreateInitialSolutionPresentation(new Random(1));

        Assert.Equal(new[] { 1, 2, 3 }, presentation.Select(e => e.Position).OrderBy(p => p));
        Assert.Equal(3, presentation.Select(e => e.SolutionId).Distinct().Count());
        foreach (PresentedSolutionDefinition entry in presentation)
        {
            InterventionSolutionDefinition solution = catalog.GetSolution(entry.SolutionId);
            Assert.Equal(entry.SolutionId, solution.Id);
            Assert.Equal(SolutionChoiceRound.INITIAL, solution.ChoiceRound);
            Assert.False(string.IsNullOrWhiteSpace(solution.Title));
            Assert.False(string.IsNullOrWhiteSpace(solution.Description));
        }
    }

    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Presents_three_distinct_justifications_in_positions_1_to_3(string phaseId)
    {
        IInterventionChoiceCatalog catalog = TestSupport.Phase(phaseId).ChoiceCatalog;

        IReadOnlyList<PresentedJustificationDefinition> presentation =
            catalog.CreateInitialJustificationPresentation(new Random(1));

        Assert.Equal(new[] { 1, 2, 3 }, presentation.Select(e => e.Position).OrderBy(p => p));
        var texts = presentation
            .Select(e => catalog.GetJustification(e.JustificationId).Text)
            .ToList();
        Assert.All(texts, text => Assert.False(string.IsNullOrWhiteSpace(text)));
        Assert.Equal(3, texts.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.False(string.IsNullOrWhiteSpace(catalog.JustificationQuestion));
        Assert.False(string.IsNullOrWhiteSpace(catalog.SolutionQuestion));
    }

    // Research instrument: one best (ADEQUATE) solution and two PARTIAL ones; all open the exit.
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Has_one_adequate_and_two_partial_solutions_that_all_open_the_exit(string phaseId)
    {
        IReadOnlyList<InterventionSolutionDefinition> solutions =
            TestSupport.Solutions(TestSupport.Phase(phaseId));

        Assert.Single(solutions, s => s.Quality == SolutionQuality.ADEQUATE);
        Assert.Equal(2, solutions.Count(s => s.Quality == SolutionQuality.PARTIAL));
        Assert.All(solutions, s => Assert.True(s.OpensExitDoor));
    }

    // Each solution must change something in the game, and not the same thing as another one.
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Each_solution_turns_on_its_own_visual_module(string phaseId)
    {
        IReadOnlyList<InterventionSolutionDefinition> solutions =
            TestSupport.Solutions(TestSupport.Phase(phaseId));

        Assert.All(solutions, s => Assert.NotEqual(SolutionVisualModule.NONE, s.VisualModules));
        Assert.Equal(3, solutions.Select(s => s.VisualModules).Distinct().Count());
    }

    [Fact]
    public void Conceptual_models_solutions_map_to_the_impostor_effects()
    {
        IInterventionChoiceCatalog catalog = PhaseCatalog.ConceptualModelsConstraints.ChoiceCatalog;

        Assert.True(catalog.GetSolution(ConceptualModelsConstraintsChoiceCatalog.DifferentiateSolutionId)
            .VisualModules.HasFlag(SolutionVisualModule.IMPOSTOR_DIFFERENTIATE));
        Assert.True(catalog.GetSolution(ConceptualModelsConstraintsChoiceCatalog.RestoreSolutionId)
            .VisualModules.HasFlag(SolutionVisualModule.IMPOSTOR_RESTORE));
        Assert.True(catalog.GetSolution(ConceptualModelsConstraintsChoiceCatalog.AnimateSolutionId)
            .VisualModules.HasFlag(SolutionVisualModule.IMPOSTOR_ANIMATE));
    }

    // The combined demonstration in room 3 only exists in phases 1 and 2, and only for the best
    // solution. Phase 3 has none (Restaurar/Animar would cancel or repeat Diferenciar).
    [Fact]
    public void Combined_demonstration_is_tied_to_the_best_solution_only_in_phases_1_and_2()
    {
        foreach (PhaseDefinition phase in new[] { PhaseCatalog.AffordanceVisibility, PhaseCatalog.DamageFeedback })
        {
            InterventionSolutionDefinition best = TestSupport.Solutions(phase)
                .Single(s => s.Quality == SolutionQuality.ADEQUATE);
            Assert.Equal(best.Id, phase.BestSolutionId);
            Assert.NotEqual(SolutionVisualModule.NONE, phase.DemonstrationModules);
            // The demonstration adds something the best solution alone does not show.
            Assert.NotEqual(SolutionVisualModule.NONE, phase.DemonstrationModules & ~best.VisualModules);
        }

        Assert.Null(PhaseCatalog.ConceptualModelsConstraints.BestSolutionId);
        Assert.Equal(SolutionVisualModule.NONE, PhaseCatalog.ConceptualModelsConstraints.DemonstrationModules);
    }

    // The order is shuffled per session so position does not bias the choice: across many
    // sessions every option must show up first at least once.
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Shuffles_solutions_and_justifications_across_sessions(string phaseId)
    {
        IInterventionChoiceCatalog catalog = TestSupport.Phase(phaseId).ChoiceCatalog;
        var firstSolutions = new HashSet<string>();
        var firstJustifications = new HashSet<string>();

        for (int seed = 0; seed < 200; seed++)
        {
            firstSolutions.Add(catalog.CreateInitialSolutionPresentation(new Random(seed))
                .Single(e => e.Position == 1).SolutionId);
            firstJustifications.Add(catalog.CreateInitialJustificationPresentation(new Random(seed))
                .Single(e => e.Position == 1).JustificationId);
        }

        Assert.Equal(3, firstSolutions.Count);
        Assert.Equal(3, firstJustifications.Count);
    }

    // "Fase concluída" screen: every solution needs its own label and explanation, and no
    // leftover development text may reach the player.
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Result_screen_texts_are_complete_and_free_of_placeholders(string phaseId)
    {
        PhaseDefinition phase = TestSupport.Phase(phaseId);
        IReadOnlyList<InterventionSolutionDefinition> solutions = TestSupport.Solutions(phase);
        string[] forbidden = { "provisóri", "provisori", "placeholder", "TODO" };

        var assessments = new List<string>();
        foreach (InterventionSolutionDefinition solution in solutions)
        {
            string assessment = phase.ChoiceCatalog.GetResultAssessment(solution.Id);
            string explanation = phase.ChoiceCatalog.GetResultExplanation(solution.Id);
            Assert.False(string.IsNullOrWhiteSpace(assessment));
            Assert.False(string.IsNullOrWhiteSpace(explanation));
            foreach (string word in forbidden)
            {
                // "TODO" is case-sensitive: the Portuguese word "todo" is valid text.
                StringComparison comparison = word == "TODO"
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;
                Assert.DoesNotContain(word, assessment, comparison);
                Assert.DoesNotContain(word, explanation, comparison);
            }
            assessments.Add(assessment);
        }

        Assert.Equal(3, assessments.Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Unknown_ids_are_rejected_instead_of_silently_recorded(string phaseId)
    {
        IInterventionChoiceCatalog catalog = TestSupport.Phase(phaseId).ChoiceCatalog;

        Assert.Throws<InvalidOperationException>(() => catalog.GetSolution("nao_existe"));
        Assert.Throws<InvalidOperationException>(() => catalog.GetJustification("nao_existe"));
    }
}
