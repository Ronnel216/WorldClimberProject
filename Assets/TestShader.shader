Shader "Custom/sample" {
	Properties{
		_MainTex("Texture", 2D) = "white"{}
	}
		SubShader{
		Tags{ "Queue" = "Transparent" }
		LOD 200

		CGPROGRAM
#pragma surface surf Standard alpha:fade
#pragma target 3.0

		struct Input {
		float2 uv_MainTex;
	};

	sampler2D _MainTex;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = c.rgb;
		o.Alpha = (c.r*0.3 + c.g*0.6 + c.b*0.1 < 0.2) ? 1 : 0.7;
	}
	ENDCG
	}
		FallBack "Diffuse"
}