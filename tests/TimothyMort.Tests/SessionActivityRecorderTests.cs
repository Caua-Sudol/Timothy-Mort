using DontLikePoetry;
using Xunit;

namespace TimothyMort.Tests;

// Telemetry collected during play (rooms, deaths, control scheme, intervention events).
public class SessionActivityRecorderTests : IDisposable
{
    private readonly ControlScheme _previousScheme = TutorialControls.Current;
    private readonly List<SessionActivityData> _saves = new();

    public SessionActivityRecorderTests()
    {
        TutorialControls.Current = ControlScheme.SHIFT;
    }

    public void Dispose() => TutorialControls.Current = _previousScheme;

    private SessionActivityRecorder NewRecorder(SessionActivityData data = null) =>
        new(data, saved => _saves.Add(saved));

    [Fact]
    public void New_session_records_game_version_and_initial_control_scheme()
    {
        SessionActivityRecorder recorder = NewRecorder();

        Assert.Equal(ResearchStudyConfiguration.GameVersion, recorder.Data.GameVersion);
        Assert.Equal(nameof(ControlScheme.SHIFT), Assert.Single(recorder.Data.ControlSchemes).Scheme);
    }

    [Fact]
    public void Continued_session_keeps_its_scheme_history()
    {
        var data = new SessionActivityData();
        data.ControlSchemes.Add(new ControlSchemeChangeData { Scheme = nameof(ControlScheme.ARROWS) });

        SessionActivityRecorder recorder = NewRecorder(data);

        Assert.Equal(nameof(ControlScheme.ARROWS), Assert.Single(recorder.Data.ControlSchemes).Scheme);
    }

    [Fact]
    public void Re_entering_a_room_adds_entries_to_the_same_record_and_saves()
    {
        SessionActivityRecorder recorder = NewRecorder();
        string phase = ScenarioCatalog.DamageFeedbackScenarioId;

        recorder.EnterRoom(phase, ScenarioCatalog.DamageFeedbackScenarioId);
        recorder.AddActiveTime(2.0);
        recorder.EnterRoom(phase, ScenarioCatalog.DamageFeedbackRoom2ScenarioId);
        recorder.EnterRoom(phase, ScenarioCatalog.DamageFeedbackScenarioId);
        recorder.AddActiveTime(1.5);

        RoomActivityData room1 = recorder.Data.Rooms.Single(r => r.ScenarioId == phase);
        Assert.Equal(2, recorder.Data.Rooms.Count);
        Assert.Equal(2, room1.Entries);
        Assert.Equal(3.5, room1.ActiveSeconds, 6);
        Assert.Equal(ScenarioCatalog.DamageFeedbackScenarioId, recorder.Data.LastScenarioId);
        Assert.Equal(3, _saves.Count);
    }

    // Time and deaths only go to a room once the player is in one; zero deaths add nothing.
    [Fact]
    public void Time_and_deaths_need_a_current_room()
    {
        SessionActivityRecorder recorder = NewRecorder();
        recorder.AddActiveTime(10);
        recorder.AddDeaths(3);
        Assert.Empty(recorder.Data.Rooms);

        recorder.EnterRoom(ScenarioCatalog.AffordanceVisibilityScenarioId,
            ScenarioCatalog.AffordanceRoom2ScenarioId);
        recorder.AddDeaths(0);
        recorder.AddDeaths(-1);
        recorder.AddDeaths(2);

        Assert.Equal(2, Assert.Single(recorder.Data.Rooms).Deaths);
    }

    [Fact]
    public void Control_scheme_is_recorded_only_when_it_changes()
    {
        SessionActivityRecorder recorder = NewRecorder();

        recorder.RecordControlScheme(ControlScheme.SHIFT);
        recorder.RecordControlScheme(ControlScheme.RIGHT_HAND);
        recorder.RecordControlScheme(ControlScheme.RIGHT_HAND);

        Assert.Equal(
            new[] { nameof(ControlScheme.SHIFT), nameof(ControlScheme.RIGHT_HAND) },
            recorder.Data.ControlSchemes.Select(c => c.Scheme));
    }

    [Fact]
    public void Intervention_events_keep_their_order()
    {
        SessionActivityRecorder recorder = NewRecorder();
        string phase = ScenarioCatalog.ConceptualModelsConstraintsScenarioId;

        recorder.RecordIntervention(phase, "solucao", ConceptualModelsConstraintsChoiceCatalog.AnimateSolutionId);
        recorder.RecordIntervention(phase, "continuar_testando", null);
        recorder.RecordIntervention(phase, "justificativa", "concept_predict");

        Assert.Equal(
            new[] { "solucao", "continuar_testando", "justificativa" },
            recorder.Data.InterventionEvents.Select(e => e.Kind));
        Assert.All(recorder.Data.InterventionEvents, e => Assert.Equal(phase, e.PhaseId));
    }

    // Telemetry must never break the game: a failing save is swallowed.
    [Fact]
    public void A_failing_save_does_not_interrupt_play()
    {
        var recorder = new SessionActivityRecorder(null, _ => throw new IOException("disco cheio"));

        recorder.EnterRoom(ScenarioCatalog.AffordanceVisibilityScenarioId,
            ScenarioCatalog.AffordanceVisibilityScenarioId);
        recorder.RecordTutorialCompleted();

        Assert.True(recorder.Data.TutorialCompleted);
    }
}
