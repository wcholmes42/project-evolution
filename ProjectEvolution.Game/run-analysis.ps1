# Compile and run the tile analyzer
$csFile = "AnalyzeTiles.cs"
$outputExe = "TileAnalyzer.exe"

Write-Host "Compiling analyzer..." -ForegroundColor Cyan
dotnet build -c Release -o bin/analyzer

Write-Host "`nRunning analysis..." -ForegroundColor Cyan
cd bin/analyzer
./ProjectEvolution.Game.exe > ../../tile-analysis-output.txt 2>&1

# Try to run the AnalyzeTiles main
Write-Host "`nRunning standalone analyzer..." -ForegroundColor Yellow

# Create a simple runner
$runnerCode = @'
using ProjectEvolution.Game;
AnalyzeTiles.Main();
'@

$runnerCode | Out-File -FilePath "../../Runner.cs" -Encoding UTF8

cd ../..
dotnet-script Runner.cs > tile-data.txt 2>&1

if (Test-Path "tile-data.txt") {
    Write-Host "`nAnalysis complete! See tile-data.txt" -ForegroundColor Green
    Get-Content "tile-data.txt" | Select-Object -First 50
} else {
    Write-Host "`nERROR: Analysis failed" -ForegroundColor Red
}
