using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class ScenarioTransition : IDisposable
{
    private const float FadeDurationSeconds = 0.75f;
    private const float SlideDurationSeconds = 0.3f;

    private enum State
    {
        None,
        FadingOut,
        FadingIn,
        Sliding
    }

    private RenderTarget2D _slideOutgoing;
    private RenderTarget2D _slideIncoming;
    private float _slideProgress;
    private int _slideDirection = 1;
    private float _slideStartOffset;
    private float _fadeAlpha;
    private string _pendingScenarioId;
    private string _pendingScenarioSpawnId;
    private State _state;

    public bool IsActive => _state != State.None;
    public bool IsSliding => _state == State.Sliding;
    public RenderTarget2D OutgoingRoom => _slideOutgoing;
    public RenderTarget2D IncomingRoom => _slideIncoming;

    public void BeginFade(string scenarioId, string spawnId)
    {
        _pendingScenarioId = scenarioId;
        _pendingScenarioSpawnId = spawnId;
        _fadeAlpha = 0.0f;
        _state = State.FadingOut;
    }

    public void BeginSlide(
        GraphicsDevice graphicsDevice,
        int direction,
        float startOffset)
    {
        _slideOutgoing ??= new RenderTarget2D(
            graphicsDevice,
            GameViewport.LogicalWidth,
            GameViewport.LogicalHeight);
        _slideIncoming ??= new RenderTarget2D(
            graphicsDevice,
            GameViewport.LogicalWidth,
            GameViewport.LogicalHeight);

        _slideDirection = direction;
        _slideStartOffset = startOffset;
        _slideProgress = 0.0f;
        _fadeAlpha = 0.0f;
        _state = State.Sliding;
    }

    public bool Update(
        double deltaTime,
        out string destinationScenarioId,
        out string destinationSpawnId)
    {
        destinationScenarioId = null;
        destinationSpawnId = null;

        if (_state == State.Sliding)
        {
            _slideProgress = Math.Min(
                1.0f,
                _slideProgress + (float)deltaTime / SlideDurationSeconds);
            if (_slideProgress >= 1.0f)
                _state = State.None;
            return false;
        }

        float fadeDelta = (float)(deltaTime / FadeDurationSeconds);
        if (_state == State.FadingOut)
        {
            _fadeAlpha = Math.Min(1.0f, _fadeAlpha + fadeDelta);
            if (_fadeAlpha < 1.0f)
                return false;

            destinationScenarioId = _pendingScenarioId;
            destinationSpawnId = _pendingScenarioSpawnId;
            _pendingScenarioId = null;
            _pendingScenarioSpawnId = null;
            _state = State.FadingIn;
            return true;
        }

        if (_state == State.FadingIn)
        {
            _fadeAlpha = Math.Max(0.0f, _fadeAlpha - fadeDelta);
            if (_fadeAlpha <= 0.0f)
                _state = State.None;
        }

        return false;
    }

    public float GetSlideBackgroundOffset(float destinationOffset)
    {
        float progress = MathHelper.SmoothStep(0, 1, _slideProgress);
        return MathHelper.Lerp(_slideStartOffset, destinationOffset, progress);
    }

    public void DrawSlide(SpriteBatch spriteBatch)
    {
        float progress = MathHelper.SmoothStep(0, 1, _slideProgress);
        int offset = (int)(GameViewport.LogicalWidth * progress) * _slideDirection;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(
            _slideOutgoing,
            new Rectangle(-offset, 0, GameViewport.LogicalWidth, GameViewport.LogicalHeight),
            Color.White);
        spriteBatch.Draw(
            _slideIncoming,
            new Rectangle(
                GameViewport.LogicalWidth * _slideDirection - offset,
                0,
                GameViewport.LogicalWidth,
                GameViewport.LogicalHeight),
            Color.White);
        spriteBatch.End();
    }

    public void DrawFade(SpriteBatch spriteBatch, Texture2D fadeTexture)
    {
        if (_state == State.None || _state == State.Sliding)
            return;

        spriteBatch.Begin();
        spriteBatch.Draw(
            fadeTexture,
            new Rectangle(0, 0, GameViewport.LogicalWidth, GameViewport.LogicalHeight),
            Color.Black * _fadeAlpha);
        spriteBatch.End();
    }

    public void Reset()
    {
        _state = State.None;
        _fadeAlpha = 0.0f;
        _pendingScenarioId = null;
        _pendingScenarioSpawnId = null;
    }

    public void Dispose()
    {
        _slideOutgoing?.Dispose();
        _slideIncoming?.Dispose();
    }
}
