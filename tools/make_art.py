"""Generates the neon sprites. White lines on a black fill, tinted in-game, matching the original art."""
import math, random, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
OUT=sys.argv[1]
SS=4
def canvas(s): return Image.new('L',(s*SS,s*SS),0), Image.new('L',(s*SS,s*SS),0)
class Spr:
    def __init__(s,size,lw=3.0):
        s.size=size; s.lw=lw
        s.line=Image.new('L',(size*SS,size*SS),0); s.fill=Image.new('L',(size*SS,size*SS),0)
        s.dl=ImageDraw.Draw(s.line); s.df=ImageDraw.Draw(s.fill)
    def P(s,pts): return [(x*SS,y*SS) for x,y in pts]
    def poly(s,pts,fill=True,lw=None):
        p=s.P(pts); w=int((lw or s.lw)*SS)
        if fill: s.df.polygon(p,fill=255)
        s.dl.line(p+[p[0],p[1]],fill=255,width=w,joint='curve')
    def lines(s,pts,lw=None):
        s.dl.line(s.P(pts),fill=255,width=int((lw or s.lw)*SS),joint='curve')
    def circ(s,x,y,r,fill=True,lw=None):
        b=[(x-r)*SS,(y-r)*SS,(x+r)*SS,(y+r)*SS]
        if fill: s.df.ellipse(b,fill=255)
        s.dl.ellipse(b,outline=255,width=int((lw or s.lw)*SS))
    def dot(s,x,y,r):
        s.dl.ellipse([(x-r)*SS,(y-r)*SS,(x+r)*SS,(y+r)*SS],fill=255)
    def mirror(s,pts): c=s.size/2; return [(x,2*c-y) for x,y in pts]
    def save(s,name):
        n=s.size
        line=s.line.resize((n,n),Image.LANCZOS); fill=s.fill.resize((n,n),Image.LANCZOS)
        l=np.asarray(line,float)/255
        g1=np.asarray(line.filter(ImageFilter.GaussianBlur(n/80+1.5)),float)/255
        g2=np.asarray(line.filter(ImageFilter.GaussianBlur(n/32+3)),float)/255
        glow=np.clip(g1*1.1+g2*0.7,0,1)
        a=np.clip(np.maximum.reduce([l,glow,np.asarray(fill,float)/255]),0,1)
        v=np.clip(l+glow*0.85,0,1)
        # straight-alpha white light over black interior
        rgb=np.where(a>0,np.clip(v/np.maximum(a,1e-3),0,1),0)
        im=np.dstack([rgb,rgb,rgb,a]); Image.fromarray((im*255).astype('uint8'),'RGBA').save(f'{OUT}/{name}.png')
def reg(n,r,c=None,off=0,ry=None):
    c=c if c is not None else 0
    return [(c+r*math.cos(off+i*2*math.pi/n),c+(ry or r)*math.sin(off+i*2*math.pi/n)) for i in range(n)]
def nreg(cx,cy,n,r,off=0,ry=None):
    return [(cx+r*math.cos(off+i*2*math.pi/n),cy+(ry or r)*math.sin(off+i*2*math.pi/n)) for i in range(n)]
def star(cx,cy,n,r1,r2,off=0):
    return [(cx+(r1 if i%2==0 else r2)*math.cos(off+i*math.pi/n),cy+(r1 if i%2==0 else r2)*math.sin(off+i*math.pi/n)) for i in range(2*n)]

# ship: the original dart, with cockpit, fins and engine detail
s=Spr(128,3.2)
s.poly([(116,64),(30,104),(14,92),(44,64),(14,36),(30,24)])
s.lines([(44,64),(82,64)],2); s.poly([(92,64),(66,54),(60,64),(66,74)],lw=2.4)
s.lines([(30,24),(52,52)],2); s.lines([(30,104),(52,76)],2); s.save('ship')
s=Spr(64,3); s.poly([(58,32),(16,50),(26,32),(16,14)]); s.dot(34,32,4); s.save('wingman')

# enemies (96px, facing +X)
c=48
s=Spr(96); s.poly([(86,48),(52,72),(22,64),(34,48),(22,32),(52,32-8)]); s.circ(54,48,8); s.save('drone')
s=Spr(96); s.poly(nreg(c,c,8,40,0,20)); s.poly(nreg(c,40,6,17,0,14)); s.lines([(14,c),(82,c)],2); s.dot(30,56,3);s.dot(48,60,3);s.dot(66,56,3); s.save('gunner')
s=Spr(96); s.poly([(90,48),(30,80),(44,48),(30,16)]); s.lines([(30,16),(8,8)],2.4); s.lines([(30,80),(8,88)],2.4); s.save('swarmer')
s=Spr(96); s.poly(star(c,c,8,42,24,0)); s.circ(c,c,12); s.dot(c,c,4); s.save('mine')
s=Spr(96); s.poly([(92,48),(36,62),(10,84),(20,48),(10,12),(36,34)]); s.lines([(36,48),(78,48)],2); s.poly([(46,48),(34,40),(34,56)],lw=2); s.save('lancer')
s=Spr(128,3.2); s.poly(nreg(58,64,6,36,0.52)); s.dl.arc([(14*SS),(10*SS),(114*SS),(118*SS)],-58,58,fill=255,width=7*SS); s.lines([(112,30),(120,64),(112,98)],2); s.circ(58,64,12); s.save('bulwark')
s=Spr(96); s.poly([(92,48),(52,56),(24,76),(30,48),(24,20),(52,40)]); s.circ(40,48,9); s.lines([(52,48),(90,48)],2); s.save('sniper')
s=Spr(192,3.4); s.poly([(180,96),(150,136),(60,150),(16,124),(36,96),(16,68),(60,42),(150,56)]);
for x in (70,100,130):
    s.poly([(x,74),(x+20,74),(x+20,118),(x,118)],lw=2.4)
s.lines([(36,96),(180,96)],2); s.save('carrier')
s=Spr(96); s.poly([(88,48),(54,30),(20,16),(30,36),(10,48),(30,60),(20,80),(54,66)]); s.circ(56,48,7,fill=False); s.save('phantom')
s=Spr(96); s.poly(nreg(c,c,8,34,0.39)); s.poly([(84,44),(84,52),(48,56),(48,40)],lw=2.4); s.circ(c,c,14); s.save('turret')

# bosses (320px)
C=160
s=Spr(320,4)  # 1 Boulder: a cratered rock
random.seed(3); pts=[(C+(140+random.randint(-22,16))*math.cos(i*2*math.pi/13),C+(140+random.randint(-22,16))*math.sin(i*2*math.pi/13)) for i in range(13)]
s.poly(pts); s.circ(120,120,30,False,3); s.circ(205,140,20,False,3); s.circ(140,215,38,False,3); s.circ(230,215,14,False,3); s.save('boss1')
s=Spr(320,4)  # 2 Drone Mother: wide saucer with bays
s.poly(nreg(C,C,12,146,0,74)); s.poly(nreg(C,132,8,62,0,52)); s.circ(C,132,24)
for i in range(5):
    s.circ(68+i*46,C+(40 if i in(1,2,3) else 26),11)
s.lines([(16,C),(304,C)],2.5); s.save('boss2')
s=Spr(320,4)  # 3 Cryo Sentinel: snowflake
s.poly(star(C,C,6,146,52,0.26))
for i in range(6):
    a=0.26+i*math.pi/3; s.lines([(C,C),(C+120*math.cos(a),C+120*math.sin(a))],3)
    for d in (70,100):
        for t in(-0.5,0.5):
            s.lines([(C+d*math.cos(a),C+d*math.sin(a)),(C+d*math.cos(a)+28*math.cos(a+t*2),C+d*math.sin(a)+28*math.sin(a+t*2))],2.5)
s.poly(nreg(C,C,6,36)); s.save('boss3')
s=Spr(320,4)  # 4 Warp Strider: tri-leg walker
for i in range(3):
    a=-math.pi/2+i*2*math.pi/3
    k=(C+100*math.cos(a+0.4),C+100*math.sin(a+0.4)); t=(C+150*math.cos(a-0.1),C+150*math.sin(a-0.1))
    s.lines([(C,C),k,t],6); s.circ(*k,10); s.circ(*t,7)
s.poly(nreg(C,C,3,84,-math.pi/2)); s.circ(C,C,34); s.circ(C,C,14,False,3); s.save('boss4')
s=Spr(320,4)  # 5 Mine Layer
s.poly(star(C,C,10,146,104,0)); s.circ(C,C,76,False,3)
for i in range(5):
    a=i*2*math.pi/5; s.circ(C+76*math.cos(a),C+76*math.sin(a),17)
s.circ(C,C,30); s.dot(C,C,10); s.save('boss5')
s=Spr(320,4)  # 6 Storm Core: rings and bolts
s.circ(C,C,142,False,3.5); s.circ(C,C,100,True,3)
for i in range(8):
    a=i*math.pi/4
    s.lines([(C+100*math.cos(a),C+100*math.sin(a)),(C+120*math.cos(a+0.18),C+120*math.sin(a+0.18)),(C+112*math.cos(a-0.05),C+112*math.sin(a-0.05)),(C+150*math.cos(a+0.1),C+150*math.sin(a+0.1))],3)
s.poly([(172,86),(128,170),(160,170),(146,236),(198,146),(166,146)]); s.save('boss6')
s=Spr(320,4)  # 7 Lance Prime: huge spear ship facing +X
s.poly([(312,C),(190,198),(120,270),(40,290),(82,C),(40,30),(120,50),(190,122)])
s.lines([(82,C),(300,C)],3); s.poly([(232,C),(150,134),(120,C),(150,186)],lw=3); s.circ(110,C,18); s.lines([(120,50),(150,134)],2.5); s.lines([(120,270),(150,186)],2.5); s.save('boss7')
s=Spr(320,4)  # 8 Magma Colossus: blocky face
s.poly([(70,36),(250,36),(304,110),(288,250),(214,298),(106,298),(32,250),(16,110)])
s.poly([(72,120),(138,136),(132,170),(84,160)],lw=3); s.poly(s.mirror([(0,0)]) if 0 else [(248,120),(182,136),(188,170),(236,160)],lw=3)
s.lines([(90,226),(120,206),(145,232),(175,206),(200,232),(230,212)],4)
s.lines([(110,36),(130,86),(112,110)],2.5); s.lines([(226,44),(210,80),(232,106)],2.5); s.lines([(48,200),(84,190)],2.5); s.lines([(276,196),(244,196)],2.5); s.save('boss8')
s=Spr(320,4)  # 9 Phantom: hooded wraith
s.poly([(C,14),(238,76),(270,200),(304,306),(236,262),(196,306),(C,258),(124,306),(84,262),(16,306),(50,200),(82,76)])
s.poly([(C,62),(214,112),(204,190),(C,216),(116,190),(106,112)],lw=3)
s.poly([(126,130),(152,146),(130,156)],lw=2.5); s.poly([(194,130),(168,146),(190,156)],lw=2.5); s.save('boss9')
s=Spr(320,4)  # 10 Singularity: broken halo around an eye
for r,w in ((148,4),(118,3)):
    for i in range(6):
        a0=i*60+(10 if r==148 else 40); s.dl.arc([(C-r)*SS,(C-r)*SS,(C+r)*SS,(C+r)*SS],a0,a0+42,fill=255,width=int(w*SS))
s.poly(star(C,C,12,92,62,0.13)); s.circ(C,C,50); s.circ(C,C,26,False,3); s.dot(C,C,10); s.save('boss10')

# pickups and icons (96px)
def pick(name,fn,hexa=True):
    s=Spr(96,3); s.poly(nreg(c,c,6 if hexa else 8,42,0.52 if hexa else 0.39)); fn(s); s.save(name)
s=Spr(96,4); s.poly(nreg(c,c,6,30,0.2)); s.poly(nreg(c,c,6,12,0.7),lw=3); s.save('pick_scrap')
pick('pick_repair',lambda s:(s.poly([(42,24),(54,24),(54,42),(72,42),(72,54),(54,54),(54,72),(42,72),(42,54),(24,54),(24,42),(42,42)],fill=False)))
pick('pick_shield',lambda s:(s.poly([(48,20),(72,30),(68,60),(48,76),(28,60),(24,30)],fill=False)))
pick('pick_bomb',lambda s:(s.circ(46,54,18,False),s.lines([(58,40),(68,26)]),s.lines([(68,26),(76,30)])))
pick('pick_spread',lambda s:(s.lines([(22,48),(76,48)]),s.lines([(22,48),(70,24)]),s.lines([(22,48),(70,72)])))
pick('pick_rapid',lambda s:(s.lines([(26,28),(48,48),(26,68)]),s.lines([(46,28),(68,48),(46,68)])))
pick('pick_pierce',lambda s:(s.lines([(18,48),(78,48)]),s.lines([(62,34),(78,48),(62,62)]),s.circ(44,48,12,False,2.4)))
pick('pick_homing',lambda s:(s.circ(48,48,22,False),s.lines([(48,16),(48,36)]),s.lines([(48,60),(48,80)]),s.lines([(16,48),(36,48)]),s.lines([(60,48),(80,48)])))
pick('pick_double',lambda s:(s.lines([(26,34),(38,26),(50,34),(26,66),(50,66)]),s.lines([(58,44),(74,66)]),s.lines([(74,44),(58,66)])))
s=Spr(96,3); s.poly(star(c,c,4,44,14,0)); s.poly(star(c,c,4,20,8,0.78),lw=2); s.save('pick_shard')
pick('up_thrust',lambda s:(s.poly([(36,26),(60,26),(68,50),(28,50)],fill=False),s.lines([(36,58),(30,74)]),s.lines([(48,58),(48,78)]),s.lines([(60,58),(66,74)])),False)
pick('up_cannon',lambda s:(s.poly([(22,40),(74,40),(74,56),(22,56)],fill=False),s.lines([(40,40),(40,56)]),s.lines([(74,34),(74,62)])),False)
pick('up_firerate',lambda s:(s.circ(48,48,26,False),s.lines([(48,48),(48,28)]),s.lines([(48,48),(62,56)])),False)
pick('up_multi',lambda s:(s.lines([(22,36),(74,36)]),s.lines([(22,60),(74,60)]),s.lines([(60,26),(74,36),(60,46)]),s.lines([(60,50),(74,60),(60,70)])),False)
pick('up_magnet',lambda s:(s.dl.arc([26*SS,26*SS,70*SS,70*SS],180,360,fill=255,width=4*SS),s.lines([(26,48),(26,70)]),s.lines([(70,48),(70,70)]),s.lines([(18,70),(34,70)]),s.lines([(62,70),(78,70)])),False)
pick('up_dash',lambda s:(s.poly([(62,48),(36,62),(42,48),(36,34)],fill=False),s.lines([(18,38),(30,38)]),s.lines([(14,48),(32,48)]),s.lines([(18,58),(30,58)]),s.lines([(70,28),(70,68)])),False)
pick('up_ship',lambda s:(s.poly([(74,48),(30,68),(40,48),(30,28)],fill=False)),False)

# bolt: bright core, soft edges (additive)
w,h=96,32
y,x=np.mgrid[0:h,0:w]; dx=(x-w/2+.5)/(w/2); dy=(y-h/2+.5)/(h/2)
d=np.sqrt(dx**2+(dy*1.0)**2); a=np.clip(1-d,0,1)**1.5; core=np.clip(1-np.sqrt((dx/.75)**2+(dy/.28)**2),0,1)
v=np.clip(a*.8+core*2,0,1); Image.fromarray((np.dstack([v,v,v,v])*255).astype('uint8'),'RGBA').save(f'{OUT}/bolt.png')
# glow
n=64; y,x=np.mgrid[0:n,0:n]; d=np.hypot(x-n/2+.5,y-n/2+.5)/(n/2); v=np.clip(1-d,0,1)**2
Image.fromarray((np.dstack([v,v,v,v])*255).astype('uint8'),'RGBA').save(f'{OUT}/glow.png')
# ring
n=256; y,x=np.mgrid[0:n,0:n]; d=np.hypot(x-n/2+.5,y-n/2+.5)
v=np.exp(-((d-120)/2.6)**2)+0.35*np.exp(-((d-120)/8)**2); v=np.clip(v,0,1)
Image.fromarray((np.dstack([np.ones_like(v),np.ones_like(v),np.ones_like(v),v])*255).astype('uint8'),'RGBA').save(f'{OUT}/ring.png')
# swirl: gravity well
n=256; y,x=np.mgrid[0:n,0:n]; X=x-n/2+.5; Y=y-n/2+.5; r=np.hypot(X,Y)/(n/2); th=np.arctan2(Y,X)
sp=(np.sin(4*th+9*np.log(r+.08))*.5+.5)**5*np.clip(1-r,0,1)**.6
core=(r<.27).astype(float); rim=np.exp(-((r-.27)/.018)**2)
v=np.clip(sp*.9*(1-core)+rim,0,1); a=np.clip(v+core,0,1); rgb=np.where(a>0,v/np.maximum(a,1e-3),0)
Image.fromarray((np.dstack([rgb,rgb,rgb,a])*255).astype('uint8'),'RGBA').save(f'{OUT}/swirl.png')
# vision: black with a soft transparent hole
n=256; y,x=np.mgrid[0:n,0:n]; d=np.hypot(x-n/2+.5,y-n/2+.5)/(n/2); a=np.clip((d-.32)/(.95-.32),0,1)**1.2; z=np.zeros_like(a)
Image.fromarray((np.dstack([z,z,z,a])*255).astype('uint8'),'RGBA').save(f'{OUT}/vision.png')
# nebulae: fractal noise, greyscale, tinted in-game
def noise(seed,w=800,h=450):
    rs=np.random.RandomState(seed); out=np.zeros((h,w))
    for o,amp in zip(range(1,8),(1,.55,.3,.17,.09,.05,.03)):
        gw,gh=4*2**(o-1),3*2**(o-1)
        g=rs.rand(gh+2,gw+2); im=Image.fromarray((g*255).astype('uint8'),'L').resize((w,h),Image.BICUBIC)
        out+=np.asarray(im,float)/255*amp
    out=(out-out.min())/(out.max()-out.min()); return out
for i,seed in enumerate((11,29,47),1):
    v=np.clip((noise(seed)-.42)*2.1,0,1)**1.7
    im=Image.fromarray((v*255).astype('uint8'),'L').filter(ImageFilter.GaussianBlur(9)).convert('RGBA'); im.putalpha(255)
    im.save(f'{OUT}/nebula{i}.png')
