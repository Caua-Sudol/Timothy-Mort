namespace DontLikePoetry;

// Key names shown on the tutorial background. Texts in LDtk use placeholders such as
// "{cima} + {dash}"; every key keeps working in play, this only chooses what is shown.
public static class TutorialControls
{
    public static string Fill(string text) => text
        .Replace("{andar}", "A / D")
        .Replace("{pular}", "Espaço")
        .Replace("{dash}", "Shift")
        .Replace("{cima}", "W")
        .Replace("{direita}", "D")
        .Replace("{esquerda}", "A");
}
