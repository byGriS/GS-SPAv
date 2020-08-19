using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Service {
  public class Stage : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] String prop = "") {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    private bool isActive = false;
    public bool IsActive {
      get { return isActive; }
      set {
        isActive = value;
        OnPropertyChanged("IsActive");
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

    public DateTime DateTime { get; set; }
    public int ID { get; set; }
    public string Text { get; set; }
    public bool IsSecond { get; set; }
    public Brush Color { get => new SolidColorBrush(Colors.Green); }

    private float capacity1Start = 0;
    public float Capacity1Start { get { return capacity1Start; }set { capacity1Start = value; OnPropertyChanged("Capacity1Start"); } }

    private float capacity1End = 0;
    public float Capacity1End { get { return capacity1End; } set { capacity1End = value; Capacity1 = Capacity1End - Capacity1Start; OnPropertyChanged("Capacity1End"); } }

    private float capacity2Start = 0;
    public float Capacity2Start { get { return capacity2Start; } set { capacity2Start = value; OnPropertyChanged("Capacity2Start"); } }

    private float capacity2End = 0;
    public float Capacity2End { get { return capacity2End; } set { capacity2End = value; Capacity2 = Capacity2End - Capacity2Start; OnPropertyChanged("Capacity1End"); } }

    private float capacity1 = 0;
    public float Capacity1 { get { return capacity1; } set { capacity1 = value; OnPropertyChanged("Capacity1"); } }

    private float capacity2 = 0;
    public float Capacity2 { get { return capacity2; } set { capacity2 = value; OnPropertyChanged("Capacity2"); } }




    public Stage Clone() {
      return new Stage {
        IsActive = this.isActive,
        DateTime = this.DateTime,
        IsSecond = this.IsSecond,
        ID = this.ID,
        Text = this.Text,
        Capacity1Start = this.Capacity1Start,
        Capacity1End = this.Capacity1End,
        Capacity2Start = this.Capacity2Start,
        Capacity2End = this.Capacity2End
      };
    }
  }
}
