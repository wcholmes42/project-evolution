# Progression Tuner - DEPLOYED ON VAULT42

## Status: LIVE ✅

**Server:** vault42 (192.168.68.42)  
**Dashboard:** http://192.168.68.42:8000  
**Container:** progression-tuner  
**GPU:** NVIDIA GeForce RTX 5070 Ti (16GB)  

## Current Stats

```json
{
  "hardware": {
    "gpu": "RTX 5070 Ti",
    "gpu_temp": 37°C,
    "gpu_vram": "7.15GB / 15.92GB used",
    "cpu_percent": 0.2%,
    "ram_used": 13.35GB
  },
  "evolution": {
    "device": "cuda",
    "population_size": 100,
    "parallel_cpus": 20 (cores 0-19)
  }
}
```

## API Endpoints

### Health Check
```bash
curl http://192.168.68.42:8000/
# {"status":"running","gpu_available":true,"version":"1.0.0"}
```

### Status (Real-time)
```bash
curl http://192.168.68.42:8000/api/status
```

### Start Evolution
```bash
curl -X POST http://192.168.68.42:8000/api/evolution/start
```

### Stop Evolution
```bash
curl -X POST http://192.168.68.42:8000/api/evolution/stop
```

### Set Throttle (0-100%)
```bash
curl -X POST http://192.168.68.42:8000/api/throttle/75
# Runs at 75% speed (auto-throttles when system busy)
```

## WebSocket (Real-Time Updates)

```javascript
const ws = new WebSocket('ws://192.168.68.42:8000/ws');
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  console.log('Gen:', data.evolution.generation);
  console.log('Fitness:', data.evolution.best_fitness);
  console.log('GPU:', data.hardware.gpu_percent + '%');
};
```

Updates every 500ms with:
- Current generation & fitness
- GPU temp/usage/memory
- CPU usage
- Auto-throttle status

## Container Management

### View Logs
```bash
ssh root@192.168.68.42 'docker logs -f progression-tuner'
```

### Restart
```bash
ssh root@192.168.68.42 'docker restart progression-tuner'
```

### Stop
```bash
ssh root@192.168.68.42 'docker stop progression-tuner'
```

### GPU Stats
```bash
ssh root@192.168.68.42 'docker exec progression-tuner nvidia-smi'
```

## Performance Configuration

### Current Setup
- **CPU:** 20 cores (0-19) @ 0.2% idle
- **GPU:** RTX 5070 Ti @ 0% idle (ready to max out)
- **RAM:** 16GB limit (13GB used by container)
- **Population:** 100 candidates in parallel

### Expected Performance
- **Candidate Generation:** 100-200/sec (GPU)
- **Fitness Evaluation:** 20 parallel C# processes
- **Throughput:** 500-2000 gen/s (vs 250 gen/s CPU-only)
- **Overnight (8hr):** 15M-60M generations

### Auto-Throttling
Automatically reduces speed when:
- CPU >80% (other processes need power)
- GPU >70% (ComfyUI, etc. need GPU)
- Temp >80°C (thermal protection)

## Output Files

Saved to `/mnt/user/GameResearch/`:
- `progression_champion.json` - Best ever found
- `progression_champion_*.json` - Timestamped backups
- `research_log.txt` - Evolution log
- `GeneratedCode/*.cs` - Auto-generated game code

## Next Steps

### 1. Start Evolution
```bash
curl -X POST http://192.168.68.42:8000/api/evolution/start
```

### 2. Monitor Progress
Open: http://192.168.68.42:8000/api/status

Or WebSocket for real-time:
```bash
websocat ws://192.168.68.42:8000/ws
```

### 3. Build Web Dashboard (Optional)
Currently API-only. To add web UI:
```bash
# Add React dashboard or use Streamlit
# See tuner-web/dashboard/app.py (TODO)
```

### 4. Check Results
```bash
ssh root@192.168.68.42 'cat /mnt/user/GameResearch/progression_champion.json | jq .Metadata'
```

## Troubleshooting

### Container Not Running
```bash
ssh root@192.168.68.42 'docker ps -a | grep progression'
ssh root@192.168.68.42 'docker logs progression-tuner'
```

### GPU Not Detected
```bash
# Check NVIDIA runtime
ssh root@192.168.68.42 'docker run --rm --gpus all nvidia/cuda:12.2.0-base nvidia-smi'
```

### High Temperature
Auto-throttles at 80°C. To lower threshold:
```bash
# Edit in container:
ssh root@192.168.68.42 'docker exec -it progression-tuner vi /app/monitoring/hardware.py'
# Change: self.temp_throttle_threshold = 75
# Then: docker restart progression-tuner
```

## Architecture

```
vault42:8000 (FastAPI)
    ↓
GPU Evolution Engine (PyTorch on RTX 5070 Ti)
    ↓ (spawns 20 parallel processes)
C# Game Simulator (real combat logic)
    ↓ (saves to)
/mnt/user/GameResearch/
```

**Status:** FULLY OPERATIONAL 🚀

GPU-accelerated hybrid tuner deployed and ready to evolve!
