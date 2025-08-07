namespace FiberTradeTask2.Models
{
    public class TrafficSettings
    {
        public string? DestinationIp { get; set; }
        public int DestinationPort { get; set; } = 7777;
        public int SourcePort { get; set; } = 0; // При 0 свободный порт выбирается автоматически
        public long BandwidthBPS { get; set; } = 1000000; // 1 Мбит/c
        public int PacketSizeBytes { get; set; } = 1024;
        public string? MacAddressFilter { get; set; }
        public NetworkInterfaceModel? SelectedInterface { get; set; }
        public string? FilePath { get; set; }
        public string? SavePath { get; set; }
    }
}
