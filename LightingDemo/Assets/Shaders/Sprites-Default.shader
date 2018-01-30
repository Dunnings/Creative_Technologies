// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "2DLighting/SS2DSS"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_SpriteLightness("Sprite Lightness", Float) = 0.1
		_LightMultiplier("Light Multiplier", Float) = 0.1
		_LightTex("Light Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Toggle] _ScreenSpaceLighting("Screen Space Lighting", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
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
			#pragma target 3.0
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
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 scrPos : TEXCOORD1;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				//Used to give the screen position of the vertex
				OUT.scrPos = ComputeScreenPos(OUT.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
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
			//This samples the light texture
			fixed4 SampleLightTexture(float2 uv)
			{
				fixed4 color = tex2D(_LightTex, uv);
				return color;
			}

			fixed4 _Color;
			float _ScreenSpaceLighting;
			float _LightMultiplier;
			float _SpriteLightness;
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 lc = SampleLightTexture(IN.scrPos);
				lc *= _LightMultiplier;
				fixed4 c = SampleSpriteTexture(IN.texcoord);
				c *= _Color;
				//lc.r 0 = no light
				//lc.r 1 = full light
				if (_ScreenSpaceLighting != 0) {
					c.rgb *= _SpriteLightness;
					c.rgb += lc.rgb * _LightMultiplier;
				}
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
}