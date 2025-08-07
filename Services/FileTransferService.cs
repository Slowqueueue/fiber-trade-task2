using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace FiberTradeTask2.Services
{
    public class FileTransferService
    {
        private const int FileChunkSize = 8192; // фрагмент файла 8 КБ

        public static async Task SendFileAsync(string filePath, string destinationIp, int port, string localIp)
        {
            using var client = new UdpClient(new IPEndPoint(IPAddress.Parse(localIp), 0));
            var destination = new IPEndPoint(IPAddress.Parse(destinationIp), port);

            var fileInfo = $"{Path.GetFileName(filePath)}|{new FileInfo(filePath).Length}";
            await client.SendAsync(Encoding.UTF8.GetBytes(fileInfo), fileInfo.Length, destination);

            using var fileStream = File.OpenRead(filePath);
            var buffer = new byte[FileChunkSize];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
            {
                await client.SendAsync(buffer, bytesRead, destination);
            }
        }

        public static async Task ReceiveFileAsync(string savePath, int port, string localIp, string? macFilter = null)
        {
            using var client = new UdpClient(new IPEndPoint(IPAddress.Parse(localIp), port));
            var fileInfoResult = await client.ReceiveAsync();

            if (!string.IsNullOrEmpty(macFilter))
            {
                var remoteMac = GetMacAddress(fileInfoResult.RemoteEndPoint.Address);
                if (remoteMac?.ToString() != macFilter)
                    throw new Exception("asdasd");
            }

            var fileInfo = Encoding.UTF8.GetString(fileInfoResult.Buffer).Split('|');

            string fileName = fileInfo[0];
            var fileSize = long.Parse(fileInfo[1]);

            string fullPath = Path.Combine(savePath, fileName);

            using var fileStream = File.Create(fullPath);
            long totalReceived = 0;

            while (totalReceived < fileSize)
            {
                var result = await client.ReceiveAsync();
                await fileStream.WriteAsync(result.Buffer.AsMemory(0, result.Buffer.Length));
                totalReceived += result.Buffer.Length;
            }
        }

        private static PhysicalAddress? GetMacAddress(IPAddress ip)
        {
            foreach (NetworkInterface NI in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ipProperties = NI.GetIPProperties();

                foreach (var addr in ipProperties.UnicastAddresses)
                {
                    if (addr.Address.Equals(ip))
                        return NI.GetPhysicalAddress();
                }
            }
            return null;
        }
    }
}
