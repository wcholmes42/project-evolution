using ProjectEvolution.Game;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectEvolution.WebApi;

public class TunerService
{
    private Task? _evolutionTask;
    private CancellationTokenSource? _cancellationToken;
    private bool _isPaused = false;

    public async Task StartEvolution()
    {
        if (_evolutionTask != null) return; // Already running

        _cancellationToken = new CancellationTokenSource();
        _evolutionTask = Task.Run(() => RunEvolution(_cancellationToken.Token));
    }

    public void StopEvolution()
    {
        _cancellationToken?.Cancel();
        _evolutionTask = null;
    }

    public void PauseEvolution()
    {
        _isPaused = !_isPaused;
    }

    private void RunEvolution(CancellationToken token)
    {
        // Call the actual C# tuner - just invoke it directly
        ProgressionFrameworkResearcher.RunContinuousResearch();
    }
}
