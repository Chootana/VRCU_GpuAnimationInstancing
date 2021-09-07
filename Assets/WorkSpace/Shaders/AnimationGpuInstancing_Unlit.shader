Shader "AnimationGpuInstancing/Unlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Shininess ("Shininess", Range(0.0, 1.0)) = 0.078125

        [NoScaleOffset]_AnimTex("Animation Texture", 2D) = "white" {}
        _StartFrame("Start Frame", Float) = 0 
        _EndFrame("End Frame", Float) = 0 
        _FrameCount("Frame Count", Float) = 1 
        _OffsetSeconds("Offset Seconds", Float) = 0 
        _PixelCountPerFrame("Pixel Count Per Frame", Float) = 0 

        _Loop ("Loop", Float) = 10

        [Toggle] _DEBUG("DEBUG", Float) = 0
    }

    CGINCLUDE

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
        float4 tangent : TANGENT;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
        float4 normal : NORMAL;
        float3 lightDir : TEXCOORD2;
        float3 viewDir : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
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
    
    sampler2D _NormalMap;

    sampler2D _AnimTex;
    float4 _AnimTex_TexelSize;

    int _PixelCountPerFrame;     
    int _Loop;

    int _DEBUG;
    half _Shininess;
    float4 _LightColor0;

    v2f vert (appdata v)
    {
        v2f o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);

        int startFrame = UNITY_ACCESS_INSTANCED_PROP(_StartFrame_arr, _StartFrame);
        int endFrame = UNITY_ACCESS_INSTANCED_PROP(_EndFrame_arr, _EndFrame);
        int frameCount = UNITY_ACCESS_INSTANCED_PROP(_FrameCount_arr, _FrameCount);
        float offsetSeconds = UNITY_ACCESS_INSTANCED_PROP(_OffsetSeconds_arr, _OffsetSeconds);

        float time = _Time.y;

        time = (_DEBUG) ? 0 : time;


        int offsetFrame = (int)((time + offsetSeconds) * 30.0);
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
            mul(bone1Matrix, v.vertex) * v.boneWeight.x + 
            mul(bone2Matrix, v.vertex) * v.boneWeight.y + 
            mul(bone3Matrix, v.vertex) * v.boneWeight.z + 
            mul(bone4Matrix, v.vertex) * v.boneWeight.w;

        pos = mul(rootMatrix, pos);

        float4 normal = 
            mul(bone1Matrix, v.normal) * v.boneWeight.x +  
            mul(bone2Matrix, v.normal) * v.boneWeight.y +  
            mul(bone3Matrix, v.normal) * v.boneWeight.z+  
            mul(bone4Matrix, v.normal) * v.boneWeight.w;  

        normal = mul(rootMatrix, normal);
        
        o.vertex = UnityObjectToClipPos(pos);
        UNITY_TRANSFER_FOG(o,o.vertex);
        o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.normal = normal;

        TANGENT_SPACE_ROTATION;
        o.lightDir = normalize(mul(rotation, ObjSpaceLightDir(v.vertex)));
        o.viewDir = normalize(mul(rotation, ObjSpaceViewDir(v.vertex)));

        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        float4 _Col = UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
        fixed4 tex = tex2D(_MainTex, i.uv);

        half3 halfDir = normalize(i.lightDir + i.viewDir);
        half3 normal = UnpackNormal(tex2D(_NormalMap, i.uv));
        half4 diff = saturate(dot(normal, i.lightDir)) * _LightColor0;

        half3 spec = pow(max(0, dot(normal, halfDir)), _Shininess * 128.0) * _LightColor0.rgb * tex.rgb;


        fixed4 col = tex * _Col;
        col.rgb = col.rgb * diff + spec;

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


            ENDCG

        }
    }

}
