using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public static class ScenarioCatalog
{
    public const string InitialScenarioId = "simple_cutscene";
    public const string AffordanceVisibilityScenarioId = "affordance_visibilidade";
    public const string AffordanceRoom2ScenarioId = "affordance_tela_2";
    public const string AffordanceRoom3ScenarioId = "affordance_tela_3";
    public const string AffordanceInterventionScenarioId = "affordance_intervencao";
    public const string DamageFeedbackScenarioId = "feedback_dano";
    public const string DamageFeedbackRoom2ScenarioId = "feedback_tela_2";
    public const string DamageFeedbackRoom3ScenarioId = "feedback_tela_3";
    public const string DamageFeedbackInterventionScenarioId = "feedback_intervencao";
    public const string ConceptualModelsConstraintsScenarioId =
        "modelos_conceituais_restricoes";
    public const string ConceptualModelsConstraintsRoom2ScenarioId =
        "modelos_conceituais_restricoes_tela_2";
    public const string ConceptualModelsConstraintsRoom3ScenarioId =
        "modelos_conceituais_restricoes_tela_3";
    public const string ConceptualModelsConstraintsInterventionScenarioId =
        "modelos_conceituais_restricoes_intervencao";
    public const string PropVisualScenarioId = "prop_visual";
    // Shown before Affordance the first time in a session; not part of any phase (no telemetry).
    public const string TutorialScenarioId = "tutorial";

    private static readonly IReadOnlyDictionary<string, ScenarioDefinition> Scenarios = new Dictionary<string, ScenarioDefinition>(StringComparer.Ordinal)
        {
            [InitialScenarioId] = new ScenarioDefinition(
                InitialScenarioId,
                "Content/SimpleCutsceneMap.tmx",
                new Vector2(10, 552)
            ),
            [AffordanceVisibilityScenarioId] = new ScenarioDefinition(
                AffordanceVisibilityScenarioId,
                "Content/Maps/Affordance.ldtk",
                new Vector2(160, 816),
                "AffordanceTela1",
                ScenarioCameraBehavior.CENTER_MAP,
                0.14f,
                true
            ),
            [AffordanceRoom2ScenarioId] = new ScenarioDefinition(
                AffordanceRoom2ScenarioId,
                "Content/Maps/Affordance.ldtk",
                new Vector2(96, 816),
                "AffordanceTela2",
                ScenarioCameraBehavior.CENTER_MAP,
                0.14f,
                true),
            [AffordanceRoom3ScenarioId] = new ScenarioDefinition(
                AffordanceRoom3ScenarioId,
                "Content/Maps/Affordance.ldtk",
                new Vector2(96, 816),
                "AffordanceTela3",
                ScenarioCameraBehavior.CENTER_MAP,
                0.14f,
                true),
            [AffordanceInterventionScenarioId] = new ScenarioDefinition(
                AffordanceInterventionScenarioId,
                "Content/Maps/Affordance.ldtk",
                new Vector2(160, 816),
                "Aff_Sala_intervencao",
                ScenarioCameraBehavior.CENTER_MAP,
                0.14f,
                true
            ),
            // Phases 2 and 3 use the Affordance room format: three rooms in the same .ldtk
            // and their own intervention room.
            [DamageFeedbackScenarioId] = Room(DamageFeedbackScenarioId, "FeedbackTela1", 160),
            [DamageFeedbackRoom2ScenarioId] = Room(DamageFeedbackRoom2ScenarioId, "FeedbackTela2", 96),
            [DamageFeedbackRoom3ScenarioId] = Room(DamageFeedbackRoom3ScenarioId, "FeedbackTela3", 96),
            [DamageFeedbackInterventionScenarioId] =
                Room(DamageFeedbackInterventionScenarioId, "Feed_Sala_intervencao2", 160),
            [ConceptualModelsConstraintsScenarioId] =
                Room(ConceptualModelsConstraintsScenarioId, "ModelosTela1", 160),
            [ConceptualModelsConstraintsRoom2ScenarioId] =
                Room(ConceptualModelsConstraintsRoom2ScenarioId, "ModelosTela2", 96),
            [ConceptualModelsConstraintsRoom3ScenarioId] =
                Room(ConceptualModelsConstraintsRoom3ScenarioId, "ModelosTela3", 96),
            [ConceptualModelsConstraintsInterventionScenarioId] =
                Room(ConceptualModelsConstraintsInterventionScenarioId, "Concept_Sala_intervencao3", 160),
            [TutorialScenarioId] = Room(TutorialScenarioId, "Tutorial", 160),
            [PropVisualScenarioId] = new ScenarioDefinition(
                PropVisualScenarioId,
                "Content/PropVisual.tmx",
                new Vector2(160, 816)
            )
        };

    // A room of the shared .ldtk, with the same camera and scale as the Affordance rooms.
    private static ScenarioDefinition Room(string id, string ldtkLevelId, int fallbackX) =>
        new ScenarioDefinition(
            id,
            "Content/Maps/Affordance.ldtk",
            new Vector2(fallbackX, 816),
            ldtkLevelId,
            ScenarioCameraBehavior.CENTER_MAP,
            0.14f,
            true);

    public static ScenarioDefinition Initial
    {
        get { return Get(InitialScenarioId); }
    }

    public static ScenarioDefinition Get(string scenarioId)
    {
        if (Scenarios.TryGetValue(scenarioId, out ScenarioDefinition scenario))
        {
            return scenario;
        }

        throw new InvalidOperationException($"Cenario nao registrado: {scenarioId}");
    }
}
