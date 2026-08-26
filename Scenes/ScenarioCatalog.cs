using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public static class ScenarioCatalog
{
    public const string InitialScenarioId = "simple_cutscene";
    public const string AffordanceVisibilityScenarioId = "affordance_visibilidade";
    public const string AffordanceInterventionScenarioId = "affordance_intervencao";
    public const string DamageFeedbackScenarioId = "feedback_dano";
    public const string DamageFeedbackInterventionScenarioId = "feedback_intervencao";
    public const string ConceptualModelsConstraintsScenarioId =
        "modelos_conceituais_restricoes";
    public const string ConceptualModelsConstraintsInterventionScenarioId =
        "modelos_conceituais_restricoes_intervencao";
    public const string PropVisualScenarioId = "prop_visual";

    private static readonly IReadOnlyDictionary<string, ScenarioDefinition> Scenarios = new Dictionary<string, ScenarioDefinition>(StringComparer.Ordinal)
        {
            [InitialScenarioId] = new ScenarioDefinition(
                InitialScenarioId,
                "Content/SimpleCutsceneMap.tmx",
                new Vector2(10, 552)
            ),
            [AffordanceVisibilityScenarioId] = new ScenarioDefinition(
                AffordanceVisibilityScenarioId,
                "Content/AffordanceVisibilidade.tmx",
                new Vector2(160, 816)
            ),
            [AffordanceInterventionScenarioId] = new ScenarioDefinition(
                AffordanceInterventionScenarioId,
                "Content/AffordanceIntervencao.tmx",
                new Vector2(160, 816)
            ),
            [DamageFeedbackScenarioId] = new ScenarioDefinition(
                DamageFeedbackScenarioId,
                "Content/FeedbackDano.tmx",
                new Vector2(160, 816)
            ),
            [DamageFeedbackInterventionScenarioId] = new ScenarioDefinition(
                DamageFeedbackInterventionScenarioId,
                "Content/FeedbackIntervencao.tmx",
                new Vector2(160, 816)
            ),
            [ConceptualModelsConstraintsScenarioId] = new ScenarioDefinition(
                ConceptualModelsConstraintsScenarioId,
                "Content/ModelosConceituaisRestricoes.tmx",
                new Vector2(160, 816)
            ),
            [ConceptualModelsConstraintsInterventionScenarioId] = new ScenarioDefinition(
                ConceptualModelsConstraintsInterventionScenarioId,
                "Content/ModelosConceituaisRestricoesIntervencao.tmx",
                new Vector2(160, 816)
            ),
            [PropVisualScenarioId] = new ScenarioDefinition(
                PropVisualScenarioId,
                "Content/PropVisual.tmx",
                new Vector2(160, 816)
            )
        };

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
