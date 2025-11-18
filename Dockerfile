# Use .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ProjectEvolution.Game/ProjectEvolution.Game.csproj ./ProjectEvolution.Game/
RUN dotnet restore "./ProjectEvolution.Game/ProjectEvolution.Game.csproj"

# Copy source code
COPY ProjectEvolution.Game/ ./ProjectEvolution.Game/

# Build the application
WORKDIR /src/ProjectEvolution.Game
RUN dotnet build "ProjectEvolution.Game.csproj" -c Release -o /app/build
RUN dotnet publish "ProjectEvolution.Game.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Install required dependencies
RUN apt-get update && apt-get install -y \
    procps \
    && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Set environment variables for parallel execution
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV DOTNET_gcServer=1
ENV DOTNET_gcConcurrent=1

# Create volume mount point for research output (Unraid share)
VOLUME /data

# Auto-run progression research on container start
# Uses -it for interactive terminal (required for Console apps)
ENTRYPOINT ["dotnet", "ProjectEvolution.Game.dll"]

# Default: auto-start research mode
# Override with: docker run -it project-evolution
