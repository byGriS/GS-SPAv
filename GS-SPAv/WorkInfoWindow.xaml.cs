using Service;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GS_SPAv {
  public partial class WorkInfoWindow : Window {

    private WorkInfo workInfo;
    private GS_SPAvWindow GS_SPA;

    public WorkInfoWindow(WorkInfo workInfo, bool startWork) {
      InitializeComponent();
      this.workInfo = workInfo;
      workInfo.Blocked = !startWork;
      this.DataContext = workInfo;
      if (startWork)
        btnOk.Visibility = Visibility.Visible;
      else
        btnOk.Visibility = Visibility.Collapsed;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      GS_SPA = (GS_SPAvWindow)Owner;
    }

    private void OnlyNumber_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
      Regex regex = new Regex("[^0-9]+");
      e.Handled = regex.IsMatch(e.Text);
    }

    private void tb_GotFocus(object sender, RoutedEventArgs e) {
      TextBox tb = sender as TextBox;
      tb.BorderBrush = new SolidColorBrush(Colors.Gray);
    }

    private void Apply_Click(object sender, RoutedEventArgs e) {
      bool error = false;
      if (tbPhone.Text.Length != 10) {
        tbPhone.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (tbWell.Text.Length < 1) {
        tbWell.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (tbField.Text.Length < 1) {
        tbField.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (tbBuilder.Text.Length < 1) {
        tbBuilder.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (tbFIO.Text.Length < 1) {
        tbFIO.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (tbPKRS.Text.Length < 1) {
        tbPKRS.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (tbBush.Text.Length < 1) {
        tbBush.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (tbCompany.Text.Length < 1) {
        tbCompany.BorderBrush = new SolidColorBrush(Colors.Red);
        error = true;
      }
      if (!error) {
        workInfo.Start = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        GS_SPA.Core.Archive.NewInjection(workInfo);
      } else {
        return;
      }
      this.DialogResult = true;
      this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
      this.Close();
    }
  }
}
