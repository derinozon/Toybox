Shader "Toybox/StandardHDRP" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] [Normal]_Normal("Normal", 2D) = "bump" {}
		_NormalStr("Normal Strength", Range(0,10)) = 1

		[NoScaleOffset] _Mask ("Mask", 2D) = "black" {}

		[NoScaleOffset] _Emission ("Emission", 2D) = "white" {}
		_EmissionStr ("Emission Strength", Range(0, 10)) = 1
		[HDR] _EmissionColor ("Emission Color", Color) = (0.0, 0.0, 0.0, 0.0)

		[NoScaleOffset] _HeightMap("Height Map", 2D) = "white" {}
		_HeightPower("Height Power", Range(0, 0.01)) = 0

		_OcclusionStrenght("Occlusion Strenght", Range(0, 1)) = 1

		// If no Mask Map //
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		_Metalness("Metalness", Range(0, 1)) = 0

		// Outline Stuff //
		_OutlineColor ("Outline Color", Color) = (1.0, 0.73, 0.0, 1.0)
		_OutlineWidth("Outline Width", Range(1, 1.1)) = 1.01

		// Transparency
		[Toggle] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
    }
	
    SubShader {
        Tags { 
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}
		LOD 200

		//////////////////
		// Outline Pass //
		//////////////////

		Pass {
			// Unity does not allow SetShaderPassEnabled with Name tag. //

			Name "OutlinePass"
			Tags { "LightMode" = "Always" } 
			//Tags { "LightMode" = "OutlinePass" }
			Zwrite Off

    		CGPROGRAM
			#pragma multi_compile __ OUTLINE_ON
			//#pragma shader_feature OUTLINE_ON

    		#pragma vertex vert
    		#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : POSITION;
				float3 normal : NORMAL;
			};

			#if OUTLINE_ON
			
			
			float _OutlineWidth;
			float4 _OutlineColor;

			v2f vert (appdata v) {
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				v.vertex.xyz *= _OutlineWidth;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f i) : COLOR {
				return _OutlineColor;
			}

			#else
			v2f vert () {
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = fixed4(0,0,0,0);
				return o;
			}

			fixed4 frag(v2f i) : COLOR0 {
				return fixed4(0,0,0,0);
			}
			#endif
            ENDCG
		}

		
		////////////////////
		// Object Drawing //
		////////////////////
		ZWrite [_ZWrite]
		Blend [_SrcBlend] [_DstBlend]

		// Tags { "RenderType" = "Opaque" }
		// LOD 200
		Cull Back

		CGPROGRAM	
		#pragma surface surf Standard fullforwardshadows keepalpha
		#include "StandardHDRPMain.cginc"
		ENDCG

		//////////////////
		// Double Sided //
		//////////////////
		ZWrite [_ZWrite]
		Blend [_SrcBlend] [_DstBlend]

		Tags { "RenderType" = "Opaque" }
		LOD 200
		Cull Front

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows keepalpha
		#pragma vertex vert
		#include "StandardHDRPMain.cginc"

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			v.normal = -v.normal;
		}
		ENDCG
    }
	SubShader {
		ZWrite [_ZWrite]
		Blend [_SrcBlend] [_DstBlend]

		Tags { "RenderType" = "Opaque" }
		LOD 200
		Cull Front

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows keepalpha
		#pragma vertex vert
		#include "StandardHDRPMain.cginc"

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			v.normal = -v.normal;
		}
		ENDCG
	}

    FallBack "Diffuse"
	CustomEditor "Toybox.StandardHDRPGUI"
}