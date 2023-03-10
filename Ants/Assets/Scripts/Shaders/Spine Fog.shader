Shader "Unlit/SpineFog"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_Color ("Color", Color) = (1,1,1,1)
        _Desaturate ("Desaturate", float) = 0
        _Darken ("Darken", float) = 0

		_StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8		// Set to Always as default
	}
	SubShader
	{
		// Tags { "RenderType"="Opaque" }								// default
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }		// respect Sorting Layers
		ZWrite Off														// respect Sorting Layers

		// Blend SrcAlpha OneMinusSrcAlpha								// normal blending
		Blend One OneMinusSrcAlpha										// premultiplied alpha blending

		LOD 100

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

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
				float4 vertexColor : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 vertexColor : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Color;
			uniform float _Desaturate;
			uniform float _Darken;


			// Converts color to luminance (grayscale)
			// https://forum.unity.com/threads/desaturation-grayscale-shader-for-ios.82105/
			float Luminance( float3 c )
			{
				return dot( c, float3( .22, .707, .071 ) );
			}

			v2f vert( appdata v )
			{
				v2f o;
				o.vertex		= UnityObjectToClipPos( v.vertex );
				o.uv			= TRANSFORM_TEX( v.uv, _MainTex );
				o.vertexColor	= v.vertexColor;

				return o;
			}

			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 col		= tex2D( _MainTex, i.uv );
				col.rgb			= lerp( col.rgb, _Color.rgb * col.a, _Color.a );
				col				*= i.vertexColor.a;

				col.rgb			*= (1 - _Darken);
				col.rgb			= lerp( col.rgb, Luminance( col.rgb ), _Desaturate );

				return col;
			}
			ENDCG
		}
	}
}

