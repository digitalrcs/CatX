$ErrorActionPreference='Stop'
$assetRoot=Join-Path $PSScriptRoot '..\src\CatX\Assets\Realistic'
$manifest=Get-Content -LiteralPath (Join-Path $assetRoot 'manifest.json') -Raw | ConvertFrom-Json
$total=0
foreach($coat in $manifest.coats) {
    foreach($clip in $manifest.clips.PSObject.Properties) {
        $folder=Join-Path (Join-Path $assetRoot $coat) $clip.Name
        $files=@(Get-ChildItem -LiteralPath $folder -Filter '*.png' -ErrorAction Stop)
        if($files.Count -ne $clip.Value.frames) { throw "Incomplete realistic cat: $coat/$($clip.Name). Finish the Blender render first." }
        for($i=0;$i -lt $clip.Value.frames;$i++) {
            $path=Join-Path $folder ('{0:000}.png' -f $i)
            if(!(Test-Path -LiteralPath $path)) { throw "Missing animation frame: $path" }
        }
        $total+=$files.Count
    }
}
Write-Output "All $total realistic animation frames are present."
