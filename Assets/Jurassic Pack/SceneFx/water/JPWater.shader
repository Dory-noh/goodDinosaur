Shader "Jurassic Pack/JPwater"
{
	Properties
	{
		[NoScaleOffset]_WaterTex("Water Texture", 2D)="" {}
		[NoScaleOffset]_EdgeTex("Edge Texture", 2D)="" {}
		[HideInInspector]_ReflectionRT("Reflection RenderTex", 2D)="" {}
		_WaterScale("Water Scale", float)=512
		_EdgeScale("Edge Scale", float)=1024
		[Space(25)]
		_WGlow("Water Glow", Range(0,16))=1.5
		_EGlow("Edge Glow", Range(0,16))=2
		_Col("Base Color", color)=(0,0.5,1,1)
		_ReefCol("Reef Color", color)=(0,1,1,0)
		_Depth("Depth Blend", Range(0,128))=16
		_EdgeBlend("Edge Blend", Range(0,16))=2
		_ReflPow("Reflection Power", Range(0,1))=0.5
		_ReflDistPow("Reflection Distortion Power", float)=16
	}

	SubShader
	{
		Tags{ "Queue"="AlphaTest" "IgnoreProjector"="True" }
		Pass
		{
			Cull Off ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _Refraction, _ReflectionRT, _CameraDepthTexture, _EdgeTex, _WaterTex;
			float4 _Col, _ReefCol;
			float _WGlow, _EGlow,_Depth, _EdgeBlend, _WaterScale, _EdgeScale, _ReflPow, _ReflDistPow;

			struct IN
			{
				float4 vertex : POSITION;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct OUT
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 wpos : TEXCOORD1;
				half3 nDir : TEXCOORD2;
				half3 tDir : TEXCOORD3;
				half3 btDir : TEXCOORD4;
				float4 screenPos : TEXCOORD5;
				UNITY_FOG_COORDS(6)
			};

			OUT vert(IN v)
			{
				OUT o;
				o.uv=v.uv;
				o.nDir=UnityObjectToWorldNormal(v.normal);
				o.tDir=normalize(mul(unity_ObjectToWorld, half4(v.tangent.xyz, 0.0)).xyz);
				o.btDir=normalize(cross(o.nDir, o.tDir) * v.tangent.w);
				o.wpos=mul(unity_ObjectToWorld, v.vertex);
				o.pos=UnityObjectToClipPos(v.vertex);
				o.screenPos=ComputeScreenPos(o.pos);
				COMPUTE_EYEDEPTH(o.screenPos.z);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			half4 frag(OUT i) : SV_Target
			{
				//depth
				float depth=LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)))-i.screenPos.z;
				float colBlend=saturate(depth/_Depth), edgeBlend=saturate(depth/_EdgeBlend);
				//textures and uv scroll
				float2 uv1=i.uv*_WaterScale,uv2=i.uv*_EdgeScale, scroll=_Time.xy*0.03;
				half3 water=tex2D(_WaterTex,uv1+scroll)-tex2D(_WaterTex,uv1-scroll);
				half4 edge=tex2D(_EdgeTex,uv2+scroll);
				//directional light
				half3 nDir=normalize(mul(lerp(half3(0,0,0.1), water, 0.5), half3x3(i.tDir, i.btDir, i.nDir)));
				half3 dir=dot(normalize(normalize(_WorldSpaceCameraPos-i.wpos.xyz)+_WorldSpaceLightPos0), nDir);
				half3 dlgt=pow(max(0, dir), 16)*pow(UNITY_LIGHTMODEL_AMBIENT, _WorldSpaceLightPos0.w)*1.5;
				//reflection
				half4 refl=tex2Dproj(_ReflectionRT, i.screenPos+half4(water*half3(0,_ReflDistPow,0),1));
				//output
				half4 c=lerp(edge*_EGlow, lerp(lerp(1,_ReefCol, edgeBlend),_Col, colBlend), edgeBlend)*_WGlow; //colors+edge
				c=lerp(c+(half4(dlgt,0.5)), half4(refl.rgb,colBlend), _ReflPow); //dirlight+reflection
				UNITY_APPLY_FOG(i.fogCoord, c); //fog
				return float4(c.rgb, c.a*edgeBlend);
			}

			ENDCG
		}
	}
	FallBack Off
}
