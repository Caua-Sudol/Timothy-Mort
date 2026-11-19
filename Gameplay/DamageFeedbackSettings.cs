namespace DontLikePoetry;

public static class DamageFeedbackSettings
{
    public const int MaximumHealth = 5;

    public const float KnockbackHorizontalSpeed = 420f;
    public const float KnockbackVerticalSpeed = -460f;

    public const double BlinkIntervalSeconds = 0.12;

    // Phase 2 hot rocks: one heart every 1 s of contact (hidden invulnerability between hits).
    // 0.6 s proved too short in the student build.
    public const double HotSurfaceDamageIntervalSeconds = 1.0;
    // Recuo: the push plays without player input for a short moment.
    public const double KnockbackControlLockSeconds = 0.2;
    public const float HotSurfaceKnockbackHorizontalSpeed = 600f;
    public const float HotSurfaceKnockbackVerticalSpeed = -300f;
    public const float WallKnockbackVerticalSpeed = -300f;
    public const float CeilingKnockbackVerticalSpeed = 300f;
    public const double HeartCrackSeconds = 0.7;
    public const double ScreenShakeSeconds = 0.35;
    public const float ScreenShakePixels = 10f;
}
