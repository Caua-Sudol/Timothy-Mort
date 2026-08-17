using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public static class AffordanceChoiceCatalog
{
    public const string JustificationQuestion = "O que esta alteracao pretende resolver?";

    public static IReadOnlyList<InterventionSolutionDefinition> InitialInadequateSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "I1",
                "Percurso",
                "Adicionar indicadores de caminho e destacar o destino.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.PATH_INDICATOR |
                SolutionVisualModule.DESTINATION_HIGHLIGHT),
            new InterventionSolutionDefinition(
                "I2",
                "Apresentacao",
                "Aplicar decoracao nas plataformas e exibir um rastro visual para indicar o dash.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.PLATFORM_DECORATION |
                SolutionVisualModule.PLAYER_DASH_TRAIL),
            new InterventionSolutionDefinition(
                "I3",
                "Contato",
                "Adicionar contorno em plataformas e exibir flash de impacto.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.PLATFORM_OUTLINE |
                SolutionVisualModule.PLAYER_SURFACE_CONTACT_FLASH)
        };

    public static IReadOnlyList<InterventionSolutionDefinition> InitialPartialSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "diferenciar_por_cor",
                "Diferenciar as superficies especiais",
                "Aplicar uma cor exclusiva as superficies que possuem a propriedade.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_COLOR)
        };

    public static IReadOnlyList<InterventionSolutionDefinition> InitialAdequateSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "textura_e_feedback_de_recarga",
                "Diferenciar e mostrar a recuperacao",
                "Aplicar textura as superficies e restaurar a cor do jogador ao recuperar o dash.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_TEXTURE |
                SolutionVisualModule.PLAYER_DASH_RECOVERY_COLOR)
        };

    public static IReadOnlyList<InterventionSolutionDefinition> InitialSolutions { get; } =
        CreateInitialSolutions();

    public static IReadOnlyList<InterventionSolutionDefinition> RevisionSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            new InterventionSolutionDefinition(
                "diferenciar_por_textura",
                "Aplicar uma textura exclusiva",
                "Usar uma textura diferente apenas nas superficies especiais.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.REVISION,
                SolutionVisualModule.GELATIN_TEXTURE),
            new InterventionSolutionDefinition(
                "diferenciar_por_brilho",
                "Adicionar brilho as superficies",
                "Aplicar um brilho apenas nas superficies especiais.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.REVISION,
                SolutionVisualModule.GELATIN_GLOW),
            new InterventionSolutionDefinition(
                "brilho_e_feedback_de_recarga",
                "Destacar e confirmar a recuperacao",
                "Combinar brilho nas superficies com a recuperacao da cor do jogador.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.REVISION,
                SolutionVisualModule.GELATIN_GLOW |
                SolutionVisualModule.PLAYER_DASH_RECOVERY_COLOR)
        };

    public static IReadOnlyList<InterventionJustificationDefinition> Justifications { get; } =
        new List<InterventionJustificationDefinition>
        {
            new InterventionJustificationDefinition(
                "embelezar_cenario",
                "Deixar o cenario mais bonito."),
            new InterventionJustificationDefinition(
                "chamar_atencao_regiao",
                "Chamar atencao para aquela regiao."),
            new InterventionJustificationDefinition(
                "mostrar_propriedade_diferente",
                "Mostrar que determinadas superficies possuem uma propriedade diferente."),
            new InterventionJustificationDefinition(
                "indicar_comportamento_especial",
                "Indicar que existe uma acao ou comportamento especial associado aquelas superficies."),
            new InterventionJustificationDefinition(
                "indicar_como_usar_acao",
                "Indicar como essa acao pode ser utilizada."),
            new InterventionJustificationDefinition(
                "evidenciar_caminho_objetivo",
                "Tornar o caminho ate o objetivo mais evidente.")
        };

    public static IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
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

        for (int index = selectedSolutions.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            InterventionSolutionDefinition temporary = selectedSolutions[index];
            selectedSolutions[index] = selectedSolutions[swapIndex];
            selectedSolutions[swapIndex] = temporary;
        }

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

    public static InterventionSolutionDefinition GetSolution(string solutionId)
    {
        foreach (InterventionSolutionDefinition solution in InitialSolutions)
        {
            if (solution.Id == solutionId)
            {
                return solution;
            }
        }

        foreach (InterventionSolutionDefinition solution in RevisionSolutions)
        {
            if (solution.Id == solutionId)
            {
                return solution;
            }
        }

        throw new InvalidOperationException($"Solucao nao registrada: {solutionId}");
    }

    private static IReadOnlyList<InterventionSolutionDefinition> CreateInitialSolutions()
    {
        var solutions = new List<InterventionSolutionDefinition>(
            InitialInadequateSolutions.Count +
            InitialPartialSolutions.Count +
            InitialAdequateSolutions.Count);

        foreach (InterventionSolutionDefinition solution in InitialInadequateSolutions)
        {
            solutions.Add(solution);
        }

        foreach (InterventionSolutionDefinition solution in InitialPartialSolutions)
        {
            solutions.Add(solution);
        }

        foreach (InterventionSolutionDefinition solution in InitialAdequateSolutions)
        {
            solutions.Add(solution);
        }

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
                $"O pool inicial {qualityName} nao possui solucoes.");
        }

        return pool[random.Next(pool.Count)];
    }

    public static InterventionJustificationDefinition GetJustification(string justificationId)
    {
        foreach (InterventionJustificationDefinition justification in Justifications)
        {
            if (justification.Id == justificationId)
            {
                return justification;
            }
        }

        throw new InvalidOperationException($"Justificativa nao registrada: {justificationId}");
    }
}
