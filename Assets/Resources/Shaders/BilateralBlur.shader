//  Copyright (c) 2015, Ben Hopkins (kode80)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
//  
//  1. Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//  
//  2. Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//  THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

 
Shader "kode80/BilateralBlur" 
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
			float _BlurQuality;
			
			const float weights[8] = { 0.071303, 0.131514, 0.189879, 0.321392, 0.452906,  0.584419, 0.715932, 0.847445 };
	
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
									   float _BlurQuality, //sampleCount
									   float2 stepSize, 
								  	   inout half4 accumulator, 
								  	   inout half denominator)
			{
				float2 offsetUV = stepSize * i + uv;
				half isSame = compareNormalAndDepth( sourceNormal, sourceDepth, offsetUV);
		        half coefficient = weights[ _BlurQuality - abs( i)] * isSame;
		        accumulator += tex2D( _MainTex, offsetUV) * coefficient;
		        denominator += coefficient;
			}


			half4 frag( v2f i) : Color
			{
				half4 specularSmoothPixel = tex2D( _CameraGBufferTexture1, i.uv);
			    float3 sourceNormal = tex2D( _CameraGBufferTexture2, i.uv).rgb * 2.0 - 1.0;
			    float sourceDepth = Linear01Depth( tex2D( _CameraDepthTexture, i.uv).r) * _ProjectionParams.z - 0.1;
			    
			    float2 stepSize = _TexelOffsetScale.xy * (1.0 - specularSmoothPixel.a);
			    half4 accumulator = tex2D( _MainTex, i.uv) * 0.214607;
			    half denominator = 0.214607;
			    
			    processSample( i.uv, sourceNormal, sourceDepth, 1, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 0.2, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 0.4, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 0.6, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 0.8, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 1.2, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 1.4, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 1.6, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 1.8, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, 1 * 2.0, _BlurQuality, stepSize, accumulator, denominator);
			    
			    processSample( i.uv, sourceNormal, sourceDepth, -1, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 0.2, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 0.4, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 0.6, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 0.8, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 1.2, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 1.4, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 1.6, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 1.8, _BlurQuality, stepSize, accumulator, denominator);
			    processSample( i.uv, sourceNormal, sourceDepth, -1 * 2.0, _BlurQuality, stepSize, accumulator, denominator);
			    
			    return accumulator / denominator;
			}
			ENDCG
		} 
	}
	FallBack "Diffuse"
}
