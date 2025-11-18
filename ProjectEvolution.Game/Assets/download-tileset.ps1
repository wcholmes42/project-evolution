# Download Kenney's Roguelike Pack tileset
$url = "https://opengameart.org/sites/default/files/Roguelike%20pack.zip"
$outputPath = "$env:TEMP\roguelike-pack.zip"
$extractPath = "$env:TEMP\roguelike-pack"
$targetPath = "D:\code\project-evolution\ProjectEvolution.Game\Assets\Tilesets"

Write-Host "Downloading Kenney's Roguelike Pack tileset..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $url -OutFile $outputPath -UseBasicParsing

Write-Host "Extracting tileset..." -ForegroundColor Cyan
Expand-Archive -Path $outputPath -DestinationPath $extractPath -Force

Write-Host "Finding spritesheet..." -ForegroundColor Cyan
$pngFiles = Get-ChildItem -Path $extractPath -Filter "*.png" -Recurse
foreach ($file in $pngFiles) {
    Write-Host "  Found: $($file.Name) ($($file.Length) bytes)" -ForegroundColor Yellow
}

# Copy the main tileset (should be the largest PNG)
$mainTileset = $pngFiles | Sort-Object Length -Descending | Select-Object -First 1
if ($mainTileset) {
    Copy-Item -Path $mainTileset.FullName -Destination "$targetPath\roguelike-pack.png" -Force
    Write-Host "SUCCESS: Tileset installed to $targetPath\roguelike-pack.png" -ForegroundColor Green
    Write-Host "  Size: $($mainTileset.Length) bytes" -ForegroundColor Gray
} else {
    Write-Host "ERROR: No tileset PNG found!" -ForegroundColor Red
}

# Cleanup
Remove-Item -Path $outputPath -Force -ErrorAction SilentlyContinue
Remove-Item -Path $extractPath -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green
