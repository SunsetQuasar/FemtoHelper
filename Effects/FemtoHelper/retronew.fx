#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

DECLARE_TEXTURE(text, 0);

float2 curve(float2 uv)
{
	uv = (uv - 0.5) * 2.0;
	uv *= 1.1;	
	uv.x *= 1.0 + pow((abs(uv.y) / 5.0), 2);
	uv.y *= 1.0 + pow((abs(uv.x) / 4.0), 2);
	uv  = (uv / 2.0) + 0.5;
	uv =  uv *0.92 + 0.04;
	return uv;
}

float mod(float x, float m){
    return (x % m + m) % m;
}

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 Dim2 = Dimensions;
    float2 worldPos = (uv * Dimensions) + CamPos;
    float4 color = SAMPLE_TEXTURE(text, uv);

    float2 fragCoord = uv * Dim2;
    float2 q = uv;
    uv = curve( uv );
    float3 oricol = SAMPLE_TEXTURE( text, float2(uv.x,uv.y)).xyz;
    float3 col = oricol;
	float x =  sin(0.3*Time+uv.y*21.0)*sin(0.7*Time+uv.y*29.0)*sin(0.3+0.33*Time+uv.y*31.0)*0.0005;

    col.r = col.r * 0.5 + 0.5 * SAMPLE_TEXTURE(text,float2(x+uv.x+0.002,uv.y+0.002)).x+0.05;
    col.g = col.g * 0.5 + 0.5 * SAMPLE_TEXTURE(text,float2(x+uv.x+0.001,uv.y-0.001)).y+0.05;
    col.b = col.b * 0.5 + 0.5 * SAMPLE_TEXTURE(text,float2(x+uv.x-0.000,uv.y+0.000)).z+0.05;
    col.r += 0.06*SAMPLE_TEXTURE(text,0.75*float2(x+0.025, -0.027)+float2(uv.x+0.002,uv.y+0.002)).x;
    col.g += 0.03*SAMPLE_TEXTURE(text,0.75*float2(x+-0.022, -0.02)+float2(uv.x+0.001,uv.y-0.001)).y;
    col.b += 0.06*SAMPLE_TEXTURE(text,0.75*float2(x+-0.02, -0.018)+float2(uv.x-0.000,uv.y+0.000)).z;

    col = clamp(col*0.6+0.4*col*col*1.0,0.0,1.0);

    float vig = (0.0 + 1.0*16.0*uv.x*uv.y*(1.0-uv.x)*(1.0-uv.y));
	col *= float3(pow(vig,0.3), pow(vig,0.3), pow(vig,0.3));

    col *= float3(0.95,1.05,0.95);
	col *= 2.8;

	float scans = clamp(0.35 + 0.35 * sin(3.5 * Time+uv.y * Dim2.y * 1.5), 0.0, 1.0);
	
	float s = pow(scans,1.7);
	col = col*float3(0.5+0.5*s, 0.5+0.5*s, 0.5+0.5*s);

    col *= 1.0+0.01*sin(110.0*Time);
	if (uv.x < 0.0 || uv.x > 1.0)
		col *= 0.0;
	if (uv.y < 0.0 || uv.y > 1.0)
		col *= 0.0;
	
    float val = clamp((mod(fragCoord.x, 2.0)-1.0)*2.0,0.0,1.0);

	col*=1.0-0.35*float3(val, val, val);
	
    float v2 = sin(Time);

    float comp = smoothstep( 0.1, 0.9,  v2);
 
	// Remove the next line to stop cross-fade between original and postprocess
    // doesn't compile for some reason? -sunset
    // col = lerp(col, oricol, comp);

    color = float4(col, 1);

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