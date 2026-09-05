"""Build review GIFs/contact sheets from the actual Blender PNG frames."""
import argparse, json
from pathlib import Path
from PIL import Image, ImageDraw

p=argparse.ArgumentParser()
p.add_argument('source',type=Path)
p.add_argument('output',type=Path)
args=p.parse_args()
args.output.mkdir(parents=True,exist_ok=True)
manifest=json.loads((args.source/'manifest.json').read_text())
for clip in ['sleep','groom']:
    paths=sorted((args.source/'Tabby'/clip).glob('*.png'))
    frames=[]
    for path in paths:
        im=Image.open(path).convert('RGBA').resize((768,672),Image.Resampling.LANCZOS)
        bg=Image.new('RGB',im.size,'#eee9df');bg.paste(im,mask=im.getchannel('A'))
        frames.append(bg)
    frames[0].save(args.output/f'{clip}.gif',save_all=True,append_images=frames[1:],
                   duration=round(1000/manifest['clips'][clip]['fps']),loop=0)
    indices=[round(i*(len(frames)-1)/7) for i in range(8)]
    sheet=Image.new('RGB',(1536,672),'#eee9df')
    for i,index in enumerate(indices):
        sheet.paste(frames[index].resize((384,336)),((i%4)*384,(i//4)*336))
    sheet.save(args.output/f'{clip}-contact.png')
