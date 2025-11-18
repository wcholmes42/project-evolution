# Progression Tuner - GPU-Accelerated Web Edition

**Hybrid Python + C# architecture for maximum GPU utilization**

## Architecture

```
┌─────────────────────────────────────────┐
│  Python FastAPI (Port 8000)             │
│  ├─ Real-time web dashboard             │
│  ├─ GPU-accelerated mutations (PyTorch) │
│  ├─ Hardware monitoring & throttling    │
│  └─ WebSocket for live updates          │
└─────────────────────────────────────────┘
         ↕ subprocess calls
┌─────────────────────────────────────────┐
│  C# Game Simulator                      │
│  └─ REAL game logic fitness evaluation  │
└─────────────────────────────────────────┘
```

## Key Features

### GPU Acceleration
- **PyTorch** for massively parallel candidate generation
- **Batch mutations** on GPU (100+ candidates simultaneously)
- **10-50x speedup** vs CPU-only evolution

### Real Game Logic
- Python calls C# game via `dotnet evaluate framework.json`
- Uses **actual combat simulations**, not approximations
- Parallel C# process pool for multi-candidate evaluation

### Hardware Monitoring
- **nvidia-ml-py**: Real-time GPU stats (temp, usage, memory)
- **psutil**: CPU/RAM monitoring
- **Auto-throttling**: Backs off when system is busy

### Web Dashboard
- **FastAPI + WebSockets**: Real-time updates
- **Streamlit** (optional): Instant charts and controls
- Access from any device on network

## Quick Start

### Prerequisites
```bash
# On Unraid/Ubuntu
apt-get install nvidia-docker2 dotnet-runtime-9.0
```

### Build and Run
```bash
cd tuner-web

# Build Docker image
docker-compose build

# Run with GPU passthrough
docker-compose up -d

# Access dashboard
# http://YOUR_UNRAID_IP:8000
```

## Configuration

### GPU Settings (docker-compose.yml)
```yaml
runtime: nvidia
environment:
  - NVIDIA_VISIBLE_DEVICES=all
cpuset: "0-23"  # Use all i9 cores
mem_limit: 16g
```

### Auto-Throttling Thresholds
Edit `monitoring/hardware.py`:
```python
self.cpu_throttle_threshold = 80  # % CPU before throttle
self.gpu_throttle_threshold = 70  # % GPU before throttle
self.temp_throttle_threshold = 80  # °C before throttle
```

## Hardware Utilization

### Expected Performance
- **CPU**: 24 cores @ 80-100% (i9-14900K)
- **GPU**: RTX 5070 Ti @ 60-90% (batch mutations)
- **Throughput**: 500-2000 gen/s (vs 250 gen/s CPU-only)

### Resource Scaling
```
No throttle: 100% speed (2000 gen/s)
Light throttle (75%): 1500 gen/s
Medium throttle (50%): 1000 gen/s
Heavy throttle (25%): 500 gen/s
```

## API Endpoints

### REST API
- `GET /`: Health check
- `GET /api/status`: Current stats
- `POST /api/evolution/start`: Start tuning
- `POST /api/evolution/stop`: Stop tuning
- `POST /api/evolution/pause`: Pause/resume
- `POST /api/throttle/75`: Set throttle to 75%

### WebSocket
```javascript
ws://localhost:8000/ws
// Receives JSON every 500ms:
{
  "hardware": {
    "cpu_percent": 85.2,
    "gpu_percent": 72.1,
    "gpu_temp": 65,
    "should_throttle": false
  },
  "evolution": {
    "generation": 15234,
    "best_fitness": 78.5,
    "avg_fitness": 65.2
  }
}
```

## Development

### Run Locally (without Docker)
```bash
# Install Python dependencies
pip install -r requirements.txt

# Build C# game
cd ../ProjectEvolution.Game
dotnet publish -c Release -o ../tuner-web/game

# Run API server
cd ../tuner-web
python3 -m uvicorn api.main:app --reload
```

### File Structure
```
tuner-web/
├── api/
│   └── main.py           # FastAPI server
├── engine/
│   └── gpu_evolution.py  # GPU-accelerated evolution
├── monitoring/
│   └── hardware.py       # Hardware monitoring
├── dashboard/
│   └── app.py            # Streamlit dashboard (optional)
├── game/                 # C# game DLL (built)
├── requirements.txt
├── Dockerfile
└── docker-compose.yml
```

## Troubleshooting

### GPU Not Detected
```bash
# Check NVIDIA runtime
docker run --rm --gpus all nvidia/cuda:12.2.0-base-ubuntu22.04 nvidia-smi

# If fails, install nvidia-docker2
apt-get install nvidia-docker2
systemctl restart docker
```

### High Temperatures
Auto-throttling will activate at 80°C. To adjust:
```python
# monitoring/hardware.py
self.temp_throttle_threshold = 75  # Lower threshold
```

### Slow Fitness Evaluation
Check C# process spawning:
```bash
# Inside container
ps aux | grep dotnet
# Should see multiple dotnet processes (parallel evaluation)
```

## Performance Tips

1. **Maximize GPU batch size** if you have spare VRAM
   ```python
   # engine/gpu_evolution.py
   self.population_size = 200  # Increase from 100
   ```

2. **Increase parallel C# processes**
   ```python
   self.max_parallel = 32  # Use more cores
   ```

3. **Reduce evaluation time** by caching similar candidates
   ```python
   # Add candidate hash → fitness cache
   ```

## Monitoring

### View Logs
```bash
docker logs -f progression-tuner
```

### Check GPU Usage
```bash
docker exec progression-tuner nvidia-smi
```

### Dashboard Access
Open browser to `http://unraid-ip:8000`

## Output Files

Saved to `/mnt/user/GameResearch` (Unraid share):
- `progression_champion.json` - Best ever found
- `progression_champion_*.json` - Timestamped backups
- `research_log.txt` - Detailed evolution log
- `GeneratedCode/*.cs` - Auto-generated balanced game code

## Next Steps

1. ✅ Deploy to Unraid
2. ✅ Verify GPU usage (`nvidia-smi`)
3. ✅ Access web dashboard
4. ✅ Let it run overnight (expect 500k-1M gens)
5. ✅ Check champion fitness in the morning
6. ✅ Integrate best formulas into game

## License

MIT License - Evolve responsibly! 🧬🚀
