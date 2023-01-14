#ifndef UTILS_INCLUDED
#define UTILS_INCLUDED

const static float PI = radians(180);
const static float INV_HALF_PI = rcp(radians(90));

float Min4(float4 x)
{
	return min(x.x, min(x.y, min(x.z, x.w)));
}

float Max4(float4 x)
{
	return max(x.x, max(x.y, max(x.z, x.w)));
}

float Remap(float v, float pMin, float pMax, float nMin, float nMax) { return nMin + (v - pMin) / (pMax - pMin) * (nMax - nMin); }
float2 Remap(float2 v, float2 pMin, float2 pMax, float2 nMin, float2 nMax) { return nMin + (v - pMin) / (pMax - pMin) * (nMax - nMin); }
float3 Remap(float3 v, float3 pMin, float3 pMax, float3 nMin, float3 nMax) { return nMin + (v - pMin) / (pMax - pMin) * (nMax - nMin); }
float4 Remap(float4 v, float4 pMin, float4 pMax, float4 nMin, float4 nMax) { return nMin + (v - pMin) / (pMax - pMin) * (nMax - nMin); }

#endif