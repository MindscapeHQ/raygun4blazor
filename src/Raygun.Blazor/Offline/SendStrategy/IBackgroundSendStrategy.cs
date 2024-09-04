using System;
using System.Threading.Tasks;

namespace Raygun.Blazor.Offline.SendStrategy;

/// <summary>
/// 
/// </summary>
public interface IBackgroundSendStrategy : IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    public event Func<Task> OnSendAsync;

    /// <summary>
    /// 
    /// </summary>
    public void Start();

    /// <summary>
    /// 
    /// </summary>
    public void Stop();
}