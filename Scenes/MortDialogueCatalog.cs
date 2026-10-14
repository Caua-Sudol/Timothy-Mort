namespace DontLikePoetry;

public static class MortDialogueCatalog
{
    public const double PageDurationSeconds = 7.0;

    public const string Passage =
        "Está vendo os símbolos flutuando? Aproxime-se e pressione E. " +
        "Vou te levar a um lugar onde podemos facilitar a travessia.";

    public static readonly string[] Introduction =
    {
        "Cuidado, garoto! Não dá para entender o que cada coisa faz só pela aparência.",
        "Vou te ajudar: use LEFT SHIFT mais uma vez ao encostar nos terrenos estranhos."
    };

    public static string AfterChoice(string solutionId) => solutionId switch
    {
        AffordanceChoiceCatalog.AppearanceSolutionId =>
            "Pronto, mudei a aparência. Experimente e veja se ela combina com o que o terreno faz.",
        AffordanceChoiceCatalog.MarkSolutionId =>
            "Agora eles têm uma marca. Ela mostra quais são diferentes... mas conta o que fazem?",
        AffordanceChoiceCatalog.MotionSolutionId =>
            "Agora eles se mexem, mesmo sem você tocar. Que comportamento esse movimento sugere?",
        _ => "Experimente. Se quiser comparar, volte aqui e escolha outra alteração."
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
        _ => "Podemos combinar ideias para dar pistas sobre o terreno. " +
             "Vou mostrar a aparência de gelatina junto com a pulsação!"
    };
}
