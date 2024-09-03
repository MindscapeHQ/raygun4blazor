using System;
using System.Threading.Tasks;

namespace Raygun.Blazor.Offline;

public interface IBackgroundSendStrategy : IDisposable
{
  public event Func<Task> OnSendAsync;
  public void Start();
  public void Stop();
}