inline float Random (float2 p)
{
    return frac(
        sin(dot(p, float2(12.9898, 78.233))) * 43758.5453
    );
}

inline float2 Random2(float2 st)
{
    st = float2(
        dot(st, float2(127.1, 311.7)),
        dot(st, float2(269.5, 183.3))
    );
    return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
}

inline float BlockNoise (float2 st)
{
    float2 p = floor(st);
    return Random(p);
}

inline float ValueNoise (float2 st)
{
    float2 p = floor(st);
    float2 f = frac(st);
    float2 u = f * f * (3.0 - 2.0 * f);

    float v_00 = Random(p + float2(0.0, 0.0));
    float v_10 = Random(p + float2(1.0, 0.0));
    float v_01 = Random(p + float2(0.0, 1.0));
    float v_11 = Random(p + float2(1.0, 1.0));


    float v_0010 = lerp(v_00, v_10, u.x);
    float v_0111 = lerp(v_01, v_11, u.x);
    return lerp(v_0010, v_0111, u.y);
}

inline float PerlinNoise (float2 st)
{
    float2 p = floor(st);
    float2 f = frac(st);
    float2 u = f * f * (3.0 - 2.0 * f);

    float v_00 = Random2(p + float2(0.0, 0.0));
    float v_10 = Random2(p + float2(1.0, 0.0));
    float v_01 = Random2(p + float2(0.0, 1.0));
    float v_11 = Random2(p + float2(1.0, 1.0));

    float v_0010 = lerp(dot(v_00, f - float2(0.0, 0.0)), dot(v_10, f - float2(1.0, 0.0)), u.x);
    float v_0110 = lerp(dot(v_01, f - float2(0.0, 1.0)), dot(v_11, f - float2(1.0, 1.0)), u.x);

    return lerp(v_0010, v_0110, u.y) + 0.5;
} 



inline float fBm (float2 st)
{
    float f = 0.0;
    float2 q = st;

    f += 0.5000 * PerlinNoise(q);
    q *= 2.01;

    f += 0.2500 * PerlinNoise(q);
    q *= 2.02;

    f += 0.1250 * PerlinNoise(q);
    q *= 2.03;

    f += 0.0625 * PerlinNoise(q);
    q *= 2.01;

    return f; 
}


/// 3D
float rand(float3 co)
{
	return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 56.787))) * 43758.5453);
}

float Noise(float3 pos)
{
	float3 ip = floor(pos);
	float3 fp = smoothstep(0, 1, frac(pos));
	float4 a = float4(
		rand(ip + float3(0, 0, 0)),
		rand(ip + float3(1, 0, 0)),
		rand(ip + float3(0, 1, 0)),
		rand(ip + float3(1, 1, 0)));
	float4 b = float4(
		rand(ip + float3(0, 0, 1)),
		rand(ip + float3(1, 0, 1)),
		rand(ip + float3(0, 1, 1)),
		rand(ip + float3(1, 1, 1)));
 
	a = lerp(a, b, fp.z);
	a.xy = lerp(a.xy, a.zw, fp.y);
	return lerp(a.x, a.y, fp.x);
}

float PerlinNoise(float3 pos)
{
	return 
		(Noise(pos) +
		Noise(pos * 2 ) +
		Noise(pos * 4) +
		Noise(pos * 8) +
		Noise(pos * 16) +
		Noise(pos * 32) ) / 6;
}




float fbm(float3 p)
{
    float3x3 m = float3x3( 0.00,  0.80,  0.60,
                -0.80,  0.36, -0.48,
                -0.60, -0.48,  0.64
    );

    float f;
    f  = 0.5000 * Noise(p); p = mul(m, p) * 2.02;
    f += 0.2500 * Noise(p); p = mul(m, p) * 2.03;
    f += 0.1250 * Noise(p);
    return f;
}