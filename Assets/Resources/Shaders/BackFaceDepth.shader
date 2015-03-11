Shader "kode80/BackFaceDepth" 
{
	SubShader 
	{
        Pass 
        {
        	Tags { "RenderType"="Opaque" }
			Cull front
			
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			#include "UnityCG.cginc"

            struct v2f {
			    float4 position : POSITION;
			    float4 linearDepth : TEXCOORD0;
			};
			
			v2f vert( appdata_base v) 
			{
			    v2f output;
			    output.position = mul( UNITY_MATRIX_MVP, v.vertex);
			    output.linearDepth.z = COMPUTE_DEPTH_01;
			    return output;
			}
			
			float4 frag( v2f input) : COLOR 
			{
				return float4( input.linearDepth.z);
			}

            ENDCG
        }
    }
}
