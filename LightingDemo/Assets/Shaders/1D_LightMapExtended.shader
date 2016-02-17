Shader "2DLighting/Deferred_2_Extended"
{

	Properties
	{
		[PerRendererData] _MainTex("Output Texture", 2D) = "black" {}
		_LightData("Input Texture", 2D) = "black" {}
		_Color("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 scrPos : SCRCOORD;
			};

			fixed4 _Color;
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				OUT.scrPos = ComputeScreenPos(OUT.vertex);
				return OUT;
			}
			
			sampler2D _LightData;
			fixed4 SampleMainTexture(float2 uv)
			{
				fixed4 color = tex2D(_LightData, uv);
				float add = 0.2;
				color += fixed4(add, add, add, 0.0);
				return color;
			}

			float sample(float2 coord, float r) {
				return step(r, SampleMainTexture(coord).r);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 norm = IN.texcoord * 2.0 - 1.0;
				float theta = atan2(-norm.y, norm.x);
				float r = length(norm);
				float coord = (theta + (3.14)) / (2.0*(3.14));
				//return SampleMainTexture(coord);
				//the tex coord to sample 1D lookup texture 
				//always 0.0 on y axis
				float2 tc = (coord, coord);

				//the center tex coord, which gives hard shadows
				float center = sample(tc, r);

				//we multiply the blur amount by our distance from center
				//this leads to more blurriness as the shadow "fades away"
				float blur = (1.0 / 256) * smoothstep(0.0, 1.0, r);

				////now we use a simple gaussian blur
				float sum = 0.0;

				sum += sample(float2(tc.x - 4.0*blur, tc.y), r) * 0.05;
				sum += sample(float2(tc.x - 3.0*blur, tc.y), r) * 0.09;
				sum += sample(float2(tc.x - 2.0*blur, tc.y), r) * 0.12;
				sum += sample(float2(tc.x - 1.0*blur, tc.y), r) * 0.15;

				sum += center * 0.16;

				sum += sample(float2(tc.x + 1.0*blur, tc.y), r) * 0.15;
				sum += sample(float2(tc.x + 2.0*blur, tc.y), r) * 0.12;
				sum += sample(float2(tc.x + 3.0*blur, tc.y), r) * 0.09;
				sum += sample(float2(tc.x + 4.0*blur, tc.y), r) * 0.05;

				//sum of 1.0 -> in light, 0.0 -> in shadow

				//multiply the summed amount by our distance, which gives us a radial falloff
				//then multiply by vertex (light) color  
				//gl_FragColor = vColor * vec4(vec3(1.0), sum * smoothstep(1.0, 0.0, r));
				float v = sum*smoothstep(1.0, 0.0, r);

				fixed4 c = fixed4(v, v, v, v) * IN.color;
				c.rgb *= c.a;
				return c;
				//return fixed4(1.0, 1.0, 1.0, 1.0) * fixed4(v, v, v, 1-v);
			}
		ENDCG
		}
	}
}
