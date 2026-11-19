using DontLikePoetry;
using Xunit;

namespace TimothyMort.Tests;

// Rules of one phase run: choices in the intervention room, hearts, deaths and rooms.
public class PhaseStateTests
{
    private static PhaseState NewState(PhaseDefinition phase) =>
        new(phase, phase.RoomScenarioIds[0]);

    private static PhaseState NewStateWithPresentations(PhaseDefinition phase)
    {
        PhaseState state = NewState(phase);
        state.RegisterInitialSolutionPresentation(
            phase.ChoiceCatalog.CreateInitialSolutionPresentation(new Random(3)));
        state.RegisterInitialJustificationPresentation(
            phase.ChoiceCatalog.CreateInitialJustificationPresentation(new Random(3)));
        return state;
    }

    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void New_phase_starts_with_full_hearts_and_no_choice(string phaseId)
    {
        PhaseDefinition phase = TestSupport.Phase(phaseId);
        PhaseState state = NewState(phase);

        Assert.Equal(phase.MaximumHealth, state.CurrentHealth);
        Assert.True(state.MaximumHealth > 0);
        Assert.False(state.HasFirstChoice);
        Assert.False(state.HasJustification);
        Assert.True(state.InterventionEntranceIsActive);
        Assert.Empty(state.TriedSolutionIds);
    }

    [Fact]
    public void A_scenario_from_another_phase_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => new PhaseState(
            PhaseCatalog.DamageFeedback,
            PhaseCatalog.AffordanceVisibility.RoomScenarioIds[0]));
    }

    // Testing solutions in the intervention room: switching counts as a revision, re-applying
    // the same one does not, and each solution is listed once in the order it was tried.
    [Theory]
    [MemberData(nameof(TestSupport.PhaseIds), MemberType = typeof(TestSupport))]
    public void Switching_solutions_records_revision_and_tried_order(string phaseId)
    {
        PhaseDefinition phase = TestSupport.Phase(phaseId);
        IReadOnlyList<InterventionSolutionDefinition> solutions = TestSupport.Solutions(phase);
        PhaseState state = NewState(phase);

        state.ApplySolution(solutions[0]);
        state.ApplySolution(solutions[0]);
        Assert.False(state.HasRevision);
        Assert.Equal(new[] { solutions[0].Id }, state.TriedSolutionIds);

        state.ApplySolution(solutions[1]);
        state.ApplySolution(solutions[0]);

        Assert.True(state.HasRevision);
        Assert.Equal(4, state.SolutionSelectionCount);
        Assert.Equal(solutions[0].Id, state.AppliedSolutionId);
        Assert.Equal(new[] { solutions[0].Id, solutions[1].Id }, state.TriedSolutionIds);
        Assert.False(state.InterventionEntranceIsActive);
    }

    [Fact]
    public void Choice_cannot_change_after_it_is_locked()
    {
        PhaseDefinition phase = PhaseCatalog.DamageFeedback;
        IReadOnlyList<InterventionSolutionDefinition> solutions = TestSupport.Solutions(phase);
        PhaseState state = NewState(phase);

        state.ApplySolution(solutions[0]);
        state.LockChoice();

        Assert.True(state.ChoiceLocked);
        Assert.Throws<InvalidOperationException>(() => state.ApplySolution(solutions[1]));
        Assert.Equal(solutions[0].Id, state.AppliedSolutionId);
    }

    [Fact]
    public void Justification_needs_an_applied_solution()
    {
        PhaseDefinition phase = PhaseCatalog.AffordanceVisibility;
        PhaseState state = NewStateWithPresentations(phase);
        InterventionJustificationDefinition justification = TestSupport.Justifications(phase)[0];

        Assert.Throws<InvalidOperationException>(() => state.SetJustification(justification));
        Assert.False(state.HasJustification);
    }

    [Fact]
    public void Justification_must_come_from_the_presented_options()
    {
        PhaseDefinition phase = PhaseCatalog.ConceptualModelsConstraints;
        PhaseState state = NewStateWithPresentations(phase);
        state.ApplySolution(TestSupport.Solutions(phase)[0]);

        // A justification of another phase was never shown in this phase.
        InterventionJustificationDefinition foreign =
            TestSupport.Justifications(PhaseCatalog.DamageFeedback)[0];
        Assert.Throws<InvalidOperationException>(() => state.SetJustification(foreign));

        InterventionJustificationDefinition valid = TestSupport.Justifications(phase)[1];
        state.SetJustification(valid);
        Assert.True(state.HasJustification);
        Assert.Equal(valid.Id, state.FirstJustificationId);
    }

    [Fact]
    public void Justification_is_refused_before_the_options_were_shown()
    {
        PhaseDefinition phase = PhaseCatalog.DamageFeedback;
        PhaseState state = NewState(phase);
        state.ApplySolution(TestSupport.Solutions(phase)[0]);

        Assert.Throws<InvalidOperationException>(
            () => state.SetJustification(TestSupport.Justifications(phase)[0]));
    }

    [Fact]
    public void Presentation_is_registered_once_and_positions_must_be_1_to_N()
    {
        PhaseDefinition phase = PhaseCatalog.AffordanceVisibility;
        string[] ids = TestSupport.Solutions(phase).Select(s => s.Id).ToArray();

        PhaseState duplicatedPosition = NewState(phase);
        Assert.Throws<InvalidOperationException>(() =>
            duplicatedPosition.RegisterInitialSolutionPresentation(new[]
            {
                new PresentedSolutionDefinition(ids[0], 1),
                new PresentedSolutionDefinition(ids[1], 1),
                new PresentedSolutionDefinition(ids[2], 3)
            }));

        PhaseState outOfRange = NewState(phase);
        Assert.Throws<InvalidOperationException>(() =>
            outOfRange.RegisterInitialSolutionPresentation(new[]
            {
                new PresentedSolutionDefinition(ids[0], 1),
                new PresentedSolutionDefinition(ids[1], 2),
                new PresentedSolutionDefinition(ids[2], 4)
            }));

        PhaseState twice = NewStateWithPresentations(phase);
        Assert.Throws<InvalidOperationException>(() =>
            twice.RegisterInitialSolutionPresentation(
                phase.ChoiceCatalog.CreateInitialSolutionPresentation(new Random(9))));
    }

    // Hidden invulnerability: a second hit only counts after the phase's invulnerability time.
    [Theory]
    [InlineData(ScenarioCatalog.DamageFeedbackScenarioId)]
    [InlineData(ScenarioCatalog.ConceptualModelsConstraintsScenarioId)]
    public void Damage_respects_the_invulnerability_window(string phaseId)
    {
        PhaseDefinition phase = TestSupport.Phase(phaseId);
        PhaseState state = NewState(phase);
        double window = phase.DamageInvulnerabilitySeconds;

        Assert.True(state.TryApplyDamage());
        Assert.Equal(phase.MaximumHealth - 1, state.CurrentHealth);

        Assert.False(state.TryApplyDamage());
        state.UpdateDamageInvulnerability(window - 0.01);
        Assert.False(state.TryApplyDamage());
        Assert.Equal(phase.MaximumHealth - 1, state.CurrentHealth);

        state.UpdateDamageInvulnerability(0.02);
        Assert.True(state.TryApplyDamage());
        Assert.Equal(phase.MaximumHealth - 2, state.CurrentHealth);
    }

    // Phase 3 impostor rock costs two hearts; hearts never go below zero.
    [Fact]
    public void Two_heart_damage_and_health_floor_at_zero()
    {
        PhaseDefinition phase = PhaseCatalog.ConceptualModelsConstraints;
        PhaseState state = NewState(phase);

        Assert.True(state.TryApplyDamage(2));
        Assert.Equal(phase.MaximumHealth - 2, state.CurrentHealth);

        while (state.CurrentHealth > 0)
        {
            state.UpdateDamageInvulnerability(phase.DamageInvulnerabilitySeconds);
            Assert.True(state.TryApplyDamage(2));
        }

        Assert.Equal(0, state.CurrentHealth);
        state.UpdateDamageInvulnerability(phase.DamageInvulnerabilitySeconds);
        Assert.False(state.TryApplyDamage());
        Assert.Throws<ArgumentOutOfRangeException>(() => state.TryApplyDamage(0));
    }

    // Room format: a new room is a checkpoint but does not refill hearts; leaving the
    // intervention room refills them and remembers where the player came from.
    [Fact]
    public void Leaving_the_intervention_room_refills_hearts_but_a_new_room_does_not()
    {
        PhaseDefinition phase = PhaseCatalog.DamageFeedback;
        PhaseState state = NewState(phase);
        state.TryApplyDamage();

        state.EnterScenario(phase.RoomScenarioIds[1]);
        Assert.Equal(phase.MaximumHealth - 1, state.CurrentHealth);

        state.EnterScenario(phase.InterventionScenarioId);
        Assert.Equal(phase.RoomScenarioIds[1], state.InterventionReturnScenarioId);
        Assert.Equal(phase.MaximumHealth - 1, state.CurrentHealth);

        state.EnterScenario(phase.RoomScenarioIds[1]);
        Assert.Equal(phase.MaximumHealth, state.CurrentHealth);
    }

    [Fact]
    public void Falling_costs_one_heart_and_every_respawn_counts_a_death()
    {
        PhaseDefinition phase = PhaseCatalog.AffordanceVisibility;
        PhaseState state = NewState(phase);

        state.RegisterRoomRespawn(lostHeartFromFall: true);
        Assert.Equal(phase.MaximumHealth - 1, state.CurrentHealth);
        Assert.Equal(1, state.TotalDeathCount);

        state.RegisterRoomRespawn(lostHeartFromFall: false);
        Assert.Equal(phase.MaximumHealth - 1, state.CurrentHealth);
        Assert.Equal(2, state.TotalDeathCount);
    }

    // The intervention entrance moves to room 1 after two deaths there, not elsewhere.
    [Fact]
    public void Intervention_entrance_moves_to_room_1_after_two_deaths_in_room_1()
    {
        PhaseDefinition phase = PhaseCatalog.AffordanceVisibility;

        PhaseState inRoom1 = NewState(phase);
        Assert.Equal(InterventionEntrancePlacement.LATE, inRoom1.InterventionEntrancePlacement);
        inRoom1.RegisterRoomRespawn(true);
        Assert.Equal(InterventionEntrancePlacement.LATE, inRoom1.InterventionEntrancePlacement);
        inRoom1.RegisterRoomRespawn(true);
        Assert.Equal(InterventionEntrancePlacement.EARLY, inRoom1.InterventionEntrancePlacement);

        PhaseState inRoom2 = NewState(phase);
        inRoom2.EnterScenario(phase.RoomScenarioIds[1]);
        inRoom2.RegisterRoomRespawn(true);
        inRoom2.RegisterRoomRespawn(true);
        Assert.Equal(InterventionEntrancePlacement.LATE, inRoom2.InterventionEntrancePlacement);
        Assert.Equal(2, inRoom2.TotalDeathCount);
    }

    [Fact]
    public void Death_in_the_same_attempt_is_counted_once()
    {
        PhaseState state = NewState(PhaseCatalog.DamageFeedback);

        state.RegisterDeath();
        state.RegisterDeath();
        Assert.Equal(1, state.TotalDeathCount);

        state.BeginAttempt();
        state.RegisterDeath();
        Assert.Equal(2, state.TotalDeathCount);
    }

    // Phase 3 starts with fixed damage feedback; the chosen solution is added on top of it.
    [Fact]
    public void Applied_modules_keep_the_phase_base_and_add_the_solution()
    {
        PhaseDefinition phase = PhaseCatalog.ConceptualModelsConstraints;
        PhaseState state = NewState(phase);
        Assert.Equal(phase.BaseVisualModules, state.AppliedVisualModules);
        Assert.True(state.AppliedVisualModules.HasFlag(SolutionVisualModule.DAMAGE_KNOCKBACK));

        InterventionSolutionDefinition animate = phase.ChoiceCatalog
            .GetSolution(ConceptualModelsConstraintsChoiceCatalog.AnimateSolutionId);
        state.ApplySolution(animate);

        Assert.Equal(phase.BaseVisualModules | animate.VisualModules, state.AppliedVisualModules);
        Assert.True(state.AppliedSolutionUsesModule(SolutionVisualModule.IMPOSTOR_ANIMATE));
        Assert.False(state.AppliedSolutionUsesModule(SolutionVisualModule.IMPOSTOR_DIFFERENTIATE));
    }

    [Fact]
    public void Each_callout_is_given_only_once_until_reset()
    {
        PhaseDefinition phase = PhaseCatalog.ConceptualModelsConstraints;
        PhaseState state = NewState(phase);

        Assert.True(state.TryTakeTryOthersCallout());
        Assert.False(state.TryTakeTryOthersCallout());
        Assert.True(state.TryTakeTrampolineCallout());
        Assert.False(state.TryTakeTrampolineCallout());
        Assert.True(state.TryTakeImpostorRockCallout());
        Assert.False(state.TryTakeImpostorRockCallout());

        state.Reset(phase.RoomScenarioIds[0]);
        Assert.True(state.TryTakeTryOthersCallout());
        Assert.True(state.TryTakeTrampolineCallout());
        Assert.True(state.TryTakeImpostorRockCallout());
    }

    [Fact]
    public void Reset_clears_choice_counters_and_hearts()
    {
        PhaseDefinition phase = PhaseCatalog.DamageFeedback;
        PhaseState state = NewStateWithPresentations(phase);
        state.ApplySolution(TestSupport.Solutions(phase)[0]);
        state.SetJustification(TestSupport.Justifications(phase)[0]);
        state.LockChoice();
        state.TryApplyDamage();
        state.RegisterRoomRespawn(true);
        state.AccumulateActiveTime(5);

        state.Reset(phase.RoomScenarioIds[0]);

        Assert.False(state.HasFirstChoice);
        Assert.False(state.HasJustification);
        Assert.False(state.ChoiceLocked);
        Assert.Null(state.AppliedSolutionId);
        Assert.Empty(state.TriedSolutionIds);
        Assert.Equal(0, state.TotalDeathCount);
        Assert.Equal(0.0, state.ElapsedSeconds);
        Assert.Equal(phase.MaximumHealth, state.CurrentHealth);
        Assert.False(state.HasInitialSolutionPresentation);
    }

    [Fact]
    public void Active_time_only_accepts_finite_non_negative_values()
    {
        PhaseState state = NewState(PhaseCatalog.AffordanceVisibility);

        state.AccumulateActiveTime(1.5);
        state.AccumulateActiveTime(0.25);
        Assert.Equal(1.75, state.ElapsedSeconds, 6);

        Assert.Throws<ArgumentOutOfRangeException>(() => state.AccumulateActiveTime(-0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.AccumulateActiveTime(double.NaN));
    }
}
