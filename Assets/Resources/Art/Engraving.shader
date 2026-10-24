Shader "Aster/Engraving" {
 Properties {_MainTex("Carved mark",2D)="white"{} _Color("Pigment",Color)=(.19,.16,.11,1)}
 SubShader {Tags{"Queue"="Transparent-10" "RenderType"="Transparent"} ZWrite Off Offset -1,-1 Cull Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;
 struct Input {float2 uv_MainTex;};
 void surf(Input IN,inout SurfaceOutputStandard o){fixed4 ink=tex2D(_MainTex,IN.uv_MainTex);o.Albedo=_Color.rgb;o.Alpha=ink.a*_Color.a;o.Metallic=0;o.Smoothness=.05;o.Occlusion=.65;}
 ENDCG
 } FallBack "Transparent/Diffuse"
}
