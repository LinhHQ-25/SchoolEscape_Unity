"""Validate bundled model provenance, UI portraits and the art dependencies."""
from pathlib import Path
import json,struct
root=Path(__file__).resolve().parents[1];g=json.loads((root/'Documentation/geometry.json').read_text())
for key,n in g['source_asset_instances'].items():
 assert n>0
 pack,name=key.split('/');folder=root/'SourceArt/ThirdParty'/pack
 assert any(p.suffix in ('.glb','.obj') for p in folder.glob(name+'.*')),key
 assert any('license' in p.name.lower() for p in folder.iterdir()),pack
for name in ['Journal','Flashlight','Disc','Handle','Gear0','Gear1','Gear2','Note0']:
 b=(root/'Assets/Resources/UI/Items'/f'{name}.png').read_bytes();assert b[:8]==b'\x89PNG\r\n\x1a\n' and struct.unpack('>II',b[16:24])==(256,256),name
assert (root/'Assets/Resources/Art/Textures/KayKit_dungeon_texture.png').is_file()
assert (root/'Assets/Resources/Art/AsterDust.shader').is_file()
print('PASS: bundled CC0 source/license for every placed model; 8 item portraits; atlas and dust shader')
