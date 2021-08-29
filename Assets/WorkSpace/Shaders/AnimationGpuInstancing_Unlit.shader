Shader "Unlit/AnimationGpuInstancing"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}


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

            #include "UnityCG.cginc"
            #include "AnimationGpuInstancing.cginc"

       
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                half4 boneIndex : TEXCOORD2;
                fixed4 boneWeight : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL;
            };

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _PixelCountPerFrame;     

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                // UNITY_INITIALIZE_OUTPUT(Input, o);

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
                
                o.vertex = UnityObjectToClipPos(pos);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

    }
}
