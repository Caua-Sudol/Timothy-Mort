namespace DontLikePoetry;

public static class MortDialogueCatalog
{
    public const double PageDurationSeconds = 7.0;

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

    public const string Conclusion =
        "Não precisamos parar em uma só ideia, garoto. Que tal combinar a aparência de gelatina com a pulsação? Vou te mostrar!";
}
