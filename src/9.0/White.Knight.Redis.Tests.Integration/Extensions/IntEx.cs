using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace White.Knight.Redis.Tests.Integration.Extensions
{
    public static class IntEx
    {
        private const string Localhost = "127.0.0.1";

        public static async Task WaitForPortToBeReleased(this int port, ITestOutputHelper helper, TimeSpan timeout)
        {
            var endTime =
                DateTime
                    .UtcNow
                    .Add(timeout);

            while (DateTime.UtcNow < endTime)
            {
                if (!IsPortInUse(port))
                {
                    helper
                        .WriteLine($"Port {port} is now free");

                    return;
                }

                helper
                    .WriteLine($"Port {port} still in use, waiting...");

                await
                    Task
                        .Delay(500);
            }

            throw new TimeoutException($"Port {port} was not released within {timeout}");
        }

        private static bool IsPortInUse(int port, string hostName = Localhost)
        {
            try
            {
                using var client = new TcpClient();

                client
                    .Connect(hostName, port);

                return true; // Port is in use
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}