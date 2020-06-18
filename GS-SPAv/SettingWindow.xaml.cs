using Service;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GS_SPAv {
  public partial class SettingWindow : Window {

    private GS_SPAvWindow GS_SPA = null;
    private WorkInfo workInfo;
    private bool working = false;

    public SettingWindow(bool working) {
      InitializeComponent();
      this.working = working;

      stackSetting.IsEnabled = !working;

      cbPortNameGS.ItemsSource = SerialPort.GetPortNames();
      cbPortNameCUDR.ItemsSource = SerialPort.GetPortNames();
      int[] baudRate = new int[] { 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
      cbBaudRateGS.ItemsSource = baudRate;
      cbBaudRateCUDR.ItemsSource = baudRate;
      int[] dataBits = new int[] { 4, 5, 6, 7, 8 };
      cbDataBitsGS.ItemsSource = dataBits;
      cbDataBitsCUDR.ItemsSource = dataBits;
      cbParityGS.ItemsSource = Enum.GetValues(typeof(Parity)).Cast<Enum>();
      cbParityCUDR.ItemsSource = Enum.GetValues(typeof(Parity)).Cast<Enum>();
      cbStopBitsGS.ItemsSource = Enum.GetValues(typeof(StopBits)).Cast<Enum>();
      cbStopBitsCUDR.ItemsSource = Enum.GetValues(typeof(StopBits)).Cast<Enum>();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      GS_SPA = (GS_SPAvWindow)Owner;
      cbPortNameGS.IsEnabled = !GS_SPA.Working;
      cbPortNameCUDR.IsEnabled = !GS_SPA.Working;
      cbPortNameGS.SelectedItem = GS_SPA.Core.Setting.inputGS.SerialPort.PortName;
      if (cbPortNameGS.SelectedItem == null && cbPortNameGS.Items.Count > 0)
        cbPortNameGS.SelectedIndex = 0;
      cbPortNameCUDR.SelectedItem = GS_SPA.Core.Setting.inputCUDR.SerialPort.PortName;
      if (cbPortNameCUDR.SelectedItem == null && cbPortNameCUDR.Items.Count > 0)
        cbPortNameCUDR.SelectedIndex = 0;
      tbDepthOnlineGraph.Text = GS_SPA.Core.Setting.DepthOnlineGraph.ToString();
      cbIsGS.IsChecked = GS_SPA.Core.Setting.IsGS;
      cbIsCUDR.IsChecked = GS_SPA.Core.Setting.IsCUDR;
      cbBaudRateGS.SelectedItem = GS_SPA.Core.Setting.inputGS.SerialPort.BaudRate;
      cbDataBitsGS.SelectedItem = GS_SPA.Core.Setting.inputGS.SerialPort.DataBits;
      cbParityGS.SelectedItem = GS_SPA.Core.Setting.inputGS.SerialPort.Parity;
      cbStopBitsGS.SelectedItem = GS_SPA.Core.Setting.inputGS.SerialPort.StopBits;
      tbTimeoutGS.Text = GS_SPA.Core.Setting.intervalReadGS.ToString();
      cbBaudRateCUDR.SelectedItem = GS_SPA.Core.Setting.inputCUDR.SerialPort.BaudRate;
      cbDataBitsCUDR.SelectedItem = GS_SPA.Core.Setting.inputCUDR.SerialPort.DataBits;
      cbParityCUDR.SelectedItem = GS_SPA.Core.Setting.inputCUDR.SerialPort.Parity;
      cbStopBitsCUDR.SelectedItem = GS_SPA.Core.Setting.inputCUDR.SerialPort.StopBits;
      tbTimeoutCUDR.Text = GS_SPA.Core.Setting.intervalReadCUDR.ToString();
    }

    private void Apply_Click(object sender, RoutedEventArgs e) {
      if (cbIsGS.IsChecked.Value && cbPortNameGS.SelectedItem == null) {
        MessageBox.Show("COM-порт для ПЛК не выбран", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      if (cbIsCUDR.IsChecked.Value && cbPortNameCUDR.SelectedItem == null) {
        MessageBox.Show("COM-порт для КУДР не выбран", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      if (cbIsGS.IsChecked.Value && cbIsCUDR.IsChecked.Value && cbPortNameCUDR.SelectedItem.ToString() == cbPortNameGS.SelectedItem.ToString()) {
        MessageBox.Show("Для ПКЛ и КУДР выбраны одинаковые com-порты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      if (!GS_SPA.Working) {
        GS_SPA.Core.Setting.inputGS.SerialPort.PortName = cbPortNameGS.SelectedItem.ToString();
        GS_SPA.Core.Setting.inputGS.SerialPort.BaudRate = (int)cbBaudRateGS.SelectedItem;
        GS_SPA.Core.Setting.inputGS.SerialPort.DataBits = (int)cbDataBitsGS.SelectedItem;
        GS_SPA.Core.Setting.inputGS.SerialPort.Parity = (Parity)cbParityGS.SelectedItem;
        GS_SPA.Core.Setting.inputGS.SerialPort.StopBits = (StopBits)cbStopBitsGS.SelectedItem;
        GS_SPA.Core.Setting.intervalReadGS = Convert.ToInt32(tbTimeoutGS.Text);

        GS_SPA.Core.Setting.inputCUDR.SerialPort.PortName = cbPortNameCUDR.SelectedItem.ToString();
        GS_SPA.Core.Setting.inputCUDR.SerialPort.BaudRate = (int)cbBaudRateCUDR.SelectedItem;
        GS_SPA.Core.Setting.inputCUDR.SerialPort.DataBits = (int)cbDataBitsCUDR.SelectedItem;
        GS_SPA.Core.Setting.inputCUDR.SerialPort.Parity = (Parity)cbParityCUDR.SelectedItem;
        GS_SPA.Core.Setting.inputCUDR.SerialPort.StopBits = (StopBits)cbStopBitsCUDR.SelectedItem;
        GS_SPA.Core.Setting.intervalReadCUDR = Convert.ToInt32(tbTimeoutCUDR.Text);
      }
      GS_SPA.Core.Setting.DepthOnlineGraph = Convert.ToInt32(tbDepthOnlineGraph.Text);
      GS_SPA.Core.Setting.IsGS = cbIsGS.IsChecked.Value;
      GS_SPA.Core.Setting.IsCUDR = cbIsCUDR.IsChecked.Value;
      GS_SPA.Core.Setting.SaveSetting();

      DialogResult = true;
      this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
      DialogResult = false;
      this.Close();
    }

    private void OnlyNumber_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
      Regex regex = new Regex("[^0-9]+");
      e.Handled = regex.IsMatch(e.Text);
    }

    private void cbIsCOM_Checked(object sender, RoutedEventArgs e) {
      cbPortNameGS.IsEnabled = cbIsGS.IsChecked.Value;
      cbBaudRateGS.IsEnabled = cbIsGS.IsChecked.Value;
      cbDataBitsGS.IsEnabled = cbIsGS.IsChecked.Value;
      cbParityGS.IsEnabled = cbIsGS.IsChecked.Value;
      cbStopBitsGS.IsEnabled = cbIsGS.IsChecked.Value;
      tbTimeoutGS.IsEnabled = cbIsGS.IsChecked.Value;
      cbPortNameCUDR.IsEnabled = cbIsCUDR.IsChecked.Value;
      cbBaudRateCUDR.IsEnabled = cbIsCUDR.IsChecked.Value;
      cbDataBitsCUDR.IsEnabled = cbIsCUDR.IsChecked.Value;
      cbParityCUDR.IsEnabled = cbIsCUDR.IsChecked.Value;
      cbStopBitsCUDR.IsEnabled = cbIsCUDR.IsChecked.Value;
      tbTimeoutCUDR.IsEnabled = cbIsCUDR.IsChecked.Value;
    }

    private void LabelGS_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      cbIsGS.IsChecked = !cbIsGS.IsChecked.Value;
    }

    private void LabelCUDR_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      cbIsCUDR.IsChecked = !cbIsCUDR.IsChecked.Value;
    }
  }
}