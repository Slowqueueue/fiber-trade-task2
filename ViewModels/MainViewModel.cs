using FiberTradeTask2.Models;
using FiberTradeTask2.MVVM;
using FiberTradeTask2.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace FiberTradeTask2.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly UdpTrafficService m_udpTrafficService;

        private TrafficSettings m_trafficSettings;
        private UdpStatistics m_udpStatistics;
        private bool m_isSending;
        private bool m_isReceiving;
        private string m_status = "Готов к работе";

        public MainViewModel()
        {
            m_udpTrafficService = new UdpTrafficService();

            m_trafficSettings = new TrafficSettings();
            m_udpStatistics = new UdpStatistics();

            BrowseFileCommand = new RelayCommand(BrowseFile);
            BrowseSavePathCommand = new RelayCommand(BrowseSavePath);
            StartSendingCommand = new RelayCommand(StartSending, CanStartSending);
            StopSendingCommand = new RelayCommand(StopSending, () => IsSending);
            StartReceivingCommand = new RelayCommand(StartReceiving, CanStartReceiving);
            StopReceivingCommand = new RelayCommand(StopReceiving, () => IsReceiving);
            SendFileCommand = new RelayCommand(SendFileAsync, CanSendFile);
            ReceiveFileCommand = new RelayCommand(ReceiveFileAsync, CanReceiveFile);
            ClearStatisticsCommand = new RelayCommand(ClearStatistics);

            LoadNetworkInterfaces();
        }

        public TrafficSettings Settings
        {
            get => m_trafficSettings;
            set => SetField(ref m_trafficSettings, value);
        }

        public UdpStatistics Statistics
        {
            get => m_udpStatistics;
            set => SetField(ref m_udpStatistics, value);
        }

        public bool IsSending
        {
            get => m_isSending;
            set
            {
                SetField(ref m_isSending, value);
                CommandManager.InvalidateRequerySuggested();   
            }
        }

        public bool IsReceiving
        {
            get => m_isReceiving;
            set
            {
                SetField(ref m_isReceiving, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Status
        {
            get => m_status;
            set => SetField(ref m_status, value);
        }

        public ObservableCollection<NetworkInterfaceModel> NetworkInterfaces { get; } = [];

        public ICommand BrowseFileCommand { get; }
        public ICommand BrowseSavePathCommand { get; }
        public ICommand StartSendingCommand { get; }
        public ICommand StopSendingCommand { get; }
        public ICommand StartReceivingCommand { get; }
        public ICommand StopReceivingCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand ReceiveFileCommand { get; }
        public ICommand ClearStatisticsCommand { get; }

        private void BrowseFile()
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите файл для отправки",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (fileDialog.ShowDialog() == true)
            {
                Settings.FilePath = fileDialog.FileName;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void BrowseSavePath()
        {
            var dialog = new WinForms.FolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = "Выбериту папку для сохранения файлов",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                Settings.SavePath = dialog.SelectedPath;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private async void StartSending()
        {
            IsSending = true;
            Status = "Отправка UDP трафика...";

            m_udpTrafficService.PacketSent -= OnPacketSent;
            m_udpTrafficService.PacketSent += OnPacketSent;

            await m_udpTrafficService.StartSendingAsync(Settings);
        }

        private void OnPacketSent(object? sender, PacketEventArgs e)
        {
            Statistics.SentPackets++;
            Statistics.SentBytes += e.PacketSize;
        }

        private void StopSending()
        {
            m_udpTrafficService.StopSending();
            IsSending = false;
            Status = "Отправка остановлена";
        }

        private bool CanStartSending() => !IsSending && !string.IsNullOrEmpty(Settings.DestinationIp);

        private async void StartReceiving()
        {
            IsReceiving = true;
            Status = "Получение UDP трафика...";

            m_udpTrafficService.PacketReceived -= OnPacketReceived;
            m_udpTrafficService.PacketReceived += OnPacketReceived;

            await m_udpTrafficService.StartReceivingAsync(Settings);
        }

        private void OnPacketReceived(object? sender, PacketEventArgs e)
        {
            Statistics.ReceivedPackets++;
            Statistics.ReceivedBytes += e.PacketSize;
        }

        private void StopReceiving()
        {
            m_udpTrafficService.StopReceiving();
            IsReceiving = false;
            Status = "Получение остановлено";
        }

        private bool CanStartReceiving() => !IsReceiving;

        private async void SendFileAsync()
        {
            Status = "Отправка файла...";
            try
            {
                await FileTransferService.SendFileAsync(Settings.FilePath!, Settings.DestinationIp!, Settings.DestinationPort, Settings.SelectedInterface!.IpAddress!.ToString());
                Status = "Файл успешно отправлен";
            }
            catch (Exception ex)
            {
                Status = $"Ошибка отправки файла: {ex.Message}";
            }
        }

        private bool CanSendFile() => !string.IsNullOrEmpty(Settings.FilePath) && !string.IsNullOrEmpty(Settings.DestinationIp) && Settings.SelectedInterface != null;

        private async void ReceiveFileAsync()
        {
            Status = "Получение файла...";
            try
            {
                await FileTransferService.ReceiveFileAsync(Settings.SavePath!, Settings.SourcePort, Settings.SelectedInterface!.IpAddress!.ToString(), Settings.MacAddressFilter);
                Status = "Файл успешно получен";
            }
            catch (Exception ex)
            {
                Status = $"Ошибка получения файла: {ex.Message}";   
            }
        }

        private bool CanReceiveFile() => !string.IsNullOrEmpty(Settings.SavePath) && Settings.SelectedInterface != null;

        private void ClearStatistics()
        {
            Statistics.SentPackets = 0;
            Statistics.SentBytes = 0;
            Statistics.ReceivedPackets = 0;
            Statistics.ReceivedBytes = 0;

            Status = "Статистика очищена";
        }
        private void LoadNetworkInterfaces()
        {
            NetworkInterfaces.Clear();
            var interfaces = NetworkInterfaceService.GetActiveInterfaces();
            
            foreach (var iface in interfaces)
            {
                NetworkInterfaces.Add(iface);
            }

            Settings.SelectedInterface = NetworkInterfaces.FirstOrDefault();
        }
    }
}
