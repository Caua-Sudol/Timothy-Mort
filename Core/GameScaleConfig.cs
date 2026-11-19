namespace DontLikePoetry;

// Scale comparison is limited to AffordanceVisibilidade and the isolated test room.
internal static class GameScaleConfig
{
    public const float OriginalCameraZoom = 1.0f;
    public const float CameraPlayerRatioSmall = 0.11f;
    public const float CameraPlayerRatioMedium = 0.125f;
    public const float CameraPlayerRatioLarge = 0.14f;
    public const int DefaultCameraPreset = 3;

    public const float PlayerScaleOriginal = 1.0f;
    public const float PlayerScaleMedium = 0.85f;
    public const float PlayerScaleSmall = 0.75f;
    public const float PlayerScaleSmallest = 0.65f;
    public const int DefaultPlayerScalePreset = 1;
    // Original 2240px canvas drawn at 0.1 scale; independent of compiled resolution.
    public const float PlayerSpriteReferenceHeight = 224f;

    // Fixed body collider: about 32 x 77 screen pixels at the default 10.5% player size
    // in a 1920 x 1080 viewport (scale 0.75, camera zoom 0.675).
    public const int PlayerReferenceColliderWidth = 63;
    public const int PlayerReferenceColliderHeight = 152;

    public const string ScaleTestScenarioId = "scale_test";
    public const int ScaleTestHudMargin = 16;

    // Baseline at scale 1. Spatial rates scale with the player; times stay fixed.
    public const double PlayerGravity = -1800.0;
    public const float PlayerHorizontalSpeed = 528f;
    public const float PlayerJumpSpeed = 620f;
    public const float PlayerGroundAcceleration = 3300f;
    public const float PlayerGroundDeceleration = 3300f;
    public const float PlayerAirAcceleration = 1800f;
    public const float PlayerAirBrakingAcceleration = 4800f;
    public const float PlayerReleasedJumpSpeed = 240f;
    public const float PlayerDashReferenceSpeed = 960f;
    public const float PlayerDashSpeed = PlayerDashReferenceSpeed * 0.80f;
    // Longer dash after the expert test (was 0.18 s, about 138 units; now about 190).
    public const float PlayerDashDuration = 0.25f;
    // Test: player holds still in the anticipation pose before the dash moves.
    public const double PlayerDashStartupDuration = 0.06;
    // More tolerance for late or early jumps (both were 0.10 s).
    public const double PlayerCoyoteTime = 0.15;
    public const double PlayerJumpBufferTime = 0.15;
    public const float PlayerGelatinMaximumFallSpeed = 96f;

    public static float GetPlayerScale(int preset) => preset switch
    {
        2 => PlayerScaleMedium,
        3 => PlayerScaleSmall,
        4 => PlayerScaleSmallest,
        _ => PlayerScaleOriginal
    };

    // World units below the TMX bottom; independent of camera zoom.
    public const int AffordanceVisibilityFallMargin = 224;

    // Checkpoint 2 keeps the chosen camera preset fixed while player scale changes.
    public static int SelectedCameraPreset { get; set; } = DefaultCameraPreset;

    public static float GetCameraZoom(float logicalHeight, float playerReferenceHeight)
    {
        float ratio = SelectedCameraPreset switch
        {
            1 => CameraPlayerRatioSmall,
            2 => CameraPlayerRatioMedium,
            3 => CameraPlayerRatioLarge,
            _ => 0.0f
        };
        if (ratio <= 0 || playerReferenceHeight <= 0 || logicalHeight <= 0)
        {
            return OriginalCameraZoom;
        }

        return logicalHeight * ratio / playerReferenceHeight;
    }

    public const float DebugGridPlayerMultiple = 1.0f;
    public const float DebugGridOpacity = 0.18f;
    public const float DebugPanelOpacity = 0.75f;
    public const int DebugRulerTickWidth = 12;
}
