namespace DontLikePoetry;

public static class MortDialogueCatalog
{
    public const double PageDurationSeconds = 7.0;

    public const string Passage =
        "Está vendo os símbolos flutuando? Aproxime-se e pressione E. " +
        "Vou te levar a um lugar onde podemos facilitar a travessia.";

    // Short: shown in the balloon when the player tries the blocked passage.
    public const string Room3Barrier =
        "Ainda não dá para seguir. Precisamos passar na sala da intervenção!";

    // Intervention room exit touched before any solution was applied.
    public const string InterventionExitBlocked = "Antes de sair, use a espada.";

    // The dash tip moved to the tutorial and to the gelatin balloon below.
    public static readonly string[] Introduction =
    {
        "Cuidado, garoto! Não dá para entender o que cada coisa faz só pela aparência."
    };

    // Feedback has no subtitle introduction: Mort speaks in a short balloon instead,
    // on the first hot-rock hit, or on entering room 2 if the player was never hit.
    // Affordance room 1: short balloon on the first touch of any gelatin, before the choice.
    public const string AffordanceFirstGelatinCallout =
        "Que terreno estranho... Encoste nele e tente o impulso de novo!";

    public const string FeedbackFirstDamageCallout = "Ei! Você se feriu e nada mostrou isso!";
    public const string FeedbackRoom2Callout = "Essas rochas ferem, e nada avisa. Precisamos arrumar isso!";

    public static string[] GetIntroduction(string phaseId) => phaseId switch
    {
        ScenarioCatalog.AffordanceVisibilityScenarioId => Introduction,
        _ => null
    };

    public static string AfterChoice(string solutionId) => solutionId switch
    {
        AffordanceChoiceCatalog.AppearanceSolutionId =>
            "Pronto, mudei a aparência. Experimente e veja se ela combina com o que o terreno faz.",
        AffordanceChoiceCatalog.MarkSolutionId =>
            "Agora eles têm uma marca. Ela mostra quais são diferentes... mas conta o que fazem?",
        AffordanceChoiceCatalog.MotionSolutionId =>
            "Agora eles se mexem, mesmo sem você tocar. Que comportamento esse movimento sugere?",
        FeedbackDamageChoiceCatalog.KnockbackSolutionId =>
            "Pronto! Agora o calor te empurra para longe. Encoste numa rocha quente e sinta a diferença.",
        FeedbackDamageChoiceCatalog.HeartCrackSolutionId =>
            "Agora um coração racha sobre você quando se fere. Ele mostra a perda... mas mostra de onde ela veio?",
        FeedbackDamageChoiceCatalog.ScreenShakeSolutionId =>
            "Agora a tela treme quando você se fere. Chama atenção... mas aponta o que causou o dano?",
        ConceptualModelsConstraintsChoiceCatalog.DifferentiateSolutionId =>
            "Pronto, as diferentes ganharam outra cor. Agora dá para saber o que esperar antes de pisar.",
        ConceptualModelsConstraintsChoiceCatalog.RestoreSolutionId =>
            "Agora todas fazem o que você já conhecia. Nada de surpresas... mas o que perdemos?",
        ConceptualModelsConstraintsChoiceCatalog.AnimateSolutionId =>
            "Agora as diferentes se mexem do jeito que agem. Repare no movimento antes de pisar.",
        _ => "Experimente e veja se ajuda."
    };

    public static string Conclusion(string solutionId) => solutionId switch
    {
        AffordanceChoiceCatalog.MarkSolutionId =>
            "Sua marca identifica os terrenos, mas é preciso tocá-los para aprender. " +
            "A gelatina pode sugerir melhor seu comportamento. Vamos combiná-la com a pulsação!",
        AffordanceChoiceCatalog.MotionSolutionId =>
            "O movimento mostra que há algo diferente. Vamos combinar essa pulsação " +
            "com a aparência de gelatina para dar mais uma pista!",
        AffordanceChoiceCatalog.AppearanceSolutionId =>
            "A gelatina já dá uma pista sobre o terreno. Vamos acrescentar a pulsação " +
            "para mostrar como podemos combinar ideias!",
        // The demonstration only runs on the best choice, so phase 2 needs only this one.
        FeedbackDamageChoiceCatalog.KnockbackSolutionId =>
            "O recuo já mostra onde e quando você se feriu. Vamos somar o coração rachando " +
            "para mostrar também o quanto perdeu!",
        _ => "Podemos combinar ideias para dar pistas sobre o terreno. " +
             "Vou mostrar a aparência de gelatina junto com a pulsação!"
    };
}
