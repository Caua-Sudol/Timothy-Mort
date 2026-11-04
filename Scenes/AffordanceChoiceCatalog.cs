using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public static class AffordanceChoiceCatalog
{
    public const string SolutionQuestion =
        "Qual aparência ajuda você a prever melhor o comportamento da plataforma?";
    public const string JustificationQuestion =
        "Que resultado você espera obter com essa intervenção?";

    public const string AppearanceSolutionId = "terrain_appearance";
    public const string MarkSolutionId = "terrain_mark";
    public const string MotionSolutionId = "terrain_motion";

    // The three alternatives change communication, never the terrain's physics.
    // Material art is still provisional; both other cues have limited semantic specificity.
    public static IReadOnlyList<InterventionSolutionDefinition> InitialSolutions { get; } =
        Array.AsReadOnly(new[]
        {
            new InterventionSolutionDefinition(
                AppearanceSolutionId,
                "Aparência",
                "Alterar a aparência dos terrenos estranhos",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_COLOR),
            new InterventionSolutionDefinition(
                MarkSolutionId,
                "Marcação",
                "Marcar os terrenos estranhos",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_TERRAIN_MARK),
            new InterventionSolutionDefinition(
                MotionSolutionId,
                "Deformação",
                "Deformar os terrenos estranhos",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_JELLY_MOTION)
        });

    // Reuse the same alternatives: revision is optional experimentation in this phase.
    public static IReadOnlyList<InterventionSolutionDefinition> AvailableRevisionSolutions =>
        InitialSolutions;

    public static IReadOnlyList<InterventionJustificationDefinition> Justifications { get; } =
        Array.AsReadOnly(new[]
        {
            // Three options (expert test): one per kind of cue the solutions give.
            new InterventionJustificationDefinition(
                "terrain_prediction", "Prever o comportamento do terreno antes de entrar em contato"),
            new InterventionJustificationDefinition(
                "terrain_identification", "Reconhecer quais terrenos possuem um comportamento diferente"),
            new InterventionJustificationDefinition(
                "terrain_attention", "Chamar atenção para os terrenos que merecem ser observados")
        });

    public static IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var solutions = new List<InterventionSolutionDefinition>(InitialSolutions);
        for (int index = solutions.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            (solutions[index], solutions[swapIndex]) = (solutions[swapIndex], solutions[index]);
        }

        var presentation = new List<PresentedSolutionDefinition>(solutions.Count);
        for (int index = 0; index < solutions.Count; index++)
            presentation.Add(new PresentedSolutionDefinition(solutions[index].Id, index + 1));
        return presentation.AsReadOnly();
    }

    public static IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var justifications = new List<InterventionJustificationDefinition>(Justifications);
        for (int index = justifications.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            (justifications[index], justifications[swapIndex]) =
                (justifications[swapIndex], justifications[index]);
        }

        var presentation = new List<PresentedJustificationDefinition>(justifications.Count);
        for (int index = 0; index < justifications.Count; index++)
            presentation.Add(new PresentedJustificationDefinition(justifications[index].Id, index + 1));
        return presentation.AsReadOnly();
    }

    public static InterventionSolutionDefinition GetSolution(string solutionId)
    {
        foreach (InterventionSolutionDefinition solution in InitialSolutions)
            if (solution.Id == solutionId)
                return solution;
        throw new InvalidOperationException($"Solucao nao registrada: {solutionId}");
    }

    public static InterventionJustificationDefinition GetJustification(string justificationId)
    {
        foreach (InterventionJustificationDefinition justification in Justifications)
            if (justification.Id == justificationId)
                return justification;
        throw new InvalidOperationException($"Justificativa nao registrada: {justificationId}");
    }

    public static string GetExplanation(string solutionId) => solutionId switch
    {
        AppearanceSolutionId =>
            "O material aparente oferece uma pista. " +
            "A aparência macia é a proposta mais adequada neste cenário, se sua aparência " +
            "corresponder ao comportamento percebido.",
        MarkSolutionId =>
            "A marca diferencia os terrenos, mas seu significado precisa ser aprendido. " +
            "Ela permite reconhecer onde há uma diferença sem explicar qual é o comportamento.",
        MotionSolutionId =>
            "A pulsação oferece uma pista. A terra em movimento sugere uma propriedade " +
            "incomum, mas também pode sugerir elasticidade ou instabilidade.",
        _ => "Nenhuma mudança foi escolhida para comparar a aparência e o comportamento dos terrenos."
    };
}
