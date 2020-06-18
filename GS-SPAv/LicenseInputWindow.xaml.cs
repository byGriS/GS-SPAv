using System.Windows;
using System.Windows.Media;

namespace GS_SPAv {
  public partial class LicenseInputWindow : Window {
    public LicenseInputWindow() {
      InitializeComponent();
    }

    private void tbKey_GotFocus(object sender, RoutedEventArgs e) {
      tbKey.Text = "";
      tbKey.Foreground = new SolidColorBrush(Colors.Black);
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      string result = License.WebCheck(tbKey.Text);
      if (result == "1") {
        this.DialogResult = true;
        MessageBox.Show("Лицензия активированна", "Лицензия", MessageBoxButton.OK, MessageBoxImage.Information);
        this.Close();
      } else {
        if (result == "2") {
          MessageBox.Show("Ключ уже занят", "Лицензия", MessageBoxButton.OK, MessageBoxImage.Error);
        } else {
          MessageBox.Show("Ключ неверный", "Лицензия", MessageBoxButton.OK, MessageBoxImage.Error);
        }

      }      
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
      this.Close();
    }
  }
}