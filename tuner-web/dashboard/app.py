"""
Streamlit web dashboard for progression tuner
Real-time monitoring of evolution, GPU, and hardware
"""
import streamlit as st
import requests
import json
import time
import plotly.graph_objects as go
from collections import deque
import pandas as pd

# Page config
st.set_page_config(
    page_title="Progression Tuner",
    page_icon="🧬",
    layout="wide"
)

# API endpoint
API_BASE = "http://localhost:8000"

# Initialize session state
if 'fitness_history' not in st.session_state:
    st.session_state.fitness_history = deque(maxlen=100)
if 'gpu_history' not in st.session_state:
    st.session_state.gpu_history = deque(maxlen=100)

# Header
st.title("🧬 Progression Tuner - GPU Accelerated")
st.markdown(f"**Server:** vault42 (192.168.68.42) | **GPU:** RTX 5070 Ti")

# Control panel
col1, col2, col3, col4 = st.columns(4)

with col1:
    if st.button("▶️ Start Evolution", use_container_width=True):
        response = requests.post(f"{API_BASE}/api/evolution/start")
        st.success("Evolution started!")

with col2:
    if st.button("⏸️ Pause", use_container_width=True):
        response = requests.post(f"{API_BASE}/api/evolution/pause")
        st.info("Evolution paused/resumed")

with col3:
    if st.button("⏹️ Stop", use_container_width=True):
        response = requests.post(f"{API_BASE}/api/evolution/stop")
        st.warning("Evolution stopped")

with col4:
    throttle = st.selectbox("Throttle", [100, 75, 50, 25], index=0)
    if st.button("Set Throttle"):
        requests.post(f"{API_BASE}/api/throttle/{throttle}")

st.divider()

# Main dashboard - auto-refresh
placeholder = st.empty()

while True:
    try:
        # Get current status
        response = requests.get(f"{API_BASE}/api/status", timeout=2)
        data = response.json()
        
        hardware = data.get("hardware", {})
        evolution = data.get("evolution", {})
        
        # Track history
        st.session_state.fitness_history.append(evolution.get("best_fitness", 0))
        st.session_state.gpu_history.append(hardware.get("gpu_percent", 0))
        
        with placeholder.container():
            # Top metrics
            m1, m2, m3, m4, m5 = st.columns(5)
            
            m1.metric("Generation", f"{evolution.get('generation', 0):,}")
            m2.metric("Best Fitness", f"{evolution.get('best_fitness', 0):.2f}")
            m3.metric("Population", evolution.get('population_size', 0))
            m4.metric("Device", evolution.get('device', 'cpu').upper())
            m5.metric("GPU Temp", f"{hardware.get('gpu_temp', 0)}°C")
            
            st.divider()
            
            # Charts
            chart_col1, chart_col2 = st.columns(2)
            
            with chart_col1:
                st.subheader("Fitness Progress")
                fitness_fig = go.Figure()
                fitness_fig.add_trace(go.Scatter(
                    y=list(st.session_state.fitness_history),
                    mode='lines',
                    name='Fitness',
                    line=dict(color='#00ff00', width=2)
                ))
                fitness_fig.update_layout(
                    height=300,
                    margin=dict(l=20, r=20, t=20, b=20),
                    yaxis_title="Fitness Score",
                    xaxis_title="Time"
                )
                st.plotly_chart(fitness_fig, use_container_width=True)
            
            with chart_col2:
                st.subheader("GPU Utilization")
                gpu_fig = go.Figure()
                gpu_fig.add_trace(go.Scatter(
                    y=list(st.session_state.gpu_history),
                    mode='lines',
                    name='GPU %',
                    line=dict(color='#ff6b00', width=2)
                ))
                gpu_fig.update_layout(
                    height=300,
                    margin=dict(l=20, r=20, t=20, b=20),
                    yaxis_title="GPU Usage %",
                    xaxis_title="Time"
                )
                st.plotly_chart(gpu_fig, use_container_width=True)
            
            st.divider()
            
            # Hardware stats
            hw_col1, hw_col2, hw_col3 = st.columns(3)
            
            with hw_col1:
                st.subheader("CPU")
                st.metric("Usage", f"{hardware.get('cpu_percent', 0):.1f}%")
                st.metric("Temperature", f"{hardware.get('cpu_temp', 'N/A')}°C" if hardware.get('cpu_temp') else "N/A")
            
            with hw_col2:
                st.subheader("GPU")
                st.metric("Usage", f"{hardware.get('gpu_percent', 0)}%")
                st.metric("VRAM", f"{hardware.get('gpu_mem_used_gb', 0):.1f} / {hardware.get('gpu_mem_total_gb', 0):.1f} GB")
            
            with hw_col3:
                st.subheader("System")
                st.metric("RAM", f"{hardware.get('ram_used_gb', 0):.1f} GB ({hardware.get('ram_percent', 0):.1f}%)")
                
                if hardware.get('should_throttle'):
                    st.warning(f"⚠️ Auto-throttling: {hardware.get('throttle_reason', 'Unknown')}")
                else:
                    st.success("✅ Full speed")
            
            # Evolution details
            st.divider()
            st.subheader("Evolution Details")
            
            detail_col1, detail_col2 = st.columns(2)
            
            with detail_col1:
                st.json({
                    "generation": evolution.get("generation", 0),
                    "best_fitness": evolution.get("best_fitness", 0),
                    "avg_fitness": evolution.get("avg_fitness", 0),
                    "population_size": evolution.get("population_size", 0)
                })
            
            with detail_col2:
                st.json({
                    "device": evolution.get("device", "cpu"),
                    "parallel_games": evolution.get("parallel_games", 0),
                })
        
    except Exception as e:
        st.error(f"Error connecting to API: {e}")
        st.info("Make sure the FastAPI server is running on localhost:8000")
    
    # Refresh every 1 second
    time.sleep(1)
    st.rerun()
