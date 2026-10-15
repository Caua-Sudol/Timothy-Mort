namespace DontLikePoetry;

public sealed class StudySessionFlow
{
    private readonly PhaseProgress _phaseProgress;
    private SessionStore _sessionStore;
    private LocalSessionInfo _pendingSession;
    private LocalSessionInfo _activeSession;

    public SessionCompletionData CompletionData { get; private set; }
    public bool ShowCompletionAfterPhaseResult { get; set; }
    public LocalSessionInfo PendingSession => _pendingSession;
    public LocalSessionInfo ActiveSession => _activeSession;

    public StudySessionFlow(PhaseProgress phaseProgress)
    {
        _phaseProgress = phaseProgress;
    }

    public bool TryOpen(out bool activatedSession)
    {
        activatedSession = false;

        try
        {
            _sessionStore ??= new SessionStore();
            _pendingSession = _sessionStore.FindMostRecentActiveSession();
            if (_pendingSession != null)
                return true;

            Activate(_sessionStore.CreateNewSession(_phaseProgress));
            activatedSession = true;
            return true;
        }
        catch
        {
            _pendingSession = null;
            return false;
        }
    }

    public bool TryContinuePendingSession()
    {
        try
        {
            _sessionStore.ContinueSession(_pendingSession.SessionId, _phaseProgress);
            Activate(_pendingSession);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryStartNewSession()
    {
        try
        {
            Activate(_sessionStore.CreateNewSession(_phaseProgress));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryRecordPhaseCompletion(
        PhaseState phaseState,
        bool completesStudy)
    {
        try
        {
            SolutionVisualModule finalVisualModules =
                string.IsNullOrEmpty(phaseState.AppliedSolutionId)
                    ? SolutionVisualModule.NONE
                    : phaseState.ChoiceCatalog
                        .GetSolution(phaseState.AppliedSolutionId)
                        .VisualModules;

            _sessionStore.SavePhaseCompletion(
                _activeSession.SessionId,
                phaseState,
                completesStudy);
            _phaseProgress.TryCompletePhase(
                phaseState.PhaseId,
                phaseState.AppliedSolutionId,
                finalVisualModules);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryPrepareCompletion(out string exportPath)
    {
        try
        {
            CompletionData = _sessionStore.LoadSessionCompletion(_activeSession.SessionId);
            exportPath = _sessionStore.ExportSession(_activeSession.SessionId);
            return true;
        }
        catch
        {
            CompletionData ??= new SessionCompletionData
            {
                SessionId = _activeSession.SessionId,
                PublicCode = _activeSession.PublicCode,
                Status = "completed"
            };
            exportPath = null;
            return false;
        }
    }

    public bool TryRefreshCompletionExport(out string exportPath)
    {
        try
        {
            CompletionData = _sessionStore.LoadSessionCompletion(_activeSession.SessionId);
            exportPath = _sessionStore.ExportSession(_activeSession.SessionId);
            return true;
        }
        catch
        {
            exportPath = null;
            return false;
        }
    }

    public void Reset()
    {
        _phaseProgress.Clear();
        _pendingSession = null;
        _activeSession = null;
        CompletionData = null;
        ShowCompletionAfterPhaseResult = false;
    }

    private void Activate(LocalSessionInfo session)
    {
        _activeSession = session;
        _pendingSession = null;
        CompletionData = null;
        ShowCompletionAfterPhaseResult = false;
    }
}
