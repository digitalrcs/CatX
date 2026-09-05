from PIL import Image
from pathlib import Path
import sys
source, target = map(Path, sys.argv[1:3])
target.parent.mkdir(parents=True, exist_ok=True)
frames=[]
for path in sorted(source.glob('*.png')):
    with Image.open(path) as im:
        frames.append(im.convert('RGB').resize((800,520)))
frames[0].save(target,save_all=True,append_images=frames[1:],duration=50,loop=0)
print(target)
