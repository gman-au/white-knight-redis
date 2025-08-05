using System.Threading.Tasks;

namespace White.Knight.Redis.Tests.Integration
{
    public interface ITestContainerManager
    {
        Task StartAsync(int hostedPort);

        Task StopAsync();
    }
}