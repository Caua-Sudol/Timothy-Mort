using System;
using System.Collections.Generic;

namespace DontLikePoetry;

// Phase 2 (Feedback): hot rocks hurt without any sign. The three alternatives only change
// how the damage is communicated; the damage itself stays the same.
public sealed class FeedbackDamageChoiceCatalog : IInterventionChoiceCatalog
{
    public const string KnockbackSolutionId = "damage_knockback";
    public const string HeartCrackSolutionId = "damage_heart_crack";
    public const string ScreenShakeSolutionId = "damage_screen_shake";

    public static FeedbackDamageChoiceCatalog Instance { get; } =
        new FeedbackDamageChoiceCatalog();

    public string SolutionQuestion =>
        "Qual reação ajuda você a perceber melhor que o personagem se feriu?";

    public string JustificationQuestion =>
        "Que resultado você espera obter com essa intervenção?";

    public IReadOnlyList<InterventionSolutionDefinition> InitialSolutions { get; } =
        Array.AsReadOnly(new[]
        {
            new InterventionSolutionDefinition(
                KnockbackSolutionId,
                "Recuo",
                "Empurrar o personagem para longe da superfície ao se ferir",
                SolutionQuality.ADEQUATE,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.DAMAGE_KNOCKBACK),
            new InterventionSolutionDefinition(
                HeartCrackSolutionId,
                "Coração rachando",
                "Mostrar um coração rachando sobre o personagem ao perder vida",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.DAMAGE_HEART_CRACK),
            new InterventionSolutionDefinition(
                ScreenShakeSolutionId,
                "Tela tremendo",
                "Tremer a tela e avermelhar as bordas ao se ferir",
                SolutionQuality.PARTIAL,
                SolutionChoiceRound.INITIAL,
                SolutionVisualModule.DAMAGE_SCREEN_SHAKE)
        });

    // The choice can be changed inside the intervention room; there is no revision round.
    public IReadOnlyList<InterventionJustificationDefinition> Justifications { get; } =
        Array.AsReadOnly(new[]
        {
            // Three options (expert test): one per kind of cue the solutions give.
            new InterventionJustificationDefinition(
                "damage_source", "Relacionar o dano à superfície que o causou"),
            new InterventionJustificationDefinition(
                "damage_amount", "Saber quanta vida foi perdida"),
            new InterventionJustificationDefinition(
                "damage_attention", "Chamar atenção para o dano, mesmo olhando para outro ponto da tela")
        });

    private FeedbackDamageChoiceCatalog()
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
        throw new InvalidOperationException($"Solução de feedback não registrada: {solutionId}");
    }

    public InterventionJustificationDefinition GetJustification(string justificationId)
    {
        foreach (InterventionJustificationDefinition justification in Justifications)
            if (justification.Id == justificationId)
                return justification;
        throw new InvalidOperationException(
            $"Justificativa de feedback não registrada: {justificationId}");
    }

    public string GetResultAssessment(string solutionId) => solutionId switch
    {
        KnockbackSolutionId => "Pista de contato e direção",
        HeartCrackSolutionId => "Pista de perda de vida",
        ScreenShakeSolutionId => "Pista de alerta geral",
        _ => "Sem escolha"
    };

    public string GetResultExplanation(string solutionId) => solutionId switch
    {
        KnockbackSolutionId =>
            "O recuo liga o dano ao contato: o personagem é afastado da superfície que o feriu, " +
            "no instante e na direção do impacto. É a proposta mais adequada neste cenário, " +
            "porque mostra o que aconteceu e onde, e ainda ajuda a sair do perigo.",
        HeartCrackSolutionId =>
            "O coração rachando mostra que houve perda de vida, perto de onde o jogador está olhando. " +
            "Mas não indica qual superfície causou o dano nem ajuda a se afastar dela.",
        ScreenShakeSolutionId =>
            "A tela tremendo chama atenção para o dano de forma intensa, mas não aponta a origem " +
            "nem a direção do impacto. Repetida com frequência, pode cansar e atrapalhar a leitura da cena.",
        _ => "Nenhuma mudança foi escolhida para comunicar o dano recebido."
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
