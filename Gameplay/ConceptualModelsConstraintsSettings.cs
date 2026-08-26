namespace DontLikePoetry;

public static class ConceptualModelsConstraintsSettings
{
    public const int MaximumHealth = 5;
    public const int ExitHealthThreshold = 2;
    public const double InvulnerabilityDurationSeconds = 1.5;
    public const float TrampolineVerticalSpeed = 360f;

    public const SolutionVisualModule BaseVisualModules =
        SolutionVisualModule.GELATIN_TEXTURE |
        SolutionVisualModule.PLAYER_DASH_RECOVERY_COLOR |
        SolutionVisualModule.DAMAGE_KNOCKBACK |
        SolutionVisualModule.INVULNERABILITY_BLINK;
}
