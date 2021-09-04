Shader "CRTUpdate"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            // インスペクタに表示したときにわかりやすいように名前を付けておく
            Name "Update"

            CGPROGRAM
            
            // UnityCustomRenderTexture.cgincをインクルードする
           #include "UnityCustomRenderTexture.cginc"
           #include "AnimationGpuInstancing.cginc"

            // 頂点シェーダは決まったものを使う
           #pragma vertex CustomRenderTextureVertexShader
           #pragma fragment frag

            // v2f構造体は決まったものを使う
            half4 frag(v2f_customrendertexture i) : SV_Target
            {
                float2 uv = i.globalTexcoord;

                float du = 1.0 / _CustomRenderTextureWidth;
                float dv = 1.0 / _CustomRenderTextureHeight;

                float4 texelSize = float4(du, dv, _CustomRenderTextureWidth, _CustomRenderTextureHeight);

                int idx = 0;
                float2 uv0 = GetUV(0, texelSize).xy;
                float2 uv1 = GetUV(1, texelSize).xy;


                float4 color = (float4)0.0;
                color.r = (uv.x == uv0.x && uv.y == uv0.y) ? 1.0 : 0.0;
                color.r = (uv.x == uv1.x && uv.y == uv1.y) ? 1.0 : color.r;
                





                

                return color;
                //return half4(_SinTime.z * 0.5 + 0.5, _CosTime.w * 0.5 + 0.5, _SinTime.w * 0.5 + 0.5, 1);
            }

            ENDCG
        }
    }
}