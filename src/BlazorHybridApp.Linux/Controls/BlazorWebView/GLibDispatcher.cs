using Microsoft.AspNetCore.Components;

namespace BlazorHybridApp.Linux.Controls.BlazorWebView;

internal sealed class GLibDispatcher : Dispatcher
{
    private readonly Thread _mainThread = Thread.CurrentThread;

    public override bool CheckAccess()
    {
        return Thread.CurrentThread == _mainThread;
    }

    public override Task InvokeAsync(Action workItem)
    {
        Func<Task<bool>> wrappedWorkItem = () =>
        {
            workItem();
            return Task.FromResult(true);
        };
        return InvokeAsync(wrappedWorkItem);
    }

    public override Task InvokeAsync(Func<Task> workItem)
    {
        Func<Task<bool>> wrappedWorkItem = async () =>
        {
            await workItem();
            return true;
        };
        return InvokeAsync(wrappedWorkItem);
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        Func<Task<TResult>> wrappedWorkItem = () => Task.FromResult(workItem());
        return InvokeAsync(wrappedWorkItem);
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        if (CheckAccess())
        {
            return RunAsync(workItem);
        }

        var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        GLib.Functions.IdleAdd(GLib.Constants.PRIORITY_DEFAULT, () =>
        {
            RunAsync(workItem).ContinueWith(task => taskCompletionSource.SetFromTask(task), TaskScheduler.Default);
            return false;
        });
        return taskCompletionSource.Task;
    }

    private static Task<TResult> RunAsync<TResult>(Func<Task<TResult>> workItem)
    {
        try
        {
            return workItem();
        }
        catch (Exception exception)
        {
            return Task.FromException<TResult>(exception);
        }
    }
}
