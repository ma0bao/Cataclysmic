sampler2D TextureSampler : register(s0);

float2 LightPosition;   
float2 ScreenSize;      
float LightRadius;
float Intensity;
float timer;

/* ###https://web.archive.org/web/20140224205853/http://obge.paradice-insight.us/wiki/Includes_(Effects)### */
 
float rand_1_05(in float2 uv)
{
	float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
	return abs(noise.x + noise.y) * 0.5;
}
 
float2 rand_2_10(in float2 uv) {
	float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
	float noiseY = sqrt(1 - noiseX * noiseX);
	return float2(noiseX, noiseY);
}
 
float2 rand_2_0004(in float2 uv)
{
	float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
	float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
	return float2(noiseX, noiseY) * 0.004;
}

/* ############################################################################# */


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    

    // Convert texCoord (0..1) into coordinates
    float2 pixelPos = texCoord * ScreenSize;

    float1 sinn = sin((pixelPos.y + (timer/5)));
    pixelPos.x = pixelPos.x + sinn * 30;
    pixelPos.y = pixelPos.y - 100 + (200.0*rand_2_10(pixelPos.x));

    float4 color = tex2D(TextureSampler, texCoord);
    if (sinn < 0) {
        color *= 1 - Intensity;
    }
    else {
        color *= 1 + Intensity;
    }

    // Distance from light
    float dist = distance(pixelPos, LightPosition);
    float attenuation = saturate(1 - dist / 2000);

    // Apply light
    float3 lit = color.rgb * (attenuation);

    return float4(lit, color.a);
}




technique LightTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
