#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

#include "Random.hlsl"
#include "Utils.hlsl"

Texture2D Input;
Texture2D Mask;
RWTexture2D<float> Result;
float _Min, _Max;
float SampleWidth, SampleHeight;
float4 InputTexelSize;
SamplerState _PointClampSampler, _LinearClampSampler;

// Returns an input value, clamped to the borders
float GetInput(uint2 position, int2 offset = 0)
{
	uint2 size;
	Input.GetDimensions(size.x, size.y);

	return Input[clamp(position + offset, 0, size - 1)].r;
}

float GetHeight(int x, int y)
{
	uint2 size;
	Input.GetDimensions(size.x, size.y);

	return Input[clamp(int2(x, y), 0, size - 1)].r;
}

void GetDerivatives(int2 id, float3 Scale, out float2 d0, out float3 d1)
{
	float tl = GetHeight(id.x - 1, id.y + 1);
    float t = GetHeight(id.x + 0, id.y + 1);
    float tr = GetHeight(id.x + 1, id.y + 1);
    float l = GetHeight(id.x - 1, id.y + 0);
    float c = GetHeight(id.x + 0, id.y + 0);
    float r = GetHeight(id.x + 1, id.y + 0);
    float bl = GetHeight(id.x - 1, id.y - 1);
    float b = GetHeight(id.x + 0, id.y - 1);
    float br = GetHeight(id.x + 1, id.y - 1);

    //p, q
	d0.x = (r - l) / (2.0 * Scale.x);
	d0.y = (t - b) / (2.0 * Scale.z);

    //r, t, s
	d1.x = (r - 2.0 * c + l) / (Scale.x * Scale.x);
	d1.y = (t - 2.0 * c + b) / (Scale.z * Scale.z);
    d1.z = (tr - br - tl + bl) / (4.0 * Scale.x * Scale.z);
}

#endif