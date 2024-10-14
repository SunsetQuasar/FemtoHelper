#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

uniform float Percent;
uniform float RandomValue;

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
    p.b += RandomValue * 18.100985;
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

float snoisefracal(float3 m) {
	return   0.5* abs(noise(m))
		+0.25* abs(noise(2.0*m))
		+0.125* abs(noise(4.0*m))
		+0.0625* abs(noise(8.0*m));
}

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{

    float col = snoisefracal(float3(uv*2, Time));
    float treshold = Percent;

    float4 color;
    if(col < treshold){
        color = float4(0, 0, 0, 1);
    } else {
        color = float4(0, 0, 0, 0);
    }

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