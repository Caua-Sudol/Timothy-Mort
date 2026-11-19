using DontLikePoetry;
using Xunit;

namespace TimothyMort.Tests;

// Phase select: which phases open, and completing a phase.
public class PhaseProgressTests
{
    private const string Affordance = ScenarioCatalog.AffordanceVisibilityScenarioId;
    private const string Feedback = ScenarioCatalog.DamageFeedbackScenarioId;
    private const string Concept = ScenarioCatalog.ConceptualModelsConstraintsScenarioId;

    [Fact]
    public void Affordance_is_always_open_and_unknown_phases_never_are()
    {
        var progress = new PhaseProgress();

        Assert.True(progress.IsUnlocked(Affordance));
        if (!ResearchStudyConfiguration.DevToolsEnabled)
            Assert.False(progress.IsUnlocked("fase_inexistente"));
    }

    // Study build (Release): each phase opens only after the previous one.
    // Debug opens every phase on purpose; run "dotnet test -c Release" to check the study order.
    [Fact]
    public void Phases_open_in_order_in_the_study_build()
    {
        var progress = new PhaseProgress();

        if (ResearchStudyConfiguration.DevToolsEnabled)
        {
            Assert.True(progress.IsUnlocked(Feedback));
            Assert.True(progress.IsUnlocked(Concept));
            return;
        }

        Assert.False(progress.IsUnlocked(Feedback));
        Assert.False(progress.IsUnlocked(Concept));

        Assert.True(progress.TryCompletePhase(Affordance, AffordanceChoiceCatalog.MarkSolutionId,
            SolutionVisualModule.GELATIN_TERRAIN_MARK));
        Assert.Equal(!ResearchStudyConfiguration.BetaAffordanceOnly, progress.IsUnlocked(Feedback));
        Assert.False(progress.IsUnlocked(Concept));

        if (ResearchStudyConfiguration.BetaAffordanceOnly) return;

        Assert.True(progress.TryCompletePhase(Feedback, FeedbackDamageChoiceCatalog.KnockbackSolutionId,
            SolutionVisualModule.DAMAGE_KNOCKBACK));
        Assert.True(progress.IsUnlocked(Concept));
    }

    [Fact]
    public void A_locked_phase_cannot_be_completed()
    {
        if (ResearchStudyConfiguration.DevToolsEnabled) return; // Debug: nothing is locked.

        var progress = new PhaseProgress();

        Assert.False(progress.TryCompletePhase(Concept, null, SolutionVisualModule.NONE));
        Assert.False(progress.IsCompleted(Concept));
    }

    [Fact]
    public void Completing_twice_reports_false_and_keeps_the_phase_completed()
    {
        var progress = new PhaseProgress();

        Assert.True(progress.TryCompletePhase(Affordance, AffordanceChoiceCatalog.MarkSolutionId,
            SolutionVisualModule.GELATIN_TERRAIN_MARK));
        Assert.False(progress.TryCompletePhase(Affordance, AffordanceChoiceCatalog.MarkSolutionId,
            SolutionVisualModule.GELATIN_TERRAIN_MARK));

        Assert.True(progress.IsCompleted(Affordance));
        Assert.Equal(AffordanceChoiceCatalog.MarkSolutionId, progress.GetFinalSolutionId(Affordance));
    }

    [Fact]
    public void Restored_and_cleared_progress()
    {
        var progress = new PhaseProgress();
        progress.RestoreCompletedPhase(Affordance, AffordanceChoiceCatalog.AppearanceSolutionId,
            SolutionVisualModule.GELATIN_COLOR);

        Assert.True(progress.IsCompleted(Affordance));
        Assert.Equal(AffordanceChoiceCatalog.AppearanceSolutionId, progress.GetFinalSolutionId(Affordance));

        progress.Clear();

        Assert.False(progress.IsCompleted(Affordance));
        Assert.Null(progress.GetFinalSolutionId(Affordance));
    }
}
