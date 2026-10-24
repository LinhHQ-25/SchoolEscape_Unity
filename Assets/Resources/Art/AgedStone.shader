Shader "Aster/AgedStone" {
 Properties { _Color("Stone tone",Color)=(.48,.43,.32,1) _MainTex("Stone grain",2D)="white"{} }
 SubShader {Tags{"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;
 struct Input {float3 worldPos;float3 worldNormal;};
 float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 float noise(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float n=noise(IN.worldPos*3)*.6+noise(IN.worldPos*17)*.25+noise(IN.worldPos*65)*.15;
 float weather=smoothstep(.54,.78,noise(IN.worldPos*1.7))*saturate(.5+IN.worldNormal.y*.5);
 float3 weights=abs(IN.worldNormal);weights/=max(dot(weights,float3(1,1,1)),.001);
 float3 grain=tex2D(_MainTex,IN.worldPos.xz*.45).rgb*weights.y+tex2D(_MainTex,IN.worldPos.xy*.45).rgb*weights.z+tex2D(_MainTex,IN.worldPos.zy*.45).rgb*weights.x;
 o.Albedo=lerp(_Color.rgb*(.72+n*.42)*grain,float3(.18,.23,.12),weather*.32);
 o.Metallic=0;o.Smoothness=.08+n*.08;o.Occlusion=.82+n*.18;o.Alpha=1;
 }
 ENDCG
 } FallBack "Diffuse"
}
