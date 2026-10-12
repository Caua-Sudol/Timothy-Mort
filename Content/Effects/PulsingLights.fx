#if OPENGL
#define PS_MODEL ps_3_0
#else
#define PS_MODEL ps_4_0_level_9_1
#endif

sampler2D LightSampler : register(s0);
float Phase;
float Strength;

float4 PulseLight(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 light = tex2D(LightSampler, uv) * color;
    float pulse = 0.5 + 0.5 * sin(Phase + uv.x * 6.0);
    // Input and output remain premultiplied; transparent pixels stay transparent.
    float amount = saturate(Strength * pulse);
    float3 brightTint = float3(0.48, 0.78, 0.72) * light.a;
    light.rgb = lerp(light.rgb, max(light.rgb, brightTint), amount);
    return light;
}

technique PulsingLights
{
    pass Pass0
    {
        PixelShader = compile PS_MODEL PulseLight();
    }
}
