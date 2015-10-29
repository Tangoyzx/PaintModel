Shader "Unlit/PaintingShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 p0;
			float4 p1;
			float4 p2;
			float4 p3;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 mid;
				float3 newUV;
				
				if (i.uv.x + i.uv.y >= 1) {
					mid = i.uv;
					newUV = p0 * (1 - mid.x - mid.y) + p1 * mid.x + p2 * mid.y;
				} else {
					mid = float2(1 - i.uv.y, i.uv.x + i.uv.y - 1);
					newUV = p2 * (1 - mid.x - mid.y) + p1 * mid.x + p3 * mid.y;
				}

				return tex2D(_MainTex, newUV.xy / newUV.zz);
			}

			ENDCG
		}
	}
}
