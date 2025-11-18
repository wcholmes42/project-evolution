"""
Hardware monitoring with dynamic throttling
Monitors CPU, GPU, RAM and throttles tuner when system is busy
"""
import psutil
import asyncio
from typing import Dict, Optional
try:
    import pynvml
    HAS_NVIDIA = True
except ImportError:
    HAS_NVIDIA = False


class HardwareMonitor:
    """Monitor system resources and auto-throttle"""
    
    def __init__(self):
        global HAS_NVIDIA
        self.running = False
        self.gpu_handle = None
        self.has_nvidia = HAS_NVIDIA

        # Thresholds for auto-throttle
        self.cpu_throttle_threshold = 80  # If other processes use >80%, throttle
        self.gpu_throttle_threshold = 70
        self.temp_throttle_threshold = 80  # °C

        # Current stats
        self.stats = {
            "cpu_percent": 0.0,
            "cpu_temp": None,
            "ram_percent": 0.0,
            "ram_used_gb": 0.0,
            "gpu_percent": 0.0,
            "gpu_temp": 0,
            "gpu_mem_used_gb": 0.0,
            "gpu_mem_total_gb": 0.0,
            "should_throttle": False,
            "throttle_reason": None
        }

        if self.has_nvidia:
            try:
                pynvml.nvmlInit()
                self.gpu_handle = pynvml.nvmlDeviceGetHandleByIndex(0)
                print("✅ NVIDIA GPU monitoring enabled")
            except Exception as e:
                print(f"⚠️  NVIDIA monitoring failed: {e}")
                self.has_nvidia = False
    
    async def start(self):
        """Start monitoring loop"""
        self.running = True
        asyncio.create_task(self._monitor_loop())
    
    async def stop(self):
        """Stop monitoring"""
        self.running = False
        if self.has_nvidia and self.gpu_handle:
            pynvml.nvmlShutdown()

    def has_gpu(self) -> bool:
        """Check if GPU is available"""
        return self.has_nvidia and self.gpu_handle is not None
    
    def get_gpu_info(self) -> str:
        """Get GPU name"""
        if not self.has_gpu():
            return "No GPU detected"
        try:
            name = pynvml.nvmlDeviceGetName(self.gpu_handle)
            return name.decode() if isinstance(name, bytes) else name
        except:
            return "Unknown GPU"
    
    async def _monitor_loop(self):
        """Background monitoring loop"""
        while self.running:
            try:
                # CPU
                cpu_percent = psutil.cpu_percent(interval=0.5)
                
                # RAM
                ram = psutil.virtual_memory()
                ram_percent = ram.percent
                ram_used_gb = ram.used / (1024**3)
                
                # CPU temp (if available)
                cpu_temp = None
                try:
                    temps = psutil.sensors_temperatures()
                    if 'coretemp' in temps:
                        cpu_temp = max(t.current for t in temps['coretemp'])
                except:
                    pass
                
                # GPU stats
                gpu_percent = 0.0
                gpu_temp = 0
                gpu_mem_used = 0.0
                gpu_mem_total = 0.0
                
                if self.has_gpu():
                    try:
                        # GPU utilization
                        util = pynvml.nvmlDeviceGetUtilizationRates(self.gpu_handle)
                        gpu_percent = util.gpu
                        
                        # GPU temperature
                        gpu_temp = pynvml.nvmlDeviceGetTemperature(self.gpu_handle, pynvml.NVML_TEMPERATURE_GPU)
                        
                        # GPU memory
                        mem_info = pynvml.nvmlDeviceGetMemoryInfo(self.gpu_handle)
                        gpu_mem_used = mem_info.used / (1024**3)
                        gpu_mem_total = mem_info.total / (1024**3)
                    except Exception as e:
                        pass
                
                # Determine if we should throttle
                should_throttle = False
                throttle_reason = None
                
                # Check CPU usage (excluding our own process)
                if cpu_percent > self.cpu_throttle_threshold:
                    should_throttle = True
                    throttle_reason = f"CPU high ({cpu_percent:.0f}%)"
                
                # Check GPU usage
                elif gpu_percent > self.gpu_throttle_threshold:
                    should_throttle = True
                    throttle_reason = f"GPU busy ({gpu_percent:.0f}%)"
                
                # Check temperatures
                elif cpu_temp and cpu_temp > self.temp_throttle_threshold:
                    should_throttle = True
                    throttle_reason = f"CPU hot ({cpu_temp:.0f}°C)"
                
                elif gpu_temp > self.temp_throttle_threshold:
                    should_throttle = True
                    throttle_reason = f"GPU hot ({gpu_temp}°C)"
                
                # Update stats
                self.stats.update({
                    "cpu_percent": cpu_percent,
                    "cpu_temp": cpu_temp,
                    "ram_percent": ram_percent,
                    "ram_used_gb": round(ram_used_gb, 2),
                    "gpu_percent": gpu_percent,
                    "gpu_temp": gpu_temp,
                    "gpu_mem_used_gb": round(gpu_mem_used, 2),
                    "gpu_mem_total_gb": round(gpu_mem_total, 2),
                    "should_throttle": should_throttle,
                    "throttle_reason": throttle_reason
                })
                
            except Exception as e:
                print(f"Monitoring error: {e}")
            
            await asyncio.sleep(2)  # Update every 2 seconds
    
    def get_stats(self) -> Dict:
        """Get current hardware stats"""
        return self.stats.copy()
    
    def should_throttle(self) -> bool:
        """Check if tuner should throttle"""
        return self.stats.get("should_throttle", False)
    
    def get_throttle_level(self) -> int:
        """Get recommended throttle level (0-100%)"""
        if not self.should_throttle():
            return 100  # Full speed
        
        # Throttle based on severity
        cpu = self.stats["cpu_percent"]
        gpu = self.stats["gpu_percent"]
        
        max_usage = max(cpu, gpu)
        
        if max_usage > 95:
            return 25  # Severe throttle
        elif max_usage > 85:
            return 50  # Moderate throttle
        else:
            return 75  # Light throttle
