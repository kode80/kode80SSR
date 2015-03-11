Shader "kode80/BilaturalBlur" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass {
			ZTest Always Cull Off ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler2D _CameraGBufferTexture1;	// Specular color (RGB), roughness (A)
			sampler2D _CameraGBufferTexture2;	// World space normal (RGB), unused (A)
			float3 _TexelOffsetScale;

			float _DepthBias;
			float _NormalBias;
			
			const float weights[3] = { 0.071303, 0.131514, 0.189879 };
	
			inline half compareNormalAndDepth( float3 sourceNormal, float sourceDepth, float2 uv)
			{
			    float3 otherNormal = tex2D( _CameraGBufferTexture2, uv).rgb * 2.0 - 1.0;
			    float otherDepth = Linear01Depth( tex2D( _CameraDepthTexture, uv).r) * _ProjectionParams.z;
				
				float3 normalDelta = abs( otherNormal - sourceNormal);
				float depthDelta = abs( otherDepth - sourceDepth);
				
				return step( normalDelta.x + normalDelta.y + normalDelta.z, _NormalBias) * step( depthDelta, _DepthBias);
			}
			
			inline void processSample( float2 uv, 
						 			   float3 sourceNormal,
						 			   float sourceDepth,
									   float i, 
									   float sampleCount, 
									   float2 stepSize, 
								  	   inout half4 accumulator, 
								  	   inout half denominator)
			{
				float2 offsetUV = stepSize * i + uv;
				half isSame = compareNormalAndDepth( sourceNormal, sourceDepth, offsetUV);
		        half coefficient = weights[ sampleCount - abs( i)] * isSame;
		        accumulator += tex2D( _MainTex, offsetUV) * coefficient;
		        denominator += coefficient;
			}


			half4 frag( v2f i) : Color
			{
				half4 specularSmoothPixel = tex2D( _CameraGBufferTexture1, i.uv);
			    float3 sourceNormal = tex2D( _CameraGBufferTexture2, i.uv).rgb * 2.0 - 1.0;
			    float sourceDepth = Linear01Depth( tex2D( _CameraDepthTexture, i.uv).r) * _ProjectionParams.z;
			    
			    float2 stepSize = _TexelOffsetScale.xy * (1.0 - specularSmoothPixel.a);
			    half4 accumulator = tex2D( _MainTex, i.uv) * 0.214607;
			    half denominator = 0.214607;
			   
			    processSample( i.uv, sourceNormal, sourceDepth, 1, 3, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 2, 3, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 3, 3, stepSize, accumulator, denominator);
			    
			    processSample( i.uv, sourceNormal, sourceDepth, -1, 3, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -2, 3, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -3, 3, stepSize, accumulator, denominator);
			    
			    return accumulator / denominator;
			}
			ENDCG
		} 
	}
	FallBack "Diffuse"
}
