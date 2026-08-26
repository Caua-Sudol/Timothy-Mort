using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class ConceptualModelsConstraintsChoiceCatalog : IInterventionChoiceCatalog
{
    private static readonly IReadOnlyList<InterventionSolutionDefinition>
        NoRevisionSolutions = Array.Empty<InterventionSolutionDefinition>();

    private static readonly IReadOnlyList<PresentedSolutionDefinition>
        FixedInitialSolutionPresentation =
            new List<PresentedSolutionDefinition>
            {
                new PresentedSolutionDefinition("A1", 1),
                new PresentedSolutionDefinition("A2", 2),
                new PresentedSolutionDefinition("A3", 3)
            }.AsReadOnly();

    private static readonly IReadOnlyList<PresentedJustificationDefinition>
        FixedInitialJustificationPresentation =
            new List<PresentedJustificationDefinition>
            {
                new PresentedJustificationDefinition("J1", 1),
                new PresentedJustificationDefinition("J2", 2),
                new PresentedJustificationDefinition("J3", 3),
                new PresentedJustificationDefinition("J4", 4),
                new PresentedJustificationDefinition("J5", 5),
                new PresentedJustificationDefinition("J6", 6)
            }.AsReadOnly();

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
                "Destacar igualmente todas as gelatinas e plataformas de espinhos.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_GLOW |
                SolutionVisualModule.DANGER_PLATFORM_HIGHLIGHT),
            new InterventionSolutionDefinition(
                "I2",
                "Regras explícitas",
                "Exibir lembretes curtos com ícones de DANO e DASH.",
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
                "Espinhos ativos",
                "Fazer todos os espinhos causarem dano e realçar as plataformas especiais.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_HARMLESS_SPIKE_DAMAGE |
                SolutionVisualModule.GELATIN_GLOW |
                SolutionVisualModule.DANGER_PLATFORM_HIGHLIGHT),
            new InterventionSolutionDefinition(
                "P2",
                "Gelatinas restauradas",
                "Restaurar as gelatinas e exibir lembretes de DANO e DASH.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN |
                SolutionVisualModule.CONCEPTUAL_RULE_REMINDERS),
            new InterventionSolutionDefinition(
                "P3",
                "Perigo consistente",
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
                "Restaurar as propriedades originais de gelatinas e espinhos.",
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
                "Gelatina restaurada",
                "Restaurar as gelatinas e diferenciar os espinhos alterados.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN |
                SolutionVisualModule.DISTINCT_HARMLESS_SPIKES |
                SolutionVisualModule.HARMLESS_SPIKE_DOUBLE_DAMAGE)
        };

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
    {
        get { return NoRevisionSolutions; }
    }

    private ConceptualModelsConstraintsChoiceCatalog()
    {
    }

    public IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        return FixedInitialSolutionPresentation;
    }

    public IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        return FixedInitialJustificationPresentation;
    }

    public InterventionSolutionDefinition GetSolution(string solutionId)
    {
        foreach (InterventionSolutionDefinition solution in InitialInadequateSolutions)
        {
            if (solution.Id == solutionId)
            {
                return solution;
            }
        }

        foreach (InterventionSolutionDefinition solution in InitialPartialSolutions)
        {
            if (solution.Id == solutionId)
            {
                return solution;
            }
        }

        foreach (InterventionSolutionDefinition solution in InitialAdequateSolutions)
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
}
