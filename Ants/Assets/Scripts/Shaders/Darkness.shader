Shader "Unlit/DarkFX"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Desaturate ("Desaturate", float) = 0
        _Darken ("Darken", float) = 0
		_Voffset("V offset", float) = 0
	}
    SubShader
    {
        // Tags { "RenderType"="Opaque" }								// default
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }		// respect Sorting Layers
        ZWrite Off														// respect Sorting Layers
        Blend SrcAlpha OneMinusSrcAlpha

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
				fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			uniform float _Desaturate;
			uniform float _Darken;
			uniform float _Voffset;


			// Converts color to luminance (grayscale)
			// https://forum.unity.com/threads/desaturation-grayscale-shader-for-ios.82105/
			float Luminance( float3 c )
			{
				return dot( c, float3( .22, .707, .071 ) );
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex		= UnityObjectToClipPos(v.vertex);
                o.uv			= TRANSFORM_TEX(v.uv, _MainTex) + float2( 0, _Voffset );
				o.color			= v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col		= tex2D(_MainTex, i.uv);

				col.rgb			*= (1 - _Darken);
				col.rgb			= lerp( col.rgb, Luminance( col.rgb ), _Desaturate );
				col				*= i.color;

                return col;
            }
            ENDCG
        }
    }
}

