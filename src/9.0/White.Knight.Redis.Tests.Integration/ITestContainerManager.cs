using System.Threading.Tasks;

namespace White.Knight.Redis.Tests.Integration
{
    public interface ITestContainerManager
    {
        Task StartAsync();

        Task StopAsync();
    }
}