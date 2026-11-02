using System;
using System.Collections.Generic;

namespace DontLikePoetry;

// Phase 3 (Conceptual models): some gelatins and hot rocks look like the ones already learned
// but act differently (impostors). The three alternatives change how that mismatch is resolved.
public sealed class ConceptualModelsConstraintsChoiceCatalog : IInterventionChoiceCatalog
{
    public const string DifferentiateSolutionId = "concept_differentiate";
    public const string RestoreSolutionId = "concept_restore";
    public const string NeutralizeSolutionId = "concept_neutralize";

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
                NeutralizeSolutionId,
                "Neutralizar",
                "Transformar as plataformas que agem diferente em plataformas sólidas comuns",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.IMPOSTOR_NEUTRALIZE)
        });

    // The choice can be changed inside the intervention room; there is no revision round.
    public IReadOnlyList<InterventionSolutionDefinition> AvailableRevisionSolutions =>
        InitialSolutions;

    public IReadOnlyList<InterventionJustificationDefinition> Justifications { get; } =
        Array.AsReadOnly(new[]
        {
            new InterventionJustificationDefinition(
                "concept_predict", "Prever o que cada plataforma faz antes de tocá-la"),
            new InterventionJustificationDefinition(
                "concept_same_look", "Fazer plataformas iguais se comportarem do mesmo jeito"),
            new InterventionJustificationDefinition(
                "concept_keep_new", "Manter os novos comportamentos disponíveis na travessia"),
            new InterventionJustificationDefinition(
                "concept_learned", "Aproveitar o que já foi aprendido nas fases anteriores"),
            new InterventionJustificationDefinition(
                "concept_surprise", "Evitar surpresas ao pisar nas plataformas"),
            new InterventionJustificationDefinition(
                "concept_fewer_rules", "Reduzir a quantidade de regras para lembrar")
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
        NeutralizeSolutionId => "Remoção do comportamento",
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
        NeutralizeSolutionId =>
            "Neutralizar troca as plataformas que enganavam por plataformas sólidas comuns. " +
            "O engano desaparece, mas junto com ele somem tanto os efeitos novos quanto os conhecidos " +
            "nesses pontos: o problema é evitado, e não comunicado.",
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
