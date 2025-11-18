# Multi-stage build for Python tuner + C# game
FROM nvidia/cuda:12.2.0-runtime-ubuntu22.04 AS base

# Install system dependencies
RUN apt-get update && apt-get install -y \
    python3.11 \
    python3-pip \
    dotnet-runtime-9.0 \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Install Python dependencies
WORKDIR /app
COPY requirements.txt .
RUN pip3 install --no-cache-dir -r requirements.txt

# Copy Python app
COPY api/ ./api/
COPY engine/ ./engine/
COPY monitoring/ ./monitoring/
COPY dashboard/ ./dashboard/

# Copy C# game DLL (built separately)
COPY --from=game-builder /app/publish ./game/

# Expose web port
EXPOSE 8000

# Set environment variables
ENV PYTHONUNBUFFERED=1
ENV NVIDIA_VISIBLE_DEVICES=all
ENV NVIDIA_DRIVER_CAPABILITIES=compute,utility

# Run FastAPI server
CMD ["python3", "-m", "uvicorn", "api.main:app", "--host", "0.0.0.0", "--port", "8000"]
