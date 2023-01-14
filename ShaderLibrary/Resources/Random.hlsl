#ifndef RANDOM_INCLUDED
#define RANDOM_INCLUDED

float1 ConstructFloat(int1 m) { return asfloat((m & 0x007FFFFF) | 0x3F800000) - 1; }
float2 ConstructFloat(int2 m) { return asfloat((m & 0x007FFFFF) | 0x3F800000) - 1; }
float3 ConstructFloat(int3 m) { return asfloat((m & 0x007FFFFF) | 0x3F800000) - 1; }
float4 ConstructFloat(int4 m) { return asfloat((m & 0x007FFFFF) | 0x3F800000) - 1; }

uint PcgHash(uint state)
{
	uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
	return (word >> 22u) ^ word;
}

uint2 PcgHash(uint2 state)
{
	uint2 word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
	return (word >> 22u) ^ word;
}

uint3 PcgHash(uint3 state)
{
	uint3 word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
	return (word >> 22u) ^ word;
}

uint4 PcgHash(uint4 state)
{
	uint4 word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
	return (word >> 22u) ^ word;
}

uint PermuteState(uint state)
{
	return state * 747796405u + 2891336453u;
}

uint RandomUint(uint value, uint seed = 0)
{
	uint state = PermuteState(value);
	return PcgHash(state + seed);
}

float RandomFloat(uint value, uint seed = 0)
{
	uint start = PermuteState(value) + seed;
	uint state = PermuteState(start);
	return ConstructFloat(PcgHash(state));
}

float2 RandomFloat2(uint value, uint seed = 0)
{
	uint start = PermuteState(value) + seed;

	uint2 state;
	state.x = PermuteState(start);
	state.y = PermuteState(state.x);
	return ConstructFloat(PcgHash(state));
}

float3 RandomFloat3(uint value, uint seed = 0)
{
	uint start = PermuteState(value) + seed;

	uint3 state;
	state.x = PermuteState(start);
	state.y = PermuteState(state.x);
	state.z = PermuteState(state.y);
	return ConstructFloat(PcgHash(state));
}

float4 RandomFloat4(uint value, uint seed, out uint outState)
{
	uint start = PermuteState(value) + seed;

	uint4 state;
	state.x = PermuteState(start);
	state.y = PermuteState(state.x);
	state.z = PermuteState(state.y);
	state.w = PermuteState(state.z);
	outState = state.w;
	return ConstructFloat(PcgHash(state));
}

#endif