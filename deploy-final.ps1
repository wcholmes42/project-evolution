Start-Sleep -Seconds 120
$img = ssh root@192.168.68.42 docker images
if ($img -match 'tuner-web') {
    ssh root@192.168.68.42 docker stop progression-tuner
    ssh root@192.168.68.42 docker rm progression-tuner
    ssh root@192.168.68.42 docker run -d --name progression-tuner --gpus all --cpuset-cpus=0-23 --memory=16g -p 8000:8000 -v /mnt/user/GameResearch:/data tuner-web:latest
    Start-Sleep -Seconds 3
    ssh root@192.168.68.42 docker ps
    Write-Host 'DEPLOYED: http://192.168.68.42:8000'
}
