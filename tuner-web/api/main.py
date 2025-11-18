"""
FastAPI server for progression tuner web interface
Real-time evolution monitoring with GPU acceleration
"""
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse
import asyncio
import json
from datetime import datetime
from typing import List

from engine.gpu_evolution import GPUEvolutionEngine
from monitoring.hardware import HardwareMonitor

app = FastAPI(title="Progression Tuner", version="1.0.0")

# Global state
evolution_engine = None
hardware_monitor = HardwareMonitor()
connected_clients: List[WebSocket] = []


@app.on_event("startup")
async def startup_event():
    """Initialize services on startup"""
    global evolution_engine
    evolution_engine = GPUEvolutionEngine()
    await hardware_monitor.start()
    print("🚀 Tuner Web API started")
    print(f"   GPU: {hardware_monitor.get_gpu_info()}")


@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup on shutdown"""
    await hardware_monitor.stop()
    if evolution_engine:
        evolution_engine.stop()


@app.get("/")
async def root():
    """Health check"""
    return {
        "status": "running",
        "gpu_available": hardware_monitor.has_gpu(),
        "version": "1.0.0"
    }


@app.get("/api/status")
async def get_status():
    """Get current evolution status"""
    hw_stats = hardware_monitor.get_stats()
    evo_stats = evolution_engine.get_stats() if evolution_engine else {}
    
    return {
        "hardware": hw_stats,
        "evolution": evo_stats,
        "timestamp": datetime.now().isoformat()
    }


@app.post("/api/evolution/start")
async def start_evolution():
    """Start evolution process"""
    if not evolution_engine:
        return {"error": "Evolution engine not initialized"}
    
    await evolution_engine.start()
    return {"status": "started"}


@app.post("/api/evolution/stop")
async def stop_evolution():
    """Stop evolution process"""
    if evolution_engine:
        evolution_engine.stop()
    return {"status": "stopped"}


@app.post("/api/evolution/pause")
async def pause_evolution():
    """Pause evolution process"""
    if evolution_engine:
        evolution_engine.pause()
    return {"status": "paused"}


@app.post("/api/throttle/{percentage}")
async def set_throttle(percentage: int):
    """Set CPU/GPU throttle (0-100%)"""
    if evolution_engine:
        evolution_engine.set_throttle(percentage)
    return {"throttle": percentage}


@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    """WebSocket for real-time updates"""
    await websocket.accept()
    connected_clients.append(websocket)
    
    try:
        while True:
            # Send updates every 500ms
            await asyncio.sleep(0.5)
            
            # Get current stats
            stats = {
                "hardware": hardware_monitor.get_stats(),
                "evolution": evolution_engine.get_stats() if evolution_engine else {},
                "timestamp": datetime.now().isoformat()
            }
            
            # Check if we should throttle based on system load
            if hardware_monitor.should_throttle():
                if evolution_engine:
                    evolution_engine.auto_throttle(hardware_monitor.get_throttle_level())
            
            await websocket.send_json(stats)
            
    except WebSocketDisconnect:
        connected_clients.remove(websocket)


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")
