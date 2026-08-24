using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class FeedbackDamageChoiceCatalog : IInterventionChoiceCatalog
{
    public static FeedbackDamageChoiceCatalog Instance { get; } =
        new FeedbackDamageChoiceCatalog();

    public string JustificationQuestion { get; } =
        "Que resultado você espera obter com essa intervenção?";

    public IReadOnlyList<InterventionSolutionDefinition> InitialInadequateSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "I1",
                "HUD reforçado",
                "Aumentar os corações e diferenciar com mais contraste os corações cheios e vazios.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE),
            new InterventionSolutionDefinition(
                "I2",
                "Perigo sinalizado",
                "Aplicar uma cor de perigo e adicionar símbolos de alerta às plataformas com espinhos.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE),
            new InterventionSolutionDefinition(
                "I3",
                "Rota alternativa",
                "Criar plataformas seguras próximas às perigosas e destacá-las como uma rota segura.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE)
        };

    public IReadOnlyList<InterventionSolutionDefinition> InitialPartialSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "P1",
                "Impacto físico",
                "Aplicar um recuo pequeno e partículas no ponto em que o personagem recebe dano.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE),
            new InterventionSolutionDefinition(
                "P2",
                "Proteção visível",
                "Fazer o personagem piscar e exibir um contorno enquanto estiver invulnerável.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE),
            new InterventionSolutionDefinition(
                "P3",
                "Recuo e alerta",
                "Aplicar um recuo pequeno e destacar visualmente as plataformas perigosas.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE)
        };

    public IReadOnlyList<InterventionSolutionDefinition> InitialAdequateSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "A1",
                "Recuo e piscar",
                "Aplicar um recuo pequeno e fazer o personagem piscar durante toda a invulnerabilidade.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE),
            new InterventionSolutionDefinition(
                "A2",
                "Recuo e pulso",
                "Aplicar um recuo pequeno e pulsar o contorno do personagem enquanto a proteção estiver ativa.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE),
            new InterventionSolutionDefinition(
                "A3",
                "Compressão e transparência",
                "Comprimir o personagem durante o recuo e alternar sua transparência durante a invulnerabilidade.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.NONE)
        };

    public IReadOnlyList<InterventionSolutionDefinition> InitialSolutions { get; }

    public IReadOnlyList<InterventionSolutionDefinition> AvailableRevisionSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "P4",
                "Reação de impacto",
                "Aplicar um recuo pequeno e manter brevemente uma marca visual no ponto de contato.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.REVISION,
                SolutionVisualModule.NONE),
            new InterventionSolutionDefinition(
                "A4",
                "Impacto e proteção",
                "Aplicar um recuo pequeno e exibir uma silhueta intermitente durante toda a invulnerabilidade.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.REVISION,
                SolutionVisualModule.NONE)
        };

    public IReadOnlyList<InterventionJustificationDefinition> Justifications { get; } =
        new List<InterventionJustificationDefinition>
        {
            new InterventionJustificationDefinition(
                "J1",
                "Facilitar a leitura da quantidade de vida que ainda resta ao personagem."),
            new InterventionJustificationDefinition(
                "J2",
                "Permitir reconhecer as plataformas perigosas antes de entrar em contato com elas."),
            new InterventionJustificationDefinition(
                "J3",
                "Oferecer uma rota segura para atravessar a área sem tocar nas plataformas perigosas."),
            new InterventionJustificationDefinition(
                "J4",
                "Tornar perceptíveis o instante e a direção em que o personagem recebe dano."),
            new InterventionJustificationDefinition(
                "J5",
                "Indicar enquanto o personagem está temporariamente protegido contra novos danos."),
            new InterventionJustificationDefinition(
                "J6",
                "Relacionar o contato à perda de vida e indicar enquanto a proteção permanece ativa.")
        };

    private FeedbackDamageChoiceCatalog()
    {
        InitialSolutions = CreateInitialSolutions();
    }

    public IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var selectedSolutions = new List<InterventionSolutionDefinition>
        {
            PickSolution(InitialInadequateSolutions, random, "inadequado"),
            PickSolution(InitialPartialSolutions, random, "parcial"),
            PickSolution(InitialAdequateSolutions, random, "adequado")
        };

        Shuffle(selectedSolutions, random);

        var presentation =
            new List<PresentedSolutionDefinition>(selectedSolutions.Count);

        for (int index = 0; index < selectedSolutions.Count; index++)
        {
            presentation.Add(new PresentedSolutionDefinition(
                selectedSolutions[index].Id,
                index + 1));
        }

        return presentation.AsReadOnly();
    }

    public IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var shuffledJustifications =
            new List<InterventionJustificationDefinition>(Justifications);

        Shuffle(shuffledJustifications, random);

        var presentation =
            new List<PresentedJustificationDefinition>(shuffledJustifications.Count);

        for (int index = 0; index < shuffledJustifications.Count; index++)
        {
            presentation.Add(new PresentedJustificationDefinition(
                shuffledJustifications[index].Id,
                index + 1));
        }

        return presentation.AsReadOnly();
    }

    public InterventionSolutionDefinition GetSolution(string solutionId)
    {
        foreach (InterventionSolutionDefinition solution in InitialSolutions)
        {
            if (solution.Id == solutionId)
            {
                return solution;
            }
        }

        foreach (InterventionSolutionDefinition solution in AvailableRevisionSolutions)
        {
            if (solution.Id == solutionId)
            {
                return solution;
            }
        }

        throw new InvalidOperationException(
            $"Solução de feedback não registrada: {solutionId}");
    }

    public InterventionJustificationDefinition GetJustification(
        string justificationId)
    {
        foreach (InterventionJustificationDefinition justification in Justifications)
        {
            if (justification.Id == justificationId)
            {
                return justification;
            }
        }

        throw new InvalidOperationException(
            $"Justificativa de feedback não registrada: {justificationId}");
    }

    private IReadOnlyList<InterventionSolutionDefinition> CreateInitialSolutions()
    {
        var solutions = new List<InterventionSolutionDefinition>(
            InitialInadequateSolutions.Count +
            InitialPartialSolutions.Count +
            InitialAdequateSolutions.Count);

        solutions.AddRange(InitialInadequateSolutions);
        solutions.AddRange(InitialPartialSolutions);
        solutions.AddRange(InitialAdequateSolutions);

        return solutions.AsReadOnly();
    }

    private static InterventionSolutionDefinition PickSolution(
        IReadOnlyList<InterventionSolutionDefinition> pool,
        Random random,
        string qualityName)
    {
        if (pool == null || pool.Count == 0)
        {
            throw new InvalidOperationException(
                $"O pool inicial {qualityName} não possui soluções.");
        }

        return pool[random.Next(pool.Count)];
    }

    private static void Shuffle<T>(IList<T> entries, Random random)
    {
        for (int index = entries.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            T temporary = entries[index];
            entries[index] = entries[swapIndex];
            entries[swapIndex] = temporary;
        }
    }
}
