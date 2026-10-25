using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace DontLikePoetry;

public sealed class LocalSessionInfo
{
    public string SessionId { get; }
    public string PublicCode { get; }

    public LocalSessionInfo(string sessionId, string publicCode)
    {
        SessionId = sessionId;
        PublicCode = publicCode;
    }
}

public sealed class SessionStore
{
    private const string DatabaseFileName = "timothy-mort.db";
    private readonly string _dataDirectory;
    private readonly string _connectionString;

    public string DatabasePath { get; }

    public SessionStore()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimothyMort");
        Directory.CreateDirectory(_dataDirectory);

        DatabasePath = Path.Combine(_dataDirectory, DatabaseFileName);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();

        EnsureSchema();
    }

    public LocalSessionInfo FindMostRecentActiveSession()
    {
        using var connection = OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT session_id, participant_code
            FROM sessions
            WHERE status = 'active'
              AND completed_utc IS NULL
            ORDER BY updated_utc DESC
            LIMIT 1;
            """;

        using SqliteDataReader reader = command.ExecuteReader();
        return reader.Read()
            ? new LocalSessionInfo(reader.GetString(0), reader.GetString(1))
            : null;
    }

    public void ContinueSession(string sessionId, PhaseProgress progress)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A sessão é obrigatória.", nameof(sessionId));
        }

        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        string now = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        using var connection = OpenConnection();
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText =
                """
                UPDATE sessions
                SET updated_utc = $now
                WHERE session_id = $sessionId
                  AND status = 'active'
                  AND completed_utc IS NULL;
                """;
            command.Parameters.AddWithValue("$sessionId", sessionId);
            command.Parameters.AddWithValue("$now", now);

            if (command.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException(
                    "A sessão selecionada não está mais ativa.");
            }
        }

        LoadProgress(connection, sessionId, progress);
    }

    public LocalSessionInfo CreateNewSession(PhaseProgress progress)
    {
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        string sessionId = Guid.NewGuid().ToString("N");
        string publicCode = CreatePublicCode(sessionId);
        string now = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        AbandonActiveSessions(connection, transaction, now);
        UpsertParticipant(connection, transaction, publicCode, now);
        CreateSession(connection, transaction, sessionId, publicCode, now);
        transaction.Commit();

        progress.Clear();
        return new LocalSessionInfo(sessionId, publicCode);
    }

    public void SavePhaseCompletion(
        string sessionId,
        PhaseState state,
        bool completeSession)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A sessão é obrigatória.", nameof(sessionId));
        }

        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        InterventionSolutionDefinition firstSolution = state.HasFirstChoice
            ? state.ChoiceCatalog.GetSolution(state.FirstSolutionId)
            : null;
        InterventionSolutionDefinition finalSolution =
            string.IsNullOrEmpty(state.AppliedSolutionId)
                ? null
                : state.ChoiceCatalog.GetSolution(state.AppliedSolutionId);
        SolutionVisualModule finalSolutionVisualModules =
            finalSolution?.VisualModules ?? SolutionVisualModule.NONE;
        string now = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        UpsertPhaseProgress(
            connection,
            transaction,
            sessionId,
            state.PhaseId,
            state.AppliedSolutionId,
            finalSolutionVisualModules,
            now);

        bool telemetryWasInserted = InsertPhaseTelemetry(
            connection,
            transaction,
            sessionId,
            state,
            firstSolution,
            finalSolution,
            now);

        if (telemetryWasInserted)
        {
            InsertDemonstration(connection, transaction, sessionId, state);
            InsertSolutionPresentation(
                connection,
                transaction,
                sessionId,
                state.PhaseId,
                state.InitialSolutionPresentation);
            InsertJustificationPresentation(
                connection,
                transaction,
                sessionId,
                state.PhaseId,
                state.InitialJustificationPresentation);
        }

        if (completeSession)
        {
            CompleteSession(connection, transaction, sessionId, now);
        }
        else
        {
            TouchSession(connection, transaction, sessionId, now);
        }

        transaction.Commit();
    }

    public SessionCompletionData LoadSessionCompletion(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A sessão é obrigatória.", nameof(sessionId));
        }

        using var connection = OpenConnection();
        SessionCompletionData session = LoadSessionHeader(connection, sessionId);
        LoadPhaseCompletions(connection, session);
        return session;
    }

    public string ExportSession(string sessionId)
    {
        SessionCompletionData session = LoadSessionCompletion(sessionId);
        string exportDirectory = Path.Combine(_dataDirectory, "exports");
        Directory.CreateDirectory(exportDirectory);

        string exportPath = Path.Combine(exportDirectory, session.SessionId + ".json");
        string json = JsonSerializer.Serialize(
            session,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        File.WriteAllText(exportPath, json);

        // Also save a copy in a folder testers can find (Documents on Windows, home on Linux).
        try
        {
            string visibleDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "TimothyMort");
            Directory.CreateDirectory(visibleDirectory);
            string visiblePath = Path.Combine(visibleDirectory,
                $"sessao_{session.PublicCode}_{session.SessionId}.json");
            File.WriteAllText(visiblePath, json);
            return visiblePath;
        }
        catch (Exception)
        {
            return exportPath;
        }
    }

    private static void UpsertPhaseProgress(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        string phaseId,
        string finalSolutionId,
        SolutionVisualModule finalSolutionVisualModules,
        string now)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO phase_progress (
                session_id,
                phase_id,
                completed,
                final_solution_id,
                final_visual_modules,
                updated_utc)
            VALUES ($sessionId, $phaseId, 1, $solutionId, $visualModules, $updatedUtc)
            ON CONFLICT(session_id, phase_id) DO UPDATE SET
                completed = excluded.completed,
                final_solution_id = excluded.final_solution_id,
                final_visual_modules = excluded.final_visual_modules,
                updated_utc = excluded.updated_utc;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$phaseId", phaseId);
        command.Parameters.AddWithValue(
            "$solutionId",
            (object)finalSolutionId ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$visualModules",
            (long)finalSolutionVisualModules);
        command.Parameters.AddWithValue("$updatedUtc", now);
        command.ExecuteNonQuery();
    }

    private static bool InsertPhaseTelemetry(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        PhaseState state,
        InterventionSolutionDefinition firstSolution,
        InterventionSolutionDefinition finalSolution,
        string completedUtc)
    {
        int? firstSolutionPosition = FindSolutionPosition(
            state.InitialSolutionPresentation,
            state.FirstSolutionId);
        int? firstJustificationPosition = FindJustificationPosition(
            state.InitialJustificationPresentation,
            state.FirstJustificationId);

        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO phase_telemetry (
                session_id,
                phase_id,
                completed_utc,
                active_seconds,
                total_deaths,
                first_solution_id,
                first_solution_quality,
                first_solution_position,
                first_justification_id,
                first_justification_position,
                had_revision,
                selection_count,
                final_solution_id,
                final_solution_quality,
                applied_visual_modules)
            VALUES (
                $sessionId,
                $phaseId,
                $completedUtc,
                $activeSeconds,
                $totalDeaths,
                $firstSolutionId,
                $firstSolutionQuality,
                $firstSolutionPosition,
                $firstJustificationId,
                $firstJustificationPosition,
                $hadRevision,
                $selectionCount,
                $finalSolutionId,
                $finalSolutionQuality,
                $appliedVisualModules)
            ON CONFLICT(session_id, phase_id) DO NOTHING;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$phaseId", state.PhaseId);
        command.Parameters.AddWithValue("$completedUtc", completedUtc);
        command.Parameters.AddWithValue("$activeSeconds", state.ElapsedSeconds);
        command.Parameters.AddWithValue("$totalDeaths", state.TotalDeathCount);
        command.Parameters.AddWithValue(
            "$firstSolutionId",
            (object)state.FirstSolutionId ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$firstSolutionQuality",
            (object)firstSolution?.Quality.ToString() ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$firstSolutionPosition",
            (object)firstSolutionPosition ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$firstJustificationId",
            (object)state.FirstJustificationId ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$firstJustificationPosition",
            (object)firstJustificationPosition ?? DBNull.Value);
        command.Parameters.AddWithValue("$hadRevision", state.HasRevision ? 1 : 0);
        command.Parameters.AddWithValue(
            "$selectionCount",
            state.SolutionSelectionCount);
        command.Parameters.AddWithValue(
            "$finalSolutionId",
            (object)state.AppliedSolutionId ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$finalSolutionQuality",
            (object)finalSolution?.Quality.ToString() ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$appliedVisualModules",
            (long)state.AppliedVisualModules);
        return command.ExecuteNonQuery() == 1;
    }

    private static void InsertDemonstration(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        PhaseState state)
    {
        if (state.Demonstration == null) return;
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO phase_demonstrations (session_id, phase_id, payload_json)
            VALUES ($sessionId, $phaseId, $payload);
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$phaseId", state.PhaseId);
        command.Parameters.AddWithValue("$payload", JsonSerializer.Serialize(state.Demonstration));
        command.ExecuteNonQuery();
    }

    private static void InsertSolutionPresentation(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        string phaseId,
        IReadOnlyList<PresentedSolutionDefinition> presentation)
    {
        foreach (PresentedSolutionDefinition entry in presentation)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                INSERT INTO phase_presented_solutions (
                    session_id,
                    phase_id,
                    position,
                    solution_id)
                VALUES ($sessionId, $phaseId, $position, $solutionId);
                """;
            command.Parameters.AddWithValue("$sessionId", sessionId);
            command.Parameters.AddWithValue("$phaseId", phaseId);
            command.Parameters.AddWithValue("$position", entry.Position);
            command.Parameters.AddWithValue("$solutionId", entry.SolutionId);
            command.ExecuteNonQuery();
        }
    }

    private static void InsertJustificationPresentation(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        string phaseId,
        IReadOnlyList<PresentedJustificationDefinition> presentation)
    {
        foreach (PresentedJustificationDefinition entry in presentation)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                INSERT INTO phase_presented_justifications (
                    session_id,
                    phase_id,
                    position,
                    justification_id)
                VALUES ($sessionId, $phaseId, $position, $justificationId);
                """;
            command.Parameters.AddWithValue("$sessionId", sessionId);
            command.Parameters.AddWithValue("$phaseId", phaseId);
            command.Parameters.AddWithValue("$position", entry.Position);
            command.Parameters.AddWithValue(
                "$justificationId",
                entry.JustificationId);
            command.ExecuteNonQuery();
        }
    }

    private static int? FindSolutionPosition(
        IReadOnlyList<PresentedSolutionDefinition> presentation,
        string solutionId)
    {
        foreach (PresentedSolutionDefinition entry in presentation)
        {
            if (entry.SolutionId == solutionId)
            {
                return entry.Position;
            }
        }

        return null;
    }

    private static int? FindJustificationPosition(
        IReadOnlyList<PresentedJustificationDefinition> presentation,
        string justificationId)
    {
        foreach (PresentedJustificationDefinition entry in presentation)
        {
            if (entry.JustificationId == justificationId)
            {
                return entry.Position;
            }
        }

        return null;
    }

    private static string CreatePublicCode(string sessionId)
    {
        string visibleId = sessionId[..12].ToUpperInvariant();
        return $"TM-{visibleId[..4]}-{visibleId[4..8]}-{visibleId[8..12]}";
    }

    private static void AbandonActiveSessions(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string now)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            UPDATE sessions
            SET status = 'abandoned',
                updated_utc = $now
            WHERE status = 'active'
              AND completed_utc IS NULL;
            """;
        command.Parameters.AddWithValue("$now", now);
        command.ExecuteNonQuery();
    }

    private static SessionCompletionData LoadSessionHeader(
        SqliteConnection connection,
        string sessionId)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT participant_code, status, created_utc, completed_utc
            FROM sessions
            WHERE session_id = $sessionId;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);

        using SqliteDataReader reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidOperationException("A sessão não foi encontrada.");
        }

        return new SessionCompletionData
        {
            SessionId = sessionId,
            PublicCode = reader.GetString(0),
            Status = reader.GetString(1),
            CreatedUtc = reader.GetString(2),
            CompletedUtc = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }

    private static void LoadPhaseCompletions(
        SqliteConnection connection,
        SessionCompletionData session)
    {
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText =
                """
                SELECT
                    phase_id,
                    completed_utc,
                    active_seconds,
                    total_deaths,
                    first_solution_id,
                    first_solution_quality,
                    first_solution_position,
                    first_justification_id,
                    first_justification_position,
                    had_revision,
                    selection_count,
                    final_solution_id,
                    final_solution_quality,
                    applied_visual_modules
                FROM phase_telemetry
                WHERE session_id = $sessionId
                ORDER BY completed_utc;
                """;
            command.Parameters.AddWithValue("$sessionId", session.SessionId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                session.Phases.Add(new PhaseCompletionData
                {
                    PhaseId = reader.GetString(0),
                    CompletedUtc = reader.GetString(1),
                    ActiveSeconds = reader.GetDouble(2),
                    TotalDeaths = reader.GetInt32(3),
                    FirstSolutionId = ReadNullableString(reader, 4),
                    FirstSolutionQuality = ReadNullableString(reader, 5),
                    FirstSolutionPosition = ReadNullableInt(reader, 6),
                    FirstJustificationId = ReadNullableString(reader, 7),
                    FirstJustificationPosition = ReadNullableInt(reader, 8),
                    HadRevision = reader.GetInt32(9) != 0,
                    SelectionCount = reader.GetInt32(10),
                    FinalSolutionId = ReadNullableString(reader, 11),
                    FinalSolutionQuality = ReadNullableString(reader, 12),
                    AppliedVisualModules = reader.GetInt64(13)
                });
            }
        }

        foreach (PhaseCompletionData phase in session.Phases)
        {
            using (SqliteCommand demoCommand = connection.CreateCommand())
            {
                demoCommand.CommandText =
                    "SELECT payload_json FROM phase_demonstrations WHERE session_id = $sessionId AND phase_id = $phaseId;";
                demoCommand.Parameters.AddWithValue("$sessionId", session.SessionId);
                demoCommand.Parameters.AddWithValue("$phaseId", phase.PhaseId);
                if (demoCommand.ExecuteScalar() is string payload)
                    phase.Demonstration = JsonSerializer.Deserialize<PedagogicalDemonstrationData>(payload);
            }
            LoadPresentedChoices(
                connection,
                session.SessionId,
                phase.PhaseId,
                true,
                phase.PresentedSolutions);
            LoadPresentedChoices(
                connection,
                session.SessionId,
                phase.PhaseId,
                false,
                phase.PresentedJustifications);
        }
    }

    private static void LoadPresentedChoices(
        SqliteConnection connection,
        string sessionId,
        string phaseId,
        bool loadSolutions,
        List<PresentedChoiceData> destination)
    {
        string tableName = loadSolutions
            ? "phase_presented_solutions"
            : "phase_presented_justifications";
        string idColumn = loadSolutions ? "solution_id" : "justification_id";

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT position, {idColumn}
            FROM {tableName}
            WHERE session_id = $sessionId
              AND phase_id = $phaseId
            ORDER BY position;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$phaseId", phaseId);

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            destination.Add(new PresentedChoiceData
            {
                Position = reader.GetInt32(0),
                ChoiceId = reader.GetString(1)
            });
        }
    }

    private static string ReadNullableString(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? ReadNullableInt(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private void EnsureSchema()
    {
        using var connection = OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS participants (
                participant_code TEXT PRIMARY KEY,
                created_utc TEXT NOT NULL,
                updated_utc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS sessions (
                session_id TEXT PRIMARY KEY,
                participant_code TEXT NOT NULL,
                created_utc TEXT NOT NULL,
                updated_utc TEXT NOT NULL,
                status TEXT NOT NULL DEFAULT 'active',
                completed_utc TEXT NULL,
                FOREIGN KEY (participant_code)
                    REFERENCES participants(participant_code)
                    ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_sessions_active_participant
                ON sessions(participant_code, completed_utc, created_utc);

            CREATE TABLE IF NOT EXISTS phase_progress (
                session_id TEXT NOT NULL,
                phase_id TEXT NOT NULL,
                completed INTEGER NOT NULL,
                final_solution_id TEXT NULL,
                final_visual_modules INTEGER NOT NULL DEFAULT 0,
                updated_utc TEXT NOT NULL,
                PRIMARY KEY (session_id, phase_id),
                FOREIGN KEY (session_id)
                    REFERENCES sessions(session_id)
                    ON DELETE CASCADE
            );
            """;
        command.ExecuteNonQuery();

        EnsureSessionStatusColumn(connection);

        using SqliteCommand telemetryCommand = connection.CreateCommand();
        telemetryCommand.CommandText =
            """
            CREATE INDEX IF NOT EXISTS ix_sessions_status_updated
                ON sessions(status, updated_utc);

            CREATE TABLE IF NOT EXISTS phase_telemetry (
                session_id TEXT NOT NULL,
                phase_id TEXT NOT NULL,
                completed_utc TEXT NOT NULL,
                active_seconds REAL NOT NULL,
                total_deaths INTEGER NOT NULL,
                first_solution_id TEXT NULL,
                first_solution_quality TEXT NULL,
                first_solution_position INTEGER NULL,
                first_justification_id TEXT NULL,
                first_justification_position INTEGER NULL,
                had_revision INTEGER NOT NULL,
                selection_count INTEGER NOT NULL,
                final_solution_id TEXT NULL,
                final_solution_quality TEXT NULL,
                applied_visual_modules INTEGER NOT NULL,
                PRIMARY KEY (session_id, phase_id),
                FOREIGN KEY (session_id)
                    REFERENCES sessions(session_id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS phase_demonstrations (
                session_id TEXT NOT NULL,
                phase_id TEXT NOT NULL,
                payload_json TEXT NOT NULL,
                PRIMARY KEY (session_id, phase_id),
                FOREIGN KEY (session_id, phase_id)
                    REFERENCES phase_telemetry(session_id, phase_id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS phase_presented_solutions (
                session_id TEXT NOT NULL,
                phase_id TEXT NOT NULL,
                position INTEGER NOT NULL,
                solution_id TEXT NOT NULL,
                PRIMARY KEY (session_id, phase_id, position),
                FOREIGN KEY (session_id, phase_id)
                    REFERENCES phase_telemetry(session_id, phase_id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS phase_presented_justifications (
                session_id TEXT NOT NULL,
                phase_id TEXT NOT NULL,
                position INTEGER NOT NULL,
                justification_id TEXT NOT NULL,
                PRIMARY KEY (session_id, phase_id, position),
                FOREIGN KEY (session_id, phase_id)
                    REFERENCES phase_telemetry(session_id, phase_id)
                    ON DELETE CASCADE
            );
            """;
        telemetryCommand.ExecuteNonQuery();
    }

    private static void EnsureSessionStatusColumn(SqliteConnection connection)
    {
        bool hasStatusColumn = false;
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info(sessions);";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), "status", StringComparison.Ordinal))
                {
                    hasStatusColumn = true;
                    break;
                }
            }
        }

        if (hasStatusColumn)
        {
            return;
        }

        using SqliteCommand alterCommand = connection.CreateCommand();
        alterCommand.CommandText =
            "ALTER TABLE sessions ADD COLUMN status TEXT NOT NULL DEFAULT 'active';";
        alterCommand.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static void UpsertParticipant(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string participantCode,
        string now)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO participants (participant_code, created_utc, updated_utc)
            VALUES ($participantCode, $now, $now)
            ON CONFLICT(participant_code) DO UPDATE SET
                updated_utc = excluded.updated_utc;
            """;
        command.Parameters.AddWithValue("$participantCode", participantCode);
        command.Parameters.AddWithValue("$now", now);
        command.ExecuteNonQuery();
    }

    private static void CreateSession(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        string participantCode,
        string now)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO sessions (
                session_id,
                participant_code,
                created_utc,
                updated_utc,
                completed_utc)
            VALUES ($sessionId, $participantCode, $now, $now, NULL);
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$participantCode", participantCode);
        command.Parameters.AddWithValue("$now", now);
        command.ExecuteNonQuery();
    }

    private static void TouchSession(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        string now)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            UPDATE sessions
            SET updated_utc = $now
            WHERE session_id = $sessionId;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$now", now);
        command.ExecuteNonQuery();
    }

    private static void CompleteSession(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sessionId,
        string now)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            UPDATE sessions
            SET status = 'completed',
                updated_utc = $now,
                completed_utc = $now
            WHERE session_id = $sessionId
              AND status = 'active'
              AND completed_utc IS NULL;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);
        command.Parameters.AddWithValue("$now", now);

        if (command.ExecuteNonQuery() != 1)
        {
            throw new InvalidOperationException(
                "A sessão não pôde ser marcada como concluída.");
        }
    }

    private static void LoadProgress(
        SqliteConnection connection,
        string sessionId,
        PhaseProgress progress)
    {
        progress.Clear();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT phase_id, final_solution_id, final_visual_modules
            FROM phase_progress
            WHERE session_id = $sessionId
              AND completed = 1;
            """;
        command.Parameters.AddWithValue("$sessionId", sessionId);

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string phaseId = reader.GetString(0);
            string finalSolutionId = reader.IsDBNull(1) ? null : reader.GetString(1);
            var visualModules = (SolutionVisualModule)reader.GetInt64(2);
            progress.RestoreCompletedPhase(phaseId, finalSolutionId, visualModules);
        }
    }
}
