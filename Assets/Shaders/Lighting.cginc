#if !defined(LIGHTING_INCLUDED)
#define LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

float4 _Tint;
sampler2D _MainTex;
float4 _MainTex_ST;
float4 _MainTex_TexelSize;
int _Dimmensions;

sampler2D _Green;
sampler2D _GreenNormal;
sampler2D _Blue;
sampler2D _BlueNormal;
sampler2D _Red;
sampler2D _RedNormal;
sampler2D _Black;
sampler2D _BlackNormal;
float _BumpScale;
float4 _Color;
float _Metallic;
float _Smoothness;

struct VertexData {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 uvSplat : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
	float3 normal : TEXCOORD3;
	#if defined(BINORMAL_PER_FRAGMENT)
		float4 tangent : TEXCOORD4;
	#else		
		float3 tangent : TEXCOORD4;
		float3 binormal : TEXCOORD5;
	#endif
		
	SHADOW_COORDS(6)
	
	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD7;
	#endif

};

void ComputeVertexLightColor(inout Interpolators i) {
	#if defined(VERTEXLIGHT_ON)
		i.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos, i.normal
			);
	#endif
}

float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}

Interpolators vert(VertexData v) {
	Interpolators i;
	UNITY_INITIALIZE_OUTPUT(Interpolators, i);

	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.normal = UnityObjectToWorldNormal(v.normal);

	#if defined(BINORMAL_PER_FRAGMENT)
		i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	#else
		i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
		i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
	#endif

	i.uv = TRANSFORM_TEX(v.uv, _MainTex);
	i.uvSplat = v.uv;

	TRANSFER_SHADOW(i);

	ComputeVertexLightColor(i);

	return i;
}

UnityLight CreateLight(Interpolators i) {
	UnityLight light;

	#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif

	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
	
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

UnityIndirect CreateIndirectLight(Interpolators i) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse = i.vertexLightColor;
	#endif

	#if defined(FORWARD_BASE_PASS)
		indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
	#endif

	return indirectLight;
}

void InitializeFragmentNormal(inout Interpolators i) {
	float4 splat = tex2D(_MainTex, i.uvSplat);
	float3 mainNormal =
		UnpackScaleNormal((tex2D(_RedNormal, i.uv*_MainTex_TexelSize.w / _Dimmensions) * splat.r +
			tex2D(_BlueNormal, i.uv*_MainTex_TexelSize.w / _Dimmensions) * splat.b +
			tex2D(_GreenNormal, i.uv*_MainTex_TexelSize.w / _Dimmensions) * splat.g + 
			tex2D(_BlackNormal, (i.uv + float2(_Time[0] * .15, _Time[0] * .5))*_MainTex_TexelSize.w / _Dimmensions) * (1-splat.r - splat.g - splat.b)), _BumpScale);
	//float3 detailNormal =
	//	UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale);
	//float3 tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
	float3 tangentSpaceNormal = mainNormal;

	#if defined(BINORMAL_PER_FRAGMENT)
		float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
	#else
		float3 binormal = i.binormal;
	#endif

	i.normal = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * binormal +
		tangentSpaceNormal.z * i.normal
		);
}

float4 frag(Interpolators i) : SV_TARGET{
	InitializeFragmentNormal(i);

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	float4 splat = tex2D(_MainTex, i.uvSplat);

	float4 color =
		(tex2D(_Red, i.uv*_MainTex_TexelSize.w / _Dimmensions) * splat.r +
		tex2D(_Blue, i.uv*_MainTex_TexelSize.w / _Dimmensions) * splat.b +
		tex2D(_Green, i.uv*_MainTex_TexelSize.w / _Dimmensions) * splat.g +
		tex2D(_Black, (i.uv + float2(_Time[0] * .15, _Time[0]*.5))*_MainTex_TexelSize.w / _Dimmensions) * (1 - splat.r - splat.g - splat.b)) * _Tint;

	float3 albedo = color.rgb;
	float alpha = color.a;

	float3 specularTint;
	float oneMinusReflectivity;

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);

	color = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i)
		);
	color.a = alpha;
	return color;
}
#endif