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
