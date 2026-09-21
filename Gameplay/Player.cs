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
    public const float DashSpeed = 960f;

    private static readonly Color DashAvailableColor = Color.Purple;
    private static readonly Color DashSpentColor = Color.SlateGray;

    private Vector2 _player;
    private int _widthPlayer;
    private int _heightPlayer;

    private Vector2 _velocity;
    private readonly Vector2 _maxVelocity;
    private readonly double _gravity;
    private bool _isGrounded;
    private Direction _direction = Direction.Right;

    private float _groundAcceleration = 3300f;
    private float _groundDeceleration = 3300f;
    private float _airAcceleration = 1800f;
    private const float AirBrakingAcceleration = 4800f;
    private const float ReleasedJumpUpwardSpeed = 240f;
    private bool _jumpCanBeCut;
    private bool _dashRequested;
    private double _coyoteTime = 0.10;
    private double _coyoteTimer;
    private double _jumpBufferTime = 0.10;
    private double _jumpBufferTimer;

    public bool CanDash { get; private set; } = true;
    public bool ShowsDashAsAvailable { get; private set; } = true;
    private Vector2 _dashDirection;
    private float _dashSpeed = DashSpeed * 0.75f;
    private float _dashDuration = 0.18f;
    private double _dashTimer;

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

    public PlayerState State { get; private set; } = PlayerState.Idle;

    private readonly PlayerInput _input = new PlayerInput();

    private Texture2D _texture;
    private Color[] _color;

    public Player(int x, int y, double gravity, int width, int height)
    {
        _player = new Vector2(x, y);
        _velocity = new Vector2();
        _maxVelocity = new Vector2(528f, 600f);
        _gravity = gravity;
        _widthPlayer = width;
        _heightPlayer = height;
    }

    public void LoadContent(GraphicsDevice graphicsDevice, int width, int height)
    {
        _texture = new Texture2D(graphicsDevice, width, height);
        _color = Enumerable.Repeat(Color.White, width * height).ToArray();
        _texture.SetData(_color);
    }

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
        UpdateSurfaceContactFlash(deltaTime);
        UpdateDashRechargeSymbol(deltaTime);
        _damageCompressionTimer = Math.Max(0, _damageCompressionTimer - deltaTime);
    UpdateDashEnergyAbsorption(deltaTime);
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
        _jumpCanBeCut = true;
        _velocity = new Vector2(_velocity.X, -_maxVelocity.Y);
        _isGrounded = false;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
        State = PlayerState.Jumping;
    }

    private void StartDash(int horizontalInput)
    {
        _jumpCanBeCut = false;
        int dashHorizontalInput = horizontalInput;
        int dashVerticalInput = _input.Vertical;

        Vector2 direction = new Vector2(dashHorizontalInput, dashVerticalInput);

        if (direction == Vector2.Zero)
            direction = new Vector2(_direction == Direction.Right ? 1 : -1, 0);

        direction.Normalize();

    _dashDirection = direction;
    _dashTimer = _dashDuration;
    _dashTrailPositions.Clear();
    RecordDashTrailPosition();
    State = PlayerState.Dashing;
        CanDash = false;
        ShowsDashAsAvailable = false;
    }

private void UpdateDash(double deltaTime)
{
    RecordDashTrailPosition();
    _velocity = _dashDirection * _dashSpeed;
        _dashTimer -= deltaTime;

        if (_dashTimer <= 0f)
            EndDash();
    }
    public void CancelDash()
    {
        if (State != PlayerState.Dashing)
            return;

        EndDash();
    }

public void EndDash()
{
    _dashTimer = 0f;
    _dashTrailPositions.Clear();
    State = _isGrounded ? PlayerState.Idle : PlayerState.Falling;

    _velocity = _velocity * 0.5f;
}

private void RecordDashTrailPosition()
{
    if (_dashTrailPositions.Count > 0)
    {
        Vector2 lastPosition =
            _dashTrailPositions[_dashTrailPositions.Count - 1];

        if (Vector2.DistanceSquared(lastPosition, _player) <
            MinimumDashTrailSpacingSquared)
        {
            return;
        }
    }

    _dashTrailPositions.Add(_player);

    if (_dashTrailPositions.Count > MaximumDashTrailPositions)
    {
        _dashTrailPositions.RemoveAt(0);
    }
}

private void UpdateSurfaceContactFlash(double deltaTime)
{
    if (_surfaceContactFlashTimer <= 0.0)
    {
        return;
    }

    _surfaceContactFlashTimer = Math.Max(
        0.0,
        _surfaceContactFlashTimer - deltaTime);
}

public void StartSurfaceContactFlash()
{
    _surfaceContactFlashTimer = SurfaceContactFlashDuration;
}

public void ClearSurfaceContactFlash()
{
    _surfaceContactFlashTimer = 0.0;
}

private void UpdateDashRechargeSymbol(double deltaTime)
{
    if (_dashRechargeSymbolTimer <= 0.0)
    {
        return;
    }

    _dashRechargeSymbolTimer = Math.Max(
        0.0,
        _dashRechargeSymbolTimer - deltaTime);
}

public void StartDashRechargeSymbol()
{
    _dashRechargeSymbolTimer = DashRechargeSymbolDuration;
}

public void ClearDashRechargeSymbol()
{
    _dashRechargeSymbolTimer = 0.0;
}

private void UpdateDashEnergyAbsorption(double deltaTime)
{
    if (_dashEnergyAbsorptionTimer <= 0.0)
    {
        return;
    }

    _dashEnergyAbsorptionTimer = Math.Max(
        0.0,
        _dashEnergyAbsorptionTimer - deltaTime);
}

public void StartDashEnergyAbsorption(Vector2 source)
{
    _dashEnergyAbsorptionSource = source;
    _dashEnergyAbsorptionTimer = DashEnergyAbsorptionDuration;
}

public void ClearDashEnergyAbsorption()
{
    _dashEnergyAbsorptionTimer = 0.0;
}

    private void ApplyNormalMovement(int horizontalInput, double deltaTime)
    {
        float targetVelocityX = horizontalInput * _maxVelocity.X;

        float accelerationRate;
        if (_isGrounded)
            accelerationRate = horizontalInput == 0 ? _groundDeceleration : _groundAcceleration;
        else if (horizontalInput == 0 ||
                 horizontalInput * _velocity.X < 0)
            accelerationRate = AirBrakingAcceleration;
        else
            accelerationRate = _airAcceleration;

        double velX = MoveToward(_velocity.X, targetVelocityX, accelerationRate * deltaTime);
        double velY = _velocity.Y;

        if (!_isGrounded)
            velY -= _gravity * deltaTime;

        if (velY >= 0)
            _jumpCanBeCut = false;

        if (_jumpCanBeCut && !_input.IsJumpHeld)
        {
            // Shorten only a normal jump, once; do not cut dash or external impulses.
            velY = Math.Max(velY, -ReleasedJumpUpwardSpeed);
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

    public void StartDamageCompression()
    {
        _damageCompressionTimer = DamageFeedbackSettings.CompressionDurationSeconds;
    }

    public void ClearDamageFeedback()
    {
        _damageCompressionTimer = 0;
    }

    public void Draw(
    SpriteBatch spriteBatch,
        SolutionVisualModule visualModules,
        double damageInvulnerabilityRemainingSeconds)
{
    if ((visualModules & SolutionVisualModule.PLAYER_DASH_TRAIL) != 0 &&
        State == PlayerState.Dashing)
    {
        DrawDashTrail(spriteBatch);
    }

    bool usesAlternativeDashRechargeFeedback =
        (visualModules &
         (SolutionVisualModule.PLAYER_DASH_RECHARGE_SYMBOL |
          SolutionVisualModule.PLAYER_DASH_ENERGY_ABSORPTION |
          SolutionVisualModule.PLAYER_DASH_AVAILABLE_OUTLINE)) != 0;
    Color color = usesAlternativeDashRechargeFeedback || ShowsDashAsAvailable
        ? DashAvailableColor
        : DashSpentColor;

    if ((visualModules &
         SolutionVisualModule.PLAYER_SURFACE_CONTACT_FLASH) != 0 &&
        _surfaceContactFlashTimer > 0.0)
    {
        color = Color.White;
    }

    if ((visualModules & SolutionVisualModule.PLAYER_DASH_AVAILABLE_OUTLINE) != 0 &&
        CanDash)
    {
        spriteBatch.Draw(
            _texture,
            new Rectangle(
                HitBox.X - 2,
                HitBox.Y - 2,
                HitBox.Width + 4,
                HitBox.Height + 4),
            Color.Aquamarine * 0.85f);
    }

    DrawDamageFeedback(
        spriteBatch,
        visualModules,
        damageInvulnerabilityRemainingSeconds,
        color);

}

    private void DrawDamageFeedback(
        SpriteBatch spriteBatch,
        SolutionVisualModule visualModules,
        double invulnerabilityRemainingSeconds,
        Color color)
    {
        bool isInvulnerable = invulnerabilityRemainingSeconds > 0;
        bool alternatePhaseIsVisible =
            ((int)(invulnerabilityRemainingSeconds / DamageFeedbackSettings.BlinkIntervalSeconds) & 1) == 0;

        Rectangle drawBounds = HitBox;
        if (_damageCompressionTimer > 0 &&
            visualModules.HasFlag(SolutionVisualModule.DAMAGE_COMPRESSION))
        {
            int compressedHeight = (int)(drawBounds.Height * 0.7f);
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
    int centerX = HitBox.Center.X;
    int top = HitBox.Top - 12 - rise;
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
    SolutionVisualModule visualModules)
{
    if ((visualModules & SolutionVisualModule.PLAYER_DASH_ENERGY_ABSORPTION) == 0 ||
        _dashEnergyAbsorptionTimer <= 0.0)
    {
        return;
    }

    float progress = (float)(1.0 -
        _dashEnergyAbsorptionTimer / DashEnergyAbsorptionDuration);
    Vector2 target = new Vector2(HitBox.Center.X, HitBox.Center.Y);
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

private void DrawDashTrail(SpriteBatch spriteBatch)
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
            _widthPlayer,
            _heightPlayer);

        spriteBatch.Draw(_texture, trailBounds, Color.White * opacity);
    }
}
}
