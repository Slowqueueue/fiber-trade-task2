using System.Net;
using System.Net.NetworkInformation;

namespace FiberTradeTask2.Models
{
    public class NetworkInterfaceModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public PhysicalAddress? MacAddress { get; set; }
        public IPAddress? IpAddress { get; set; }

        public override string ToString() => $"{Name} ({IpAddress})";
    }
}
