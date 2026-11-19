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

    private Texture2D _texture;
    private Texture2D _previewSprite;
    private Texture2D _leftPreviewSprite;
    private Texture2D[] _walkFrames;
    private const double WalkFramesPerSecond = 10.0;
    private double _walkAnimationTime;
    private Texture2D[] _jumpFrames;
    private Texture2D[] _glideFrames;
    // Same glide loop with the spent-dash colors (hat and Mort's eye).
    private Texture2D[] _glideDashUsedFrames;
    // glide0000 is the rising pose; the loop is the frames after it.
    private bool _airRising;
    private Texture2D[] _upDashFrames;
    private Texture2D[] _sideDashFrames;
    // Body lean for a fully sideways dash; diagonals lean proportionally less.
    private const float DashMaxLeanDegrees = 25f;
    // Share of the dash shown with frame 0000 before switching to 0001.
    private const double DashFirstFrameFraction = 0.75;
    private const double AirFramesPerSecond = 10.0;
    private enum AirAnimation { None, Jump, Glide, Landing, Dash, DashHold }
    private AirAnimation _airAnimation;
    private double _airAnimationTime;
    private bool _wasAirborne;
    // Standing still and holding up/down: look in that direction.
    // Up shows the intermediate pose first, then the full look up.
    private Texture2D[] _lookUpFrames;
    private Texture2D _lookDownFrame;
    private const double LookUpIntermediateSeconds = 0.08;
    private int _lookDirection;
    private double _lookTime;

    public void SetAirAnimations(Texture2D[] jumpFrames, Texture2D[] glideFrames,
        Texture2D[] glideDashUsedFrames)
    {
        _jumpFrames = jumpFrames;
        _glideFrames = glideFrames;
        _glideDashUsedFrames = glideDashUsedFrames;
        _airAnimation = AirAnimation.None;
        _airAnimationTime = 0;
        _wasAirborne = false;
    }

    public void UpdateAnimation(
        double deltaTime,
        PlayerState state,
        bool isGrounded,
        Vector2 velocity,
        int horizontalInput,
        int verticalInput)
    {
        UpdateWalkAnimation(deltaTime, horizontalInput);
        UpdateLook(deltaTime, state, isGrounded, horizontalInput, verticalInput);
        if (_jumpFrames == null || _glideFrames == null)
            return;

        if (state == PlayerState.Dashing && HasDashFrames)
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
            if (_airAnimation == AirAnimation.Dash || _airAnimation == AirAnimation.DashHold)
            {
                // Keep the last dash pose (its hat shows the spent dash) until the fall starts.
                _airAnimation = velocity.Y < 0 ? AirAnimation.DashHold : AirAnimation.Glide;
                _airAnimationTime = 0;
            }
            else if (_airAnimation != AirAnimation.Jump && _airAnimation != AirAnimation.Glide)
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
                _airAnimationTime %= GlideLoopLength / AirFramesPerSecond;
            _airRising = velocity.Y < 0;
        }
        _wasAirborne = !isGrounded;
    }

    public void SetDashAnimations(Texture2D[] upFrames, Texture2D[] sideFrames)
    {
        _upDashFrames = upFrames;
        _sideDashFrames = sideFrames;
    }

    private int GlideLoopLength => Math.Max(1, _glideFrames.Length - 1);

    private Texture2D GetGlideFrame(int frame, bool canDash)
    {
        if (!canDash && _glideDashUsedFrames is { Length: > 0 })
            return _glideDashUsedFrames[frame % _glideDashUsedFrames.Length];
        if (_airRising || _glideFrames.Length == 1)
            return _glideFrames[0];
        return _glideFrames[1 + frame % GlideLoopLength];
    }

    private bool HasDashFrames => _upDashFrames != null && _sideDashFrames != null;

    public void SetLookAnimations(Texture2D[] upFrames, Texture2D downFrame)
    {
        _lookUpFrames = upFrames;
        _lookDownFrame = downFrame;
    }

    private void UpdateLook(double deltaTime, PlayerState state, bool isGrounded,
        int horizontalInput, int verticalInput)
    {
        int direction = isGrounded && horizontalInput == 0 && state != PlayerState.Dashing
            ? verticalInput : 0;
        if (direction != _lookDirection)
        {
            _lookDirection = direction;
            _lookTime = 0;
        }
        else
            _lookTime += deltaTime;
    }

    // Null while not looking or when the look sprites are missing.
    private Texture2D GetLookSprite()
    {
        if (_lookDirection > 0) return _lookDownFrame;
        if (_lookDirection < 0 && _lookUpFrames is { Length: > 0 })
            return _lookUpFrames[_lookTime < LookUpIntermediateSeconds ? 0 : _lookUpFrames.Length - 1];
        return null;
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

    public void Draw(
        SpriteBatch spriteBatch,
        Player player,
        SolutionVisualModule visualModules,
        double damageInvulnerabilityRemainingSeconds)
    {
    // Only the placeholder rectangle (no sprite) uses these colors.
    Color color = player.ShowsDashAsAvailable ? DashAvailableColor : DashSpentColor;

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

        Texture2D lookSprite = _airAnimation == AirAnimation.None ? GetLookSprite() : null;
        if (lookSprite != null)
        {
            sprite = lookSprite;
            source = sprite.Bounds;
            spriteWidth = (int)Math.Round(sprite.Width * _directionalPreviewScale);
            spriteHeight = (int)Math.Round(sprite.Height * _directionalPreviewScale);
            // Look poses face right, like the walk frames.
            spriteEffects = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        float spriteRotation = 0f;
        if (_jumpFrames != null && _glideFrames != null && _airAnimation != AirAnimation.None)
        {
            int frame = (int)(_airAnimationTime * AirFramesPerSecond);
            sprite = _airAnimation switch
            {
                AirAnimation.Jump => _jumpFrames[3],
                AirAnimation.Glide => GetGlideFrame(frame, player.CanDash),
                AirAnimation.Landing => _jumpFrames[2 - Math.Min(frame, 2)],
                // Dash poses are chosen below.
                _ => _jumpFrames[_jumpFrames.Length - 1]
            };
            spriteEffects = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (_airAnimation == AirAnimation.Dash || _airAnimation == AirAnimation.DashHold)
            {
                Vector2 dashDirection = player.DashDirectionForPresentation;
                if (dashDirection.X != 0)
                    facingLeft = dashDirection.X < 0;
                spriteEffects = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                bool holding = _airAnimation == AirAnimation.DashHold;
                double dashElapsed = player.DashDurationForPresentation -
                    player.DashTimerForPresentation;
                int dashFrame = holding ||
                    dashElapsed >= player.DashDurationForPresentation * DashFirstFrameFraction
                    ? 1 : 0;
                // Always use the upright pose and lean it slightly toward the dash direction.
                sprite = _upDashFrames[Math.Min(dashFrame, _upDashFrames.Length - 1)];
                spriteRotation = dashDirection.X * MathHelper.ToRadians(DashMaxLeanDegrees);
            }
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
            color = Color.White;
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
}
