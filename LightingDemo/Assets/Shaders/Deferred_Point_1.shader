Shader "2DLighting/Deferred_Point_1"
{

	Properties
	{
		//The texture to write the output to
		[PerRendererData] _MainTex("Output Texture", 2D) = "black" {}
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
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				return OUT;
			}

			//Samples input texture at given UV coordinates and returns the colour
			sampler2D _MainTex;
			fixed4 SampleMainTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				return color;
			}

			//Main fragment shader
			fixed4 frag(v2f IN) : SV_Target
			{
				//Height of camera render texture
				float height = 512;
				float y = 0;
				//Used to store the closest occluding pixel's distance from the center in normalized coordinates of 0.0 - 1.0
				float distance = 1.0;

				//Break the next loop into two sub-loops due to HLSL's limitation on loops with greater than 1024 iterations
				//Loop through the first half of the render texture
				[unroll(512)]
				while(y < height)
				{
					//Translate the texture coordinate
					float2 coord = float2(IN.texcoord[0], y / height) * 2.0 - 1.0;
					//Calculate Theta
					float theta = (3.14)*1.5 + coord[0] * (3.14);

					//Calculate R
					float r = (1.0 + coord[1]) * 0.5;

					//Coordinate to sample from input render texture
					float2 newCoord = float2(-r * sin(theta), -r * cos(theta)) / 2.0 + 0.5;

					//Sample the input render texture at the new coordinate	
					fixed4 data = SampleMainTexture(newCoord);

					//Calculate the distance from the top of the render texture
					float currentDistance = y / height;

					//If we have hit an occluder pixel, update distance to be the minimum value between the current distance and the recorded distance
					if (!(data.r == 0.0 && data.g == 1.0 && data.b == 0.0)) {
						distance = min(distance, currentDistance);
						break;
					}
					y++;
				}
				distance += 0.01;
				//Return the final colour using the final calculated distance
				return fixed4(distance,distance,distance, 1.0);
			}
		ENDCG
		}
	}
}
