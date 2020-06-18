using System;
using System.Collections.Generic;
using System.Windows;

namespace GS_SPAv {
  public partial class ArchivePrintWindow : Window {

    private Random r = new Random();
    private List<int> selectedGraph;

    public ArchivePrintWindow(List<int> selectedGraph) {
      InitializeComponent();
      this.selectedGraph = selectedGraph;
    }

    private void Close_Click(object sender, RoutedEventArgs e) {
      this.Close();
    }

    private void Print_Click(object sender, RoutedEventArgs e) {
      if (cbFlow.IsChecked.Value)
        selectedGraph[0] = r.Next(int.MaxValue);
      if (cbPress.IsChecked.Value)
        selectedGraph[1] = r.Next(int.MaxValue);
      if (cbRo.IsChecked.Value)
        selectedGraph[2] = r.Next(int.MaxValue);
      if (cbND1.IsChecked.Value)
        selectedGraph[3] = r.Next(int.MaxValue);
      if (cbND2.IsChecked.Value)
        selectedGraph[4] = r.Next(int.MaxValue);
      if (cbScrew.IsChecked.Value)
        selectedGraph[5] = r.Next(int.MaxValue);
      DialogResult = true;
      this.Close();
    }
  }
}

