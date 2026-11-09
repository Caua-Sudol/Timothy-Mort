using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class MortSubtitle
{
    private const float MaximumScale = 1f; // font dialog
    private const int PanelWidth = 1440;
    private const int Padding = 20;
    private const int MaximumPanelHeight = 150;
    private const double CharactersPerSecond = 45.0;
    private static readonly Color SpeakerColor = new Color(100, 185, 255);
    private readonly SpriteFont _font;
    private readonly Queue<string> _pages = new Queue<string>();
    private string _text;
    private double _remainingSeconds;
    private double _visibleCharacters;
    private bool _holdLastPage;
    private float _scale;
    private int _panelHeight;

    public MortSubtitle(SpriteFont font)
    {
        _font = font;
    }

    public bool IsShowing => _text != null;

    public void Clear()
    {
        _pages.Clear();
        _text = null;
        _remainingSeconds = 0;
        _holdLastPage = false;
    }

    public void Show(bool holdLastPage, params string[] pages)
    {
        Clear();
        _holdLastPage = holdLastPage;
        foreach (string page in pages)
            if (!string.IsNullOrWhiteSpace(page))
                _pages.Enqueue(page);
        Advance();
    }

    public void Update(double deltaTime)
    {
        if (_text == null)
            return;
        // Type the page first; its reading time only starts once the whole text is visible.
        if (_visibleCharacters < _text.Length)
        {
            _visibleCharacters += deltaTime * CharactersPerSecond;
            return;
        }
        if (_holdLastPage && _pages.Count == 0)
            return;
        _remainingSeconds -= deltaTime;
        if (_remainingSeconds <= 0)
            Advance();
    }

    private void Advance()
    {
        if (_pages.Count == 0)
        {
            Clear();
            return;
        }

        string page = _pages.Dequeue();
        _scale = MaximumScale;
        for (int attempt = 0; attempt < 60; attempt++)
        {
            _text = Wrap(page, (PanelWidth - Padding * 2) / _scale);
            _panelHeight = (int)Math.Ceiling(
                (_font.LineSpacing + _font.MeasureString(_text).Y) * _scale) + Padding * 2;
            if (_panelHeight <= MaximumPanelHeight &&
                _font.MeasureString(_text).X * _scale <= PanelWidth - Padding * 2)
                break;
            _scale *= 0.95f;
        }
        _remainingSeconds = MortDialogueCatalog.PageDurationSeconds;
        _visibleCharacters = 0;
    }

    private string Wrap(string text, float width)
    {
        var lines = new List<string>();
        string line = string.Empty;
        foreach (string word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = line.Length == 0 ? word : line + " " + word;
            if (line.Length > 0 && _font.MeasureString(candidate).X > width)
            {
                lines.Add(line);
                line = word;
            }
            else
                line = candidate;
        }
        lines.Add(line);
        return string.Join("\n", lines);
    }

    public void Draw(SpriteBatch batch, Texture2D pixel, Vector2 dimensions, bool onResult = false)
    {
        if (_text == null)
            return;
        int y = onResult ? 245 : (int)dimensions.Y - _panelHeight - 24;
        var panel = new Rectangle(((int)dimensions.X - PanelWidth) / 2, y,
            PanelWidth, _panelHeight);
        batch.Begin(samplerState: SamplerState.PointClamp);
        batch.Draw(pixel, panel, Color.Black * 0.82f);
        var position = new Vector2(panel.X + Padding, panel.Y + Padding);
        batch.DrawString(_font, "Mort", position, SpeakerColor,
            0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        position.Y += _font.LineSpacing * _scale;
        // The page is wrapped before typing, so revealed words never jump to another line.
        string visibleText = _text.Substring(0, Math.Min(_text.Length, (int)_visibleCharacters));
        batch.DrawString(_font, visibleText, position, new Color(236, 230, 217),
            0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        batch.End();
    }
}
