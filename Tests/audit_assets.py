"""Run from project root: python3 Tests/audit_assets.py. No Unity dependency."""
from pathlib import Path
import json,wave
root=Path(__file__).resolve().parents[1];g=json.loads((root/'Documentation/geometry.json').read_text());anchors=g['anchors'];missing=[]
required=['P_Spawn','P_Look','P_Courtyard','P_Water','P_End','P_Inspect','P_Receiver','I_Journal','I_Flashlight','I_Disc','I_GateLever','I_Shutter','I_Handle','I_Valve','I_MachineLever','I_FinalValve','A_GateLeft','A_GateRight','A_FinalLeft','A_FinalRight','A_SideGate','A_Shutter','A_PoolWater','A_Waterwheel']
for prefix,n in [('I_Ring',3),('I_Mirror',2),('I_Channel',3),('I_Gear',3),('I_Socket',3),('A_Gear',3),('I_Note',3)]:required.extend(prefix+str(i) for i in range(n))
assert len(required)==len(set(required))
for name in required:
 if name not in anchors:missing.append(name)
assert not missing,missing
for name in ['exploration','forest','ending','step','stone','lever','gate','valve','click','pickup','success','fail','metal','water','mechanism']:
 with wave.open(str(root/'Assets/Resources/Audio'/(name+'.wav'))) as w:assert w.getnchannels()==2 and w.getframerate()==22050 and w.getnframes()>0
for f in ['Assets/Resources/Art/World.fbx','SourceArt/World.blend','Assets/Resources/Fonts/Body.ttf','Assets/Resources/Fonts/Title.ttf','Assets/Resources/AR/DiscCard.png','Assets/Shaders/Water.shader']:
 assert (root/f).stat().st_size>100,f
print('PASS:',len(required),'anchors; 15 stereo WAV assets; FBX, Blend, fonts, marker and shader exist')
