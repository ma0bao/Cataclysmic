sampler2D TextureSampler : register(s0);

float2 LightPosition;   
float2 ScreenSize;      
float LightRadius;
float3 LightColor;
float Intensity;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, texCoord);

    // Convert texCoord (0..1) into coordinates
    float2 pixelPos = texCoord * ScreenSize;

    // Distance from light
    float dist = distance(pixelPos, LightPosition);

    float attenuation = saturate(1 - dist / LightRadius);

    // Apply light
    float3 lit = color.rgb * (LightColor * attenuation * Intensity);

    return float4(lit, color.a);
}

technique LightTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
