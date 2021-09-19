Shader "Skybox/GradationSky"
{
	Properties
	{
		_TopColor("TopColor",Color) = (0,0,0,0)
		_UnderColor("UnderColor",Color) = (0,0,0,0)

		_ColorBorder("ColorBorder",Range(0,3)) = 0.5
	}

		SubShader
	{
		Tags
		{
			"RenderType" = "Background" 
			"Queue" = "Background" 
			"PreviewType" = "SkyBox" 
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _UnderColor;
			float4 _TopColor;
			float _ColorBorder;

			struct appdata
			{
				float4 vertex: POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : WORLD_POS;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.worldPos = v.vertex.xyz;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{

				float3 dir = normalize(i.worldPos);
				float2 rad = float2(atan2(dir.x, dir.z), asin(dir.y));
				float2 uv = rad / float2(2.0 * UNITY_PI, UNITY_PI / 2);

				return lerp(_UnderColor, _TopColor, uv.y + _ColorBorder);
			}
			ENDCG
		}
	}
}