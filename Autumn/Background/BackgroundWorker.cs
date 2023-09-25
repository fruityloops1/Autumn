namespace Autumn.Background;

internal class BackgroundWorker
{
    private readonly List<BackgroundTask> _tasks = new();

    private readonly Thread _thread;

    public string StatusMessage { get; private set; } = string.Empty;

    public bool IsIdle { get; private set; } = true;

    public bool IsStopping { get; private set; } = false;

    public BackgroundWorker() => _thread = new(new ThreadStart(BackgroundWork));

    public void Add(string message, Action action) => Add(new(message, action));

    public void Add(BackgroundTask task) => _tasks.Add(task);

    public void Run() => _thread.Start();

    public void Stop()
    {
        IsStopping = true;

        _thread.Join();
    }

    private void BackgroundWork()
    {
        while (!IsStopping)
        {
            if (_tasks.Count > 0)
            {
                BackgroundTask task = _tasks[0];
                IsIdle = false;

                StatusMessage = task.Message;
                task.Action.Invoke();

                _tasks.RemoveAt(0);
            }
            else
            {
                IsIdle = true;

                StatusMessage = string.Empty;
            }
        }
    }
}
