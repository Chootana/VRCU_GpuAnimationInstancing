Shader "VolumetricFog/1"
{
    Properties
    {
        _NearColor ("Near Color", Color) = (0, 0, 0, 1)
        _FogColor ("Fog Color", Color) = (0.1, 0.1, 0.1, 1)
        _NearOpacity("Near Opacity", Range(0, 1)) = 0.01
        _FarOpacity("Far Opacity", Range(0, 1)) = 0.1
        _BlurMaxDistance("Blur Max Distance", Float) = 10.0
        _BlurMax ("Blur Max", Range(0, 10)) = 0.1


        _Factor("Factor", Range(0, 1)) = 0
        _DepthStart("Depth Start ", Range(0, 100)) = 0
        _DepthEnd("Dpeth End", Range(0, 100)) = 10 

        _HeightStart("Height Start", Range(0, 100)) = 0 
        _HeightEnd("Height End", Range(0, 10)) = 1


        _NoiseTex("Noise Texture", 2D) = "white"{}
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest+400"  "LightMode"="ForwardBase"}
        
        LOD 100
        Ztest Always
        ZWrite Off 
        Blend SrcAlpha OneMinusSrcAlpha

        GrabPass {
            "_GrabTex"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/CGINC/Noises.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 projCoord : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float4 grabPos : TEXCOORD3;
            };

            sampler2D _GrabTex;
            sampler2D _CameraDepthTexture;
            sampler2D _NoiseTex;
            float4 _NoiseTex_TexelSize;

            float4 _NearColor;
            float _NearOpacity;
            float _FarOpacity;
            float4 _FogColor;
            float _BlurMaxDistance;
            float _BlurMax;

            float _DepthStart;
            float _DepthEnd;
            float _HeightStart;
            float _HeightEnd;

            float _Factor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(1-2*v.uv.x,2*v.uv.y-1,0,1);
                float3 localViewDir = mul(unity_CameraInvProjection, float4(o.vertex.x, -o.vertex.y, 0, 1)).xyz;
                o.viewDir = mul(transpose(UNITY_MATRIX_V), localViewDir);
                
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.projCoord = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                const float _DepthMaxDistance = 1000;

                float3 camPos = _WorldSpaceCameraPos;
                float3 camDir = unity_CameraToWorld._m02_m12_m22;
                float3 viewDir = normalize(i.viewDir);


                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.projCoord))/dot(camDir, viewDir);

                if (depth > _DepthMaxDistance) clip(-1);
                float3 worldPos = camPos + viewDir * depth;

                float depthNorm = smoothstep(_DepthStart, _DepthEnd, depth);

                float time = _Time.x * 1.0;
                float noise = 0.5 * PerlinNoise( worldPos * 0.01 + i.projCoord.xyz * 0.1+ float3( time, 0.0, time)) + 0.5;
                // noise = pow(noise, 2.0)
                // noise = 1.0;


                worldPos.y = (worldPos.y < _HeightStart) ? _HeightStart : worldPos.y;
                worldPos.y *= noise;
                float heightNorm = 1.0 - smoothstep(_HeightStart, _HeightEnd, worldPos.y);

                depthNorm = pow(depthNorm, 0.8);
                heightNorm = pow(heightNorm, 0.8);


                float fogRatio = depthNorm * (0.1 + heightNorm);
                fogRatio = lerp(0.0, fogRatio, noise);


                float4 grabCol = float4(0.0, 0.0, 0.0, 0.0);
                float blur = saturate(depth / _BlurMaxDistance) * _BlurMax;
                float2 dxGrabPos = ddx(i.grabPos);
                float2 dyGrabPos = ddy(i.grabPos);
                grabCol += tex2D(_GrabTex, (i.grabPos.xy + dxGrabPos * blur) / i.grabPos.w);
                grabCol += tex2D(_GrabTex, (i.grabPos.xy - dxGrabPos * blur) / i.grabPos.w);
                grabCol += tex2D(_GrabTex, (i.grabPos.xy + dyGrabPos * blur) / i.grabPos.w);
                grabCol += tex2D(_GrabTex, (i.grabPos.xy - dyGrabPos * blur) / i.grabPos.w);
                grabCol /= 4.0;


                // Debug for World Pos
                float3 lineDebugColor = pow(abs(cos(worldPos.xyz * UNITY_PI * 4)), 20);

                float4 col;
                // col = float4(lineDebugColor, 0.5);


                col = grabCol;
                float3 colFog = _NearColor.rgb  * fogRatio;
                // col.rgb = colFog;
                col.rgb = lerp(col.rgb, colFog.rgb, saturate(fogRatio));
                // col = lerp(col, _FogColor, 1 - exp(-fogRatio * _FarOpacity)); 



                return col;

            }
            ENDCG
        }
    }
}
