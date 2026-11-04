namespace DontLikePoetry;

public enum ControlScheme
{
    SHIFT = 0,
    ARROWS = 1,
    RIGHT_HAND = 2
}

// Key names shown on the tutorial background. Texts in LDtk use placeholders such as
// "{cima} + {dash}"; every key keeps working in play, the scheme only chooses what is shown.
public static class TutorialControls
{
    // Kept for the whole session (static), chosen in the tutorial's control menu.
    // Starts on the most familiar layout (A/D, Space, Shift); the "Controles" sign changes it.
    public static ControlScheme Current { get; set; } = ControlScheme.SHIFT;

    private static (string Walk, string Jump, string Dash, string Up, string Right, string Left) Keys(
        ControlScheme scheme) => scheme switch
    {
        // The font has no arrow glyphs, so arrow keys are written as words.
        ControlScheme.ARROWS => ("Setas", "C", "X", "Cima", "Direita", "Esquerda"),
        ControlScheme.RIGHT_HAND => ("A / D", "K", "J", "W", "D", "A"),
        _ => ("A / D", "Espaço", "Shift", "W", "D", "A")
    };

    public static string Describe(ControlScheme scheme)
    {
        var keys = Keys(scheme);
        return $"{keys.Walk} anda   ·   {keys.Jump} pula   ·   {keys.Dash} impulso";
    }

    // Keys come out in brackets ("[Shift]") so they stand out in the tutorial texts.
    public static string Fill(string text)
    {
        var keys = Keys(Current);
        return text
            .Replace("{andar}", $"[{keys.Walk}]")
            .Replace("{pular}", $"[{keys.Jump}]")
            .Replace("{dash}", $"[{keys.Dash}]")
            .Replace("{cima}", $"[{keys.Up}]")
            .Replace("{direita}", $"[{keys.Right}]")
            .Replace("{esquerda}", $"[{keys.Left}]");
    }
}
