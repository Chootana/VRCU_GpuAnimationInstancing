Shader "AnimationGpuInstancing/Standard" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

        [NoScaleOffset]_AnimTex("Animation Texture", 2D) = "white" {}
        _StartFrame("Start Frame", Float) = 0 
        _EndFrame("End Frame", Float) = 0 
        _FrameCount("Frame Count", Float) = 1 
        _OffsetSeconds("Offset Seconds", Float) = 0 
        _PixelCountPerFrame("Pixel Count Per Frame", Int) = 0 

        _Loop ("Loop", Float) = 10
	}

	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#include "UnityCG.cginc"
        #include "AnimationGpuInstancing.cginc"

		#pragma multi_compile_instancing
		#pragma target 4.5

        sampler2D _MainTex;
        // float4 _MainTex_ST;
        int _PixelCountPerFrame;     
        sampler2D _AnimTex;
        float4 _AnimTex_TexelSize;

        int _Loop;
		
        struct Input {
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
			UNITY_DEFINE_INSTANCED_PROP(float, _OffsetSeconds)
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

		void vert(inout appdata v, out Input o) {
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);
			UNITY_INITIALIZE_OUTPUT(Input, o);

			int startFrame = UNITY_ACCESS_INSTANCED_PROP(_StartFrame_arr, _StartFrame);
			int endFrame = UNITY_ACCESS_INSTANCED_PROP(_EndFrame_arr, _EndFrame);
			int frameCount = UNITY_ACCESS_INSTANCED_PROP(_FrameCount_arr, _FrameCount);
			float offsetSeconds = UNITY_ACCESS_INSTANCED_PROP(_OffsetSeconds_arr, _OffsetSeconds);

            int offsetFrame = (int)((_Time.y + offsetSeconds) * 30.0);
            int currentFrame = startFrame + offsetFrame % frameCount;

            int loopMax = 20;
            int loopNum = _Loop;
            loopNum = min(_Loop, loopMax);
            
            int currentLoopIndex =  (int)(offsetFrame / frameCount) % loopNum;
            currentLoopIndex = (_Loop < 1) ? 0 : currentLoopIndex;

            int clampedIndex = currentFrame * _PixelCountPerFrame;
            int clampedLoopIndex = (endFrame + 1) * _PixelCountPerFrame + currentLoopIndex * 3;

            

            float4x4 bone1Matrix = GetMatrix(clampedIndex, v.boneIndex.x, _AnimTex, _AnimTex_TexelSize);
            float4x4 bone2Matrix = GetMatrix(clampedIndex, v.boneIndex.y, _AnimTex, _AnimTex_TexelSize);
            float4x4 bone3Matrix = GetMatrix(clampedIndex, v.boneIndex.z, _AnimTex, _AnimTex_TexelSize);
            float4x4 bone4Matrix = GetMatrix(clampedIndex, v.boneIndex.w, _AnimTex, _AnimTex_TexelSize);

            float4x4 rootMatrix = GetMatrix(clampedLoopIndex, 0, _AnimTex, _AnimTex_TexelSize);

            float4 pos = 
                mul(mul(rootMatrix, bone1Matrix), v.vertex) * v.boneWeight.x + 
                mul(mul(rootMatrix, bone2Matrix), v.vertex) * v.boneWeight.y + 
                mul(mul(rootMatrix, bone3Matrix), v.vertex) * v.boneWeight.z + 
                mul(mul(rootMatrix, bone4Matrix), v.vertex) * v.boneWeight.w;


            float4 normal = 
                mul(mul(rootMatrix, bone1Matrix), v.normal) * v.boneWeight.x +  
                mul(mul(rootMatrix, bone2Matrix), v.normal) * v.boneWeight.y +  
                mul(mul(rootMatrix, bone3Matrix), v.normal) * v.boneWeight.z+  
                mul(mul(rootMatrix, bone4Matrix), v.normal) * v.boneWeight.w;  


            // float4 pos = 
            //     mul(bone1Matrix, v.vertex) * v.boneWeight.x + 
            //     mul(bone2Matrix, v.vertex) * v.boneWeight.y + 
            //     mul(bone3Matrix, v.vertex) * v.boneWeight.z + 
            //     mul(bone4Matrix, v.vertex) * v.boneWeight.w; 
            
            // float4 normal = 
            //     mul(bone1Matrix, v.normal) * v.boneWeight.x + 
            //     mul(bone2Matrix, v.normal) * v.boneWeight.y + 
            //     mul(bone3Matrix, v.normal) * v.boneWeight.z + 
            //     mul(bone4Matrix, v.normal) * v.boneWeight.w; 
            
			v.vertex = pos;
			v.normal = normal;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
