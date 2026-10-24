Shader "Aster/FlowWater" {
 Properties { _MainTex("Flow coordinates",2D)="white"{} _Color("Water tint",Color)=(.12,.36,.32,.82) _Speed("Downstream speed",Float)=.75}
 SubShader {Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade vertex:vert
 #pragma target 3.0
 fixed4 _Color;float _Speed;
 struct Input {float2 uv_MainTex;float3 worldPos;};
 sampler2D _MainTex;
 void vert(inout appdata_full v){v.vertex.y+=sin(v.texcoord.y*12-_Time.y*4)*.003*sin(v.texcoord.x*3.14159);}
 void surf(Input IN,inout SurfaceOutputStandard o){float2 uv=IN.uv_MainTex;float phase=uv.y-_Time.y*_Speed;
 float ripple=sin(phase*17+sin(uv.x*19)*.4)*.5+.5;
 float foam=pow(saturate(ripple),18)*.13+pow(abs(uv.x*2-1),12)*.3;
 o.Albedo=lerp(_Color.rgb,float3(.67,.8,.74),foam);o.Normal=normalize(float3(cos(uv.x*24+phase*3)*.045,sin(phase*17)*.05,1));
 o.Smoothness=.62;o.Metallic=0;o.Alpha=_Color.a+foam*.15;o.Occlusion=1;}
 ENDCG
 } FallBack "Transparent/Diffuse"
}
