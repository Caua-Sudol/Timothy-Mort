using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Temporary synchronous loading measurements. Output is buffered until loading ends.
internal sealed class LoadDiagnostics : IDisposable
{
    public static bool Enabled { get; set; } = true;
    private static readonly List<string> Lines = new();
    private static int _depth;
    private static int _run;
    private readonly bool _enabled = Enabled;
    private readonly string _name;
    private readonly Stopwatch _clock = new();
    private double _previousMs;

    public LoadDiagnostics(string name)
    {
        _name = name;
        if (!_enabled) return;
        if (_depth++ == 0)
        {
            Lines.Clear();
            Lines.Add($"[LOAD #{++_run}] {name}");
        }
        _clock.Start();
    }

    public void Mark(string stage)
    {
        if (!_enabled) return;
        double now = _clock.Elapsed.TotalMilliseconds;
        Lines.Add($"[LOAD] {_name} / {stage}: {now - _previousMs:F2} ms");
        _previousMs = now;
    }

    public static Texture2D LoadTexture(ContentManager content, string asset)
    {
        if (!Enabled) return content.Load<Texture2D>(asset);
        using var timing = new LoadDiagnostics($"asset {asset}");
        Texture2D texture = content.Load<Texture2D>(asset);
        timing.Mark("Content.Load (pode retornar cache)");
        timing.Texture(texture);
        return texture;
    }

    public void Texture(Texture2D texture)
    {
        if (!_enabled) return;
        Lines.Add($"[LOAD] {_name}: {texture.Width}x{texture.Height}, {texture.Format}");
    }

    public void Dispose()
    {
        if (!_enabled) return;
        _clock.Stop();
        Lines.Add($"[LOAD] {_name} / TOTAL: {_clock.Elapsed.TotalMilliseconds:F2} ms");
        if (--_depth != 0) return;
        // Nested totals overlap; do not add them. Console output is outside the timers.
        Console.WriteLine(string.Join(Environment.NewLine, Lines));
        Lines.Clear();
    }
}
