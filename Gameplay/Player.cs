using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public enum Direction
{
    Top = 1,
    Down = 2,
    Right = 3,
    Left = 4
}

public enum PlayerState
{
    Idle,
    Moving,
    Jumping,
    Falling,
    Dashing
}

public class Player
{
    public const float DashSpeed = GameScaleConfig.PlayerDashReferenceSpeed;

    private Vector2 _player;
    private int _widthPlayer;
    private int _heightPlayer;
    private readonly int _originalWidth;
    private readonly int _originalHeight;

    public float RelativeScale { get; private set; } = 1.0f;
    public float ColliderOffsetFromSpriteX { get; set; }
    public bool ScaleChangeBlocked { get; private set; }

    private Vector2 _velocity;
    private readonly Vector2 _maxVelocity;
    private readonly double _gravity;
    private bool _isGrounded;
    private Direction _direction = Direction.Right;

    private float _groundAcceleration = GameScaleConfig.PlayerGroundAcceleration;
    private float _groundDeceleration = GameScaleConfig.PlayerGroundDeceleration;
    private float _airAcceleration = GameScaleConfig.PlayerAirAcceleration;
    private const float AirBrakingAcceleration = GameScaleConfig.PlayerAirBrakingAcceleration;
    private const float ReleasedJumpUpwardSpeed = GameScaleConfig.PlayerReleasedJumpSpeed;
    private bool _jumpCanBeCut;
    private bool _dashRequested;
    private double _coyoteTime = GameScaleConfig.PlayerCoyoteTime;
    private double _coyoteTimer;
    private double _jumpBufferTime = GameScaleConfig.PlayerJumpBufferTime;
    private double _jumpBufferTimer;

    public bool CanDash { get; private set; } = true;
    public bool ShowsDashAsAvailable { get; private set; } = true;
    private Vector2 _dashDirection;
    private float _dashSpeed = GameScaleConfig.PlayerDashSpeed;
    private float _dashDuration = GameScaleConfig.PlayerDashDuration;
    private double _dashTimer;
    private double _dashStartupTimer;

    public PlayerState State { get; private set; } = PlayerState.Idle;

    private readonly PlayerInput _input = new PlayerInput();

    private readonly PlayerPresentation _presentation = new();

    public int UnscaledReferenceHeight =>
        _presentation.GetReferenceHeight(_originalHeight);

    public float DebugReferenceHeight => UnscaledReferenceHeight * RelativeScale;

    public void SetAirAnimations(Texture2D[] jumpFrames, Texture2D[] glideFrames) =>
        _presentation.SetAirAnimations(jumpFrames, glideFrames);

    public void SetDashAnimations(Texture2D[] upFrames, Texture2D[] sideFrames) =>
        _presentation.SetDashAnimations(upFrames, sideFrames);

    public void SetLookAnimations(Texture2D[] upFrames, Texture2D downFrame) =>
        _presentation.SetLookAnimations(upFrames, downFrame);

    public void SetWalkAnimation(Texture2D[] frames) =>
        _presentation.SetWalkAnimation(frames);

    public void SetPreviewSprite(Texture2D texture, Rectangle source, int width, int height) =>
        _presentation.SetPreviewSprite(texture, source, width, height);

    public void SetDirectionalPreviewSprites(Texture2D right, Texture2D left, float scale) =>
        _presentation.SetDirectionalPreviewSprites(right, left, scale);

    public void SetPreviewSprite(Texture2D texture, int height) =>
        _presentation.SetPreviewSprite(texture, height);

    public void UpdateAnimation(double deltaTime) =>
        _presentation.UpdateAnimation(
            deltaTime, State, _isGrounded, _velocity, _input.Horizontal, _input.Vertical);

    public bool TrySetRelativeScale(float scale, IEnumerable<Rectangle> solidColliders)
    {
        if (!float.IsFinite(scale) || scale <= 0)
            return false;

        ScaleChangeBlocked = false;
        if (scale == RelativeScale)
            return true;

        int width = Math.Max(1, (int)Math.Round(_originalWidth * scale));
        int height = Math.Max(1, (int)Math.Round(_originalHeight * scale));
        Rectangle current = HitBox;
        var candidate = new Rectangle(
            current.Center.X - width / 2, current.Bottom - height, width, height);

        foreach (Rectangle solid in solidColliders)
        {
            if (candidate.Intersects(solid))
            {
                ScaleChangeBlocked = true;
                return false;
            }
        }

        // Keep the feet and horizontal center fixed; preserve subpixel movement.
        _player += new Vector2(candidate.X - current.X, candidate.Y - current.Y);
        _widthPlayer = width;
        _heightPlayer = height;
        // Preserve velocity in player-heights/second, including mid-jump/dash swaps.
        _velocity *= scale / RelativeScale;
        RelativeScale = scale;
        return true;
    }


    public Player(int x, int y, double gravity, int width, int height)
    {
        _player = new Vector2(x, y);
        _velocity = new Vector2();
        _maxVelocity = new Vector2(
            GameScaleConfig.PlayerHorizontalSpeed, GameScaleConfig.PlayerJumpSpeed);
        _gravity = gravity;
        _widthPlayer = width;
        _heightPlayer = height;
        _originalWidth = width;
        _originalHeight = height;
    }

    public void LoadContent(GraphicsDevice graphicsDevice, int width, int height) =>
        _presentation.LoadContent(graphicsDevice, width, height);

    public Vector2 Position
    {
        set { _player = value; }
        get { return _player; }
    }

    public Vector2 Velocity
    {
        get { return _velocity; }
    }

    public bool IsGrounded
    {
        get { return _isGrounded; }
    }

    public Rectangle HitBox
    {
        get { return new Rectangle((int)_player.X, (int)_player.Y, _widthPlayer, _heightPlayer); }
    }

public void Update(double deltaTime)
{
        _presentation.UpdateEffects(deltaTime);
    _input.Update();
        int horizontalInput = _input.Horizontal;

        UpdateCoyoteTimer(deltaTime);
        UpdateJumpBufferTimer(deltaTime);

        if (horizontalInput != 0)
            _direction = horizontalInput > 0 ? Direction.Right : Direction.Left;

        UpdateState(horizontalInput);

        if (State == PlayerState.Dashing)
            UpdateDash(deltaTime);
        else
            ApplyNormalMovement(horizontalInput, deltaTime);
    }

    private void UpdateState(int horizontalInput)
    {
        if (State == PlayerState.Dashing)
            return;

        // Capture aim on the update after Shift so near-simultaneous inputs settle.
        if (_dashRequested)
        {
            _dashRequested = false;
            if (CanDash)
            {
                StartDash(horizontalInput);
                return;
            }
        }

        if (CanDash && _input.DashWasPressed)
            _dashRequested = true;

        if (HasBufferedJump() && CanJump())
        {
            StartJump();
            return;
        }

        if (_isGrounded)
        {
            State = horizontalInput != 0 ? PlayerState.Moving : PlayerState.Idle;
        }
        else
        {
            State = _velocity.Y < 0 ? PlayerState.Jumping : PlayerState.Falling;
        }
    }

    private void UpdateCoyoteTimer(double deltaTime)
    {
        if (_isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else if (_coyoteTimer > 0)
        {
            _coyoteTimer -= deltaTime;
        }
    }

    private bool CanJump()
    {
        return _isGrounded || _coyoteTimer > 0;
    }

    private void UpdateJumpBufferTimer(double deltaTime)
    {
        if (_input.JumpWasPressed)
        {
            _jumpBufferTimer = _jumpBufferTime;
        }
        else if (_jumpBufferTimer > 0)
        {
            _jumpBufferTimer -= deltaTime;
        }
    }

    private bool HasBufferedJump()
    {
        return _jumpBufferTimer > 0;
    }

    private void StartJump()
    {
        _presentation.BeginJumpAnimation();
        _jumpCanBeCut = true;
        _velocity = new Vector2(_velocity.X, -_maxVelocity.Y * RelativeScale);
        _isGrounded = false;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
        State = PlayerState.Jumping;
    }

    // Dash direction from the current input, or null when no direction is held.
    // No downward dash: down is ignored, so down + left/right becomes a side dash.
    private Vector2? GetDashAimDirection(int horizontalInput)
    {
        var direction = new Vector2(horizontalInput, Math.Min(_input.Vertical, 0));
        if (direction == Vector2.Zero)
            return null;
        direction.Normalize();
        return direction;
    }

    private void StartDash(int horizontalInput)
    {
        _jumpCanBeCut = false;
        Vector2 direction = GetDashAimDirection(horizontalInput) ??
            new Vector2(_direction == Direction.Right ? 1 : -1, 0);

    _dashDirection = direction;
    _dashTimer = _dashDuration;
        _dashStartupTimer = GameScaleConfig.PlayerDashStartupDuration;
    State = PlayerState.Dashing;
        CanDash = false;
        ShowsDashAsAvailable = false;
    }

private void UpdateDash(double deltaTime)
{
        if (_dashStartupTimer > 0)
        {
            // Hold still (no gravity) so the anticipation pose is readable.
            _dashStartupTimer -= deltaTime;
            _velocity = Vector2.Zero;
            // Aim is read again when the pause ends, so a direction pressed just after
            // the dash key still counts (like Celeste's dash freeze).
            if (_dashStartupTimer <= 0 && GetDashAimDirection(_input.Horizontal) is Vector2 aim)
                _dashDirection = aim;
            return;
        }

    _velocity = _dashDirection * (_dashSpeed * RelativeScale);
        _dashTimer -= deltaTime;

        if (_dashTimer <= 0f)
            EndDash();
    }
    public void ResyncInput() => _input.Resync();

    public void CancelDash()
    {
        if (State != PlayerState.Dashing)
            return;

        EndDash();
    }

public void EndDash()
{
    _dashTimer = 0f;
        _dashStartupTimer = 0;
    State = _isGrounded ? PlayerState.Idle : PlayerState.Falling;

    _velocity = _velocity * 0.5f;
}

    public void StartSurfaceContactFlash() => _presentation.StartSurfaceContactFlash();
    public void ClearSurfaceContactFlash() => _presentation.ClearSurfaceContactFlash();
    public void StartDashRechargeSymbol() => _presentation.StartDashRechargeSymbol();
    public void ClearDashRechargeSymbol() => _presentation.ClearDashRechargeSymbol();
    public void StartDashEnergyAbsorption(Vector2 source) =>
        _presentation.StartDashEnergyAbsorption(source);
    public void ClearDashEnergyAbsorption() => _presentation.ClearDashEnergyAbsorption();

    private void ApplyNormalMovement(int horizontalInput, double deltaTime)
    {
        float targetVelocityX = horizontalInput * _maxVelocity.X * RelativeScale;

        float accelerationRate;
        if (_isGrounded)
            accelerationRate = horizontalInput == 0 ? _groundDeceleration : _groundAcceleration;
        else if (horizontalInput == 0 ||
                 horizontalInput * _velocity.X < 0)
            accelerationRate = AirBrakingAcceleration;
        else
            accelerationRate = _airAcceleration;

        double velX = MoveToward(
            _velocity.X, targetVelocityX, accelerationRate * RelativeScale * deltaTime);
        double velY = _velocity.Y;

        if (!_isGrounded)
            velY -= _gravity * RelativeScale * deltaTime;

        if (velY >= 0)
            _jumpCanBeCut = false;

        if (_jumpCanBeCut && !_input.IsJumpHeld)
        {
            // Shorten only a normal jump, once; do not cut dash or external impulses.
            velY = Math.Max(velY, -ReleasedJumpUpwardSpeed * RelativeScale);
            _jumpCanBeCut = false;
        }

        _velocity = new Vector2((float)velX, (float)velY);
    }

    private static double MoveToward(double current, double target, double maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;

        return current + Math.Sign(target - current) * maxDelta;
    }

    public void Move(int x, int y)
    {
        _player.Y = y;
        _player.X = x;
    }

    public void Walk(float velocityX, float velocityY, double deltaTime)
    {
        _player.X += velocityX * (float)deltaTime;
        _player.Y += velocityY * (float)deltaTime;
    }

    public void WalkX(float velocityX, double deltaTime)
    {
        _player.X += velocityX * (float)deltaTime;
    }

    public void WalkY(float velocityY, double deltaTime)
    {
        _player.Y += velocityY * (float)deltaTime;
    }

    public void StopWalkX()
    {
        _velocity = new Vector2(0, _velocity.Y);
        CancelDash();
    }

    public void StopWalkY()
    {
        _velocity = new Vector2(_velocity.X, 0);
        CancelDash();
    }

    public void StopFalling()
    {
        _velocity = new Vector2(_velocity.X, 0);
        CancelDash();
    }

    public void ResetVelocity()
    {
        _presentation.ResetAnimation();
        _velocity = Vector2.Zero;
        _dashRequested = false;
        _jumpCanBeCut = false;
    }

    public void LimitFallSpeed(float maximumFallSpeed)
    {
        if (_velocity.Y <= maximumFallSpeed)
        {
            return;
        }

        _velocity = new Vector2(_velocity.X, maximumFallSpeed);
    }

    public void Grounded()
    {
        _jumpCanBeCut = false;
        _isGrounded = true;
        _coyoteTimer = _coyoteTime;
    }

    public void NotGrounded()
    {
        _isGrounded = false;
    }

    public void RestoreDash()
    {
        CanDash = true;
        ShowsDashAsAvailable = true;
    }

    public void RestoreDashWithoutVisualFeedback()
    {
        CanDash = true;
    }

    public void ApplyUpwardImpulse(float speed)
    {
        if (speed <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speed));
        }

        _dashRequested = false;
        _jumpCanBeCut = false;
        CancelDash();
        _velocity = new Vector2(_velocity.X, -speed);
        _isGrounded = false;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
        State = PlayerState.Jumping;
    }

    public void ApplyDamageKnockback(int horizontalDirection)
    {
        if (horizontalDirection == 0)
        {
            horizontalDirection = _direction == Direction.Left ? -1 : 1;
        }

        _dashRequested = false;
        _jumpCanBeCut = false;
        CancelDash();
        _velocity = new Vector2(
            horizontalDirection * DamageFeedbackSettings.KnockbackHorizontalSpeed,
            DamageFeedbackSettings.KnockbackVerticalSpeed);
        NotGrounded();
        State = PlayerState.Jumping;
    }

    public void StartDamageCompression() => _presentation.StartDamageCompression();

    public void ClearDamageFeedback() => _presentation.ClearDamageFeedback();

    public void Draw(
        SpriteBatch spriteBatch,
        SolutionVisualModule visualModules,
        double damageInvulnerabilityRemainingSeconds) =>
        _presentation.Draw(
            spriteBatch,
            this,
            visualModules,
            damageInvulnerabilityRemainingSeconds);

    public void DrawDashRechargeSymbol(
        SpriteBatch spriteBatch,
        SolutionVisualModule visualModules) =>
        _presentation.DrawDashRechargeSymbol(spriteBatch, this, visualModules);

    public void DrawDashEnergyAbsorption(
        SpriteBatch spriteBatch,
        SolutionVisualModule visualModules) =>
        _presentation.DrawDashEnergyAbsorption(spriteBatch, this, visualModules);

    internal Direction FacingDirection => _direction;
    internal Vector2 DashDirectionForPresentation => _dashDirection;
    internal float DashDurationForPresentation => _dashDuration;
    internal double DashTimerForPresentation => _dashTimer;
    internal bool IsInDashStartupForPresentation => _dashStartupTimer > 0;
    internal int HorizontalInputForPresentation => _input.Horizontal;

}
