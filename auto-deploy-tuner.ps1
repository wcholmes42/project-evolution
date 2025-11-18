#!/usr/bin/env pwsh
# Auto-deploy progression tuner to vault42
# Waits for build, then starts container with GPU

$vault42 = "192.168.68.42"

Write-Host "=== Progression Tuner Auto-Deploy ===" -ForegroundColor Cyan
Write-Host "Target: vault42 ($vault42)" -ForegroundColor Yellow
Write-Host ""

# Step 1: Wait for Docker build to complete
Write-Host "[1/4] Waiting for Docker build to complete..." -ForegroundColor Cyan
$buildComplete = $false
$checkCount = 0

while (-not $buildComplete) {
    $checkCount++
    $images = ssh root@$vault42 'docker images | grep tuner-web'
    
    if ($images -match "tuner-web") {
        Write-Host "Build complete!" -ForegroundColor Green
        $buildComplete = $true
    } else {
        $buildProc = ssh root@$vault42 'ps aux | grep "docker build" | grep -v grep'
        if (-not $buildProc) {
            Write-Host "Build failed or stopped! Check logs:" -ForegroundColor Red
            Write-Host "  ssh root@$vault42 'cat /mnt/user/appdata/progression-tuner/tuner-web/build.log | tail -50'" -ForegroundColor Yellow
            exit 1
        }
        
        Write-Host "  Still building... (check #$checkCount)" -ForegroundColor Yellow
        Start-Sleep -Seconds 30
    }
}

# Step 2: Stop any existing container
Write-Host "`n[2/4] Stopping any existing container..." -ForegroundColor Cyan
ssh root@$vault42 'docker stop progression-tuner 2>/dev/null; docker rm progression-tuner 2>/dev/null'

# Step 3: Start container with GPU
Write-Host "`n[3/4] Starting tuner container with GPU..." -ForegroundColor Cyan
$runCmd = @"
docker run -d \
  --name progression-tuner \
  --gpus all \
  --cpuset-cpus='0-23' \
  --memory=16g \
  -p 8000:8000 \
  -v /mnt/user/GameResearch:/data \
  --restart unless-stopped \
  tuner-web:latest
"@

ssh root@$vault42 $runCmd

Start-Sleep -Seconds 3

# Step 4: Verify deployment
Write-Host "`n[4/4] Verifying deployment..." -ForegroundColor Cyan

# Check container status
$containerStatus = ssh root@$vault42 'docker ps | grep progression-tuner'
if ($containerStatus) {
    Write-Host "  Container running!" -ForegroundColor Green
} else {
    Write-Host "  Container failed to start! Logs:" -ForegroundColor Red
    ssh root@$vault42 'docker logs progression-tuner'
    exit 1
}

# Check GPU access
Write-Host "`n  Checking GPU access..." -ForegroundColor Cyan
$gpuCheck = ssh root@$vault42 'docker exec progression-tuner nvidia-smi 2>&1'
if ($gpuCheck -match "NVIDIA") {
    Write-Host "  GPU detected!" -ForegroundColor Green
} else {
    Write-Host "  GPU not detected (may not be critical)" -ForegroundColor Yellow
}

# Show logs
Write-Host "`n  Container logs:" -ForegroundColor Cyan
ssh root@$vault42 'docker logs progression-tuner 2>&1 | tail -20'

Write-Host "`n=== DEPLOYMENT COMPLETE ===" -ForegroundColor Green
Write-Host ""
Write-Host "Dashboard URL: http://$vault42:8000" -ForegroundColor Cyan
Write-Host "View logs:     ssh root@$vault42 'docker logs -f progression-tuner'" -ForegroundColor Yellow
Write-Host "GPU stats:     ssh root@$vault42 'docker exec progression-tuner nvidia-smi'" -ForegroundColor Yellow
Write-Host ""
