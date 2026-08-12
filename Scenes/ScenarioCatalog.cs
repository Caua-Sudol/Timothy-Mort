using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DontLikePoetry;

public static class ScenarioCatalog
{
    public const string InitialScenarioId = "simple_cutscene";
    public const string AffordanceVisibilityScenarioId = "affordance_visibilidade";

    private static readonly IReadOnlyDictionary<string, ScenarioDefinition> Scenarios =
        new Dictionary<string, ScenarioDefinition>(StringComparer.Ordinal)
        {
            [InitialScenarioId] = new ScenarioDefinition(
                InitialScenarioId,
                "Content/SimpleCutsceneMap.tmx",
                new Vector2(10, 552)),
            [AffordanceVisibilityScenarioId] = new ScenarioDefinition(
                AffordanceVisibilityScenarioId,
                "Content/AffordanceVisibilidade.tmx",
                new Vector2(160, 816))
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
