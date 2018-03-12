Shader "Tutorial/Display UVs"
{
	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 color : COLOR0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color.rg = v.texcoord.rg;
				o.color.b = 0;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return half4(i.color, 1);
			}

			ENDCG
		}
	}
	Fallback "VertexLit"
}