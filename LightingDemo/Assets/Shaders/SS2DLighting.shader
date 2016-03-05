Shader "Unlit/Screen Position"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_LightTex("Texture", 2D) = "black" {}
	}
		SubShader
	{
		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0

		// note: no SV_POSITION in this struct
	struct v2f {
		float2 uv : TEXCOORD0;
	};

	v2f vert(	
		float4 vertex : POSITION, // vertex position input
		float2 uv : TEXCOORD0, // texture coordinate input
		out float4 outpos : SV_POSITION // clip space position output
		)
	{
		v2f o;
		o.uv = uv;
		outpos = mul(UNITY_MATRIX_MVP, vertex);
		return o;
	}

	sampler2D _MainTex;
	sampler2D _LightTex;

	fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
	{
		screenPos.xy /= screenPos.w;
		fixed4 c = tex2D(_LightTex, screenPos.xy);
		/*fixed4 c = tex2D(_MainTex, i.uv);
		c.rgb *= 0.1f;
		c.rgb += tex2D(_LightTex, screenPos.xy).rgb;
		c.rgb *= c.a;*/
		return c;
	}
		ENDCG
	}
	}
}