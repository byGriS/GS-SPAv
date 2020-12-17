using Service;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GS_SPAv {
  public partial class GS_SPAvWindow : Window {
    public Core.Core Core = null;
    public bool Working { get; set; }

    public GS_SPAvWindow() {
      CheckRunning();
      InitializeComponent();
      this.Title = "GS_SPAv " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0,
           System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Length - 4);
    }
    #region проверка на уже запущенное приложение
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int ShowWindow_Restore = 3;

    private void CheckRunning() {
      Process this_process = Process.GetCurrentProcess();
      Process[] other_processes = Process.GetProcessesByName(this_process.ProcessName).Where(pr => pr.Id != this_process.Id).ToArray();
      foreach (var pr in other_processes) {
        pr.WaitForInputIdle(1000); //на случай, если процесс еще не загрузился

        //берем первый процесс с окном
        IntPtr hWnd = pr.MainWindowHandle;
        if (hWnd == IntPtr.Zero) continue;

        //активируем окно и выходим
        ShowWindow(hWnd, ShowWindow_Restore);
        SetForegroundWindow(hWnd);
        Environment.Exit(0);
      }
    }
    #endregion

    private void Window_Loaded(object sender, RoutedEventArgs e) {

      /*if (!License.KeyChecking()) {
        LicenseInputWindow liw = new LicenseInputWindow();
        liw.Owner = this;
        if (liw.ShowDialog() != true) {
          this.Close();
        }
      }*/
      Grid d = new Grid();
      Core = new Core.Core();
      InitTableDataParams();
      InitOnlineGraph();
      InitOnlineStages();
      cbTable.IsChecked = Core.Setting.isTable;
      cbSoundAlarm.IsChecked = Core.Setting.isTotalSoundAlarm;
      lVolume.Content = Core.Setting.volumeSoundAlarm;
      eventLogsMini.ItemsSource = Core.Setting.EventLogs;
      Core.Setting.ListStages.CollectionChanged += ListStages_CollectionChanged;
      WorkChangeState("GS", 2, "Не запущен");
      WorkChangeState("CUDR", 2, "Не запущен");
      WorkChangeState("Web", 2, "Не запущен");
      Core.OnChangedState += WorkChangeState;
      cbIsSendWeb.IsChecked = Core.Setting.IsWEB;
      System.Threading.Thread checkUpdate = new System.Threading.Thread(new System.Threading.ThreadStart(CheckUpdate));
      checkUpdate.Start();
    }

    private void InitTableDataParams() {
      //tableDataParams.ListDataParams = Core.Setting.ListDataParams;
      tableDataParams.ListDataParams.Clear();
      for (int i = 0; i < 9; i++) {
        tableDataParams.ListDataParams.Add(Core.Setting.ListDataParams[i]);
      }
      tableDataParams4.ListDataParams.Clear();
      for (int i = 9; i < Core.Setting.ListDataParams.Count; i++) {
        tableDataParams4.ListDataParams.Add(Core.Setting.ListDataParams[i]);
      }

      tableDataParams2.ListDataParams.Clear();
      for (int i = 0; i < 9; i++) {
        tableDataParams2.ListDataParams.Add(Core.Setting.ListDataParams[i]);
      }
      tableDataParams3.ListDataParams.Clear();
      for (int i = 9; i < Core.Setting.ListDataParams.Count; i++) {
        tableDataParams3.ListDataParams.Add(Core.Setting.ListDataParams[i]);
      }
    }

    private void InitOnlineStages() {
      tableStages.Stages = Core.Setting.ListStages;
    }

    private void InitOnlineGraph() {
      graph1.DataParams.Add(Core.Setting.GetParamByTitleSmall("Q1"));
      graph1.DataParams.Add(Core.Setting.GetParamByTitleSmall("Q2"));
      graph1.DataParams.Add(Core.Setting.GetParamByTitleSmall("V1"));
      graph1.DataParams.Add(Core.Setting.GetParamByTitleSmall("V2"));
      graph1.Setting = Core.Setting;

      graph2.DataParams.Add(Core.Setting.GetParamByTitleSmall("P1"));
      graph2.DataParams.Add(Core.Setting.GetParamByTitleSmall("P2"));
      graph2.DataParams.Add(Core.Setting.GetParamByTitleSmall("P3"));
      graph2.Setting = Core.Setting;

      graph3.DataParams.Add(Core.Setting.GetParamByTitleSmall("T"));
      graph3.DataParams.Add(Core.Setting.GetParamByTitleSmall("po"));
      graph3.Setting = Core.Setting;

    }

    private void ListStages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
        graph1.AddStageAnnotation(Core.Setting.ListStages[Core.Setting.ListStages.Count - 1]);
        graph2.AddStageAnnotation(Core.Setting.ListStages[Core.Setting.ListStages.Count - 1]);
        graph3.AddStageAnnotation(Core.Setting.ListStages[Core.Setting.ListStages.Count - 1]);
        lStage.Content = Core.Setting.ListStages[Core.Setting.ListStages.Count - 1].Text;

        Core.Setting.ListStages[Core.Setting.ListStages.Count - 1].Capacity1Start = Core.Setting.GetParamByTitleSmall("V1").LastValue.Value;
        Core.Setting.ListStages[Core.Setting.ListStages.Count - 1].Capacity2Start = Core.Setting.GetParamByTitleSmall("V2").LastValue.Value;
        Core.Setting.ListStages[Core.Setting.ListStages.Count - 1].Start = DateTime.Now.ToString("HH:mm dd/MM/yy");
        if (Core.Setting.ListStages.Count>1)
          Core.Setting.ListStages[Core.Setting.ListStages.Count - 2].End = DateTime.Now.ToString("HH:mm dd/MM/yy");
      }
    }

    private void Start_Click(object sender, RoutedEventArgs e) {
      //Core.Setting.WorkInfo = new WorkInfo();
      if (btnStart.Header.ToString() == "Начать работу") {
        //SettingWindow sw = new SettingWindow(Core.Setting.WorkInfo, true, Working);
        WorkInfoWindow wiw = new WorkInfoWindow(Core.Setting.WorkInfo, true);
        wiw.Owner = this;
        if (wiw.ShowDialog() == true) {
          btnStart.Header = "Остановить работу";
          Working = true;
          miWorkInfo.Visibility = Visibility.Visible;
          StartInjection();
          cbIsSendWeb.IsEnabled = false;
          Core.Setting.EventLogs.Clear();
        }
      } else {
        Working = false;
        miWorkInfo.Visibility = Visibility.Collapsed;
        Core.Stop();
        btnStart.Header = "Начать работу";
        cbIsSendWeb.IsEnabled = true;
        WorkChangeState("GS", 2, "Не запущен");
        WorkChangeState("CUDR", 2, "Не запущен");
        WorkChangeState("Web", 2, "Не запущен");
        InitTableDataParams();

        if (Core.Setting.ListStages.Count > 0)
          Core.Setting.ListStages[Core.Setting.ListStages.Count - 1].End = DateTime.Now.ToString("HH:mm dd/MM/yy");
      }
    }

    private void StartInjection() {
      graph1.ClearData();
      graph2.ClearData();
      graph3.ClearData();
      foreach (DataParam param in Core.Setting.ListDataParams) {
        param.Points.Clear();
      }
      //Core.Setting.ListMarks.Clear();
      Core.Setting.ListStages.Clear();
      try {
        Core.Start();
      } catch (Exception e) {
        MessageBox.Show(e.Message, "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error);
        Start_Click(null, null);
      }
    }

    private void Setting_Click(object sender, RoutedEventArgs e) {
      SettingWindow sw = new SettingWindow(Working);
      sw.Owner = this;
      sw.ShowDialog();
    }

    private void Archive_Click(object sender, RoutedEventArgs e) {
      ArchiveWindow aw = new ArchiveWindow();
      aw.Owner = this;
      aw.ShowDialog();
    }

    private void EventLog_Click(object sender, RoutedEventArgs e) {
      if (eventLogsMini.SelectedItem == null) return;
      for (int i = 0; i < Core.Setting.EventLogs.Count; i++) {
        if (Core.Setting.EventLogs[i] == (AlarmMark)eventLogsMini.SelectedItem) {
          Core.Setting.EventLogs.Remove(Core.Setting.EventLogs[i]);
          return;
        }
      }
    }

    private void cbIsSendWeb_Checked(object sender, RoutedEventArgs e) {
      Core.Setting.IsWEB = cbIsSendWeb.IsChecked.Value;
      try {
        Core.Setting.SaveSetting();
      } catch { }
    }

    private void WorkChangeState(string source, int state, string error) {
      try {
        this.Dispatcher.Invoke(new System.Threading.ThreadStart(delegate {
          if (source == "GS") {
            canvasInputGS.Children.Clear();
          } else {
            if (source == "CUDR")
              canvasInputCUDR.Children.Clear();
            else
              canvasWebWork.Children.Clear();
          }
          Ellipse myEllipse = new Ellipse();
          SolidColorBrush mySolidColorBrush = new SolidColorBrush();
          mySolidColorBrush.Color = Colors.Gray;
          myEllipse.Fill = mySolidColorBrush;
          myEllipse.StrokeThickness = 2;
          myEllipse.Stroke = Brushes.Black;
          myEllipse.Width = 20;
          myEllipse.Height = 20;
          switch (state) {
            case 0:
              mySolidColorBrush.Color = Colors.Red;
              if (source == "GS")
                canvasInputGS.ToolTip = error;
              else
                if (source == "CUDR")
                canvasInputCUDR.ToolTip = error;
              else
                canvasWebWork.ToolTip = error;
              break;
            case 1:
              mySolidColorBrush.Color = Colors.Green;
              if (source == "GS")
                canvasInputGS.ToolTip = "Порт работает";
              else
               if (source == "CUDR")
                canvasInputCUDR.ToolTip = "Порт работает";
              else
                canvasWebWork.ToolTip = "Передача данных работает";
              break;
            case 2:
              mySolidColorBrush.Color = Colors.Gray;
              if (source == "GS")
                canvasInputGS.ToolTip = "Порт не запущен";
              else
               if (source == "CUDR")
                canvasInputCUDR.ToolTip = "Порт не запущен";
              else
                canvasWebWork.ToolTip = "Передача данных не запущена";
              break;
          }

          if (source == "GS") {
            canvasInputGS.Children.Add(myEllipse);
          } else {
            if (source == "CUDR")
              canvasInputCUDR.Children.Add(myEllipse);
            else
              canvasWebWork.Children.Add(myEllipse);
          }
        }));
      } catch { }
    }

    #region Update
    private string hostName = "http://spautomation.ru/gs-spav/";

    private void CheckUpdate() {
      Application.Current.Dispatcher.Invoke((Action)(() => {
        string version = "";
        try {
          HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hostName + "version.txt");
          HttpWebResponse response = (HttpWebResponse)request.GetResponse();
          if (response.StatusCode == HttpStatusCode.OK) {
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = null;
            if (response.CharacterSet == null) {
              readStream = new StreamReader(receiveStream);
            } else {
              readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
            }
            version = readStream.ReadToEnd();
            response.Close();
            readStream.Close();
          }
        } catch {
        }
        if (version == "")
          return;
        string curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0,
           System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Length - 4);
        if (version != curVersion) {
          if (MessageBox.Show("Имеется более новая версия, обновиться?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
            UpdateVersion();
          }
        }
      }));
    }
    private void UpdateVersion() {
      Application.Current.Dispatcher.Invoke((Action)(() => {
        string version = "";
        try {
          HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hostName + "version.txt");
          HttpWebResponse response = (HttpWebResponse)request.GetResponse();
          if (response.StatusCode == HttpStatusCode.OK) {
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = null;
            if (response.CharacterSet == null) {
              readStream = new StreamReader(receiveStream);
            } else {
              readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
            }
            version = readStream.ReadToEnd();
            response.Close();
            readStream.Close();
          } else {
            MessageBox.Show("Сервер обновлений недоступен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
          }
        } catch {
          MessageBox.Show("Сервер обновлений недоступен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        if (version == "")
          return;
        string curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0,
           System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Length - 4);
        if (version != curVersion) {
          string fileName = "Updater.exe", myStringWebResource = null;
          WebClient myWebClient = new WebClient();
          myWebClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
          myStringWebResource = hostName + fileName;
          Uri url = new Uri(myStringWebResource);
          myWebClient.DownloadFileAsync(url, Service.Constants.ENVPATH + fileName);
          System.Threading.Thread.Sleep(500);
          Process updater = new Process();
          updater.StartInfo.FileName = "updater";
          updater.StartInfo.Arguments = "d " + "\"" + Service.Constants.ENVPATH;
          for (int i = 0; i < 10; i++) {
            if (File.Exists(Service.Constants.ENVPATH + "Updater.exe")) {
              try {
                updater.Start();
                break;
              } catch {

              }
            } else {
              System.Threading.Thread.Sleep(500);
            }
          }
        } else {
          MessageBox.Show("Текущая версия актуальна", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
      }));
    }
    #endregion

    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
      CheckBox cb = sender as CheckBox;
      if (Core != null)
        Core.Setting.isTable = cb.IsChecked.Value;
      try {
        if (cb.IsChecked.Value) {
          gTable.Visibility = Visibility.Visible;
          gTableAndGraph.Visibility = Visibility.Hidden;
        } else {
          gTable.Visibility = Visibility.Hidden;
          gTableAndGraph.Visibility = Visibility.Visible;
        }
      } catch { }
      try {
        Core.Setting.SaveSetting();
      } catch { }
    }

    private void WorkInfoSetting_Click(object sender, RoutedEventArgs e) {
      WorkInfoWindow wiw = new WorkInfoWindow(Core.Setting.WorkInfo, false);
      wiw.ShowDialog();
    }

    private void SettingAlarm_Click(object sender, RoutedEventArgs e) {
      SettingAlarmWindow saw = new SettingAlarmWindow(Working);
      saw.Owner = this;
      saw.ShowDialog();
    }

    private void cbSoundAlarm_Checked(object sender, RoutedEventArgs e) {
      CheckBox cb = sender as CheckBox;
      if (Core != null) {
        Core.Setting.isTotalSoundAlarm = cb.IsChecked.Value;
        if (!Core.Setting.isTotalSoundAlarm)
          Core.StopAlarm();
      }      
      try {
        Core.Setting.SaveSetting();
      } catch { }
    }

    private void VolumeUp_Click(object sender, RoutedEventArgs e) {
      Core.Setting.volumeSoundAlarm += 10;
      if (Core.Setting.volumeSoundAlarm > 100)
        Core.Setting.volumeSoundAlarm = 100;
      lVolume.Content = Core.Setting.volumeSoundAlarm;
      Core.ChangeVolumeWMP();
    }

    private void VolumeDown_Click(object sender, RoutedEventArgs e) {
      Core.Setting.volumeSoundAlarm -= 10;
      if (Core.Setting.volumeSoundAlarm < 0)
        Core.Setting.volumeSoundAlarm = 0;
      lVolume.Content = Core.Setting.volumeSoundAlarm;
      Core.ChangeVolumeWMP();
    }
  }
}