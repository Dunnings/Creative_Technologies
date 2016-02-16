Shader "Sprites/SS2DLit"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_LightStrength("Lighting Strength", Float) = 0.1
		_SpriteLightness("Sprite Lightness", Float) = 0.1
		_LightTex("Light Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
				//Used to give the screen position of the vertex
				OUT.scrPos = ComputeScreenPos(OUT.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				return color;
			}

			sampler2D _LightTex;
			float _LightStrength;
			//This samples the light texture
			fixed4 SampleLightTexture(float2 uv)
			{
				fixed4 color = tex2D(_LightTex, uv) * _LightStrength;
				return color;
			}

			float _SpriteLightness;
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 lc = SampleLightTexture(IN.scrPos);
				fixed4 c = SampleSpriteTexture(IN.texcoord);
				float mod = lc.r + lc.g + lc.b;
				mod /= 3;
				if (mod == 0) {
					c.rgb *= _SpriteLightness;
				}
				return c;
			}
		ENDCG
		}
	}
}
