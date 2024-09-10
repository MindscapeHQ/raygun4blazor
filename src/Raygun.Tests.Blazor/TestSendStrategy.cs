using System;
using System.Linq;
using System.Threading.Tasks;
using Raygun.Blazor.Offline.SendStrategy;

namespace Raygun.Tests.Blazor;

public class TestSendStrategy : IBackgroundSendStrategy
{
    public event Func<Task> OnSendAsync;

    public async Task SendAll()
    {
        var invocationList = OnSendAsync?.GetInvocationList();
        if (invocationList != null)
        {
            var tasks = invocationList.OfType<Func<Task>>().Select(handler => handler());
            await Task.WhenAll(tasks);
        }
    }

    public void Dispose()
    {
        // Nothing
    }

    public void Start()
    {
        // Nothing
    }

    public void Stop()
    {
        // Nothing
    }
}