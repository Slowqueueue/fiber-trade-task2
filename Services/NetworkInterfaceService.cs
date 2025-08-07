using FiberTradeTask2.Models;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace FiberTradeTask2.Services
{
    public class NetworkInterfaceService
    {
        public static IEnumerable<NetworkInterfaceModel> GetActiveInterfaces()
        {
            foreach (NetworkInterface NI in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (NI.OperationalStatus != OperationalStatus.Up || NI.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProperties = NI.GetIPProperties();
                var unicastAddress = ipProperties.UnicastAddresses.FirstOrDefault(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork);

                if (unicastAddress == null)
                    continue;

                yield return new NetworkInterfaceModel
                {
                    Name = NI.Name,
                    Description = NI.Description,
                    MacAddress = NI.GetPhysicalAddress(),
                    IpAddress = unicastAddress.Address
                };
            }
        }
    }
}
