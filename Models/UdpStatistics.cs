using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FiberTradeTask2.Models
{
    public class UdpStatistics : INotifyPropertyChanged
    {
        private long m_sentPackets;
        private long m_receivedPackets;
        private long m_sentBytes;
        private long m_receivedBytes;

        public long SentPackets
        {
            get => m_sentPackets;
            set { m_sentPackets = value; OnPropertyChanged(); }
        }

        public long ReceivedPackets
        {
            get => m_receivedPackets;
            set { m_receivedPackets = value; OnPropertyChanged(); }
        }

        public long SentBytes
        {
            get => m_sentBytes;
            set { m_sentBytes = value; OnPropertyChanged(); }
        }

        public long ReceivedBytes
        {
            get => m_receivedBytes;
            set { m_receivedBytes = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
