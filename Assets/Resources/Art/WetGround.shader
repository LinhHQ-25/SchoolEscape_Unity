Shader "Aster/WetGround" {
 Properties {_Color("Silt",Color)=(.20,.235,.16,1)}
 SubShader {Tags{"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard
 #pragma target 3.0
 fixed4 _Color;struct Input{float3 worldPos;};
 void surf(Input IN,inout SurfaceOutputStandard o){
 float2 p=IN.worldPos.xz;
 float damp=sin(p.x*3+sin(p.y*4))*cos(p.y*2.2+p.x*.8)*.5+.5;
 float grain=sin(p.x*137)*sin(p.y*123)*.025;
 o.Albedo=_Color.rgb*(.7+damp*.3+grain);o.Smoothness=lerp(.15,.52,damp);o.Metallic=0;o.Occlusion=1;
 }
 ENDCG
 } FallBack "Standard"
}
