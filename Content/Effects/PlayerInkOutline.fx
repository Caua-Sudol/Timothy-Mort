#if OPENGL
#define PS_MODEL ps_3_0
#else
#define PS_MODEL ps_4_0
#endif

sampler2D SpriteSampler : register(s0);
float3 InkColor;
float2 HatchReferenceSize;
float OutlineWidth;
float HatchSpacing;
float HatchStrength;

float AlphaAt(float2 uv)
{
    // Clamp sampling must not extend opaque pixels beyond the sprite rectangle.
    float inside = step(0.0, uv.x) * step(uv.x, 1.0)
                 * step(0.0, uv.y) * step(uv.y, 1.0);
    return tex2D(SpriteSampler, uv).a * inside;
}

float HatchLine(float coordinate, float smoothing)
{
    float distanceToLine = abs(frac(coordinate) - 0.5);
    return 1.0 - smoothstep(0.10, 0.10 + smoothing, distanceToLine);
}

float4 Ink(float4 tint : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(SpriteSampler, uv);
    // Derivatives keep outline width in render-target pixels, including during dash rotation.
    float2 dx = ddx(uv) * OutlineWidth;
    float2 dy = ddy(uv) * OutlineWidth;
    float surrounding = max(max(AlphaAt(uv + dx), AlphaAt(uv - dx)),
                            max(AlphaAt(uv + dy), AlphaAt(uv - dy)));
    float2 diagonalA = (dx + dy) * 0.7071;
    float2 diagonalB = (dx - dy) * 0.7071;
    surrounding = max(surrounding,
        max(max(AlphaAt(uv + diagonalA), AlphaAt(uv - diagonalA)),
            max(AlphaAt(uv + diagonalB), AlphaAt(uv - diagonalB))));

    float3 original = sprite.rgb / max(sprite.a, 0.0001);
    float brightness = dot(original, float3(0.299, 0.587, 0.114));
    float2 hatchPosition = uv * HatchReferenceSize / HatchSpacing;
    float first = hatchPosition.x + hatchPosition.y;
    float second = hatchPosition.x - hatchPosition.y;
    float smoothFirst = max(abs(ddx(first)) + abs(ddy(first)), 0.01);
    float smoothSecond = max(abs(ddx(second)) + abs(ddy(second)), 0.01);
    float shadows = 1.0 - smoothstep(0.25, 0.70, brightness);
    float deepShadows = 1.0 - smoothstep(0.12, 0.35, brightness);
    float hatch = max(HatchLine(first, smoothFirst) * shadows,
                      HatchLine(second, smoothSecond) * deepShadows) * HatchStrength;
    float3 shaded = lerp(original, InkColor, hatch) * sprite.a;
    float outlineAlpha = saturate(surrounding - sprite.a);
    // Preserve premultiplied alpha, including the player's damage transparency.
    return float4(shaded + InkColor * outlineAlpha, sprite.a + outlineAlpha) * tint;
}

technique PlayerInkOutline
{
    pass Pass0
    {
        PixelShader = compile PS_MODEL Ink();
    }
}
