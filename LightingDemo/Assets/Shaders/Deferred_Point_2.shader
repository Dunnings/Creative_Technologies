// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "2DLighting/Deferred_Point_2"
{

	Properties
	{
		//The texture to write the output to
		[PerRendererData] _MainTex("Output Texture", 2D) = "black" {}
		//The one dimensional depth map
		_DepthMap("Depth Map", 2D) = "black" {}
		//The colour of the light
		_Color("Light Colour", Color) = (1,1,1,1)
		//Apply gradient to light (0=false, 1=true)
		_GradientFalloff("Gradient Falloff", Int) = 1
		//Apply penumbra to light (0=false, 1=true)
		_Penumbra("Penumbra", Int) = 1
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
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				return OUT;
			}
			
			//Samples depth map at given UV coordinates and returns the colour
			sampler2D _DepthMap;
			fixed4 SampleDepthMap(float2 uv)
			{
				fixed4 color = tex2D(_DepthMap, uv);
				return color;
			}

			//Samples depth map at given UV coordinates and returns 1 if the given float value is greater than the red value of the sample
			float sample(float2 UV, float r) {
				return step(r, SampleDepthMap(UV).r);
			}

			//Main fragment shader
			int _GradientFalloff;
			int _Penumbra;
			fixed4 frag(v2f IN) : SV_Target
			{
				//Translate the texture coordinate
				float2 coord = IN.texcoord * 2.0 - 1.0;
				//Calculate Theta
				float theta = atan2(-coord.y, coord.x);

				/*if (theta > 3.14 / 5.0 || theta < -(3.14 / 5.0)) {
					return fixed4(0.0, 0.0, 0.0, 0.0);
				}*/

				//Calculate R
				float r = length(coord);
				//Calculate the polar coordinate from Theta and R
				float newCoord = (theta + (3.14)) / (2.0*(3.14));

				//The texture coordinate to sample from the depth map
				float2 tc = (newCoord, newCoord);

				//The center texture coordinate
				float center = sample(tc, r);

				float sum = center;

				//If penumbra is set to true (0=false, 1=true)
				//Create the penumbra for the light using a Gaussian blur
				if (_Penumbra == 1) {
					float blur = (1.0 / 256) * smoothstep(0.0, 1.0, r);
					sum = 0.0;

					sum += sample(float2(tc.x - 4.0*blur, tc.y), r) * 0.05;
					sum += sample(float2(tc.x - 3.0*blur, tc.y), r) * 0.09;
					sum += sample(float2(tc.x - 2.0*blur, tc.y), r) * 0.12;
					sum += sample(float2(tc.x - 1.0*blur, tc.y), r) * 0.15;

					sum += center * 0.16;

					sum += sample(float2(tc.x + 1.0*blur, tc.y), r) * 0.15;
					sum += sample(float2(tc.x + 2.0*blur, tc.y), r) * 0.12;
					sum += sample(float2(tc.x + 3.0*blur, tc.y), r) * 0.09;
					sum += sample(float2(tc.x + 4.0*blur, tc.y), r) * 0.05;
				}

				//If gradient falloff is set to true (0=false, 1=true)
				if (_GradientFalloff == 1) {
					//The final value scaled between 0 and 1 depending on the distance from the center
					sum *= smoothstep(1.0, 0.0, r);
				}

				//Create final colour by using the calculated value as the red, green, blue and alpha channel value
				//Then multiply by the light colour
				fixed4 c = fixed4(sum, sum, sum, sum) * IN.color;

				//Multiply colour by alpha
				c.rgb *= c.a;

				//Return the final colour
				return c;
			}
		ENDCG
		}
	}
}
