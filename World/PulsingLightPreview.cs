using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

// Optional visual experiment; only the test scene passes this effect to its lights.
internal sealed class PulsingLightPreview
{
    private readonly Effect _effect;
    private float _phase;
    private int _strengthIndex = 1;
    private int _periodIndex = 1;
    private static readonly float[] Strengths = { 0.3f, 0.65f, 1f };
    private static readonly float[] Periods = { 6f, 4f, 2f };
    public bool Enabled { get; private set; } = true;

    public PulsingLightPreview(ContentManager content)
    {
        _effect = content.Load<Effect>("Effects/PulsingLights");
    }

    public void Update(KeyboardState keys, KeyboardState previous, double deltaTime)
    {
        bool Pressed(Keys key) => keys.IsKeyDown(key) && previous.IsKeyUp(key);
        if (Pressed(Keys.F6)) Enabled = !Enabled;
        if (Pressed(Keys.F7)) _strengthIndex = (_strengthIndex + 1) % Strengths.Length;
        if (Pressed(Keys.F8)) _periodIndex = (_periodIndex + 1) % Periods.Length;
        if (Enabled)
            _phase = (_phase + (float)deltaTime * MathHelper.TwoPi / Periods[_periodIndex])
                % MathHelper.TwoPi;
    }

    public Effect Prepare()
    {
        if (!Enabled) return null;
        _effect.Parameters["Phase"].SetValue(_phase);
        _effect.Parameters["Strength"].SetValue(Strengths[_strengthIndex]);
        return _effect;
    }

    public string Status => $"LUZ: {(Enabled ? "ON" : "OFF")} | F6: ligar/desligar | " +
        $"F7: intensidade {Strengths[_strengthIndex]:0.00} | F8: ciclo {Periods[_periodIndex]:0}s";
}
