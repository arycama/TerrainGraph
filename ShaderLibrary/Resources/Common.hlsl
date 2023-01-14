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
	float z1 = GetHeight(id.x - 1, id.y + 1);
    float z2 = GetHeight(id.x + 0, id.y + 1);
    float z3 = GetHeight(id.x + 1, id.y + 1);
    float z4 = GetHeight(id.x - 1, id.y + 0);
    float z5 = GetHeight(id.x + 0, id.y + 0);
    float z6 = GetHeight(id.x + 1, id.y + 0);
    float z7 = GetHeight(id.x - 1, id.y - 1);
    float z8 = GetHeight(id.x + 0, id.y - 1);
    float z9 = GetHeight(id.x + 1, id.y - 1);

    //p, q
	d0.x = (z1 + 2.0 * z4 + z7 - z3 - 2.0 * z6 - z9) / (8.0 * Scale.x);
	d0.y = (z1 + 2.0 * z2 + z3 - z7 - 2.0 * z8 - z9) / (8.0 * Scale.z);

    //r, t, s
	d1.x = (-z1 + 2.0 * z2 - z3 - 2.0 * z4 + 4.0 * z5 - 2.0 * z6 - z7 + 2.0 * z8 - z9) / (4.0 * Scale.x * Scale.x);
	d1.y = (-z1 - 2.0 * z2 - z3 + 2.0 * z4 + 4.0 * z5 + 2.0 * z6 - z7 - 2.0 * z8 - z9) / (4.0 * Scale.z * Scale.z);
    d1.z = (-z1 + z3 + z7 - z9) / (4.0 * Scale.x * Scale.z);

	// Evans young
	//d0.x = (h[2][2] + h[2][1] + h[2][0] - h[0][2] - h[0][1] - h[0][0]) / (6.0 * Scale.x);
	//d0.y = (h[0][2] + h[1][2] + h[2][2] - h[0][0] - h[1][0] - h[2][0]) / (6.0 * Scale.z);

	//d1.x = (h[0][2] + h[2][2] + h[0][1] + h[2][1] + h[0][0] + h[2][0] - 2.0 * (h[1][2] + h[1][1] + h[1][0])) / (3.0 * Scale.x * Scale.x);
	//d1.y = (h[0][2] + h[1][2] + h[2][2] + h[0][0] + h[1][0] + h[2][0] - 2.0 * (h[0][1] + h[1][1] + h[2][1])) / (3.0 * Scale.z * Scale.z);
	//d1.z = (h[2][2] + h[0][0] - h[0][2] - h[2][0]) / (4.0 * Scale.x * Scale.z);

	// Sobel
	//d0.x = ((h[2][0] + 2.0 * h[2][1] + h[2][2]) - (h[0][0] + 2.0 * h[0][1] + h[0][2])) / (8.0 * Scale.x);
	//d0.y = ((h[0][2] + 2.0 * h[1][2] + h[2][2]) - (h[0][0] + 2.0 * h[1][0] + h[2][0])) / (8.0 * Scale.z);

	//d1.x = (h[0][0] + 2.0 * h[0][1] + h[0][2] + h[2][0] + 2.0 * h[2][1] + h[2][2] - (2.0 * h[1][0] + 4.0 * h[1][1] + 2.0 * h[1][2])) / (4.0 * Scale.x * Scale.x);
	//d1.y = (h[0][0] + 2.0 * h[1][0] + h[2][0] + h[0][2] + 2.0 * h[1][2] + h[2][2] - (2.0 * h[0][1] + 4.0 * h[1][1] + 2.0 * h[2][1])) / (4.0 * Scale.z * Scale.z);
	//d1.z = (h[0][2] + h[2][0] - h[0][0] - h[2][2]) / (4.0 * Scale.x * Scale.z);
}

#endif