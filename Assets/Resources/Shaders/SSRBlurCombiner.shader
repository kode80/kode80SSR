Shader "kode80/SSRBlurCombiner" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
			ZTest Always Cull Off ZWrite Off
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _BlurTex;
			
			sampler2D _CameraGBufferTexture0;	// Diffuse color (RGB), unused (A)
			sampler2D _CameraGBufferTexture1;	// Specular color (RGB), roughness (A)
			sampler2D _CameraGBufferTexture2;	// World space normal (RGB), unused (A)
			sampler2D _CameraGBufferTexture3;	// ARGBHalf (HDR) format: Emission + lighting + lightmaps + reflection probes buffer
			
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_img v)
			{
			   	v2f o;
				o.position = mul( UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				
			   	return o;
			}
			
			half4 frag (v2f input) : COLOR
			{
				#ifdef DEBUG_SPEC
				return tex2D( _CameraGBufferTexture1, input.uv);
				#endif
				
				#ifdef DEBUG_ROUGH
				float roughness = tex2D( _CameraGBufferTexture1, input.uv).a;
				return half4( roughness);
				#endif
				
				half4 originalPixel = tex2D( _MainTex, input.uv);
				half4 blurPixel = tex2D( _BlurTex, input.uv);
				
				return half4( (blurPixel.rgb * blurPixel.a) + (originalPixel.rgb * (1.0 - blurPixel.a)), 1.0);
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
