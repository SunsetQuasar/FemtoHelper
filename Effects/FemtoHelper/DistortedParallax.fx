#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

uniform float2 textureSize;
uniform float2 loopMul;
uniform float2 Speed;
uniform float2 Scroll;
uniform float2 offset;
uniform float4 flipInfo;

uniform float4 Amplitudes;
uniform float4 Periods;
uniform float4 Offsets;
uniform float4 WaveSpeeds;

uniform float4 ScaleInfo;
uniform float2 RotInfo;

uniform float4 tint;

uniform bool waveFix;

//TODO: comment what all of these variables mean since they're still a little cryptic

DECLARE_TEXTURE(text, 0);
DECLARE_TEXTURE(parallax, 2);

float4 specialSample(float2 uv)
{
    float4 col = SAMPLE_TEXTURE(parallax, uv);
    if(uv.x > 1 || uv.x < 0) col *= loopMul.x;
    if(uv.y > 1 || uv.y < 0) col *= loopMul.y;
    return col;
}


float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 ScaledCam = CamPos / textureSize;

    float2 factor = (Dimensions / textureSize);

    float scale = ScaleInfo.x + ((0.5+0.5*sin((Time*ScaleInfo.z) + ScaleInfo.w)) * ScaleInfo.y);

    float2 scroll2 = ((float2(Scroll.x, Scroll.y)*scale)-(scale-1)) * factor;

    float2 worldPos = (uv * Dimensions) + CamPos * Scroll;

    float coss = cos((Time * RotInfo.x) + RotInfo.y);
	float sinn = sin((Time * RotInfo.x) + RotInfo.y);

    uv -= (0.5);

	uv = mul(uv, float2x2(coss, -sinn * (Dimensions.x/Dimensions.y), sinn * (Dimensions.y/Dimensions.x), coss));

    uv += (0.5);

    float2 uv2 = (uv * factor) - (offset / Dimensions);

    //position -= (CamPos * scroll2); //camera movement, parallax;
    uv2 += ((CamPos * scroll2) / Dimensions);

    uv2 -= (0.5 * factor) + ScaledCam;

    uv2 /= scale;
    worldPos /= scale;

    uv2 += (0.5 * factor) + ScaledCam;

    float4 Amp2 = Amplitudes * float4(factor.x, factor.x, factor.y, factor.y); // scale amplitude by factor so smaller textures don't wave less

    float2 waveoffset = float2(
        (sin(Time*WaveSpeeds.x + (worldPos.x / Periods.x) + Offsets.x) * Amp2.x) + (sin(Time*WaveSpeeds.y + (worldPos.y / Periods.y) + Offsets.y) * Amp2.y),
        (sin(Time*WaveSpeeds.z + (worldPos.x / Periods.z) + Offsets.z) * Amp2.z) + (sin(Time*WaveSpeeds.w + (worldPos.y / Periods.w) + Offsets.w) * Amp2.w)
    );

    waveoffset = mul(waveoffset, waveFix ? float2x2(coss, -sinn, sinn, coss) : 1);

    //position += (Speed * Time) * factor;
    uv2 -= ((Speed * Time) * factor) / Dimensions;

    //position = floor(position);

    uv2 -= waveoffset / Dimensions; //distortion applied

    uv2 *= flipInfo.xy;
    uv2 += (textureSize / Dimensions) * flipInfo.zw;

    float4 color = specialSample(uv2);

    return (color * tint);
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