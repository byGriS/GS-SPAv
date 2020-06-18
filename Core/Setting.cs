using Service;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Windows.Media;

namespace Core {
  public class Setting {

    private ObservableCollection<DataParam> listDataParams = null;
    public ObservableCollection<DataParam> ListDataParams {
      get {
        if (listDataParams == null) {
          listDataParams = new ObservableCollection<DataParam>();
          listDataParams.Add(new DataParam { Title = "Расход (турбина)", ColorLine = new SolidColorBrush(Color.FromRgb(0, 0, 255)), TitleSmall = "Q1", Unit = "м3/мин" });
          listDataParams.Add(new DataParam { Title = "Объем (турбина)", ColorLine = new SolidColorBrush(Color.FromRgb(0, 0, 0)), TitleSmall = "V1", Unit = "м3" });
          listDataParams.Add(new DataParam { Title = "Расход э/м", ColorLine = new SolidColorBrush(Color.FromRgb(155, 155, 255)), TitleSmall = "Q2", Unit = "м3/мин" });
          listDataParams.Add(new DataParam { Title = "Объем э/м", ColorLine = new SolidColorBrush(Color.FromRgb(112, 112, 112)), TitleSmall = "V2", Unit = "м3" });
          listDataParams.Add(new DataParam { Title = "Давление 1", ColorLine = new SolidColorBrush(Color.FromRgb(234, 0, 0)), TitleSmall = "P1", Unit = "атм" });
          listDataParams.Add(new DataParam { Title = "Давление 2", ColorLine = new SolidColorBrush(Color.FromRgb(255, 66, 160)), TitleSmall = "P2", Unit = "атм" });
          listDataParams.Add(new DataParam { Title = "Давление затруба", ColorLine = new SolidColorBrush(Color.FromRgb(255, 160, 255)), TitleSmall = "P3", Unit = "атм" });
          listDataParams.Add(new DataParam { Title = "Температура", ColorLine = new SolidColorBrush(Color.FromRgb(0, 255, 0)), TitleSmall = "T", Unit = "C" });
          listDataParams.Add(new DataParam { Title = "Плотность", ColorLine = new SolidColorBrush(Color.FromRgb(141, 141, 141)), TitleSmall = "po", Unit = "кг/м3" });
          listDataParams.Add(new DataParam { Title = "Концентрация НД1", ColorLine = new SolidColorBrush(Color.FromRgb(255, 128, 64)), TitleSmall = "Nnd1", Unit = "%" });
          listDataParams.Add(new DataParam { Title = "Объем НД1", ColorLine = new SolidColorBrush(Color.FromRgb(248, 102, 14)), TitleSmall = "Vnd1", Unit = "л" });
          listDataParams.Add(new DataParam { Title = "Концентрация НД2", ColorLine = new SolidColorBrush(Color.FromRgb(128, 128, 255)), TitleSmall = "Nnd2", Unit = "%" });
          listDataParams.Add(new DataParam { Title = "Объем НД2", ColorLine = new SolidColorBrush(Color.FromRgb(0, 0, 111)), TitleSmall = "Vnd2", Unit = "л" });
          listDataParams.Add(new DataParam { Title = "Концентрация ДШ", ColorLine = new SolidColorBrush(Color.FromRgb(128, 128, 0)), TitleSmall = "Nsh", Unit = "%" });
          listDataParams.Add(new DataParam { Title = "Масса ДШ", ColorLine = new SolidColorBrush(Color.FromRgb(64, 128, 128)), TitleSmall = "Msh", Unit = "кг" });
          listDataParams.Add(new DataParam { Title = "Расход НД1", ColorLine = new SolidColorBrush(Color.FromRgb(128, 64, 64)), TitleSmall = "Qnd1", Unit = "л/ч" });
          listDataParams.Add(new DataParam { Title = "Расход НД2", ColorLine = new SolidColorBrush(Color.FromRgb(193, 97, 0)), TitleSmall = "Qnd2", Unit = "л/ч" });
          listDataParams.Add(new DataParam { Title = "Расход ДШ", ColorLine = new SolidColorBrush(Color.FromRgb(115, 115, 115)), TitleSmall = "Qsh", Unit = "кг/ч" });
          foreach (DataParam arg in listDataParams)
            arg.ListUnits = FillListUnits(arg.Title);
        }
        return listDataParams;
      }
    }

    //public ObservableCollection<IMark> ListMarks { get; set; } = new ObservableCollection<IMark>();
    public Input.Input inputCUDR = null;
    public Input.Input inputGS = null;
    public ObservableCollection<Stage> ListStages { get; set; } = new ObservableCollection<Stage>();
    public Color OxyPlot { get; private set; }
    public WorkInfo WorkInfo { get; set; } = new WorkInfo();
    public ObservableCollection<AlarmMark> EventLogs = new ObservableCollection<AlarmMark>();
    public int DepthOnlineGraph { get; set; } = 0;

    public bool IsCUDR = true;
    public bool IsGS = true;
    public bool IsWEB = true;
    public bool isTable = true;
    public int intervalReadCUDR = 2;
    public int intervalReadGS = 3;
    public bool isTotalSoundAlarm = true;
    public int volumeSoundAlarm = 100;

    public DataParam GetParamByTitle(string title) {
      foreach (DataParam param in ListDataParams)
        if (param.Title == title)
          return param;
      return null;
    }

    public DataParam GetParamByTitleSmall(string title) {
      foreach (DataParam param in ListDataParams)
        if (param.TitleSmall == title)
          return param;
      return null;
    }

    public void SaveSetting() {
      string save = "";
      foreach (DataParam dataParam in ListDataParams) {
        save += dataParam.Title.Split(',')[0] + "|" + 
          dataParam.AlarmMin.ToString() + "|" +
          dataParam.AlarmMax.ToString() + "|" + 
          dataParam.ColorLine.Color.R + "|" + 
          dataParam.ColorLine.Color.G + "|" +
          dataParam.ColorLine.Color.B + "|"+ 
          dataParam.TitleSmall+ "|" +
          dataParam.IsChecked.ToString() + "|" +
          dataParam.Unit.ToString() + "|"+
          dataParam.IsAlarmColor.ToString() + "|" +
          dataParam.IsAlarmSound + "|" + 
          dataParam.Accuracy.ToString() + "\r\n";
      }
      save += inputCUDR.SerialPort.PortName + "\r\n";
      save += inputCUDR.SerialPort.BaudRate.ToString() + "\r\n";
      save += inputCUDR.SerialPort.DataBits.ToString() + "\r\n";
      save += inputCUDR.SerialPort.Parity.ToString() + "\r\n";
      save += inputCUDR.SerialPort.StopBits.ToString() + "\r\n";
      save += intervalReadCUDR.ToString() + "\r\n";
      save += inputGS.SerialPort.PortName + "\r\n";
      save += inputGS.SerialPort.BaudRate.ToString() + "\r\n";
      save += inputGS.SerialPort.DataBits.ToString() + "\r\n";
      save += inputGS.SerialPort.Parity.ToString() + "\r\n";
      save += inputGS.SerialPort.StopBits.ToString() + "\r\n";
      save += intervalReadGS.ToString() + "\r\n";
      save += DepthOnlineGraph.ToString() + "\r\n";
      save += IsCUDR.ToString() + "\r\n";
      save += IsGS.ToString() + "\r\n";
      save += IsWEB.ToString() + "\r\n";
      save += isTable.ToString() + "\r\n";
      save += isTotalSoundAlarm.ToString() + "\r\n";
      save += volumeSoundAlarm.ToString();
      File.WriteAllText("setting.dat", save);
    }

    public void LoadSetting() {
      SerialPort portCUDR = new System.IO.Ports.SerialPort("COM1");
      SerialPort portGS = new System.IO.Ports.SerialPort("COM2");
      if (File.Exists("setting.dat")) {
        string[] lines = File.ReadAllLines("setting.dat");
        int index = 0;
        foreach (DataParam dataParam in ListDataParams) {
          string[] data = lines[index++].Split('|');
          dataParam.Title = data[0].Split(',')[0];
          dataParam.AlarmMin = Convert.ToSingle(data[1]);
          dataParam.AlarmMax = Convert.ToSingle(data[2]);
          dataParam.ColorLine = new SolidColorBrush(Color.FromRgb(Convert.ToByte(data[3]), Convert.ToByte(data[4]), Convert.ToByte(data[5])));
          dataParam.TitleSmall = data[6];
          dataParam.IsChecked = Convert.ToBoolean(data[7]);
          dataParam.Unit = data[8];
          dataParam.ListUnits = FillListUnits(dataParam.Title);
          dataParam.IsAlarmColor = Convert.ToBoolean(data[9]);
          dataParam.IsAlarmSound = Convert.ToBoolean(data[10]);
          try {
            dataParam.Accuracy = Convert.ToInt32(data[11]);
          } catch { }
        }
        if (lines[index].Trim() != "")
          portCUDR.PortName = lines[index++];
        else
          index++;
        portCUDR.BaudRate = Convert.ToInt32(lines[index++]);
        portCUDR.DataBits = Convert.ToInt32(lines[index++]);
        portCUDR.Parity = (Parity)Enum.Parse(typeof(Parity), lines[index++]);
        portCUDR.StopBits = (StopBits)Enum.Parse(typeof(StopBits), lines[index++]);
        intervalReadCUDR = Convert.ToInt32(lines[index++]);
        if (lines[index].Trim() != "")
          portGS.PortName = lines[index++];
        else
          index++;
        portGS.BaudRate = Convert.ToInt32(lines[index++]);
        portGS.DataBits = Convert.ToInt32(lines[index++]);
        portGS.Parity = (Parity)Enum.Parse(typeof(Parity), lines[index++]);
        portGS.StopBits = (StopBits)Enum.Parse(typeof(StopBits), lines[index++]);
        intervalReadGS = Convert.ToInt32(lines[index++]);
        DepthOnlineGraph = Convert.ToInt32(lines[index++]);
        IsCUDR = Convert.ToBoolean(lines[index++]);
        IsGS = Convert.ToBoolean(lines[index++]);
        IsWEB = Convert.ToBoolean(lines[index++]);
        isTable = Convert.ToBoolean(lines[index++]);
        try {
          isTotalSoundAlarm = Convert.ToBoolean(lines[index++]);
          volumeSoundAlarm = Convert.ToInt32(lines[index++]);
        } catch { }
      }
      inputCUDR = new Input.Input { SerialPort = portCUDR };
      for (int i = 9; i < 18; i++) {
        inputCUDR.InputParams.Add(new Input.InputParam {
          Param = ListDataParams[i],
          Address = (ushort)(i * 2),
          Command = Input.Command.ReadHoldingRegisters,
          SlaveID = 1,
          TypeParam = Input.TypeParam.FLOAT
        });
      }
      inputCUDR.InputParams[0].Address = 0;//Концентрация НД1
      inputCUDR.InputParams[1].Address = 22;//Объем НД1
      inputCUDR.InputParams[2].Address = 0;//Концентрация НД2
      inputCUDR.InputParams[3].Address = 24;//Объем НД2
      inputCUDR.InputParams[4].Address = 0;//Концентрация ДШ
      inputCUDR.InputParams[5].Address = 28;//Масса ДШ
      inputCUDR.InputParams[6].Address = 0;//Расход НД1
      inputCUDR.InputParams[7].Address = 0;//Расход НД2
      inputCUDR.InputParams[8].Address = 0;//Расход ДШ
      inputGS = new Input.Input { SerialPort = portGS };
      for (int i = 0; i < 9; i++) {
        inputGS.InputParams.Add(new Input.InputParam {
          Param = ListDataParams[i]
        });
      }
    }

    private ObservableCollection<string> FillListUnits(string title) {
      ObservableCollection<string> listUnits = new ObservableCollection<string>();
     if (title.Contains("Давление")) {
        listUnits.Add("кгс/см2");
        listUnits.Add("атм");
        listUnits.Add("кПа");
        listUnits.Add("МПа");
      }
      if (title.Contains("Расход")) {
        listUnits.Add("м3/мин");
        listUnits.Add("м3/ч" );
        listUnits.Add("л/ч");
        listUnits.Add("кг/ч");
        listUnits.Add("л/сек");
        listUnits.Add("м3/сут");
      }
      if (title.Contains("Температура")) {
        listUnits.Add("C");
      }
      if (title.Contains("Объем")) {
        listUnits.Add("м3");
        listUnits.Add("л");
      }
      if (title.Contains("Масса")) {
        listUnits.Add("кг");
      }
      if (title.Contains("Плотность")) {
        listUnits.Add("кг/м3");
        listUnits.Add("т/м3");
      }
      if (title.Contains("Концентрация")) {
        listUnits.Add("%");
      }
      return listUnits;
    }

    public void LoadWorkInfo() {
      try {
        FileInfo[] files = new DirectoryInfo(Constants.ENVPATH + "Archives\\").GetFiles("*.csv");
        string[] info = File.ReadAllLines(files[files.Length - 1].FullName)[0].Split('|');
        WorkInfo.PhoneNumber = info[0];
        WorkInfo.Well = info[1];
        WorkInfo.Field = info[2];
        WorkInfo.Builder = info[3];
        WorkInfo.FIO = info[4];
        WorkInfo.NumPKRS = info[5];
        WorkInfo.Company = info[6];
        WorkInfo.Bush = info[7];
        WorkInfo.Start = info[8];
      } catch { }
    }

  }
}
