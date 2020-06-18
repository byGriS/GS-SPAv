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

    public Stage Clone() {
      return new Stage {
        IsActive = this.isActive,
        DateTime = this.DateTime,
        IsSecond = this.IsSecond,
        ID = this.ID,
        Text = this.Text
      };
    }
  }
}
