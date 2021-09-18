#ifndef _AnimationGpuInstancing
#define _AnimationGpuInstancing

float4 GetUV(uint index, float4 texelSize)
{
    float row = index / (uint)texelSize.z + 0.5;
    float col = index % (uint)texelSize.z + 0.5;

    return float4(col  / texelSize.z, row / texelSize.w, 0.0, 0.0);
}

float4x4 GetMatrix(uint startIndex, float boneIndex, sampler2D tex, float4 texelSize)
{
    uint index = startIndex + boneIndex * 3;

    float4 row0 = tex2Dlod(tex, GetUV(index, texelSize));
    float4 row1 = tex2Dlod(tex, GetUV(index + 1, texelSize));
    float4 row2 = tex2Dlod(tex, GetUV(index + 2, texelSize));
    float4 row3 = float4(0.0, 0.0, 0.0, 1.0);

    return float4x4(row0, row1, row2, row3);
}

#endif 