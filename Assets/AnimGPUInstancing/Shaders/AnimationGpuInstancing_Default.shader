Shader "AnimationGpuInstancing/Base"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Shininess ("Shininess", Range(0.0, 1.0)) = 0.078125

        [NoScaleOffset]_AnimTex("Animation Texture", 2D) = "white" {}


        _StartFrame("Start Frame", Float) = 0 
        _FrameCount("Frame Count", Float) = 1 
        _OffsetSeconds("Offset Seconds", Float) = 0 
        _PixelsPerFrame("Pixels Per Frame", Float) = 0 


        [Toggle]
        _ROOT_MOTION("Apply Root Motion", Float) = 0
        [NoScaleOffset]_RepeatTex("Repeat Texture", 2D) = "white" {}
        _RepeatStartFrame("Repeat Start Frame", Float) = 0
        _RepeatMax("Repeat Max", FLoat) = 1
        _RepeatNum ("Repeat Num", Float) = 1
    

        [KeywordEnum(UNLIT, REAL)]
        _LIGHTING("Lighting", Float) = 0

        [Toggle] _INSTANCING("Gpu Instancing", Float) = 1
    }

    CustomEditor "AGI_ShaderInspector"

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Includes/Utils.cginc"
    

    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
        float4 texcoord1 : TEXCOORD1;
        half4 boneIndex : TEXCOORD2;
        fixed4 boneWeight : TEXCOORD3;
        float4 tangent : TANGENT;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
        float4 normal : NORMAL;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        
        #ifdef _LIGHTING_REAL
        float3 lightDir : TEXCOORD2;
        float3 viewDir : TEXCOORD3;
        #endif 
        
    };

    UNITY_INSTANCING_BUFFER_START(Props)

    UNITY_DEFINE_INSTANCED_PROP(uint, _StartFrame)
    #define _StartFrame_arr Props 

    UNITY_DEFINE_INSTANCED_PROP(uint, _FrameCount)
    #define _FrameCount_arr Props
    
    UNITY_DEFINE_INSTANCED_PROP(uint, _OffsetSeconds)
    #define _OffsetSeconds_arr Props

    UNITY_DEFINE_INSTANCED_PROP(uint, _ROOT_MOTION)
    #define _ROOT_MOTION_arr Props

    UNITY_DEFINE_INSTANCED_PROP(uint, _RepeatStartFrame)
    #define _RepeatStartFrame_arr Props 

    UNITY_DEFINE_INSTANCED_PROP(uint, _RepeatNum)
    #define _RepeatNum_arr Props

    UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
    #define _Color_arr Props
    

    UNITY_INSTANCING_BUFFER_END(Props)

    sampler2D _MainTex;
    float4 _MainTex_ST;
    
    sampler2D _BumpMap;

    sampler2D _AnimTex;
    float4 _AnimTex_TexelSize;


    sampler2D _RepeatTex;
    float4 _RepeatTex_TexelSize;
    uint _PixelsPerFrame;  

    uint _RepeatMax;   

    #define Mat4x4Identity float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)

    half _Shininess;
    float4 _LightColor0;

    v2f vert (appdata v)
    {
        v2f o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);

        uint startFrame = UNITY_ACCESS_INSTANCED_PROP(_StartFrame_arr, _StartFrame);
        uint frameCount = UNITY_ACCESS_INSTANCED_PROP(_FrameCount_arr, _FrameCount);
        float offsetSeconds = UNITY_ACCESS_INSTANCED_PROP(_OffsetSeconds_arr, _OffsetSeconds);

        float time = _Time.y;

        uint offsetFrame = (uint)((time + offsetSeconds) * 30.0);
        uint currentFrame = startFrame + offsetFrame % frameCount;

        uint clampedIndex = currentFrame * _PixelsPerFrame;
        float4x4 bone1Mat = GetMatrix(clampedIndex, v.boneIndex.x, _AnimTex, _AnimTex_TexelSize);
        float4x4 bone2Mat = GetMatrix(clampedIndex, v.boneIndex.y, _AnimTex, _AnimTex_TexelSize);
        float4x4 bone3Mat = GetMatrix(clampedIndex, v.boneIndex.z, _AnimTex, _AnimTex_TexelSize);
        float4x4 bone4Mat = GetMatrix(clampedIndex, v.boneIndex.w, _AnimTex, _AnimTex_TexelSize);

        float4 pos = 
            mul(bone1Mat, v.vertex) * v.boneWeight.x + 
            mul(bone2Mat, v.vertex) * v.boneWeight.y + 
            mul(bone3Mat, v.vertex) * v.boneWeight.z + 
            mul(bone4Mat, v.vertex) * v.boneWeight.w;


        float4 normal = 
            mul(bone1Mat, v.normal) * v.boneWeight.x +  
            mul(bone2Mat, v.normal) * v.boneWeight.y +  
            mul(bone3Mat, v.normal) * v.boneWeight.z+  
            mul(bone4Mat, v.normal) * v.boneWeight.w;  


        uint _root_motion = UNITY_ACCESS_INSTANCED_PROP(_ROOT_MOTION_arr, _ROOT_MOTION);
        uint repeatStartFrame = UNITY_ACCESS_INSTANCED_PROP(_RepeatStartFrame_arr, _RepeatStartFrame);
        uint repeatNum  = UNITY_ACCESS_INSTANCED_PROP(_RepeatNum_arr, _RepeatNum);
        repeatNum = max(1, repeatNum);
        uint currentRepeatIndex =  (uint)(offsetFrame / frameCount) % repeatNum;
        uint currentRepeatFrame = (currentRepeatIndex == 0)? 0 :  repeatStartFrame + currentRepeatIndex - 1;
        uint clampedRepeatIndex = currentRepeatFrame * 3;
        float4x4 rootMat = GetMatrix(clampedRepeatIndex, 0, _RepeatTex, _RepeatTex_TexelSize);

        rootMat = (_root_motion) ? rootMat : Mat4x4Identity;


        pos = mul(rootMat, pos);
        normal = mul(rootMat, normal);

        o.vertex = UnityObjectToClipPos(pos);
        UNITY_TRANSFER_FOG(o,o.vertex);
        o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.normal = normal;

#ifdef _LIGHTING_REAL
            TANGENT_SPACE_ROTATION;
            o.lightDir = normalize(mul(rotation, ObjSpaceLightDir(v.vertex)));
            o.viewDir = normalize(mul(rotation, ObjSpaceViewDir(v.vertex)));
#endif

        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        float4 _Col = UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
        fixed4 tex = tex2D(_MainTex, i.uv);
        fixed4 col = tex * _Col;

#ifdef _LIGHTING_REAL
            half3 halfDir = normalize(i.lightDir + i.viewDir);
            half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
            half4 diff = saturate(dot(normal, i.lightDir)) * _LightColor0;
            half3 spec = pow(max(0, dot(normal, halfDir)), _Shininess * 128.0) * _LightColor0.rgb * tex.rgb;
            col.rgb = col.rgb * diff + spec;
#endif

        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);

        return col;
    }

    fixed4 frag_shadow(v2f i) : SV_Target {
        return (fixed4)0;
    }
    ENDCG

    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 4.5
            #pragma shader_feature _LIGHTING_UNLIT _LIGHTING_REAL

            ENDCG
        }

    }

}
