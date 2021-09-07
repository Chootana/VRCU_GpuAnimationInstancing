// UV to ID
inline uint2 uvToCoord(float2 uv, uint2 textureSize)
{
    return uv * textureSize;
}

inline uint coordToID(uint2 coord, uint2 textureSize)
{
    return coord.y * textureSize.x + coord.x;
}

inline uint uvToID(float2 uv, uint2 textureSize)
{
    uint2 coord = uvToCoord(uv, textureSize.xy);
    return coordToID(coord, textureSize);
}

// ID to UV
inline uint2 idToCellCoord(uint id, uint textureWidth)
{
    uint x = id % textureWidth;
    uint y = id / textureWidth;
    return uint2(x, y);
}

inline float2 coordToUV(uint2 coord, float2 texelSize)
{
    return coord * texelSize.xy + texelSize.xy / 2.0;
}

inline float2 idToUV(uint id, float3 texelSize)
{
    return coordToUV(idToCellCoord(id, texelSize.z), texelSize);
}

// convert every 3pixel to ID to express particle coord
inline uint2 uvToParticleID(float2 uv, uint2 textureSize)
{
    uint rawID = uvToID(uv, textureSize);
    uint particleID = rawID / 3.0;
    uint localOffset = rawID % 3;
    return uint2(particleID, localOffset);
}

// float to fixed4
fixed4 pack(float value)
{
    uint uintVal = asuint(value);
    uint4 elements = uint4(uintVal >> 0, uintVal >> 8, uintVal >> 16, uintVal >> 24);
    fixed4 color = ((elements & 0x000000FF) + 0.5) / 255.0;
    return color;
}

//decode fixed4 to float
float unpackFloat(fixed4 col)
{
    uint R = uint(col.r * 255) << 0;
    uint G = uint(col.g * 255) << 8;
    uint B = uint(col.b * 255) << 16;
    uint A = uint(col.a * 255) << 24;
    return asfloat(R | G | B | A);
}

// get pos from particle ID 
float3 getPos(uint particleID, sampler2D _Tex, float4 _Tex_TexelSize){
    uint rawID = particleID * 3;
    float x = unpackFloat(tex2Dlod(_Tex, float4(idToUV(rawID,   _Tex_TexelSize),0,0)));
    float y = unpackFloat(tex2Dlod(_Tex, float4(idToUV(rawID+1, _Tex_TexelSize),0,0)));
    float z = unpackFloat(tex2Dlod(_Tex, float4(idToUV(rawID+2, _Tex_TexelSize),0,0)));
    return float3(x,y,z);
}