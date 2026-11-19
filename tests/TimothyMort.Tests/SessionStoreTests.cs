using System.Text.Json;
using System.Text.RegularExpressions;
using DontLikePoetry;
using Microsoft.Data.Sqlite;
using Xunit;

namespace TimothyMort.Tests;

// Local database and exported session file, in temporary folders (never the player's data).
public class SessionStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "timothy-mort-tests-" + Guid.NewGuid().ToString("N"));
    private readonly string _exportDirectory;
    private readonly SessionStore _store;

    public SessionStoreTests()
    {
        _exportDirectory = Path.Combine(_root, "Documentos");
        _store = new SessionStore(Path.Combine(_root, "dados"), _exportDirectory);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { }
    }

    [Fact]
    public void Database_stays_in_the_given_folder()
    {
        Assert.StartsWith(_root, _store.DatabasePath);
        Assert.True(File.Exists(_store.DatabasePath));
    }

    // The code shown to the player identifies the session file: TM-XXXX-XXXX-XXXX from the id.
    [Fact]
    public void New_session_gets_a_public_code_derived_from_its_id()
    {
        LocalSessionInfo session = _store.CreateNewSession(new PhaseProgress());

        Assert.Matches(new Regex("^TM-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}$"), session.PublicCode);
        Assert.Equal(session.SessionId[..12].ToUpperInvariant(), session.PublicCode.Replace("TM-", "").Replace("-", ""));
    }

    [Fact]
    public void A_new_session_abandons_the_previous_active_one()
    {
        LocalSessionInfo first = _store.CreateNewSession(new PhaseProgress());
        LocalSessionInfo second = _store.CreateNewSession(new PhaseProgress());

        Assert.Equal(second.SessionId, _store.FindMostRecentActiveSession().SessionId);
        Assert.Equal("abandoned", _store.LoadSessionCompletion(first.SessionId).Status);
        Assert.Equal("active", _store.LoadSessionCompletion(second.SessionId).Status);
    }

    // What the research reads: solution, justification and their positions, revision, time, deaths.
    [Fact]
    public void Phase_completion_is_read_back_as_played()
    {
        PhaseDefinition phase = PhaseCatalog.DamageFeedback;
        LocalSessionInfo session = _store.CreateNewSession(new PhaseProgress());
        PhaseState state = new(phase, phase.RoomScenarioIds[0]);
        state.RegisterInitialSolutionPresentation(phase.ChoiceCatalog.CreateInitialSolutionPresentation(new Random(5)));
        state.RegisterInitialJustificationPresentation(phase.ChoiceCatalog.CreateInitialJustificationPresentation(new Random(5)));
        IReadOnlyList<InterventionSolutionDefinition> solutions = TestSupport.Solutions(phase);
        state.ApplySolution(solutions[0]);
        state.ApplySolution(solutions[1]);
        state.LockChoice();
        PresentedJustificationDefinition chosen = state.InitialJustificationPresentation[2];
        state.SetJustification(phase.ChoiceCatalog.GetJustification(chosen.JustificationId));
        state.AccumulateActiveTime(12.5);
        state.RegisterRoomRespawn(lostHeartFromFall: true);

        _store.SavePhaseCompletion(session.SessionId, state, completeSession: true);
        SessionCompletionData loaded = _store.LoadSessionCompletion(session.SessionId);

        Assert.Equal("completed", loaded.Status);
        PhaseCompletionData saved = Assert.Single(loaded.Phases);
        Assert.Equal(phase.Id, saved.PhaseId);
        Assert.Equal(solutions[1].Id, saved.FinalSolutionId);
        Assert.Equal(solutions[1].Quality.ToString(), saved.FinalSolutionQuality);
        Assert.True(saved.HadRevision);
        Assert.Equal(2, saved.SelectionCount);
        Assert.Equal(chosen.JustificationId, saved.FirstJustificationId);
        Assert.Equal(3, saved.FirstJustificationPosition);
        Assert.Equal(12.5, saved.ActiveSeconds, 6);
        Assert.Equal(1, saved.TotalDeaths);
        Assert.Equal(
            state.InitialSolutionPresentation.Select(e => (e.Position, e.SolutionId)),
            saved.PresentedSolutions.OrderBy(e => e.Position).Select(e => (e.Position, e.ChoiceId)));
        Assert.Equal(
            state.InitialJustificationPresentation.Select(e => (e.Position, e.JustificationId)),
            saved.PresentedJustifications.OrderBy(e => e.Position).Select(e => (e.Position, e.ChoiceId)));
    }

    // Saving the same phase again (e.g. a repeated save) must not duplicate the research row.
    [Fact]
    public void Saving_the_same_phase_twice_keeps_one_telemetry_row()
    {
        PhaseDefinition phase = PhaseCatalog.AffordanceVisibility;
        LocalSessionInfo session = _store.CreateNewSession(new PhaseProgress());
        PhaseState state = new(phase, phase.RoomScenarioIds[0]);
        state.ApplySolution(TestSupport.Solutions(phase)[0]);

        _store.SavePhaseCompletion(session.SessionId, state, completeSession: false);
        _store.SavePhaseCompletion(session.SessionId, state, completeSession: false);

        Assert.Single(_store.LoadSessionCompletion(session.SessionId).Phases);
    }

    [Fact]
    public void Activity_is_saved_and_loaded_with_the_session()
    {
        LocalSessionInfo session = _store.CreateNewSession(new PhaseProgress());
        var recorder = new SessionActivityRecorder(null, data => _store.SaveActivity(session.SessionId, data));
        recorder.EnterRoom(ScenarioCatalog.AffordanceVisibilityScenarioId, ScenarioCatalog.TutorialScenarioId);
        recorder.AddDeaths(1);
        recorder.RecordTutorialCompleted();

        SessionActivityData loaded = _store.LoadActivity(session.SessionId);

        Assert.True(loaded.TutorialCompleted);
        Assert.Equal(1, Assert.Single(loaded.Rooms).Deaths);
        Assert.Equal(ResearchStudyConfiguration.GameVersion, _store.LoadSessionCompletion(session.SessionId).GameVersion);
    }

    // The file the student e-mails: named with the visible code, with version and activity,
    // and without personal data (user or machine name).
    [Fact]
    public void Exported_file_has_the_session_code_version_and_no_personal_data()
    {
        LocalSessionInfo session = _store.CreateNewSession(new PhaseProgress());
        var recorder = new SessionActivityRecorder(null, data => _store.SaveActivity(session.SessionId, data));
        recorder.EnterRoom(ScenarioCatalog.AffordanceVisibilityScenarioId, ScenarioCatalog.AffordanceVisibilityScenarioId);

        string path = _store.ExportSession(session.SessionId);

        Assert.Equal(_exportDirectory, Path.GetDirectoryName(path));
        Assert.Contains(session.PublicCode, Path.GetFileName(path));
        string json = File.ReadAllText(path);
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal(session.PublicCode, document.RootElement.GetProperty("publicCode").GetString());
        Assert.Equal(ResearchStudyConfiguration.GameVersion, document.RootElement.GetProperty("gameVersion").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("activity").GetProperty("rooms").GetArrayLength());

        foreach (string personal in new[] { Environment.UserName, Environment.MachineName })
            if (!string.IsNullOrEmpty(personal) && personal.Length >= 3)
                Assert.DoesNotContain(personal, json, StringComparison.OrdinalIgnoreCase);
    }
}
