using System;
using System.Collections.Generic;

namespace DontLikePoetry;

// Phase 3 (Conceptual models): some gelatins and hot rocks look like the ones already learned
// but act differently (impostors). The three alternatives change how that mismatch is resolved.
public sealed class ConceptualModelsConstraintsChoiceCatalog : IInterventionChoiceCatalog
{
    public const string DifferentiateSolutionId = "concept_differentiate";
    public const string RestoreSolutionId = "concept_restore";
    public const string AnimateSolutionId = "concept_animate";

    public static ConceptualModelsConstraintsChoiceCatalog Instance { get; } =
        new ConceptualModelsConstraintsChoiceCatalog();

    public string SolutionQuestion =>
        "Qual mudança ajuda você a prever o que cada plataforma faz?";

    public string JustificationQuestion =>
        "Que resultado você espera obter com essa intervenção?";

    public IReadOnlyList<InterventionSolutionDefinition> InitialSolutions { get; } =
        Array.AsReadOnly(new[]
        {
            new InterventionSolutionDefinition(
                DifferentiateSolutionId,
                "Diferenciar",
                "Dar uma nova aparência às plataformas que agem diferente",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.IMPOSTOR_DIFFERENTIATE),
            new InterventionSolutionDefinition(
                RestoreSolutionId,
                "Restaurar",
                "Fazer as plataformas que agem diferente voltarem ao efeito já conhecido",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.IMPOSTOR_RESTORE),
            new InterventionSolutionDefinition(
                AnimateSolutionId,
                "Animar",
                "Dar às plataformas que agem diferente um movimento que mostre o que elas fazem",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.IMPOSTOR_ANIMATE)
        });

    // The choice can be changed inside the intervention room; there is no revision round.
    public IReadOnlyList<InterventionJustificationDefinition> Justifications { get; } =
        Array.AsReadOnly(new[]
        {
            // Three options (expert test): one per kind of answer the solutions give.
            new InterventionJustificationDefinition(
                "concept_predict", "Prever o que cada plataforma faz antes de tocá-la"),
            new InterventionJustificationDefinition(
                "concept_same_look", "Fazer plataformas iguais se comportarem do mesmo jeito"),
            new InterventionJustificationDefinition(
                "concept_keep_new", "Manter os novos comportamentos disponíveis na travessia")
        });

    private ConceptualModelsConstraintsChoiceCatalog()
    {
    }

    public IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var solutions = new List<InterventionSolutionDefinition>(InitialSolutions);
        Shuffle(solutions, random);
        var presentation = new List<PresentedSolutionDefinition>(solutions.Count);
        for (int index = 0; index < solutions.Count; index++)
            presentation.Add(new PresentedSolutionDefinition(solutions[index].Id, index + 1));
        return presentation.AsReadOnly();
    }

    public IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var justifications = new List<InterventionJustificationDefinition>(Justifications);
        Shuffle(justifications, random);
        var presentation = new List<PresentedJustificationDefinition>(justifications.Count);
        for (int index = 0; index < justifications.Count; index++)
            presentation.Add(new PresentedJustificationDefinition(justifications[index].Id, index + 1));
        return presentation.AsReadOnly();
    }

    public InterventionSolutionDefinition GetSolution(string solutionId)
    {
        foreach (InterventionSolutionDefinition solution in InitialSolutions)
            if (solution.Id == solutionId)
                return solution;
        throw new InvalidOperationException(
            $"Solução de modelos conceituais não registrada: {solutionId}");
    }

    public InterventionJustificationDefinition GetJustification(string justificationId)
    {
        foreach (InterventionJustificationDefinition justification in Justifications)
            if (justification.Id == justificationId)
                return justification;
        throw new InvalidOperationException(
            $"Justificativa de modelos conceituais não registrada: {justificationId}");
    }

    public string GetResultAssessment(string solutionId) => solutionId switch
    {
        DifferentiateSolutionId => "Pista de aparência",
        RestoreSolutionId => "Consistência com o já conhecido",
        AnimateSolutionId => "Pista de movimento",
        _ => "Sem escolha"
    };

    public string GetResultExplanation(string solutionId) => solutionId switch
    {
        DifferentiateSolutionId =>
            "Diferenciar dá a cada comportamento a sua própria aparência: o que já foi aprendido " +
            "continua valendo, e as plataformas novas avisam que são outra coisa antes do contato. " +
            "É a proposta mais adequada neste cenário, porque permite prever o efeito e mantém a variedade da travessia.",
        RestoreSolutionId =>
            "Restaurar faz a aparência voltar a corresponder ao comportamento já conhecido, " +
            "então nada surpreende. Mas os novos comportamentos deixam de existir, " +
            "e a travessia perde as possibilidades que eles traziam.",
        AnimateSolutionId =>
            "Animar mostra o comportamento pelo movimento: a gelatina que impulsiona estica para cima " +
            "e a rocha que fere mais solta faíscas. Ajuda a prever o efeito, mas o aviso depende de " +
            "observar o movimento: parada, a plataforma continua igual à já conhecida.",
        _ => "Nenhuma mudança foi escolhida para alinhar a aparência ao comportamento das plataformas."
    };

    private static void Shuffle<T>(IList<T> entries, Random random)
    {
        for (int index = entries.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            (entries[index], entries[swapIndex]) = (entries[swapIndex], entries[index]);
        }
    }
}
