#!/bin/bash
# Start both FastAPI (port 8000) and Streamlit dashboard (port 8501)

# Start FastAPI in background
python3 -m uvicorn api.main:app --host 0.0.0.0 --port 8000 &

# Wait for API to be ready
sleep 3

# Start Streamlit dashboard (foreground)
streamlit run dashboard/app.py --server.port 8501 --server.address 0.0.0.0
