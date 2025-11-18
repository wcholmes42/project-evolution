# Docker Deployment for Unraid

## Quick Start

### Option 1: Docker Compose (Recommended)
```bash
# On your Unraid server
cd /mnt/user/appdata/project-evolution
git clone https://github.com/wcholmes42/project-evolution.git .

# Edit docker-compose.yml to match your setup
# - Adjust cpuset for your CPU core count
# - Verify /mnt/user/GameResearch path exists

# Build and run
docker-compose up --build -d

# View logs in real-time
docker logs -f project-evolution-research

# Stop
docker-compose down
```

### Option 2: Manual Docker
```bash
# Build image
docker build -t project-evolution:latest .

# Run with all cores and Unraid share mount
docker run -it \
  --name project-evolution \
  --cpuset-cpus="0-23" \
  -v /mnt/user/GameResearch:/data \
  -e DOTNET_gcServer=1 \
  project-evolution:latest
```

## Performance Configuration

### CPU Optimization
The parallel system is configured for maximum CPU utilization:

- **Population Size**: 16 candidates evaluated simultaneously
- **Parallel Metrics**: All 6 metrics run in parallel per candidate
- **Total Parallelism**: Up to 96 threads (16 candidates × 6 metrics)
- **Thread Pool**: Auto-scales to CPU core count

### Your i9-14900K Setup:
```yaml
cpuset: "0-23"      # 24 threads (8P + 16E cores)
cpu_count: 24
mem_limit: 8g       # Plenty for this workload
```

### Expected Performance:
- **Single-threaded**: ~7-10 gen/s (1 core)
- **Parallel (16-pop)**: ~80-120 gen/s (all cores) = **10-15× speedup!**
- **Overnight run**: 500k-1M generations (was 30k)

## Output Configuration

### Unraid Share Mount
```yaml
volumes:
  - /mnt/user/GameResearch:/data
```

Research outputs save to `/data` inside container:
- `progression_framework.json` - Current best
- `progression_champion.json` - All-time best
- `progression_champion_*.json` - Timestamped backups
- `progression_research.log` - Detailed log
- `progression_summary.log` - Generation summary
- `GeneratedCode/*.cs` - Auto-generated balanced code

### Create Unraid Share First:
```bash
# On Unraid console
mkdir -p /mnt/user/GameResearch
chmod 777 /mnt/user/GameResearch
```

## Monitoring

### View Live Evolution
```bash
# Attach to running container
docker attach project-evolution-research

# Detach without stopping: Ctrl+P, Ctrl+Q
```

### View Logs
```bash
# Recent logs
docker logs --tail 100 project-evolution-research

# Follow logs (live)
docker logs -f project-evolution-research
```

### Check CPU Usage
```bash
# On Unraid
htop

# Or docker stats
docker stats project-evolution-research
```

You should see **95-100% CPU usage** across all cores when running!

## Controls

### While Running:
- **ESC**: Stop and save (must be attached)
- **[R]**: Manual reset → save champion
- Resize window: Auto-adapts (min 80×35)

### Stop/Start Container:
```bash
docker stop project-evolution-research
docker start project-evolution-research
```

## Advanced: GPU Passthrough (Future)

The 5070 Ti isn't currently used (discrete simulations don't benefit from GPU).

**Future optimizations** that could use GPU:
- Monte Carlo tree search (CUDA)
- Neural network-based fitness prediction
- Parallelized minimax combat simulation
- Reinforcement learning agent training

For now, focus on CPU parallelization = best ROI.

## Troubleshooting

### Container Won't Start
```bash
# Check logs
docker logs project-evolution-research

# Common issues:
# 1. Share path doesn't exist
mkdir -p /mnt/user/GameResearch

# 2. Permission denied
chmod 777 /mnt/user/GameResearch

# 3. Port conflict (shouldn't happen - no ports exposed)
```

### Slow Performance
```bash
# Check CPU affinity
docker inspect project-evolution-research | grep -i cpu

# Verify parallel execution in logs
docker logs --tail 50 project-evolution-research
# Should see: "⚡ Evaluating 16 frameworks (using all cores)"
```

### Out of Memory
```bash
# Increase memory limit in docker-compose.yml
mem_limit: 16g  # Increase from 8g

# Restart
docker-compose down && docker-compose up -d
```

## Expected Results

### Performance Gains:
| Setup | Throughput | Overnight (8hr) |
|-------|-----------|-----------------|
| Single-threaded | 7-10 gen/s | 30k gens |
| Parallel (8 cores) | 40-60 gen/s | 200k gens |
| **Parallel (24 cores)** | **80-120 gen/s** | **500k-1M gens** |

### File Sizes:
- Champion backups: ~10KB each
- Research log: 1MB per 10k generations (auto-managed)
- Generated code: ~5KB per update

**Disk usage**: ~100-500MB for overnight run (safe for Unraid share)

## Maintenance

### Clean Old Backups
```bash
# Keep only last 10 champion backups
cd /mnt/user/GameResearch
ls -t progression_champion_*.json | tail -n +11 | xargs rm -f
```

### View Best Results
```bash
# On Unraid or any machine
cat /mnt/user/GameResearch/progression_champion.json | jq '.Metadata'
```

## Next Steps

1. ✅ Build and deploy container
2. ✅ Verify CPU usage (should be 95-100%)
3. ✅ Let it run overnight
4. ✅ Check champion fitness in the morning
5. ✅ Integrate best formulas into game

Happy evolving! 🧬🚀
