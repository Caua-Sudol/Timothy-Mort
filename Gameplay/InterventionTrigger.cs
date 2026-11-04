using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// How much the sword calls for attention in the intervention room.
public enum SwordEmphasis
{
    NONE,
    // Before the first solution, and again after a second one has been tried.
    SIMPLE,
    // After the first solution only: invites the player to try another one.
    STRONG
}

// The intervention control is a sword used with E or a pressure button.
public sealed class InterventionTrigger
{
    private const float PressedOffset = 8.0f;
    private const float PressSpeed = 64.0f;
    // Red: dark dots disappear against the intervention room's colours.
    private static readonly Color EmberColor = new Color(210, 45, 45);
    private Texture2D _swordTexture;
    private float _pressOffset;
    private bool _activationPending;
    private bool _activationIssued;
    private double _effectTime;

    public bool IsPressed { get; private set; }
    public SwordEmphasis Emphasis { get; set; }

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
        _effectTime += deltaTime;
        if (usesInteractionKey)
        {
            Rectangle reach = playerHitBox;
            // Taller reach (was 24) so E works from a slightly higher or lower step.
            reach.Inflate(64, 64);
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
            {
                Rectangle sword = GetSwordBounds(marker);
                DrawRisingDots(spriteBatch, sword, buttonTexture);
                spriteBatch.Draw(_swordTexture, GetPulsedBounds(sword), Color.White);
            }
            else
                spriteBatch.Draw(buttonTexture, pressedButton, Color.Orange);
        }
    }

    // Only STRONG pulses: the sword grows and shrinks from its base.
    private Rectangle GetPulsedBounds(Rectangle sword)
    {
        if (Emphasis != SwordEmphasis.STRONG) return sword;
        double wave = Math.Abs(Math.Sin(_effectTime * 7.0));
        float scale = 1.0f + (float)wave * 0.10f;
        int width = (int)Math.Round(sword.Width * scale);
        int height = (int)Math.Round(sword.Height * scale);
        return new Rectangle(sword.Center.X - width / 2, sword.Bottom - height, width, height);
    }

    // Red smoke dots rising around the sword; more, faster and bigger when STRONG.
    private void DrawRisingDots(SpriteBatch spriteBatch, Rectangle sword, Texture2D pixel)
    {
        if (Emphasis == SwordEmphasis.NONE) return;
        bool strong = Emphasis == SwordEmphasis.STRONG;
        int count = strong ? 14 : 6;
        double speed = strong ? 0.9 : 0.4;
        int size = strong ? 9 : 6;
        float spread = sword.Width / 2f + (strong ? 40f : 24f);
        for (int i = 0; i < count; i++)
        {
            // Each dot has its own offset so they rise out of step.
            double cycle = (_effectTime * speed + i * 0.618) % 1.0;
            float side = (i % 2 == 0 ? -1f : 1f) * spread * (0.4f + 0.6f * ((i * 37 % 10) / 10f));
            float x = sword.Center.X + side + (float)Math.Sin((_effectTime + i) * 2.0) * 6f;
            float y = sword.Bottom - (float)cycle * sword.Height * 1.2f;
            float alpha = (float)(Math.Sin(cycle * Math.PI) * (strong ? 0.9 : 0.6));
            spriteBatch.Draw(pixel,
                new Rectangle((int)x - size / 2, (int)y - size / 2, size, size),
                EmberColor * alpha);
        }
    }
}
