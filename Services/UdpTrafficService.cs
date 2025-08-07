using FiberTradeTask2.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace FiberTradeTask2.Services
{
    public class UdpTrafficService
    {
        private UdpClient? m_senderClient;
        private UdpClient? m_receiverClient;
        private CancellationTokenSource? m_sendingCts;
        private CancellationTokenSource? m_receivingCts;

        public event EventHandler<PacketEventArgs>? PacketSent;
        public event EventHandler<PacketEventArgs>? PacketReceived;

        public async Task StartSendingAsync(TrafficSettings settings)
        {
            m_sendingCts = new CancellationTokenSource();
            m_senderClient = new UdpClient(new IPEndPoint(settings.SelectedInterface!.IpAddress!, settings.SourcePort));

            var destination = new IPEndPoint(IPAddress.Parse(settings.DestinationIp!), settings.DestinationPort);
            var packet = new byte[settings.PacketSizeBytes];
            new Random().NextBytes(packet); // Пакет заполняется случайными числами

            double bitsCountPerPacket = settings.PacketSizeBytes * 8;
            double packetsCountPerSecond = settings.BandwidthBPS / bitsCountPerPacket;
            int delay = (int)(1000 / packetsCountPerSecond); // Задержка между пакетами в мс

            try
            {
                while (!m_sendingCts.Token.IsCancellationRequested)
                {
                    await m_senderClient.SendAsync(packet, packet.Length, destination);
                    PacketSent?.Invoke(this, new PacketEventArgs(packet.Length));

                    if (delay > 0)
                        await Task.Delay(delay, m_sendingCts.Token);
                }
            }
            catch (OperationCanceledException) 
            {
                // Ожидаемая отмена, ничего не делаем
            }
            finally
            {
                m_senderClient.Close();
            }
        }

        public void StopSending()
        {
            m_sendingCts?.Cancel();
        }

        public async Task StartReceivingAsync(TrafficSettings settings)
        {
            m_receivingCts = new CancellationTokenSource();
            m_receiverClient = new UdpClient(new IPEndPoint(settings.SelectedInterface!.IpAddress!, settings.SourcePort));

            PhysicalAddress? MacFilter = null;

            if (!string.IsNullOrEmpty(settings.MacAddressFilter))
            {
                try
                {
                    MacFilter = PhysicalAddress.Parse(settings.MacAddressFilter
                        .Replace(":", "")
                        .Replace("-", "")
                        .ToUpper());
                }
                catch
                {
                    MacFilter = null;
                }
            }

            var macCache = new ConcurrentDictionary<IPAddress, PhysicalAddress>();

            try
            {
                while (!m_receivingCts.Token.IsCancellationRequested)
                {
                    var receiveTask = m_receiverClient.ReceiveAsync();
                    var delayTask = Task.Delay(1000, m_receivingCts.Token);

                    var completedTask = await Task.WhenAny(receiveTask, delayTask);

                    if (completedTask == delayTask)
                        continue;

                    var result = await receiveTask;

                    if (MacFilter != null)
                    {
                        var remoteMac = await Task.Run(() =>
                            macCache.GetOrAdd(result.RemoteEndPoint.Address, GetMacAddress));

                        if (!MacFilter.Equals(remoteMac))
                            continue;
                    }

                    PacketReceived?.Invoke(this, new PacketEventArgs(result.Buffer.Length));
                }
            }
            catch (OperationCanceledException)
            {
                // Ожидаемая отмена, ничего не делаем
            }
            finally
            {
                m_receiverClient.Close();
            }
        }

        public void StopReceiving()
        {
            m_receivingCts?.Cancel();
        }

        private static PhysicalAddress GetMacAddress(IPAddress ip)
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
            return PhysicalAddress.None;
        }
    }

    public class PacketEventArgs(int packetSize) : EventArgs
    {
        public int PacketSize { get; } = packetSize;
    }
}
