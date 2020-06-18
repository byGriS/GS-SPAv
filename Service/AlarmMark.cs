using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Service {
  public class AlarmMark : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] String prop = "") {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
    
    public int ID { get; set; }
    public string Text { get; set; }
    public Brush Color { get => new SolidColorBrush(Colors.Blue); }
    public bool IsShow { get; set; }
    public bool IsSecond { get; set; }

    private DateTime dateTime = new DateTime();
    public DateTime DateTime {
      get { return dateTime; }
      set {
        dateTime = value;
        OnPropertyChanged("DateTime");
      }
    }

    public AlarmMark Clone() {
      return new AlarmMark {
        ID = this.ID,
        Text = this.Text,
        DateTime = this.DateTime,
      };
    }
  }
}
