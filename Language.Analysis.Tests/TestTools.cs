namespace TestProject1;

internal static class TestTools
{
    static Task? _timeoutsCheckTask;
    static readonly object _locker = new();
    static readonly Dictionary<Action, (TaskCompletionSource tcs, Task task, DateTime Now, TimeSpan timeout)> _actions = new();
    internal static Func<Task> ApplyTimeout(this Action action, TimeSpan timeout)
    {
        return async () =>
        {
            lock (_locker)
            {
                _timeoutsCheckTask ??= Task.Run(CheckTimeouts);
            }
            
            var task = new Task(action);

            var tcs = new TaskCompletionSource();
            
            _actions.Add(action, (tcs, task, DateTime.Now, timeout));
            task.Start();
            await tcs.Task;
        };
    }
    
    internal static async Task CheckTimeouts()
    {
        while (true)
        {
            for (int i = 0; i < _actions.Count; i++)
            {
                var (action, (taskCompletionSource, task, start, timeout)) = _actions.ToList()[i];
                if (task.IsCompleted)
                {
                    _actions.Remove(action);
                    if (task.Exception != null)
                    {
                        taskCompletionSource.SetException(task.Exception);
                    }
                    else
                    {
                        taskCompletionSource.SetResult();
                    }
                }
                else if (start + timeout < DateTime.Now)
                {
                    _actions.Remove(action);
                    taskCompletionSource.SetException(new TimeoutException());
                }
            }
            await Task.Delay(30);
        }
    }
}