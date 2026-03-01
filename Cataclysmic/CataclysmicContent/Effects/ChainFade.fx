sampler2D TextureSampler : register(s0);

float2 ScreenSize;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, texCoord);

    float2 pixelPos = texCoord * ScreenSize;

    if (pixelPos.y > 500)
        color.a = 1 - (pixelPos.y - 500)/ 300;
    else if (pixelPos.y > 800)
        color.a = 0;

    return color;
}

technique LightTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
