namespace DontLikePoetry;

public static class ConceptualModelsConstraintsSettings
{
    public const int MaximumHealth = 5;
    // Was 1.5 s (walked across the fire), then 0.6 s (too short).
    public const double InvulnerabilityDurationSeconds = 1.0;
    // Mini-jump: about twice the normal jump height, since it does not refill the dash.
    public const float TrampolineVerticalSpeed = 880f;

    // Fixed damage feedback from the start: recuo + coração rachando on hot rocks;
    // the impostor rock swaps the heart for screen shake (see Scene).
    public const SolutionVisualModule BaseVisualModules =
        SolutionVisualModule.DAMAGE_KNOCKBACK |
        SolutionVisualModule.DAMAGE_HEART_CRACK;
}
