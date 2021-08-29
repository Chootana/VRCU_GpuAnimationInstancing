sampler2D _AnimTex;
float4 _AnimTex_TexelSize;

float4 GetUV(int index)
{
    int row = index / (int)_AnimTex_TexelSize.z;
    int col = index % (int)_AnimTex_TexelSize.z;

    return float4(col / _AnimTex_TexelSize.z, row / _AnimTex_TexelSize.w, 0.0, 0.0);
}

float4x4 GetMatrix(int startIndex, float boneIndex)
{
    int matrixIndex = startIndex + boneIndex * 3;

    float4 row0 = tex2Dlod(_AnimTex, GetUV(matrixIndex));
    float4 row1 = tex2Dlod(_AnimTex, GetUV(matrixIndex + 1));
    float4 row2 = tex2Dlod(_AnimTex, GetUV(matrixIndex + 2));
    float4 row3 = float4(0.0, 0.0, 0.0, 1.0);

    return float4x4(row0, row1, row2, row3);
}
