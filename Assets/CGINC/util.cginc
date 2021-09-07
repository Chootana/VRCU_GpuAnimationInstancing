inline float3 RGB(float r, float g, float b) 
{
    return float3(r/255.0, g/255.0, b/255.0);
}

inline float3 hsv(float h, float s, float v){
    float4 t = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(float3(h,h,h) + t.xyz) * 6.0 - float3(t.w, t.w, t.w));
    return v * lerp(float3(t.x, t.x, t.x), clamp(p - float3(t.x, t.x, t.x), 0.0, 1.0), s);
}