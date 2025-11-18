Write-Host 'Waiting 3 minutes for build...'
Start-Sleep -Seconds 180

$img = ssh root@192.168.68.42 docker images
if ($img -match 'tuner-web') {
    Write-Host 'BUILD COMPLETE! Deploying...'
    ssh root@192.168.68.42 docker stop progression-tuner
    ssh root@192.168.68.42 docker rm progression-tuner
    ssh root@192.168.68.42 docker run -d --name progression-tuner --gpus all --cpuset-cpus=0-23 --memory=16g -p 8000:8000 -v /mnt/user/GameResearch:/data --restart unless-stopped tuner-web:latest
    Start-Sleep -Seconds 5
    ssh root@192.168.68.42 docker logs progression-tuner
    Write-Host 'Dashboard: http://192.168.68.42:8000'
} else {
    Write-Host 'Still building...'
}
