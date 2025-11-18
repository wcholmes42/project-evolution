# Re-download and extract the full tileset package
$url = "https://opengameart.org/sites/default/files/Roguelike%20pack.zip"
$outputPath = "D:\code\project-evolution\ProjectEvolution.Game\Assets\Tilesets\roguelike-pack-full.zip"
$extractPath = "D:\code\project-evolution\ProjectEvolution.Game\Assets\Tilesets\extracted"

Write-Host "Downloading tileset package..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $url -OutFile $outputPath -UseBasicParsing

Write-Host "Extracting all files..." -ForegroundColor Cyan
Expand-Archive -Path $outputPath -DestinationPath $extractPath -Force

Write-Host ""
Write-Host "All extracted files:" -ForegroundColor Yellow
Get-ChildItem -Path $extractPath -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Replace($extractPath, "")
    if ($_.PSIsContainer) {
        Write-Host "  [DIR]  $relativePath" -ForegroundColor Cyan
    } else {
        Write-Host "  [FILE] $relativePath - $($_.Length) bytes" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
