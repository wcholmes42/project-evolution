# Monitor Docker build on vault42
Write-Host "Checking Docker build progress on vault42..." -ForegroundColor Cyan

while ($true) {
    $buildProcess = ssh root@192.168.68.42 'ps aux | grep "docker build" | grep -v grep'
    
    if ($buildProcess) {
        Write-Host "Build still running..." -ForegroundColor Yellow
        ssh root@192.168.68.42 'tail -5 /mnt/user/appdata/progression-tuner/tuner-web/build.log'
    } else {
        Write-Host "`nBuild completed!" -ForegroundColor Green
        ssh root@192.168.68.42 'tail -20 /mnt/user/appdata/progression-tuner/tuner-web/build.log'
        break
    }
    
    Start-Sleep -Seconds 30
}

Write-Host "`nReady to start container!" -ForegroundColor Green
