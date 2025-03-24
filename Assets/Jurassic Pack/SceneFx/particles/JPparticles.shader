Shader "Jurassic Pack/JPparticles"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Particle Texture", 2D)="white" {}
		_Glow("Glow", float)=1
		_Scale("Scale", float)=1
	}

	SubShader
	{
		Pass
		{
			Tags{ "Queue"="Transparent-100" "IgnoreProjector"="True" }
			Blend SrcAlpha OneMinusSrcAlpha
			Cull back ZWrite Off

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex; fixed _Glow, _Scale;

			struct In
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				fixed4 color : COLOR;
				fixed2 texcoord : TEXCOORD0;
			};

			struct Out
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				fixed2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			Out vert (In v)
			{
				Out o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.uv=v.texcoord;
				o.color=v.color;
				o.vertex=UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (Out i) : SV_Target
			{
				fixed4 c=tex2D(_MainTex, i.uv*_Scale)*_Glow*i.color;
				UNITY_APPLY_FOG(i.fogCoord, c);
				return fixed4(c.rgb, c.a*i.color.a);
			}
			ENDCG
		}
	}

}
