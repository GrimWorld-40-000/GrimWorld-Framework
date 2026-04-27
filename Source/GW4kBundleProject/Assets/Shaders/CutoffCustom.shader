Shader "Unlit/CutoffCustom"
{
	Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex ("Texture", 2D) = "black" {}
		_Color ("ColorA", Vector) = (1,0,0,1)
		_ColorTwo ("ColorB", Vector) = (0,1,0,1)
		_ColorThree ("ColorC", Vector) = (0,0,1,1)
	}
	SubShader
    {
		Tags { "RenderType" = "Opague" }
        
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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;

            float4 _Color;
            float4 _ColorTwo;
            float4 _ColorThree;

			fixed4 frag(v2f i) : SV_Target
			{
				float4 _MainTexColor = tex2D(_MainTex, i.uv);
				float4 _MaskTexColor = tex2D(_MaskTex, i.uv);
				float4 col = _MainTexColor;

				float r = _MaskTexColor.r;
				float g = _MaskTexColor.g;
				float b = _MaskTexColor.b;
                float a = 1 - r - g - b;

                col *= _Color * r + _ColorTwo * g + _ColorThree * b + a;

                clip(col.a - 0.5f);
				return col;
			}
			ENDCG
        }
	}
}