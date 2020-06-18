using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Service {
  public class DataParam : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] String prop = "") {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public delegate void ChangeValue(DataParam param);
    public event ChangeValue OnChangeValue;

    public DataParam() {
      Points.CollectionChanged += Points_CollectionChanged;
    }

    private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
        DataParamPoint lastValue = new DataParamPoint();
        lastValue.DateTime = Points[Points.Count - 1].DateTime;
        if (Points[Points.Count - 1].Value == -1) {
          isND = true;
          lastValue.Value = 0;
          lastValue.ValueString = "Н/Д";
        } else {
          isND = false;
          lastValue.Value = Points[Points.Count - 1].Value;
          string countZero = "0.";
          for (int i = 0; i < Accuracy; i++)
            countZero += "#";
          lastValue.ValueString = Points[Points.Count - 1].Value.ToString(countZero);
        }
        LastValue = lastValue;
      }
    }

    public ObservableCollection<DataParamPoint> Points { get; set; } = new ObservableCollection<DataParamPoint>();

    public string TitleSmall { get; set; }

    private DataParamPoint lastValue = new DataParamPoint();
    public DataParamPoint LastValue {
      get { return lastValue; }
      private set {
        lastValue = value;
        OnPropertyChanged("LastValue");
        OnChangeValue?.Invoke(this);
      }
    }

    public bool isND { get; set; }

    private string title = "";
    public string Title {
      get { return title + ", " + unit; }
      set {
        this.title = value;
        OnPropertyChanged("Title");
      }
    }

    private string unit = "";
    public string Unit {
      get { return unit; }
      set {
        unit = value;
        OnPropertyChanged("Unit");
        OnPropertyChanged("Title");
      }
    }

    private int accuracy = 3;
    public int Accuracy {
      get { return accuracy; }
      set {
        accuracy = value;
        OnPropertyChanged("Accuracy");
      }
    }

    private ObservableCollection<string> listUnits = new ObservableCollection<string>();
    public ObservableCollection<string> ListUnits {
      get { return listUnits; }
      set {
        listUnits = value;
        OnPropertyChanged("ListUnits");
      }
    }

    private float alarmMin = 0;
    public float AlarmMin {
      get { return alarmMin; }
      set {
        this.alarmMin = value;
        OnPropertyChanged("AlarmMin");
      }
    }

    private float alarmMax = 0;
    public float AlarmMax {
      get { return alarmMax; }
      set {
        this.alarmMax = value;
        OnPropertyChanged("AlarmMax");
      }
    }

    private bool isAlarmColor = false;
    public bool IsAlarmColor {
      get { return isAlarmColor; }
      set {
        isAlarmColor = value;
        OnPropertyChanged("IsAlarmColor");
      }
    }

    private bool isAlarmSound = false;
    public bool IsAlarmSound {
      get { return isAlarmSound; }
      set {
        isAlarmSound = value;
        OnPropertyChanged("IsAlarmSound");
      }
    }

    private bool isChecked = false;
    public bool IsChecked {
      get { return isChecked; }
      set {
        isChecked = value;
        OnPropertyChanged("IsChecked");
      }
    }

    private SolidColorBrush colorLine = Brushes.Black;
    public SolidColorBrush ColorLine {
      get { return colorLine; }
      set {
        colorLine = value;
        OnPropertyChanged("ColorLine");
      }
    }

    private bool inAlarm = false;
    public bool InAlarm {
      get { return inAlarm; }
      set {
        inAlarm = value;
        OnPropertyChanged("InAlarm");
        if (isAlarmColor) {
          InAlarmColor = inAlarm;
        } else {
          InAlarmColor = false;
        }

      }
    }

    private bool inAlarmColor = false;
    public bool InAlarmColor {
      get { return inAlarmColor; }
      set {
        inAlarmColor = value;
        OnPropertyChanged("InAlarmColor");
      }
    }

    private bool isBlock = false;
    public bool IsBlock {
      get { return isBlock; }
      set {
        isBlock = value;
        OnPropertyChanged("IsBlock");
      }
    }

    public DataParam CopyNewDataParam() {
      DataParam dp = new DataParam() {
        AlarmMax = this.AlarmMax,
        AlarmMin = this.AlarmMin,
        Title = this.Title.Split(',')[0],
        TitleSmall = this.TitleSmall,
        ColorLine = new SolidColorBrush(Color.FromRgb(this.ColorLine.Color.R, this.ColorLine.Color.G, this.ColorLine.Color.B)),
        InAlarm = this.inAlarm,
        unit = this.unit,
        IsAlarmColor = this.IsAlarmColor,
        IsAlarmSound = this.IsAlarmSound,
        Accuracy = this.Accuracy
      };
      dp.ListUnits = new ObservableCollection<string>();
      foreach (string arg in this.ListUnits)
        dp.ListUnits.Add(arg);
      return dp;
    }


    public float CalcValue(float input, bool isCUDR= false) {
      if (isCUDR)
        return input;
      float output = input;
      if (input == -1)
        return -1;
      if (title.Contains("Давление")) {
        switch (unit) {
          case "атм":
            output = input * (float)0.967841;
            break;
          case "кПа":
            output = input * (float)98.0665;
            break;
          case "МПа":
            output = input * (float)0.0980665;
            break;
        }
      }
      if (title.Contains("Расход")) {
        switch (unit) {
          case "м3/ч":
            output = input / (float)60;
            break;
          case "л/ч":
            output = input * (float)1000 / (float)60;
            break;
          case "л/сек":
            output = input * (float)1000 * (float)60;
            break;
          case "м3/сут":
            output = input / (float)1440;
            break;
          case "кг/ч":
            output = input * (float)60000;
            break;
        }
      }

      if (title.Contains("Объем")) {
        switch (unit) {
          case "л":
            output = input * (float)1000;
            break;
        }
      }
      if (title.Contains("Плотность")) {
        switch (unit) {
          case "кг/м3":
            output = input * (float)1;
            break;
          case "т/м3":
            output = input/ (float)1000.0;
            break;
        }
      }
      return output;
    }

    public float GetFlow() {
      float output = 0;
      if (title.Contains("Расход")) {
        switch (unit) {
          case "м3/ч":
            output = lastValue.Value * (float)1000;
            break;
          case "л/ч":
            output = lastValue.Value;
            break;
          case "л/сек":
            output = lastValue.Value / (float)60;
            break;
          case "м3/сут":
            output = lastValue.Value * (float)1000 / (float)24;
            break;
          case "м3/мин":
            output = lastValue.Value * (float)1000 / (float)60;
            break;
        }
      }
      return output;
    }
  }
}