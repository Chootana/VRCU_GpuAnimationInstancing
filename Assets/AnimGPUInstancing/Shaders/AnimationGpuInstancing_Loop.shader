Shader "AnimationGpuInstancing/Loop"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Shininess ("Shininess", Range(0.0, 1.0)) = 0.078125

        [NoScaleOffset]_AnimTex("Animation Texture", 2D) = "white" {}


        _StartFrame("Start Frame", Float) = 0 
        _FrameCount("Frame Count", Float) = 1 
        _OffsetSeconds("Offset Seconds", Float) = 0 
        _PixelCountPerFrame("Pixel Count Per Frame", Float) = 0 


        [Toggle]
        _ROOT_MOTION("Apply Root Motion", Float) = 0
        [NoScaleOffset]_AnimLoopTex("Animation Loop Texture", 2D) = "white" {}
        _LoopStartFrame("Loop Start Frame", Float) = 0
        _LoopMax("Loop Max", FLoat) = 1
        _LoopNum ("Loop Num", Float) = 1
        

        [Toggle]
        _PAUSE("Pause Motion", Float) = 0

        [KeywordEnum(UNLIT, REAL)]
        _LIGHTING("Lighting", Float) = 0
    }

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

        UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
    #define _Color_arr Props
    
    UNITY_INSTANCING_BUFFER_END(Props)

    sampler2D _MainTex;
    float4 _MainTex_ST;
    
    sampler2D _NormalMap;

    sampler2D _AnimTex;
    float4 _AnimTex_TexelSize;

    sampler2D _AnimLoopTex;
    float4 _AnimLoopTex_TexelSize;

    uint _PixelCountPerFrame;  

    uint _ROOT_MOTION;
    uint _LoopStartFrame;
    uint _LoopMax;   
    uint _LoopNum;

    uint _PAUSE;
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

#ifdef _PAUSE_ON
            time = 0;
#endif


        uint offsetFrame = (uint)((time + offsetSeconds) * 30.0);
        uint currentFrame = startFrame + offsetFrame % frameCount;


        uint clampedIndex = currentFrame * _PixelCountPerFrame;
        float4x4 bone1Matrix = GetMatrix(clampedIndex, v.boneIndex.x, _AnimTex, _AnimTex_TexelSize);
        float4x4 bone2Matrix = GetMatrix(clampedIndex, v.boneIndex.y, _AnimTex, _AnimTex_TexelSize);
        float4x4 bone3Matrix = GetMatrix(clampedIndex, v.boneIndex.z, _AnimTex, _AnimTex_TexelSize);
        float4x4 bone4Matrix = GetMatrix(clampedIndex, v.boneIndex.w, _AnimTex, _AnimTex_TexelSize);

        float4 pos = 
            mul(bone1Matrix, v.vertex) * v.boneWeight.x + 
            mul(bone2Matrix, v.vertex) * v.boneWeight.y + 
            mul(bone3Matrix, v.vertex) * v.boneWeight.z + 
            mul(bone4Matrix, v.vertex) * v.boneWeight.w;


        float4 normal = 
            mul(bone1Matrix, v.normal) * v.boneWeight.x +  
            mul(bone2Matrix, v.normal) * v.boneWeight.y +  
            mul(bone3Matrix, v.normal) * v.boneWeight.z+  
            mul(bone4Matrix, v.normal) * v.boneWeight.w;  

#ifdef _ROOT_MOTION_ON
        uint loopNum = max(1, _LoopNum);
        uint currentLoopIndex =  (uint)(offsetFrame / frameCount) % loopNum;
        uint currentLoopFrame = (currentLoopIndex == 0)? 0 :  _LoopStartFrame + currentLoopIndex - 1;
        uint clampedLoopIndex = currentLoopFrame * 3;
        float4x4 rootMatrix = GetMatrix(clampedLoopIndex, 0, _AnimLoopTex, _AnimLoopTex_TexelSize);

        pos = mul(rootMatrix, pos);
        normal = mul(rootMatrix, normal);
#endif 


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
            half3 normal = UnpackNormal(tex2D(_NormalMap, i.uv));
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
            #pragma shader_feature _PAUSE_ON
            #pragma shader_feature _ROOT_MOTION_ON

            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}
            Zwrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag _shadow
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #pragma target 4.5
            #pragma shader_feature _PAUSE_ON
            #pragma shader_feature _ROOT_MOTION_ON
            


            ENDCG

        }
    }

}
