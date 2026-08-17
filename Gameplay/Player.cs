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
    private double _coyoteTime = 0.10;
    private double _coyoteTimer;
    private double _jumpBufferTime = 0.10;
    private double _jumpBufferTimer;

    public bool CanDash { get; private set; } = true;
    public bool ShowsDashAsAvailable { get; private set; } = true;
    private Vector2 _dashDirection;
    private float _dashSpeed = 960f;
    private float _dashDuration = 0.18f;
    private double _dashTimer;

    private const int MaximumDashTrailPositions = 12;
    private const float MinimumDashTrailSpacingSquared = 16.0f;
    private readonly List<Vector2> _dashTrailPositions = new List<Vector2>();
    private const double SurfaceContactFlashDuration = 0.12;
    private double _surfaceContactFlashTimer;
    private const double DashRechargeSymbolDuration = 0.45;
    private double _dashRechargeSymbolTimer;
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

        if (CanDash && _input.DashWasPressed)
        {
            StartDash(horizontalInput);
            return;
        }

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
        _velocity = new Vector2(_velocity.X, -_maxVelocity.Y);
        _isGrounded = false;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
        State = PlayerState.Jumping;
    }

    private void StartDash(int horizontalInput)
    {
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
        if (horizontalInput == 0 && _isGrounded)
            accelerationRate = _groundDeceleration;
        else
            accelerationRate = _isGrounded ? _groundAcceleration : _airAcceleration;

        double velX = MoveToward(_velocity.X, targetVelocityX, accelerationRate * deltaTime);
        double velY = _velocity.Y;

        if (!_isGrounded)
            velY -= _gravity * deltaTime;

        if (!_input.IsJumpHeld && velY < 0)
            velY *= 0.85f;

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

public void Draw(
    SpriteBatch spriteBatch,
    SolutionVisualModule visualModules)
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

    spriteBatch.Draw(_texture, HitBox, color);

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
