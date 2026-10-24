"""Original high-detail asymmetric image target; print square at width 150 mm."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import random,math
r=random.Random(19);root=Path(__file__).resolve().parents[1];im=Image.new('RGB',(1024,1024),'#efe4c9');d=ImageDraw.Draw(im)
d.rounded_rectangle((18,18,1006,1006),45,fill='#123b3d');d.rectangle((55,55,969,969),fill='#ead8ae')
for i in range(550):
 x=r.randint(70,950);y=r.randint(80,945);rr=r.randint(1,4);d.ellipse((x-rr,y-rr,x+rr,y+rr),fill=r.choice(['#c8b17d','#d8c99d','#b6a479']))
font=ImageFont.truetype(str(root/'Assets/Resources/Fonts/Body.ttf'),45);small=ImageFont.truetype(str(root/'Assets/Resources/Fonts/Body.ttf'),22)
d.text((100,90),'ASTER  /  FIELD ARCHIVE',font=font,fill='#123b3d');d.text((100,155),'01   •   THE WATERKEEPER',font=small,fill='#644c2e')
# deliberately asymmetric silhouette with individual masonry outlines
for row in range(8):
 for col in range(7):
  x=220+col*78+(row%2)*12;y=340+row*51
  if row<2 and col>4:continue
  d.rounded_rectangle((x,y,x+73,y+46),5,fill=r.choice(['#b88b4f','#bd955c','#ceaa70']),outline='#554c36',width=3)
d.polygon([(185,348),(480,230),(785,348)],fill='#274c46',outline='#123b3d',width=5)
d.ellipse((416,284,509,377),fill='#d8b55e',outline='#123b3d',width=8)
d.rounded_rectangle((443,514,574,760),55,fill='#143c3b')
for x in [205,350,635,765]:
 d.rectangle((x,385,x+39,755),fill='#e4c88e',outline='#5b4c2e',width=4);d.rectangle((x-10,377,x+49,410),fill='#9f733c',outline='#3d4432',width=3)
for k in range(18):
 x=120+k*14;y=380+k*23;d.line((x,y,x+70,y-40),fill='#2b5141',width=8);d.ellipse((x-24,y-25,x+20,y+11),fill=r.choice(['#456342','#244c40','#78905c']),outline='#244335',width=3)
for i in range(9):
 x=670+i*23;y=805+int(math.sin(i)*25);d.ellipse((x,y,x+34,y+17),fill='#345c58')
d.line([(150,805),(360,826),(480,784),(620,840)],fill='#305c59',width=16)
d.text((105,892),'OBSERVE  •  ROTATE  •  DISCOVER',font=small,fill='#123b3d');d.text((760,925),'A / 071',font=small,fill='#644c2e')
for out in [root/'Assets/Resources/AR/DiscCard.png',root/'Documentation/AR_Card.png']:im.save(out,dpi=(1024/15*2.54,)*2)
print('AR marker created: print 150 x 150 mm, no crop.')
