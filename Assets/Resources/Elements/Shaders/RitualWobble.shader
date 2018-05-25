Shader "Magic/RitualWobble"
{
	Properties
	{
		_Tint("Tint", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_Amount("Amount", Range(0,1)) = 0.05
		_Speed("Speed", Range(0,2)) = 1.0
		_Scale("Scale", Range(0,50)) = 1.0
		_Metallic("Metallic", Range(0,1)) = 1.0
		_Smoothness("Smoothness", Range(0,1)) = 0.5
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma surface surf Standard vertex:vert addshadow

		struct Input
		{
			float2 uv_MainTex;
		};

		float _Amount;
		float _Speed;
		float _Scale;
		float3 DisplaceVertex(float3 localpt)
		{
			float3 worldVertex = localpt;// mul(localpt, unity_ObjectToWorld).xyz;
			localpt.x += sin((_Time.y * _Speed + worldVertex.y + worldVertex.z) * _Scale) * _Amount;
			localpt.y += sin(((_Time.x + _Time.y) * _Speed + worldVertex.z + worldVertex.x) * _Scale) * _Amount;
			localpt.z += sin(((_Time.z + _Time.y) * _Speed + worldVertex.x + worldVertex.y) * _Scale) * _Amount;
			return localpt;
		}

		void vert(inout appdata_full v)
		{
			float3 position = DisplaceVertex(v.vertex.xyz);
			float3 nposition = DisplaceVertex(v.vertex.xyz + v.normal * 0.01);
			v.vertex = float4(position.xyz, v.vertex.w);
			v.normal = normalize(nposition - position);

			/*float3 bitangent = cross(v.normal, v.tangent.xyz) * v.tangent.w;

			float3 positionAndTangent = DisplaceVertex(v.vertex.xyz + v.tangent.xyz * 0.01);
			float3 positionAndBitangent = DisplaceVertex(v.vertex.xyz + bitangent * 0.01);

			float3 newTangent = (positionAndTangent - position);
			float3 newBitangent = (positionAndBitangent - position);
			v.normal = cross(newTangent, newBitangent);*/
		}

		float _Metallic;
		float _Smoothness;
		float4 _Tint;
		sampler2D _MainTex;
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Tint.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
		}
		ENDCG
	}
	Fallback "Diffuse"
}