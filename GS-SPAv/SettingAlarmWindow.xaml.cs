using Service;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GS_SPAv {
  public partial class SettingAlarmWindow : Window {
    private GS_SPAvWindow GS_SPA = null;
    private ObservableCollection<DataParam> dataParams = null;
    private bool working;

    public SettingAlarmWindow(bool working) {
      InitializeComponent();
      this.working = working;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      GS_SPA = (GS_SPAvWindow)Owner;
      dataParams = new ObservableCollection<DataParam>();
      foreach (DataParam param in GS_SPA.Core.Setting.ListDataParams) {
        dataParams.Add(param.CopyNewDataParam());
        dataParams[dataParams.Count - 1].IsBlock = !working;
      }
      dgParamSetting.ItemsSource = dataParams;
      cbSelectFlow.Items.Add(GS_SPA.Core.Setting.inputGS.InputParams[0].Param.Title.Split(',')[0]);
      cbSelectFlow.Items.Add(GS_SPA.Core.Setting.inputGS.InputParams[2].Param.Title.Split(',')[0]);
      if (GS_SPA.Core.Setting.firstFlow.Value == "1")
        cbSelectFlow.SelectedIndex = 0;
      else
        cbSelectFlow.SelectedIndex = 1;
    }

    private void ColorChange_Click(object sender, RoutedEventArgs e) {
      if (dgParamSetting.SelectedItem == null)
        return;
      Button b = (Button)sender;
      System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
      cd.Color = System.Drawing.Color.FromArgb(((Color)b.Background.GetValue(SolidColorBrush.ColorProperty)).R,
         ((Color)b.Background.GetValue(SolidColorBrush.ColorProperty)).G,
         ((Color)b.Background.GetValue(SolidColorBrush.ColorProperty)).B);
      if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
        DataParam dataParam = (DataParam)dgParamSetting.SelectedItem;
        dataParam.ColorLine = new SolidColorBrush(Color.FromRgb(cd.Color.R, cd.Color.G, cd.Color.B));
        b.Background = dataParam.ColorLine;
      }
    }

    private void Apply_Click(object sender, RoutedEventArgs e) {
      for (int i = 0; i < dataParams.Count; i++) {
        GS_SPA.Core.Setting.ListDataParams[i].ColorLine = dataParams[i].ColorLine;
        GS_SPA.Core.Setting.ListDataParams[i].AlarmMin = dataParams[i].AlarmMin;
        GS_SPA.Core.Setting.ListDataParams[i].AlarmMax = dataParams[i].AlarmMax;
        //GS_SPA.Core.Setting.ListDataParams[i].Title = dataParams[i].Title;
        GS_SPA.Core.Setting.ListDataParams[i].TitleSmall = dataParams[i].TitleSmall;
        GS_SPA.Core.Setting.ListDataParams[i].Unit = dataParams[i].Unit;
        GS_SPA.Core.Setting.ListDataParams[i].Accuracy = dataParams[i].Accuracy;
        GS_SPA.Core.Setting.ListDataParams[i].IsAlarmColor = dataParams[i].IsAlarmColor;
        GS_SPA.Core.Setting.ListDataParams[i].IsAlarmSound = dataParams[i].IsAlarmSound;
      }
      if (cbSelectFlow.SelectedIndex == 0)
        GS_SPA.Core.Setting.firstFlow.Value = "1";
      else
        GS_SPA.Core.Setting.firstFlow.Value = "0";

      GS_SPA.Core.Setting.SaveSetting();
      this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
      this.Close();
    }

    private void UpItem_Click(object sender, RoutedEventArgs e) {
      if (dgParamSetting.SelectedItem == null || dgParamSetting.SelectedIndex == 0 || dgParamSetting.SelectedIndex > 8)
        return;
      dataParams.Insert(dgParamSetting.SelectedIndex - 1, (DataParam)dgParamSetting.SelectedItem);
      dataParams.RemoveAt(dgParamSetting.SelectedIndex);
    }

    private void DownItem_Click(object sender, RoutedEventArgs e) {
      if (dgParamSetting.SelectedItem == null || dgParamSetting.SelectedIndex > 7)
        return;
      dataParams.Insert(dgParamSetting.SelectedIndex + 2, (DataParam)dgParamSetting.SelectedItem);
      dataParams.RemoveAt(dgParamSetting.SelectedIndex);
    }

  }
}