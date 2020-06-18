using Service;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GS_SPAv {
  public partial class TableDataParamsView : UserControl {
    public TableDataParamsView() {
      InitializeComponent();
      listDataParams.CollectionChanged += ListDataParams_CollectionChanged;
    }

    private void ListDataParams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      DataContext = listDataParams;
    }

    private ObservableCollection<DataParam> listDataParams = new ObservableCollection<DataParam>();
    public ObservableCollection<DataParam> ListDataParams {
      get { return listDataParams; }
      set {
        listDataParams = value;
        DataContext = listDataParams;
      }
    }

    private bool isOnlyTable = false;
    public bool IsOnlyTable {
      get { return isOnlyTable; }
      set {
        isOnlyTable = value;
        if (isOnlyTable) {
          { 
          Style style = this.FindResource("tbStyleParam") as Style;
          Setter setter = (Setter)style.Setters[0];
          setter.Value = 32.0;
          Setter setter2 = (Setter)style.Setters[1];
          setter2.Value = 500.0;
            Setter setter3 = (Setter)style.Setters[2];
            setter3.Value = new Thickness(0, 5, 0, 5);
          }
          {
            Style style = this.FindResource("tbStyleValue") as Style;
            Setter setter = (Setter)style.Setters[0];
            setter.Value = 32.0;
            Setter setter3 = (Setter)style.Setters[1];
            setter3.Value = new Thickness(0, 5, 0, 5);
          }


        } 
      }
    }
  }

  public class ColorAlarmText : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if ((bool)value) 
        return new SolidColorBrush(Colors.Red);      
      return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }

  public class WeightAlarmText : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if ((bool)value) 
        return FontWeight.FromOpenTypeWeight(700);      
      return FontWeight.FromOpenTypeWeight(400);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }
}
