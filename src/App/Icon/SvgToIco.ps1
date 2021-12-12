Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# The script requires https://imagemagick.org/script/download.php
$imageMagickCommand = Get-Command -Name magick

$iconResolutions = 16,20,24,32,40,48,64,256
$pngImages = @()
Foreach($r in $iconResolutions) {
    & $imageMagickCommand convert -size "${r}x${r}" -depth 8 "$PSScriptRoot/icon.svg" "${r}.png"
    $pngImages += "${r}.png"
}

& $imageMagickCommand convert $pngImages -compress jpeg "icon.ico"

Foreach($image in $pngImages) {
    Remove-Item $image
}
