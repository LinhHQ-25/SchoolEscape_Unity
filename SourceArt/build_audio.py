"""Original procedural soundtrack / Foley. Python: numpy, scipy."""
from pathlib import Path
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter,sosfilt
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Assets/Resources/Audio';OUT.mkdir(parents=True,exist_ok=True)
SR=22050;rng=np.random.default_rng(157)
def write(n,a):
 a=np.asarray(a);a=a/(max(1,np.max(abs(a))/.83));
 if a.ndim==1:a=np.column_stack((a,np.roll(a,53)*.98))
 wavfile.write(OUT/(n+'.wav'),SR,(np.clip(a,-1,1)*32767).astype(np.int16))
def tone(f,d,kind='bell'):
 t=np.arange(int(SR*d))/SR
 if kind=='bell':return (np.sin(2*np.pi*f*t)*np.exp(-t*2)+.25*np.sin(2*np.pi*f*2.01*t)*np.exp(-t*4))*(1-np.exp(-t*100))
 return (np.sin(2*np.pi*f*t)+.18*np.sin(2*np.pi*f*2*t))*(.5-.5*np.cos(2*np.pi*t/d))
def music(d=64,ending=False):
 a=np.zeros(int(SR*d));chords=[[146.83,174.61,220],[130.81,164.81,196],[116.54,146.83,174.61],[130.81,174.61,220]]
 for bar in range(int(d/8)):
  start=bar*8;chord=chords[bar%4]
  for f in chord:
   s=tone(f,10,'pad')*.095;idx=(np.arange(len(s))+int(start*SR))%len(a);np.add.at(a,idx,s)
  for beat in range(4):
   s=tone(chord[(beat+bar)%3]*2,3)*.10;idx=(np.arange(len(s))+int((start+beat*2)*SR))%len(a);np.add.at(a,idx,s)
 if ending:a*=np.minimum(1,np.arange(len(a))/SR)*np.minimum(1,(len(a)-np.arange(len(a)))/(SR*4))
 return a
write('exploration',music());write('ending',music(24,True))
def noise(d,low,high):
 a=rng.normal(0,1,int(SR*d));a=sosfilt(butter(3,[low,high],fs=SR,btype='bandpass',output='sos'),a);return a
forest=noise(40,200,1400)*.09
for k in range(22):
 start=int(rng.uniform(0,39)*SR);d=.15+rng.random()*.25;t=np.arange(int(d*SR))/SR;chirp=np.sin(2*np.pi*(1800*t+800*t*t))*np.sin(np.pi*t/d)**2*.06;end=min(len(forest),start+len(chirp));forest[start:end]+=chirp[:end-start]
write('forest',forest)
for n,d,lo,hi in [('step',.18,100,1000),('stone',.6,120,1700),('lever',.5,80,1300),('gate',2.2,50,900),('valve',.8,180,1800)]:
 t=np.arange(int(SR*d))/SR;a=noise(d,lo,hi)*np.exp(-t*4/d)*np.minimum(t*80,1)*.45;write(n,a)
for n,fs,d in [('click',[780],.11),('pickup',[523,784],.55),('success',[523,659,784,1046],1.8),('fail',[196,185],.55),('metal',[420,630,1090],.7)]:
 a=np.zeros(int(SR*(d+.3)))
 for i,f in enumerate(fs):
  offset=int(i*.09*SR);s=tone(f,d)*.17;end=min(len(a),offset+len(s));a[offset:end]+=s[:end-offset]
 write(n,a)
water=noise(12,350,4500)*.14;write('water',water)
t=np.arange(8*SR)/SR;mechanical=(np.sin(2*np.pi*62*t)*.06+noise(8,100,800)*.08)*(.7+.3*np.sin(2*np.pi*2*t));write('mechanism',mechanical)
print('Audio generated:',len(list(OUT.glob('*.wav'))))
