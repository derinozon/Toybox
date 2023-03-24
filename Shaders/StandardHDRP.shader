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
		_HeightPower("Height Power", Range(0,0.15)) = 0

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

		Pass {
			// Unity does not allow SetShaderPassEnabled with Name tag. //

			//Name "OutlinePass"
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
		
		ZWrite [_ZWrite]
        Blend [_SrcBlend] [_DstBlend]
		Cull Off

        CGPROGRAM	
        #pragma surface surf Standard fullforwardshadows keepalpha

		#pragma target 3.0

		

		#pragma multi_compile __ MASK_ON

		
        #include "UnityPBSLighting.cginc"
        
 
        sampler2D _MainTex;
        sampler2D _Normal;
		sampler2D _Emission;
		sampler2D _Mask;
		sampler2D _HeightMap;
		
        struct Input {
            float2 uv_MainTex;
			float2 uv_Normal;
			float3 viewDir;
        };
 
 
		
		float _HeightPower;
		float _NormalStr;


        fixed4 _Color;
		fixed3 _EmissionColor;
		float _EmissionStr;
		float _Smoothness;
		float _Metalness;
 
       
        UNITY_INSTANCING_BUFFER_START(Props)
            
        UNITY_INSTANCING_BUFFER_END(Props)
 
        void surf (Input IN, inout SurfaceOutputStandard o) {

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			
			float2 texOffset = ParallaxOffset(tex2D(_HeightMap, IN.uv_MainTex).r, _HeightPower, IN.viewDir);
			o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal + 0));
			o.Emission = tex2D(_Emission, IN.uv_MainTex).rgb * _EmissionColor;// * _EmissionStr;

			// MASK //
			#if MASK_ON
			fixed4 maskCol = tex2D (_Mask, IN.uv_MainTex);

			o.Smoothness = maskCol.a;
			o.Occlusion = maskCol.g;
			o.Metallic = maskCol.r;
			#else
			o.Smoothness = _Smoothness;
			o.Occlusion = 1.0;
			o.Metallic = _Metalness;
			#endif

			o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
	CustomEditor "Toybox.StandardHDRPGUI"
}