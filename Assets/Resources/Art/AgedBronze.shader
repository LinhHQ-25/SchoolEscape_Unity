Shader "Aster/AgedBronze" {
 Properties {_Color("Bronze",Color)=(.39,.28,.12,1)}
 SubShader {Tags {"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard
 #pragma target 3.0
 fixed4 _Color;
 struct Input {float3 worldPos;};
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 p=IN.worldPos;
 float grain=sin(p.x*61+sin(p.z*47))*sin(p.y*73+p.z*13);
 float tarnish=smoothstep(.18,.65,sin(p.x*9+p.y*12)*cos(p.z*17-p.y*7));
 o.Albedo=lerp(_Color.rgb*(.88+grain*.12),float3(.11,.22,.17),tarnish*.6);
 o.Metallic=lerp(.65,.12,tarnish);o.Smoothness=lerp(.3,.15,tarnish);o.Occlusion=1;
 }
 ENDCG
 } FallBack "Standard"
}
