using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// The intervention control is a sword used with E or a pressure button.
public sealed class InterventionTrigger
{
    private const float PressedOffset = 8.0f;
    private const float PressSpeed = 64.0f;
    private Texture2D _swordTexture;
    private float _pressOffset;
    private bool _activationPending;
    private bool _activationIssued;

    public bool IsPressed { get; private set; }

    // Borrowed from ContentManager; its lifetime is managed by the content owner.
    public void SetSwordTexture(Texture2D texture) => _swordTexture = texture;

    public Rectangle GetSwordBounds(Rectangle marker)
    {
        if (_swordTexture == null) return marker;
        const int height = 192;
        int width = (int)Math.Round(height * _swordTexture.Width /
            (double)_swordTexture.Height);
        return new Rectangle(marker.Center.X - width / 2, marker.Bottom - height, width, height);
    }

    public void Reset()
    {
        IsPressed = false;
        _pressOffset = 0.0f;
        _activationPending = false;
        _activationIssued = false;
    }

    public bool TryTakeActivation()
    {
        if (!_activationPending) return false;
        _activationPending = false;
        return true;
    }

    public void Update(
        IEnumerable<Rectangle> markers,
        Rectangle playerHitBox,
        bool playerIsGrounded,
        bool usesInteractionKey,
        bool interactionPressed,
        double deltaTime)
    {
        if (usesInteractionKey)
        {
            Rectangle reach = playerHitBox;
            reach.Inflate(64, 24);
            bool nearSword = markers.Any(marker => reach.Intersects(GetSwordBounds(marker)));
            IsPressed = nearSword && interactionPressed;
            if (IsPressed) _activationPending = true;
            return;
        }

        bool playerIsOnButton = HasButtonUnderPlayer(markers, playerHitBox, playerIsGrounded);
        if (playerIsOnButton)
        {
            IsPressed = true;
        }
        else
        {
            IsPressed = false;
            _pressOffset = MathHelper.Max(0.0f, _pressOffset - PressSpeed * (float)deltaTime);
            if (_pressOffset <= 0.0f) _activationIssued = false;
            return;
        }

        if (_pressOffset < PressedOffset)
            _pressOffset = MathHelper.Min(PressedOffset, _pressOffset + PressSpeed * (float)deltaTime);

        if (_pressOffset >= PressedOffset && !_activationIssued)
        {
            _activationIssued = true;
            _activationPending = true;
        }
    }

    private static bool HasButtonUnderPlayer(
        IEnumerable<Rectangle> markers,
        Rectangle playerHitBox,
        bool playerIsGrounded)
    {
        if (!playerIsGrounded) return false;
        var playerFeet = new Rectangle(
            playerHitBox.X, playerHitBox.Bottom - 1, playerHitBox.Width, 1);

        foreach (Rectangle marker in markers)
        {
            if (playerFeet.Intersects(marker)) return true;
        }

        return false;
    }

    public void Draw(SpriteBatch spriteBatch, IEnumerable<Rectangle> markers, Texture2D buttonTexture)
    {
        foreach (Rectangle marker in markers)
        {
            var pressedButton = new Rectangle(
                marker.X, marker.Y + (int)_pressOffset, marker.Width, marker.Height);
            if (_swordTexture != null)
                spriteBatch.Draw(_swordTexture, GetSwordBounds(marker), Color.White);
            else
                spriteBatch.Draw(buttonTexture, pressedButton, Color.Orange);
        }
    }
}
