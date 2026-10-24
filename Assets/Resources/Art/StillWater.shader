Shader "Aster/StillWater" {
 Properties {_Color("Water tint",Color)=(.10,.28,.24,.78)}
 SubShader {Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade
 #pragma target 3.0
 fixed4 _Color;
 struct Input {float3 worldPos;};
 void surf(Input IN,inout SurfaceOutputStandard o){
 float2 p=IN.worldPos.xz;float time=_Time.y;
 float wave=sin(p.x*5.7+p.y*3.2-time*.7)*sin(p.y*8-p.x*2.5+time*.43);
 o.Albedo=_Color.rgb*(.92+wave*.08);
 o.Normal=normalize(float3(cos(p.x*5.7+p.y*3.2-time*.7)*.045,sin(p.y*8-p.x*2.5+time*.43)*.045,1));
 o.Metallic=0;o.Smoothness=.63;o.Alpha=_Color.a;o.Occlusion=1;
 }
 ENDCG
 } FallBack "Transparent/Diffuse"
}
