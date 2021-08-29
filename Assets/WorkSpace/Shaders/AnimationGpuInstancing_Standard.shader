Shader "Custom/AnimationGpuInstancing_Standard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [NoScaleOffset]_AnimTex("Animation Texture", 2D) = "white" {}
        _StartFrame("Start Frame", Int) = 0 
        _EndFrame("End Frame", Int) = 0 
        _FrameCount("Frame Count", Int) = 1 
        _OffsetSeconds("Offset Seconds", Float) = 0 
        _PixelCountPerFrame("Pixel Count Per Frame", Int) = 0 

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert

        #pragma multi_compile_instancing
        #pragma target 4.5 // [TODO] why is this target used?

        #include "AnimationGpuInstancing.cginc"

        sampler2D _MainTex;
        // sampler2D _AnimTex;
        // float4 _AnimTex_TexelSize;

        int _PixelCountPerFrame;


        struct Input
        {
            float2 uv_MainTex;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        half _Glossiness;
        half _Metallic;

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(int, _StartFrame)
#define _StartFrame_arr Props 
            UNITY_DEFINE_INSTANCED_PROP(int, _EndFrame)
#define _EndFrame_arr Props
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameCount)
#define _FrameCount_arr Props
            UNITY_DEFINE_INSTANCED_PROP(int, _OffsetSeconds)
#define _OffsetSeconds_arr Props

            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
#define _Color_arr Props
        UNITY_INSTANCING_BUFFER_END(Props)



        struct appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            half4 boneIndex : TEXCOORD2;
            fixed4 boneWeight : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };


        void vert (inout appdata v, out Input o)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_OUTPUT(Input, o);

            int startFrame = UNITY_ACCESS_INSTANCED_PROP(_StartFrame_arr, _StartFrame);
            int endFrame = UNITY_ACCESS_INSTANCED_PROP(_EndFrame_arr, _EndFrame);
            int frameCount = UNITY_ACCESS_INSTANCED_PROP(_FrameCount_arr, _FrameCount);
            float offsetSeconds = UNITY_ACCESS_INSTANCED_PROP(_OffsetSeconds_arr, _OffsetSeconds);

            int offsetFrame = (int)((_Time.y + offsetSeconds) * 30.0);
            int currentFrame = startFrame + offsetFrame % frameCount;

            int clampedIndex = currentFrame * _PixelCountPerFrame;

            float4x4 bone1Matrix = GetMatrix(clampedIndex, v.boneIndex.x);
            float4x4 bone2Matrix = GetMatrix(clampedIndex, v.boneIndex.y);
            float4x4 bone3Matrix = GetMatrix(clampedIndex, v.boneIndex.z);
            float4x4 bone4Matrix = GetMatrix(clampedIndex, v.boneIndex.w);

            float4 pos = 
                mul(bone1Matrix, v.vertex) * v.boneWeight.x + 
                mul(bone2Matrix, v.vertex) * v.boneWeight.y + 
                mul(bone3Matrix, v.vertex) * v.boneWeight.z + 
                mul(bone4Matrix, v.vertex) * v.boneWeight.w; 
            
            float4 normal = 
                mul(bone1Matrix, v.normal) * v.boneWeight.x + 
                mul(bone2Matrix, v.normal) * v.boneWeight.y + 
                mul(bone3Matrix, v.normal) * v.boneWeight.z + 
                mul(bone4Matrix, v.normal) * v.boneWeight.w; 
            
            v.vertex = pos;
            v.normal = normal;
                
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
