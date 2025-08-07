using FiberTradeTask2.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace FiberTradeTask2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void PortValidation_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            var newText = textBox.Text + e.Text;
            e.Handled = !(int.TryParse(newText, out int num) && num >= 0 && num <= 65535);
        }

        private void PortValidation_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));

                if (!int.TryParse(pastedText, out int num) || num < 0 || num > 65535)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void IpValidation_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            bool isDot = e.Text == ".";
            bool isValidChar = isDot || char.IsDigit(e.Text[0]);

            if (!isValidChar)
            {
                e.Handled = true;
                return;
            }

            e.Handled = !IsValidIpPartial(newText);
        }

        private void IpValidation_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidIpPartial(pastedText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void IpValidation_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;

            if (string.IsNullOrWhiteSpace(textBox.Text))
                return;

            if (!IsValidFullIp(textBox.Text))
            {
                if (DataContext is MainViewModel vm)
                    vm.Status = "Ошибка: Неверный IP-адрес! Формат: 192.168.1.1";
                Dispatcher.BeginInvoke(new Action(() => textBox.Focus()));
            }
        }

        private static bool IsValidIpPartial(string text)
        {
            string[] parts = text.Split('.');
            if (parts.Length > 4) return false;

            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;

                if (!int.TryParse(parts[i], out int num) || num < 0 || num > 255)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsValidFullIp(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            string[] parts = text.Split('.');
            if (parts.Length != 4) return false;

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                {
                    return false;
                }
            }
            return true;
        }

        private void PacketSizeValidation_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            var newText = textBox.Text + e.Text;
            e.Handled = !(int.TryParse(newText, out int num) && num >= 1 && num <= 65535);
        }

        private void PacketSizeValidation_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));

                if (!int.TryParse(pastedText, out int num) || num < 1 || num > 65535)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void MacValidation_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            bool isValidChar = MacValidation_PreviewTextInputRegex().IsMatch(e.Text);
            if (!isValidChar)
            {
                e.Handled = true;
                return;
            }

            e.Handled = !IsValidMacPartial(newText);
        }

        private void MacValidation_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidMacPartial(pastedText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void MacValidation_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;

            if (string.IsNullOrWhiteSpace(textBox.Text))
                return;

            if (!IsValidFullMac(textBox.Text))
            {
                if (DataContext is MainViewModel vm)
                    vm.Status = "Ошибка: Неверный MAC-адрес! Формат: 00:1A:2B:3C:4D:5E";
                Dispatcher.BeginInvoke(new Action(() => textBox.Focus()));
            }
        }

        private static bool IsValidMacPartial(string text)
        {
            string[] parts = text.Split(':', '-');
            if (parts.Length > 6) return false;

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                if (part.Length > 2 || !IsValidMacPartialRegex().IsMatch(part))
                    return false;
            }
            return true;
        }

        private static bool IsValidFullMac(string text)
        {
            return IsValidFullMacRegex().IsMatch(text);
        }

        [GeneratedRegex(@"^[0-9A-Fa-f:-]$")]
        private static partial Regex MacValidation_PreviewTextInputRegex();

        [GeneratedRegex(@"^[0-9A-Fa-f]{1,2}$")]
        private static partial Regex IsValidMacPartialRegex();

        [GeneratedRegex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$")]
        private static partial Regex IsValidFullMacRegex();
    }
}
