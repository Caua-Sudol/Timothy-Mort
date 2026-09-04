using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class ConceptualModelsConstraintsChoiceCatalog : IInterventionChoiceCatalog
{
    public static ConceptualModelsConstraintsChoiceCatalog Instance { get; } =
        new ConceptualModelsConstraintsChoiceCatalog();

    public string JustificationQuestion { get; } =
        "Que resultado você espera obter com essa intervenção?";

    public IReadOnlyList<InterventionSolutionDefinition> InitialInadequateSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "I1",
                "Realce uniforme",
                "Destacar todas as plataformas especiais das normais.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_GLOW |
                SolutionVisualModule.DANGER_PLATFORM_HIGHLIGHT),
            new InterventionSolutionDefinition(
                "I2",
                "Regras explícitas",
                "Exibir lembretes curtos com ícones de dano e impulso.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.CONCEPTUAL_RULE_REMINDERS),
            new InterventionSolutionDefinition(
                "I3",
                "Rota indicada",
                "Indicar visualmente a rota até a saída.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.PATH_INDICATOR)
        };

    public IReadOnlyList<InterventionSolutionDefinition> InitialPartialSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "P1",
                "Perigo consistente",
                "Fazer todos os espinhos causarem dano e realçar as plataformas especiais.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_HARMLESS_SPIKE_DAMAGE |
                SolutionVisualModule.GELATIN_GLOW |
                SolutionVisualModule.DANGER_PLATFORM_HIGHLIGHT),
            new InterventionSolutionDefinition(
                "P2",
                "Propriedade restauradas",
                "Todas as plataformas não sólidas recuperam a propriedade já vista e exibir lembretes de dano e impulso.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN |
                SolutionVisualModule.CONCEPTUAL_RULE_REMINDERS),
            new InterventionSolutionDefinition(
                "P3",
                "Perigo e rota",
                "Fazer todos os espinhos causarem dano e indicar a rota até a saída.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_HARMLESS_SPIKE_DAMAGE |
                SolutionVisualModule.PATH_INDICATOR)
        };

    public IReadOnlyList<InterventionSolutionDefinition> InitialAdequateSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "A1",
                "Plataformas restauradas",
                "Restaurar as propriedades já vistas para os perigos e plataformas não sólidas.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN |
                SolutionVisualModule.RESTORE_HARMLESS_SPIKE_DAMAGE),
            new InterventionSolutionDefinition(
                "A2",
                "Variantes diferenciadas",
                "Diferenciar as plataformas alteradas e ajustar suas propriedades.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.DISTINCT_TRAMPOLINE_GELATIN |
                SolutionVisualModule.TRAMPOLINE_DASH_EQUIVALENT |
                SolutionVisualModule.DISTINCT_HARMLESS_SPIKES |
                SolutionVisualModule.HARMLESS_SPIKE_DOUBLE_DAMAGE),
            new InterventionSolutionDefinition(
                "A3",
                "Propriedade antiga e novos perigos",
                "Propriedade de plataformas não sólidas restaurada e diferenciar visualmente novos perigos.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN |
                SolutionVisualModule.DISTINCT_HARMLESS_SPIKES |
                SolutionVisualModule.HARMLESS_SPIKE_DOUBLE_DAMAGE)
        };

    public IReadOnlyList<InterventionSolutionDefinition> InitialSolutions { get; }

    public IReadOnlyList<InterventionJustificationDefinition> Justifications { get; } =
        new List<InterventionJustificationDefinition>
        {
            new InterventionJustificationDefinition(
                "J1",
                "Facilitar a identificação das plataformas importantes para a travessia."),
            new InterventionJustificationDefinition(
                "J2",
                "Explicar diretamente quais recursos cada plataforma deveria oferecer."),
            new InterventionJustificationDefinition(
                "J3",
                "Indicar o percurso necessário para alcançar a saída."),
            new InterventionJustificationDefinition(
                "J4",
                "Fazer plataformas visualmente iguais manterem o mesmo comportamento."),
            new InterventionJustificationDefinition(
                "J5",
                "Diferenciar plataformas que possuem comportamentos diferentes."),
            new InterventionJustificationDefinition(
                "J6",
                "Alinhar aparência, comportamento e restrições do percurso.")
        };

    public IReadOnlyList<InterventionSolutionDefinition> AvailableRevisionSolutions
    { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "P4",
                "Plataformas originais",
                "Restaurar apenas as plataformas não sólidas ao comportamento original.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.REVISION,
                SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN),
            new InterventionSolutionDefinition(
                "A4",
                "Plataformas distinta",
                "Diferenciar plataformas com propriedades ainda não vistas.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.REVISION,
                SolutionVisualModule.DISTINCT_TRAMPOLINE_GELATIN |
                SolutionVisualModule.TRAMPOLINE_DASH_EQUIVALENT |
                SolutionVisualModule.RESTORE_HARMLESS_SPIKE_DAMAGE)
        };

    private ConceptualModelsConstraintsChoiceCatalog()
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
            new List<PresentedJustificationDefinition>(
                shuffledJustifications.Count);

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
            $"Solução de modelos conceituais não registrada: {solutionId}");
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
            $"Justificativa de modelos conceituais não registrada: {justificationId}");
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
