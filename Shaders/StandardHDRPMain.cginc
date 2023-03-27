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
float _OcclusionStrenght;
float _EmissionStr;
float _Smoothness;
float _Metalness;


UNITY_INSTANCING_BUFFER_START(Props)
UNITY_INSTANCING_BUFFER_END(Props)



void surf (Input IN, inout SurfaceOutputStandard o) {
	float2 texOffset = ParallaxOffset(tex2D(_HeightMap, IN.uv_MainTex).r, _HeightPower, IN.viewDir);
     fixed4 c = tex2D (_MainTex, IN.uv_MainTex + texOffset) * _Color;
     o.Albedo = c.rgb;
	
	o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
	o.Emission = tex2D(_Emission, IN.uv_MainTex).rgb * _EmissionColor;// * _EmissionStr;
	// MASK //
	#if MASK_ON
	fixed4 maskCol = tex2D (_Mask, IN.uv_MainTex);
	o.Smoothness = maskCol.a;
	o.Occlusion = maskCol.g; //lerp(0, maskCol.g, _OcclusionStrenght);
	o.Metallic = maskCol.r;
	#else
	o.Smoothness = _Smoothness;
	o.Occlusion = 1.0;
	o.Metallic = _Metalness;
	#endif
	o.Alpha = c.a;
 }
