float time;
float zoom;
float2 screenSize;
float2 ratio;
float2 offset;
float4x4 transform;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture3;
sampler2D samplerTex3 = sampler_state { texture = <sampleTexture3>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0, float4 vPos : VPOS) : COLOR0
{
    float2 st = screenSpace.xy;
    float2 sam = st * (ratio.xy) - (offset / (screenSize));
    
    sam -= float2(0.5, 0.5);
    sam = mul(sam, transform);
    sam += float2(0.5, 0.5);
    
    float power = tex2D(samplerTex2, sam).a;
    
    for (float x = -2.0; x <= 2.0; x += 1.0)
    {
        for (float y = -2.0; y <= 2.0; y += 1.0)
        {
            power += tex2D(samplerTex2, sam + (float2(x, y) * 1.0) / screenSize).a > 0 ? 1.0 : 0.0;
        }
    }
    
    power /= 16.0;
    
    float2 wobble = float2(sin(st.x * 100.0 + time * 10.0) * 6, cos(st.y * 100.0 + time * 8.0) * 6.0) / screenSize * power;
    
    float4 color = float4(0.0, 0.0, 0.0, 1.0);
    color.r = tex2D(samplerTex, st + wobble).r;
    color.g = tex2D(samplerTex, st + wobble * 0.6).g;
    color.b = tex2D(samplerTex, st + wobble * 0.2).b;
    
    float sinPower = sin(power * 3.14);
    color.r += tex2D(samplerTex, st + float2(cos(time + 0), sin(time + 0)) * (sinPower / screenSize * 8.0)).r * sinPower * 0.33;
    color.g += tex2D(samplerTex, st + float2(cos(time + 2), sin(time + 2)) * (sinPower / screenSize * 8.0)).g * sinPower * 0.33;
    color.b += tex2D(samplerTex, st + float2(cos(time + 4), sin(time + 4)) * (sinPower / screenSize * 8.0)).b * sinPower * 0.33;
    
    color *= lerp(float4(1.0, 1.0, 1.0, 1.0), tex2D(samplerTex3, st), power > 0.0 ? 0.0 : 0.0);
    
    //return color * 0.01 + tex2D(samplerTex2, sam);
    return color;
}

technique Technique1
{
    pass PrimitivesPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};