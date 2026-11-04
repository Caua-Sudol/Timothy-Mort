using System;
using System.Globalization;

namespace DontLikePoetry;

// Collects what happens during play (rooms, deaths, control scheme, intervention choices)
// and saves it at meaningful moments, so even an abandoned session leaves a useful file.
public sealed class SessionActivityRecorder
{
    private readonly Action<SessionActivityData> _save;
    private RoomActivityData _currentRoom;

    public SessionActivityData Data { get; }

    public SessionActivityRecorder(SessionActivityData data, Action<SessionActivityData> save)
    {
        Data = data ?? new SessionActivityData();
        Data.GameVersion = ResearchStudyConfiguration.GameVersion;
        _save = save;
        if (Data.ControlSchemes.Count == 0)
            Data.ControlSchemes.Add(new ControlSchemeChangeData
            {
                Utc = Now(),
                Scheme = TutorialControls.Current.ToString()
            });
    }

    public void EnterRoom(string phaseId, string scenarioId)
    {
        _currentRoom = Data.Rooms.Find(room =>
            room.PhaseId == phaseId && room.ScenarioId == scenarioId);
        if (_currentRoom == null)
        {
            _currentRoom = new RoomActivityData { PhaseId = phaseId, ScenarioId = scenarioId };
            Data.Rooms.Add(_currentRoom);
        }
        _currentRoom.Entries++;
        Data.LastPhaseId = phaseId;
        Data.LastScenarioId = scenarioId;
        Save();
    }

    // Called every active frame; saved with the next event.
    public void AddActiveTime(double seconds)
    {
        if (_currentRoom != null) _currentRoom.ActiveSeconds += seconds;
    }

    public void AddDeaths(int count)
    {
        if (_currentRoom != null && count > 0) _currentRoom.Deaths += count;
    }

    public void RecordControlScheme(ControlScheme scheme)
    {
        string name = scheme.ToString();
        if (Data.ControlSchemes.Count > 0 && Data.ControlSchemes[^1].Scheme == name) return;
        Data.ControlSchemes.Add(new ControlSchemeChangeData { Utc = Now(), Scheme = name });
        Save();
    }

    public void RecordTutorialCompleted()
    {
        Data.TutorialCompleted = true;
        Save();
    }

    public void RecordIntervention(string phaseId, string kind, string choiceId)
    {
        Data.InterventionEvents.Add(new InterventionEventData
        {
            Utc = Now(),
            PhaseId = phaseId,
            Kind = kind,
            ChoiceId = choiceId
        });
        Save();
    }

    public void Save()
    {
        Data.UpdatedUtc = Now();
        try
        {
            _save?.Invoke(Data);
        }
        catch
        {
            // Telemetry must never interrupt the game; the next save tries again.
        }
    }

    private static string Now() => DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
}
