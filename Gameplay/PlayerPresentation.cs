using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

internal sealed class PlayerPresentation
{
    private static readonly Color DashAvailableColor = Color.Purple;
    private static readonly Color DashSpentColor = Color.SlateGray;

    private const int MaximumDashTrailPositions = 12;
    private const float MinimumDashTrailSpacingSquared = 16.0f;
    private readonly List<Vector2> _dashTrailPositions = new List<Vector2>();
    private const double SurfaceContactFlashDuration = 0.12;
    private double _surfaceContactFlashTimer;
    private const double DashRechargeSymbolDuration = 0.45;
    private double _dashRechargeSymbolTimer;
    private double _damageCompressionTimer;
    private const double DashEnergyAbsorptionDuration = 0.36;
    private double _dashEnergyAbsorptionTimer;
    private Vector2 _dashEnergyAbsorptionSource;

    private Texture2D _texture;
    private Texture2D _previewSprite;
    private Texture2D _leftPreviewSprite;
    private Texture2D[] _walkFrames;
    private const double WalkFramesPerSecond = 10.0;
    private double _walkAnimationTime;
    private Texture2D[] _jumpFrames;
    private Texture2D[] _glideFrames;
    private const double AirFramesPerSecond = 10.0;
    private enum AirAnimation { None, Jump, Glide, Landing, Dash }
    private AirAnimation _airAnimation;
    private double _airAnimationTime;
    private bool _wasAirborne;

    public void SetAirAnimations(Texture2D[] jumpFrames, Texture2D[] glideFrames)
    {
        _jumpFrames = jumpFrames;
        _glideFrames = glideFrames;
        _airAnimation = AirAnimation.None;
        _airAnimationTime = 0;
        _wasAirborne = false;
    }

    public void UpdateAnimation(
        double deltaTime,
        PlayerState state,
        bool isGrounded,
        Vector2 velocity,
        int horizontalInput)
    {
        UpdateWalkAnimation(deltaTime, horizontalInput);
        if (_jumpFrames == null || _glideFrames == null)
            return;

        if (state == PlayerState.Dashing)
        {
            _airAnimation = AirAnimation.Dash;
            _airAnimationTime = 0;
        }
        else if (isGrounded)
        {
            if (_wasAirborne)
            {
                _airAnimation = AirAnimation.Landing;
                _airAnimationTime = 0;
            }
            else if (_airAnimation == AirAnimation.Landing)
            {
                _airAnimationTime += deltaTime;
                if (_airAnimationTime >= 3.0 / AirFramesPerSecond)
                    _airAnimation = AirAnimation.None;
            }
            else
                _airAnimation = AirAnimation.None;
        }
        else
        {
            if (_airAnimation != AirAnimation.Jump && _airAnimation != AirAnimation.Glide)
            {
                _airAnimation = velocity.Y < 0 ? AirAnimation.Jump : AirAnimation.Glide;
                _airAnimationTime = 0;
            }
            else
                _airAnimationTime += deltaTime;

            if (_airAnimation == AirAnimation.Jump &&
                _airAnimationTime >= _jumpFrames.Length / AirFramesPerSecond)
            {
                _airAnimationTime -= _jumpFrames.Length / AirFramesPerSecond;
                _airAnimation = AirAnimation.Glide;
            }
            if (_airAnimation == AirAnimation.Glide)
                _airAnimationTime %= _glideFrames.Length / AirFramesPerSecond;
        }
        _wasAirborne = !isGrounded;
    }

    private bool IsWalking(int horizontalInput) =>
        _walkFrames != null && _walkFrames.Length > 0 && horizontalInput != 0;

    public void SetWalkAnimation(Texture2D[] frames)
    {
        _walkFrames = frames;
        _walkAnimationTime = 0;
    }

    private void UpdateWalkAnimation(double deltaTime, int horizontalInput)
    {
        if (!IsWalking(horizontalInput))
        {
            _walkAnimationTime = 0;
            return;
        }

        _walkAnimationTime += deltaTime;
        double animationDuration = _walkFrames.Length / WalkFramesPerSecond;
        if (_walkAnimationTime >= animationDuration)
        {
            // walk0001 introduces the movement; subsequent cycles start at walk0002.
            double loopStart = _walkFrames.Length > 1 ? 1.0 / WalkFramesPerSecond : 0;
            _walkAnimationTime = loopStart +
                (_walkAnimationTime - animationDuration) % (animationDuration - loopStart);
        }
    }
    private float _directionalPreviewScale;
    private Rectangle _previewSource;
    private int _previewHeight;
    private int _previewWidth;

    public int GetReferenceHeight(int fallbackHeight) =>
        _previewSprite != null ? _previewHeight : fallbackHeight;

    public void SetPreviewSprite(Texture2D texture, Rectangle source, int width, int height)
    {
        _leftPreviewSprite = null;
        _previewSprite = texture;
        _previewSource = source;
        _previewWidth = width;
        _previewHeight = height;
    }

    public void SetDirectionalPreviewSprites(Texture2D right, Texture2D left, float scale)
    {
        SetPreviewSprite(right, right.Bounds,
            (int)Math.Round(right.Width * scale),
            (int)Math.Round(right.Height * scale));
        _leftPreviewSprite = left;
        _directionalPreviewScale = scale;
    }

    public void SetPreviewSprite(Texture2D texture, int height)
    {
        _leftPreviewSprite = null;
        var pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);
        int left = texture.Width, top = texture.Height, right = -1, bottom = -1;
        for (int y = 0; y < texture.Height; y++)
        for (int x = 0; x < texture.Width; x++)
        {
            if (pixels[y * texture.Width + x].A == 0) continue;
            left = Math.Min(left, x);
            top = Math.Min(top, y);
            right = Math.Max(right, x);
            bottom = Math.Max(bottom, y);
        }
        if (right < left || bottom < top) return;
        _previewSprite = texture;
        _previewSource = new Rectangle(left, top, right - left + 1, bottom - top + 1);
        _previewHeight = height;
        _previewWidth = Math.Max(1, (int)Math.Round(
            height * _previewSource.Width / (double)_previewSource.Height));
    }
    private Color[] _color;

    public void LoadContent(GraphicsDevice graphicsDevice, int width, int height)
    {
        _texture = new Texture2D(graphicsDevice, width, height);
        _color = Enumerable.Repeat(Color.White, width * height).ToArray();
        _texture.SetData(_color);
    }

    public void UpdateEffects(double deltaTime)
    {
        _surfaceContactFlashTimer = Math.Max(0.0, _surfaceContactFlashTimer - deltaTime);
        _dashRechargeSymbolTimer = Math.Max(0.0, _dashRechargeSymbolTimer - deltaTime);
        _damageCompressionTimer = Math.Max(0.0, _damageCompressionTimer - deltaTime);
        _dashEnergyAbsorptionTimer = Math.Max(0.0, _dashEnergyAbsorptionTimer - deltaTime);
    }

    public void BeginJumpAnimation()
    {
        _airAnimation = AirAnimation.Jump;
        _airAnimationTime = 0;
    }

    public void ResetAnimation()
    {
        _walkAnimationTime = 0;
        _airAnimation = AirAnimation.None;
        _airAnimationTime = 0;
        _wasAirborne = false;
    }

    public void RecordDashTrailPosition(Vector2 position)
    {
        if (_dashTrailPositions.Count > 0 &&
            Vector2.DistanceSquared(_dashTrailPositions[^1], position) <
                MinimumDashTrailSpacingSquared)
            return;

        _dashTrailPositions.Add(position);
        if (_dashTrailPositions.Count > MaximumDashTrailPositions)
            _dashTrailPositions.RemoveAt(0);
    }

    public void ClearDashTrail() => _dashTrailPositions.Clear();
    public void StartSurfaceContactFlash() => _surfaceContactFlashTimer = SurfaceContactFlashDuration;
    public void ClearSurfaceContactFlash() => _surfaceContactFlashTimer = 0.0;
    public void StartDashRechargeSymbol() => _dashRechargeSymbolTimer = DashRechargeSymbolDuration;
    public void ClearDashRechargeSymbol() => _dashRechargeSymbolTimer = 0.0;

    public void StartDashEnergyAbsorption(Vector2 source)
    {
        _dashEnergyAbsorptionSource = source;
        _dashEnergyAbsorptionTimer = DashEnergyAbsorptionDuration;
    }

    public void ClearDashEnergyAbsorption() => _dashEnergyAbsorptionTimer = 0.0;
    public void StartDamageCompression() =>
        _damageCompressionTimer = DamageFeedbackSettings.CompressionDurationSeconds;
    public void ClearDamageFeedback() => _damageCompressionTimer = 0.0;

    public void Draw(
        SpriteBatch spriteBatch,
        Player player,
        SolutionVisualModule visualModules,
        double damageInvulnerabilityRemainingSeconds)
    {
    if ((visualModules & SolutionVisualModule.PLAYER_DASH_TRAIL) != 0 &&
        player.State == PlayerState.Dashing)
    {
        DrawDashTrail(spriteBatch, player.HitBox.Size);
    }

    bool usesAlternativeDashRechargeFeedback =
        (visualModules &
         (SolutionVisualModule.PLAYER_DASH_RECHARGE_SYMBOL |
          SolutionVisualModule.PLAYER_DASH_ENERGY_ABSORPTION |
          SolutionVisualModule.PLAYER_DASH_AVAILABLE_OUTLINE)) != 0;
    Color color = usesAlternativeDashRechargeFeedback || player.ShowsDashAsAvailable
        ? DashAvailableColor
        : DashSpentColor;

    if ((visualModules &
         SolutionVisualModule.PLAYER_SURFACE_CONTACT_FLASH) != 0 &&
        _surfaceContactFlashTimer > 0.0)
    {
        color = Color.White;
    }

    if ((visualModules & SolutionVisualModule.PLAYER_DASH_AVAILABLE_OUTLINE) != 0 &&
        player.CanDash)
    {
        spriteBatch.Draw(
            _texture,
            new Rectangle(
                player.HitBox.X - 2,
                player.HitBox.Y - 2,
                player.HitBox.Width + 4,
                player.HitBox.Height + 4),
            Color.Aquamarine * 0.85f);
    }

    DrawDamageFeedback(
        spriteBatch,
        player,
        visualModules,
        damageInvulnerabilityRemainingSeconds,
        color);

}


    private void DrawDamageFeedback(
        SpriteBatch spriteBatch,
        Player player,
        SolutionVisualModule visualModules,
        double invulnerabilityRemainingSeconds,
        Color color)
    {
        bool isInvulnerable = invulnerabilityRemainingSeconds > 0;
        bool alternatePhaseIsVisible =
            ((int)(invulnerabilityRemainingSeconds / DamageFeedbackSettings.BlinkIntervalSeconds) & 1) == 0;

        Texture2D sprite = _previewSprite;
        SpriteEffects spriteEffects = SpriteEffects.None;
        bool facingLeft = player.FacingDirection == Direction.Left;
        Rectangle source = _previewSource;
        int spriteWidth = _previewWidth;
        int spriteHeight = _previewHeight;
        if (player.FacingDirection == Direction.Left && _leftPreviewSprite != null)
        {
            sprite = _leftPreviewSprite;
            source = sprite.Bounds;
            spriteWidth = (int)Math.Round(sprite.Width * _directionalPreviewScale);
            spriteHeight = (int)Math.Round(sprite.Height * _directionalPreviewScale);
        }

        if (IsWalking(player.HorizontalInputForPresentation))
        {
            int frame = (int)(_walkAnimationTime * WalkFramesPerSecond) % _walkFrames.Length;
            sprite = _walkFrames[frame];
            source = sprite.Bounds;
            spriteWidth = (int)Math.Round(sprite.Width * _directionalPreviewScale);
            spriteHeight = (int)Math.Round(sprite.Height * _directionalPreviewScale);
            spriteEffects = player.FacingDirection == Direction.Left
                ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        float spriteRotation = 0f;
        if (_jumpFrames != null && _glideFrames != null && _airAnimation != AirAnimation.None)
        {
            int frame = (int)(_airAnimationTime * AirFramesPerSecond);
            sprite = _airAnimation switch
            {
                AirAnimation.Jump => _jumpFrames[3],
                AirAnimation.Glide => _glideFrames[frame % _glideFrames.Length],
                AirAnimation.Landing => _jumpFrames[2 - Math.Min(frame, 2)],
                AirAnimation.Dash => player.DashDurationForPresentation - player.DashTimerForPresentation < 1.0 / AirFramesPerSecond
                    ? _glideFrames[2] : _jumpFrames[3],
                _ => _jumpFrames[_jumpFrames.Length - 1]
            };
            if (_airAnimation == AirAnimation.Dash)
            {
                if (player.DashDirectionForPresentation.X != 0)
                    facingLeft = player.DashDirectionForPresentation.X < 0;
                // The final jump pose points diagonally upward; rotate it toward the dash.
                float poseAngle = facingLeft ? -3f * MathHelper.PiOver4 : -MathHelper.PiOver4;
                spriteRotation = (float)Math.Atan2(player.DashDirectionForPresentation.Y, player.DashDirectionForPresentation.X) - poseAngle;
            }
            spriteEffects = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            source = sprite.Bounds;
            spriteWidth = (int)Math.Round(sprite.Width * _directionalPreviewScale);
            spriteHeight = (int)Math.Round(sprite.Height * _directionalPreviewScale);
        }

        Vector2 spriteDrawSize = new Vector2(spriteWidth, spriteHeight) * player.RelativeScale;
        float facingSign = facingLeft ? -1f : 1f;
        float spriteCenterX = player.HitBox.Center.X - player.ColliderOffsetFromSpriteX * player.RelativeScale * facingSign;
        Rectangle drawBounds = player.HitBox;
        if (_previewSprite != null)
        {
            int width = Math.Max(1, (int)Math.Round(spriteDrawSize.X));
            int height = Math.Max(1, (int)Math.Round(spriteDrawSize.Y));
            drawBounds = new Rectangle((int)Math.Round(spriteCenterX - width / 2f),
                player.HitBox.Bottom - height, width, height);
            color = visualModules.HasFlag(SolutionVisualModule.PLAYER_DASH_RECOVERY_COLOR) &&
                !player.ShowsDashAsAvailable ? Color.SlateGray : Color.White;
        }
        if (_damageCompressionTimer > 0 &&
            visualModules.HasFlag(SolutionVisualModule.DAMAGE_COMPRESSION))
        {
            int compressedHeight = (int)(drawBounds.Height * 0.7f);
            spriteDrawSize.Y *= compressedHeight / (float)drawBounds.Height;
            drawBounds = new Rectangle(
                drawBounds.X,
                drawBounds.Bottom - compressedHeight,
                drawBounds.Width,
                compressedHeight);
        }

        if (isInvulnerable &&
            visualModules.HasFlag(SolutionVisualModule.INVULNERABILITY_TRANSPARENCY) &&
            !alternatePhaseIsVisible)
        {
            color *= 0.35f;
        }

        if (isInvulnerable &&
            visualModules.HasFlag(SolutionVisualModule.INVULNERABILITY_OUTLINE))
        {
            DrawDamageOutline(spriteBatch, drawBounds, Color.White, 2);
        }

        if (isInvulnerable &&
            visualModules.HasFlag(SolutionVisualModule.INVULNERABILITY_OUTLINE_PULSE))
        {
            int thickness = 2 + (int)Math.Round(
                (Math.Sin(invulnerabilityRemainingSeconds * 12) + 1) * 1.5);
            DrawDamageOutline(spriteBatch, drawBounds, Color.Cyan, thickness);
        }

        if (isInvulnerable &&
            visualModules.HasFlag(SolutionVisualModule.INVULNERABILITY_SILHOUETTE) &&
            alternatePhaseIsVisible)
        {
            Rectangle silhouetteBounds = new(
                drawBounds.X - 4,
                drawBounds.Y,
                drawBounds.Width,
                drawBounds.Height);
            spriteBatch.Draw(_texture, silhouetteBounds, Color.White * 0.35f);
        }

        bool hideForBlink =
            isInvulnerable &&
            visualModules.HasFlag(SolutionVisualModule.INVULNERABILITY_BLINK) &&
            !alternatePhaseIsVisible;

        if (!hideForBlink)
        {
            if (_previewSprite != null)
            {
                if (player.RelativeScale != 1.0f)
                    spriteBatch.Draw(sprite,
                        new Vector2(spriteCenterX, player.HitBox.Bottom - spriteDrawSize.Y / 2f),
                        source, color, spriteRotation,
                        new Vector2(source.Width / 2f, source.Height / 2f),
                        new Vector2(spriteDrawSize.X / source.Width,
                            spriteDrawSize.Y / source.Height), spriteEffects, 0f);
                else if (spriteRotation != 0f)
                    spriteBatch.Draw(sprite,
                        new Vector2(drawBounds.X + drawBounds.Width / 2f,
                            drawBounds.Y + drawBounds.Height / 2f),
                        source, color, spriteRotation,
                        new Vector2(source.Width / 2f, source.Height / 2f),
                        new Vector2(drawBounds.Width / (float)source.Width,
                            drawBounds.Height / (float)source.Height), spriteEffects, 0f);
                else
                    spriteBatch.Draw(sprite, drawBounds, source, color,
                        0f, Vector2.Zero, spriteEffects, 0f);
            }
            else
                spriteBatch.Draw(_texture, drawBounds, color);
        }
    }

    private void DrawDamageOutline(
        SpriteBatch spriteBatch,
        Rectangle bounds,
        Color color,
        int thickness)
    {
        spriteBatch.Draw(
            _texture,
            new Rectangle(bounds.X - thickness, bounds.Y - thickness, bounds.Width + thickness * 2, thickness),
            color);
        spriteBatch.Draw(
            _texture,
            new Rectangle(bounds.X - thickness, bounds.Bottom, bounds.Width + thickness * 2, thickness),
            color);
        spriteBatch.Draw(
            _texture,
            new Rectangle(bounds.X - thickness, bounds.Y, thickness, bounds.Height),
            color);
        spriteBatch.Draw(
            _texture,
            new Rectangle(bounds.Right, bounds.Y, thickness, bounds.Height),
            color);
    }

    public void DrawDashRechargeSymbol(
        SpriteBatch spriteBatch,
        Player player,
        SolutionVisualModule visualModules)
{
    if ((visualModules & SolutionVisualModule.PLAYER_DASH_RECHARGE_SYMBOL) == 0 ||
        _dashRechargeSymbolTimer <= 0.0)
    {
        return;
    }

    float progress = (float)(1.0 -
        _dashRechargeSymbolTimer / DashRechargeSymbolDuration);
    float opacity = 1.0f - progress * 0.35f;
    int rise = (int)(progress * 4.0f);
    int centerX = player.HitBox.Center.X;
    int top = player.HitBox.Top - 12 - rise;
    Color symbolColor = Color.LimeGreen * opacity;

    spriteBatch.Draw(
        _texture,
        new Rectangle(centerX - 4, top + 3, 8, 2),
        symbolColor);
    spriteBatch.Draw(
        _texture,
        new Rectangle(centerX - 1, top, 2, 8),
        symbolColor);
}

public void DrawDashEnergyAbsorption(
        SpriteBatch spriteBatch,
        Player player,
        SolutionVisualModule visualModules)
{
    if ((visualModules & SolutionVisualModule.PLAYER_DASH_ENERGY_ABSORPTION) == 0 ||
        _dashEnergyAbsorptionTimer <= 0.0)
    {
        return;
    }

    float progress = (float)(1.0 -
        _dashEnergyAbsorptionTimer / DashEnergyAbsorptionDuration);
    Vector2 target = new Vector2(player.HitBox.Center.X, player.HitBox.Center.Y);
    Color particleColor = Color.Aquamarine * (0.9f - progress * 0.3f);

    for (int index = 0; index < 3; index++)
    {
        float laneOffset = (index - 1) * 6.0f;
        float particleProgress = MathHelper.Clamp(
            progress * 1.15f - index * 0.06f,
            0.0f,
            1.0f);
        Vector2 source = _dashEnergyAbsorptionSource +
            new Vector2(0.0f, laneOffset);
        Vector2 position = Vector2.Lerp(source, target, particleProgress);

        spriteBatch.Draw(
            _texture,
            new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4),
            particleColor);
    }
}

private void DrawDashTrail(SpriteBatch spriteBatch, Point playerSize)
{
    for (int index = 0; index < _dashTrailPositions.Count; index++)
    {
        float progress =
            (index + 1.0f) / (_dashTrailPositions.Count + 1.0f);
        float opacity = 0.15f + 0.35f * progress;
        Vector2 position = _dashTrailPositions[index];

        var trailBounds = new Rectangle(
            (int)position.X,
            (int)position.Y,
            playerSize.X,
            playerSize.Y);

        spriteBatch.Draw(_texture, trailBounds, Color.White * opacity);
    }
}
}

