# Tuner Web Integration - Implementation Guide

## Current State

### ✅ COMPLETED:
1. **C# Tuner** - Working at 20k gen/s with real fitness evaluation
2. **TunerWebState** - Shared state class for web integration  
3. **WebApi** - Serves Ultima dashboard + `/api/state` endpoint
4. **Ultima Dashboard** - Full 3-column web UI (zero flicker)

### 🔧 TO FINISH (30 minutes):

## Step 1: Run Both Processes Together

**Option A: Separate Processes** (Simplest for testing)
```bash
# Terminal 1: Run tuner
cd ProjectEvolution.Game
dotnet run

# Terminal 2: Run WebApi
cd ProjectEvolution.WebApi
dotnet run

# Browser: http://localhost:8000
```

**Option B: Process Manager** (Production)
Create `run-tuner-web.ps1`:
```powershell
Start-Job { cd ProjectEvolution.Game; dotnet run }
Start-Sleep -Seconds 2
cd ProjectEvolution.WebApi; dotnet run
```

## Step 2: Update Dashboard to Poll C# API

In `ProjectEvolution.WebApi/wwwroot/ultima.html`, change WebSocket to polling:

```javascript
// OLD: const ws = new WebSocket('ws://192.168.68.42:8000/ws');
// NEW:
setInterval(async () => {
    const response = await fetch('/api/state');
    const data = await response.json();
    updateDashboard(data);
}, 500); // Poll every 500ms
```

## Step 3: Docker Deployment

Update `Dockerfile` to run both:
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
COPY ProjectEvolution.Game/bin/Release/net9.0/ /app/game/
COPY ProjectEvolution.WebApi/bin/Release/net9.0/ /app/webapi/

# Start script
COPY start-tuner-web.sh /app/
CMD ["/app/start-tuner-web.sh"]
```

`start-tuner-web.sh`:
```bash
#!/bin/bash
cd /app/webapi && dotnet ProjectEvolution.WebApi.dll &
cd /app/game && dotnet ProjectEvolution.Game.dll
```

## Expected Performance

- **Throughput**: 20k gen/s (full C# speed)
- **Web UI**: Updates every 500ms via polling
- **No flicker**: Ultima dashboard with smooth DOM updates
- **Real fitness**: Actual game simulation

## Files Ready:
- `ProjectEvolution.Game/TunerWebState.cs` - Shared state
- `ProjectEvolution.Game/ProgressionFramework.cs` - Updates web state
- `ProjectEvolution.WebApi/Program.cs` - Serves dashboard + state
- `ProjectEvolution.WebApi/wwwroot/ultima.html` - Full Ultima UI

## Next Session:
1. Test locally (both processes)
2. Update dashboard to poll instead of WebSocket
3. Create Docker launcher script
4. Deploy to vault42
5. Verify 20k gen/s + web UI working together

**The foundation is complete - just needs final wiring!**
