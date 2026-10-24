Shader "Aster/Foliage" {
 Properties {_MainTex("Leaves",2D)="white"{} _Color("Tint",Color)=(.8,.9,.75,1) _Cutoff("Alpha cutoff",Range(0,1))=.35}
 SubShader {Tags{"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard alphatest:_Cutoff addshadow vertex:vert
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;
 struct Input {float2 uv_MainTex;};
 void vert(inout appdata_full v){float3 w=mul(unity_ObjectToWorld,v.vertex).xyz;float sway=sin(_Time.y*.8+w.x*.6+w.z*.4)*.025;v.vertex.x+=sway*saturate(v.vertex.y*.2);}
 void surf(Input IN,inout SurfaceOutputStandard o){fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;o.Albedo=c.rgb;o.Alpha=c.a;o.Smoothness=.16;o.Occlusion=.9;}
 ENDCG
 } FallBack "Transparent/Cutout/Diffuse"
}
