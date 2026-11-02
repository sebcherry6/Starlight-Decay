"""Synthesises the new sound effects and two music loops as 16-bit mono WAV."""
import sys, wave
import numpy as np
OUT=sys.argv[1]; SR=44100; rs=np.random.RandomState(7)
def t(d): return np.arange(int(SR*d))/SR
def save(name,x,peak=.85,sr=SR):
    x=np.asarray(x,float); x=x/max(1e-6,np.abs(x).max())*peak
    w=wave.open(f'{OUT}/{name}.wav','wb'); w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr)
    w.writeframes((x*32767).astype('<i2').tobytes()); w.close()
def sweep(a,b,d): T=t(d); f=a*(b/a)**(T/d); return np.cumsum(2*np.pi*f/SR)
def env(n,a=.003,r=None):
    T=np.arange(n)/SR; d=n/SR; return np.minimum(T/a,1)*np.exp(-T/(r or d/4))
def noise(d): return rs.uniform(-1,1,int(SR*d))
def lp(x,c):
    a=np.exp(-2*np.pi*c/SR); y=np.zeros_like(x); s=0.
    for i,v in enumerate(x): s=(1-a)*v+a*s; y[i]=s
    return y
def tones(fs,d,step): 
    out=np.zeros(int(SR*(d+step*len(fs))))
    for i,f in enumerate(fs):
        T=t(d); y=(np.sin(2*np.pi*f*T)+.3*np.sin(4*np.pi*f*T))*env(len(T),.004,d/3)
        s=int(i*step*SR); out[s:s+len(y)]+=y
    return out
save('enemy_shot',np.sign(np.sin(sweep(900,220,.14)))*.5*env(int(.14*SR),.002,.05),.6)
save('pickup',np.sin(sweep(900,1500,.07))*env(int(.07*SR),.002,.025),.6)
save('powerup',tones([523,659,784,1047],.16,.06),.7)
save('hit',np.sin(sweep(500,180,.06))*env(int(.06*SR),.001,.02)+.4*noise(.06)*env(int(.06*SR),.001,.012),.6)
save('shield_hit',(np.sin(sweep(1400,500,.22))+.5*np.sin(sweep(2100,800,.22)))*env(int(.22*SR),.002,.07),.7)
d=1.6;T=t(d);save('bomb',(lp(noise(d),500)*2.5+np.sin(sweep(120,28,d))*1.4)*env(len(T),.005,.42),.95)
save('buy',tones([659,988],.12,.07),.7)
save('denied',np.sign(np.sin(sweep(170,120,.22)))*.5*env(int(.22*SR),.003,.09),.6)
d=.26;save('dash',lp(noise(d),3000)*np.sin(np.linspace(0,np.pi,int(SR*d)))**2+.5*np.sin(sweep(300,1100,d))*env(int(SR*d),.01,.1),.7)
d=2.6;T=t(d);f=np.where((T%.65)<.325,520,760);x=np.sign(np.sin(np.cumsum(2*np.pi*f/SR)))*.5
save('warning',lp(x,2400)*np.minimum(1,(d-T)/.2),.75)
d=2.8;T=t(d);save('boss_die',(lp(noise(d),900)*2+np.sin(sweep(190,24,d))*1.5+.5*np.sin(sweep(1300,60,d)))*env(len(T),.005,.8),.95)
save('sector_clear',tones([392,523,659,784,1047,1319],.5,.13),.75)
d=.7;T=t(d);save('beam',(np.sin(sweep(70,240,d))+.5*np.sign(np.sin(sweep(140,480,d))))*np.sin(np.pi*T/d),.65)
d=.5;save('ion',(lp(noise(d),6000)*(rs.uniform(0,1,int(SR*d))>.55)+np.sin(sweep(800,70,d)))*env(int(SR*d),.001,.1),.8)

def note(m): return 440*2**((m-69)/12)
def voice(f,d,kind,sr=SR):
    n=int(sr*d);T=np.arange(n)/sr
    ph=2*np.pi*f*T
    if kind=='bass':y=lp(np.sign(np.sin(ph))*.6+np.sin(ph),500)*np.minimum(1,T/.005)*np.exp(-T/(d*.8))
    elif kind=='arp':y=(np.sin(ph)+.5*np.sin(2*ph)+.25*np.sin(3*ph))*np.minimum(1,T/.003)*np.exp(-T/(d*.35))
    else:y=(np.sin(ph)+.4*np.sin(ph*1.004)+.3*np.sin(2*ph))*np.sin(np.pi*T/d)**.7
    return y
def loop(bpm,bars,chords,arp,kick_every,hat,name):
    beat=60/bpm;st=beat/4;steps=bars*16;N=int(steps*st*SR);out=np.zeros(N+SR*2)
    def put(y,p,g):
        s=int(p*st*SR);out[s:s+len(y)]+=y*g
    T=t(.22);kick=np.sin(sweep(150,42,.22))*np.exp(-T/.07)
    Th=t(.05);hh=noise(.05)-lp(noise(.05),5000);hh=hh*np.exp(-Th/.012)
    for b in range(bars):
        root,tri=chords[b%len(chords)]
        put(voice(note(root+12),st*16,'pad'),b*16,.16); put(voice(note(root+12+tri[1]),st*16,'pad'),b*16,.12)
        for i in range(16):
            p=b*16+i
            if i%kick_every==0:put(kick,p,.9)
            if i%hat==2%hat:put(hh,p,.16)
            if i%4 in(0,3) and i%8!=4:put(voice(note(root),st*1.8,'bass'),p,.55)
            put(voice(note(root+24+arp[(i+b*3)%len(arp)]),st*1.6,'arp'),p,.13)
    out[:SR*2]+=out[N:N+SR*2];x=out[:N]
    save(name,x,.8)
loop(112,8,[(33,(0,3,7)),(33,(0,3,7)),(29,(0,4,7)),(31,(0,4,7))],[0,7,12,3,7,12,15,7],8,4,'music_sector')
loop(148,8,[(33,(0,3,7)),(34,(0,4,7)),(33,(0,3,7)),(28,(0,4,7))],[0,3,7,12,13,12,7,3],4,2,'music_boss')
