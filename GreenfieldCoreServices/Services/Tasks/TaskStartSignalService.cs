namespace GreenfieldCoreServices.Services.Tasks;

public class TaskStartSignalService
{
    private readonly TaskCompletionSource _signal = new();
    
    public Task WaitForStartAsync(CancellationToken cancellationToken = default)
    {
        return _signal.Task.WaitAsync(cancellationToken);
    }
    
    public void SignalStart()
    {
        _signal.TrySetResult();
    }
}