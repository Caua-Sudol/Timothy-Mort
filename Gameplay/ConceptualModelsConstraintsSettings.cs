namespace DontLikePoetry;

public static class ConceptualModelsConstraintsSettings
{
    public const int MaximumHealth = 5;
    public const double InvulnerabilityDurationSeconds = 1.5;
    // Mini-jump: about twice the normal jump height, since it does not refill the dash.
    public const float TrampolineVerticalSpeed = 880f;
    public const float TrampolineDashEquivalentVerticalSpeed = Player.DashSpeed;

    public const SolutionVisualModule BaseVisualModules =
        SolutionVisualModule.GELATIN_TEXTURE |
        SolutionVisualModule.PLAYER_DASH_RECOVERY_COLOR |
        SolutionVisualModule.DAMAGE_KNOCKBACK |
        SolutionVisualModule.INVULNERABILITY_BLINK;
}
