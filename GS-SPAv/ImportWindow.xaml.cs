using Service;
using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace GS_SPAv {
  public partial class ImportWindow : Window {

    public ImportWindow(Core.Core core) {
      InitializeComponent();
      xcArchives.ItemsSource = core.Archive.GetListWorks();
      xcArchives.SelectionChanged += XcArchives_SelectionChanged;
      btnOk.IsEnabled = false;
    }

    private void XcArchives_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (xcArchives.SelectedItems.Count == 0) {
        btnOk.IsEnabled = false;
      } else
        btnOk.IsEnabled = true;
    }

    private void Apply_Click(object sender, RoutedEventArgs e) {
      System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
      folderBrowser.ShowDialog();
      if (xcArchives.SelectedItems != null) {
        try {
          foreach (WorkInfo i in xcArchives.SelectedItems) {
            string fileName = i.Start.Replace("-", "_").Replace(":", "_").Replace(" ", "_") + ".csv";
            File.Copy(Environment.CurrentDirectory + @"\Archives\" + fileName, folderBrowser.SelectedPath + @"\" + fileName);
          }
          MessageBox.Show("Скопировано", "Выгрузка", MessageBoxButton.OK, MessageBoxImage.Information);
          this.Close();
        } catch {
          MessageBox.Show("Файлы не скопировались", "Выгрузка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }
  }
}
