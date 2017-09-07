// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Pillar Shader"
{
	Properties
	{
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Map", 2D) = "white" {}
		[NoScaleOffset] _Red("Red", 2D) = "white" {}
		[NoScaleOffset] _RedNormal("Red Normal", 2D) = "bump" {}
	    [NoScaleOffset] _Green("Green", 2D) = "white" {}
		[NoScaleOffset] _GreenNormal("Green Normal", 2D) = "bump" {}
		[NoScaleOffset] _Blue ("Blue", 2D) = "white" {}
		[NoScaleOffset] _BlueNormal("Blue Normal", 2D) = "bump" {}
		[NoScaleOffset] _Black("Black", 2D) = "white" {}
		[NoScaleOffset] _BlackNormal("Black Normal", 2D) = "bump" {}
		_BumpScale("Bump Scale", Range(-2,2)) = 1
		_Color("Color Tint", Color) = (1.0,1.0,1.0,1.0)
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(0, 1)) = 0.1

	}
	SubShader
	{
		Tags{ "Queue" = "Transparent-1" "RenderType" = "Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha
		//AlphaTest Greater 0

		CGINCLUDE

		#define BINORMAL_PER_FRAGMENT

		ENDCG

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma shader_feature _RENDERING_CUTOUT	
			#pragma vertex vert
			#pragma fragment frag

			#define FORWARD_BASE_PASS


			#include "Lighting.cginc"

			ENDCG
		}

		Pass
		{
			Tags{ "LightMode" = "ForwardAdd" }

			Blend One One
			Zwrite Off

			CGPROGRAM

			#pragma target 3.0
	
			#pragma multi_compile_fwdadd_fullshadows
#pragma shader_feature _RENDERING_CUTOUT
			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			ENDCG
		}

		Pass
		{
			Tags{"LightMode" = "ShadowCaster"}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster

			#pragma vertex vert
			#pragma fragment frag

			#include "Shadows.cginc"

			ENDCG
		}
	}
}
