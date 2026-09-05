"""Check every rendered frame and create a pose contact sheet for visual review."""
import argparse, json
from pathlib import Path
from PIL import Image, ImageDraw

parser=argparse.ArgumentParser()
parser.add_argument('--assets', default='src/CatX/Assets/Realistic')
parser.add_argument('--preview', default='tmp/realistic-contact-sheet.png')
args=parser.parse_args()
root=Path(args.assets)
manifest=json.loads((root/'manifest.json').read_text())
count=0
clipped=[]
for coat in manifest['coats']:
    for clip,info in manifest['clips'].items():
        files=list((root/coat/clip).glob('*.png'))
        assert len(files)==info['frames'], (coat,clip,len(files),info['frames'])
        for index in range(info['frames']):
            path=root/coat/clip/f'{index:03d}.png'
            with Image.open(path) as img:
                assert img.size==(manifest['width'],manifest['height']),path
                assert img.mode=='RGBA',path
                bbox=img.getchannel('A').point(lambda x:255 if x>12 else 0).getbbox()
                assert bbox and bbox[2]-bbox[0]>40 and bbox[3]-bbox[1]>25,(path,bbox)
                if bbox[0]<2 or bbox[1]<2 or bbox[2]>img.width-2 or bbox[3]>img.height-2: clipped.append(str(path))
            count+=1
assert not clipped, 'Clipped frames: '+str(clipped[:10])
sheet=Image.new('RGB',(256*3,260*len(manifest['coats'])), '#ebedf1')
draw=ImageDraw.Draw(sheet)
for row,coat in enumerate(manifest['coats']):
    for col,clip in enumerate(['walk','groom','sleep']):
        index=manifest['clips'][clip]['frames']//2
        with Image.open(root/coat/clip/f'{index:03d}.png') as img:
            sheet.paste(img,(col*256,row*260+25),img)
        draw.text((col*256+12,row*260+8),f'{coat} - {clip}',fill='#253238')
output=Path(args.preview)
output.parent.mkdir(parents=True,exist_ok=True)
sheet.save(output)
print(f'Validated {count} RGBA frames; no blank frames or clipped heads, paws, or tails. Preview: {output}')
