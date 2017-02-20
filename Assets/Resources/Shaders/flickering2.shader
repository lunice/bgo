//Shader "Hidden/flickering"
Shader "Custom/flickering" {
	Properties
	{
	//	[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	//_Color("Tint", Color) = (1, 1, 1, 1)
	//	[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile DUMMY PIXELSNAP_ON
#include "UnityCG.cginc"

		struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex   : SV_POSITION;
		fixed4 color : COLOR;
		half2 texcoord  : TEXCOORD0;
	};

	fixed4 _Color;

	v2f vert(appdata_t IN)
	{
		v2f OUT;
		OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
		OUT.texcoord = IN.texcoord;
		OUT.color = IN.color;
#ifdef PIXELSNAP_ON
		OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

		return OUT;
	}

	sampler2D _MainTex;

	fixed4 frag(v2f IN) : COLOR
	{
		fixed4 c = tex2D(_MainTex, IN.texcoord);
	
	c.a *= IN.color.a;
	//c.rgb = lerp(c.rgb, step(0.5, IN.color.rgb), IN.color.rgb);
	float glaringCoef;
	// здесь менять силу засвечивания объекта. 
	// █ При изменении коэфициэнта, подсвечиваемый объект должен 
	return c * IN.color * 1.5;//(c * 2);// +IN.color * 0.5;
	}
		ENDCG
	}
	}
}