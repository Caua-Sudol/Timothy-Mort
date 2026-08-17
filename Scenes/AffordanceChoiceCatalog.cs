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
                "Orientar o percurso",
                "Adicionar indicadores de caminho e destacar o destino.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.PATH_INDICATOR |
                SolutionVisualModule.DESTINATION_HIGHLIGHT),
            new InterventionSolutionDefinition(
                "I2",
                "Reforcar a apresentacao do movimento",
                "Aplicar a mesma decoracao a solidos e gelatinas e exibir um rastro durante todo o dash.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.PLATFORM_DECORATION |
                SolutionVisualModule.PLAYER_DASH_TRAIL),
            new InterventionSolutionDefinition(
                "I3",
                "Reforcar o contato com o cenario",
                "Aplicar o mesmo contorno a solidos e gelatinas e exibir o mesmo flash no contato com qualquer superficie.",
                SolutionQuality.INADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.PLATFORM_OUTLINE |
                SolutionVisualModule.PLAYER_SURFACE_CONTACT_FLASH)
        };

    public static IReadOnlyList<InterventionSolutionDefinition> InitialSolutions { get; } =
        new List<InterventionSolutionDefinition>
        {
            InitialInadequateSolutions[2],
            new InterventionSolutionDefinition(
                "diferenciar_por_cor",
                "Diferenciar as superficies especiais",
                "Aplicar uma cor exclusiva as superficies que possuem a propriedade.",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_COLOR),
            new InterventionSolutionDefinition(
                "textura_e_feedback_de_recarga",
                "Diferenciar e mostrar a recuperacao",
                "Aplicar textura as superficies e restaurar a cor do jogador ao recuperar o dash.",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.GELATIN_TEXTURE |
                SolutionVisualModule.PLAYER_DASH_RECOVERY_COLOR)
        };

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

    public static InterventionSolutionDefinition GetSolution(string solutionId)
    {
        foreach (InterventionSolutionDefinition solution in InitialInadequateSolutions)
        {
            if (solution.Id == solutionId)
            {
                return solution;
            }
        }

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
