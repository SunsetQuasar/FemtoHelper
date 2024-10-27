#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time;

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Seed;
uniform float2 BlockSize;

DECLARE_TEXTURE(text, 3);

#define mod(x, y) (x - y * floor(x / y))

float3 rand3(float3 c) {
	float j = 4096.0*sin(dot(c,float3(17.0, 59.4, 15.0)));
	float3 r;
	r.z = frac(512.0*j);
	j *= .125;
	r.x = frac(512.0*j);
	j *= .125;
	r.y = frac(512.0*j);
	return r-0.5;
}

float noise(float3 p) {
  	const float F3 =  0.3333333;
  	const float G3 =  0.1666667;
	float3 s = floor(p + dot(p, float3(F3,F3,F3)));
	float3 x =   p - s + dot(s, float3(G3,G3,G3));
	 
	float3 e = step(float3(0.0, 0.0, 0.0), x - x.yzx);
	float3 i1 = e*(1.0 - e.zxy);
	float3 i2 = 1.0 - e.zxy*(1.0 - e);
	 	
	float3 x1 = x - i1 + G3;
	float3 x2 = x - i2 + 2.0*G3;
	float3 x3 = x - 1.0 + 3.0*G3;
	 
	float4 w, d;
	 
	w.x = dot(x, x);
	w.y = dot(x1, x1);
	w.z = dot(x2, x2);
	w.w = dot(x3, x3);
	 
	w = max(0.6 - w, 0.0);
	 
	d.x = dot(rand3(s), x);
	d.y = dot(rand3(s + i1), x1);
	d.z = dot(rand3(s + i2), x2);
	d.w = dot(rand3(s + 1.0), x3);
	 
	w *= w;
	w *= w;
	d *= w;
	 
	return dot(d, float4(52.0,52.0,52.0,52.0));
}

float EaseQuartOut(float t) 
{
	return t*t*t;
}

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 uniformUV = uv * BlockSize;

    //float value = -0.2 + (EaseQuartOut((1.0 - abs((uv.x * 2.0) - 1.0))) * EaseQuartOut((1.0 - abs((uv.y * 2.0) - 1.0))) * 1.2);

	float padding = 8;

	float2 value = min(-abs(uniformUV - (BlockSize / 2)) + (BlockSize / 2), padding) / padding;

	float4 color = SAMPLE_TEXTURE(text, (uniformUV + 
	float2(
		sin(uniformUV.x / 5 + Time) * 2 + cos(uniformUV.y / 5 + Time) * 2,
		sin(uniformUV.y / 5 + Time + 0.5) * 2 + cos(uniformUV.x / 5 + Time + 0.76) * 2
	) * EaseQuartOut(value.x * value.y)) / BlockSize);

	color *= 0.5 + ((round(noise(float3(uniformUV.x / 20, uniformUV.y / 20, Time / 4 + Seed)) * 5) / 5) * 0.5);

	//color = float4(EaseQuartOut(value.x * value.y), EaseQuartOut(value.x * value.y), EaseQuartOut(value.x * value.y), EaseQuartOut(value.x * value.y));

    return color;
}

void SpriteVertexShader(inout float4 color    : COLOR0,
                        inout float2 texCoord : TEXCOORD0,
                        inout float4 position : SV_Position)
{
    position = mul(position, ViewMatrix);
    position = mul(position, TransformMatrix);
}

technique Shader
{
    pass pass0
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}