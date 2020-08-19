using System.Windows;
using Service;
using OxyPlot;
using System.Collections.ObjectModel;
using System;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows.Media;
using OxyPlot.Annotations;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace GS_SPAv {
  public partial class ArchiveWindow : Window {

    private ObservableCollection<DataParam> dataParams = new ObservableCollection<DataParam>();
    private ObservableCollection<Stage> stages = new ObservableCollection<Stage>();
    //private ObservableCollection<IMark> marksShow = new ObservableCollection<IMark>();
    private ObservableCollection<DataParam> checkedParam = new ObservableCollection<DataParam>();
    private GS_SPAvWindow GS_SPA = null;
    private PlotModel model = new PlotModel();
    private PlotController controller = new PlotController();
    private bool isNeedSave = false;

    private const int TextFontSize = 16;
    
    public ArchiveWindow() {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      GS_SPA = (GS_SPAvWindow)this.Owner;
      WorkInfo[] listWork = GS_SPA.Core.Archive.GetListWorks();
      lvArchives.ItemsSource = listWork;

      controller.UnbindAll();
      controller.BindMouseDown(OxyMouseButton.Right, PlotCommands.PanAt);
      controller.BindMouseWheel(PlotCommands.ZoomWheel);
      controller.BindKeyDown(OxyKey.R, PlotCommands.Reset);

      controller.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
             (view, controller, args) =>
                controller.AddMouseManipulator(view, new WpbTrackerManipulator(view), args)));

      plotter.Controller = controller;
    }

    private void Close_Click(object sender, RoutedEventArgs e) {
      AllToFalse();
      if (isNeedSave) {
        if (MessageBox.Show("Выйти без сохранения?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
          Close();
      } else {
        Close();
      }
    }

    #region блокируем ctrl
    bool blockCtrl = false;
    private void lbArchives_PreviewKeyDownUp(object sender, System.Windows.Input.KeyEventArgs e) {
      blockCtrl = false;
      if ((System.Windows.Input.Keyboard.Modifiers &
          System.Windows.Input.ModifierKeys.Control) ==
                 System.Windows.Input.ModifierKeys.Control) {
        blockCtrl = true;
      }
    }
    private void lbArchives_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      e.Handled = blockCtrl;
    }

    #endregion

    private void btnReadArchive_Click(object sender, RoutedEventArgs e) {
      if (lvArchives.SelectedItem == null) {
        MessageBox.Show("Не выбраны архивные файлы", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      AllToFalse();
      dataParams = new ObservableCollection<DataParam>();
      stages = new ObservableCollection<Stage>();
      GS_SPA.Core.Archive.GetData((WorkInfo)lvArchives.SelectedItem, dataParams, stages);
      SelectedAll_Click(null, null);
      lbStages.ItemsSource = stages;

      lbParams.ItemsSource = GS_SPA.Core.Setting.ListDataParams;
      btnShowGraph.IsEnabled = true;
      btnShowGraphStage.IsEnabled = true;
      lSelectedArchive.Content = "скв. " + ((WorkInfo)lvArchives.SelectedItem).Well + " дата " + ((WorkInfo)lvArchives.SelectedItem).Start;
      lbStages.SelectedIndex = 0;
    }


    private void SelectedAll_Click(object sender, RoutedEventArgs e) {
      foreach (Stage stage in stages)
        stage.IsChecked = true;
    }

    private void DeselectedAll_Click(object sender, RoutedEventArgs e) {
      foreach (Stage stage in stages)
        stage.IsChecked = false;
    }

    private void btnShowGraphStage_Click(object sender, RoutedEventArgs e) {      
      bool nullChecked = true;
      foreach (Stage stage in stages) {
        if (stage.IsChecked)
          nullChecked = false;
      }
      if (nullChecked) {
        MessageBox.Show("Не выбраны ни одна стадия", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      checkedParam.Clear();
      foreach (DataParam param in GS_SPA.Core.Setting.ListDataParams) {
        if (param.IsChecked) {
          foreach (DataParam paramArchive in dataParams) {
            if (param.Title == paramArchive.Title) {
              checkedParam.Add(paramArchive);
            }
          }
        }
      }
      DrawGraph(checkedParam, true);
    }


    private void btnShowGraph_Click(object sender, RoutedEventArgs e) {
      checkedParam.Clear();
      foreach (DataParam param in GS_SPA.Core.Setting.ListDataParams) {
        if (param.IsChecked) {
          foreach (DataParam paramArchive in dataParams) {
            if (param.Title == paramArchive.Title) {
              checkedParam.Add(paramArchive);
            }
          }
        }
      }
      DrawGraph(checkedParam, false);
    }

    private void DrawGraph(ObservableCollection<DataParam> checkedParams, bool byStages) {
      bool nullChecked = true;
      foreach (DataParam param in GS_SPA.Core.Setting.ListDataParams) {
        if (param.IsChecked)
          nullChecked = false;
      }
      if (nullChecked) {
        MessageBox.Show("Не выбраны параметры", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      GS_SPA.Core.Setting.SaveSetting();

      int pointStart = -1;
      int pointEnd = -1;
      if (byStages) {
        int indexStart = -1;
        int indexEnd = -1;
        for (int i = 0; i < stages.Count; i++) {
          if (stages[i].IsSecond) continue;
          if (indexStart == -1) {
            if (stages[i].IsChecked) {
              indexStart = i;
              indexEnd = i;
            }
          } else
              if (stages[i].IsChecked)
            indexEnd = i;
        }
        for (int i = indexStart; i <= indexEnd; i++) {
          if (stages[i].IsSecond) continue;
          stages[i].IsChecked = true;
        }       
        DateTime endLastStage = checkedParams[0].Points[checkedParams[0].Points.Count - 1].DateTime;
        foreach (Stage stage in stages)
          if (stage.ID == stages[indexEnd].ID && stage.IsSecond)
            endLastStage = stage.DateTime;
        for (int point = 0; point < checkedParams[0].Points.Count; point++) {
          if (pointStart == -1 && (checkedParams[0].Points[point].DateTime >= stages[indexStart].DateTime || Math.Abs((checkedParams[0].Points[point].DateTime - stages[indexStart].DateTime).TotalSeconds) <= 1))
            pointStart = point;
          if (checkedParams[0].Points[point].DateTime <= endLastStage)
            pointEnd = point;
        }
      }

      model = new PlotModel();
      DateTimeAxis axeX = new DateTimeAxis() {
        Title = "Время",
        Position = AxisPosition.Bottom,
        StringFormat = "HH:mm:ss\ndd/MM/yy",
      };
      axeX.MajorGridlineStyle = LineStyle.Solid;
      axeX.MajorGridlineThickness = 1;
      axeX.AxisChanged += AxeX_AxisChanged;
      model.Axes.Add(axeX);
      bool first = true;
      int indexTier = 0;
      foreach (DataParam checkedParam in checkedParams) {
        bool addAxe = true;
        if (addAxe) {
          LinearAxis axeY = new LinearAxis() {
            Key = checkedParam.Title.Split(',')[0],
            Position = AxisPosition.Left,
            PositionTier = indexTier++,
            TextColor = OxyColor.FromRgb(checkedParam.ColorLine.Color.R, checkedParam.ColorLine.Color.G, checkedParam.ColorLine.Color.B),
            AxislineColor = OxyColor.FromRgb(checkedParam.ColorLine.Color.R, checkedParam.ColorLine.Color.G, checkedParam.ColorLine.Color.B),
            AxislineStyle = LineStyle.Solid
          };
          if (first) {
            first = false;
            axeY.AxisChanged += AxeY_AxisChanged;
            axeY.MajorGridlineStyle = LineStyle.Solid;
            axeY.MajorGridlineThickness = 1;
          }
          model.Axes.Add(axeY);
        }
        LineSeries series = new LineSeries() {
          Title = checkedParam.Title,
          YAxisKey = checkedParam.Title.Split(',')[0],
          Color = OxyColor.FromRgb(checkedParam.ColorLine.Color.R, checkedParam.ColorLine.Color.G, checkedParam.ColorLine.Color.B)
        };
        model.Series.Add(series);
        int sample = 1;
        if (byStages)
          for (int point = pointStart; point < pointEnd; point += sample) {
            series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(checkedParam.Points[point].DateTime), checkedParam.Points[point].Value));
          }
        else
          for (int point = 0; point < checkedParam.Points.Count; point += sample) {
            series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(checkedParam.Points[point].DateTime), checkedParam.Points[point].Value));
          }
      }


      plotter.Model = model;
      WorkInfo wi = (WorkInfo)lvArchives.SelectedItem;
      model.Title = wi.Builder + "\nМесторождение: " + wi.Field + ", скважина:" + wi.Well + "\nНачало: " + DateTime.Parse(wi.Start).ToString() + "\nКонец: " + DateTimeAxis.ToDateTime(model.Axes[0].ActualMaximum).ToString();
      model.TitleFontWeight = 1;
      model.TitleFontSize = 14;
      model.LegendPosition = LegendPosition.BottomCenter;
      model.LegendPlacement = LegendPlacement.Outside;
      model.LegendOrientation = LegendOrientation.Horizontal;

      model.MouseDown += Model_MouseDown;
      model.MouseMove += Model_MouseMove;

 
      RenderAnno();
    }

    private void AxeX_AxisChanged(object sender, AxisChangedEventArgs e) {
      RenderTextAnnotaions();
    }

    private void AxeY_AxisChanged(object sender, AxisChangedEventArgs e) {
      Axis ax = sender as Axis;
      if (e.ChangeType == AxisChangeTypes.Pan) {
        double delta = ax.ActualMaximum - ax.ActualMinimum;
        foreach (Axis axis in plotter.ActualModel.Axes) {
          if ((axis.GetType() != typeof(DateTimeAxis)) && (axis != ax)) {
            axis.Pan(e.DeltaMaximum * (axis.ActualMaximum - axis.ActualMinimum) / delta * axis.Scale * -1);
          }
        }
      }
      if (e.ChangeType == AxisChangeTypes.Zoom) {
        double delta = (ax.ActualMaximum - e.DeltaMaximum) - (ax.ActualMinimum - e.DeltaMinimum);
        foreach (Axis axis in plotter.ActualModel.Axes) {
          if ((axis.GetType() != typeof(DateTimeAxis)) && (axis != ax)) {
            double z = e.DeltaMaximum * (axis.ActualMaximum - axis.ActualMinimum) / delta;
            double x = e.DeltaMinimum * (axis.ActualMaximum - axis.ActualMinimum) / delta;
            axis.Zoom(axis.ActualMinimum + x, axis.ActualMaximum + z);
          }
        }
      }
      RenderTextAnnotaions();
    }

    private void RenderAnno() {
      model.Annotations.Clear();
      int indexAnno = 0;
      for (int i = 0; i < stages.Count; i++) {
        if (stages[i].IsChecked && !stages[i].IsSecond) {
          RectangleAnnotation anno = new RectangleAnnotation();
          anno.Layer = AnnotationLayer.BelowSeries;
          
          anno.Text = stages[i].Text;
          anno.TextRotation = -90;
          anno.MinimumX = DateTimeAxis.ToDouble(stages[i].DateTime);
          Stage second = FindSecondStage(stages[i].ID);
          if (second != null)
            anno.MaximumX = DateTimeAxis.ToDouble(second.DateTime);
          else
            anno.MaximumX = model.Axes[0].ActualMaximum;
          anno.TextPosition = CalcTextPoint(anno);
          anno.Fill = (indexAnno++ % 2 == 1) ? OxyColor.FromRgb(225, 225, 225) : OxyColor.FromRgb(240, 240, 240);
          anno.MouseDown += Stage_MouseDown;
          anno.FontSize = TextFontSize;
          anno.TextColor = OxyColors.Black;
          anno.Tag = stages[i].ID;
          model.Annotations.Add(anno);
        }
      }
      plotter.InvalidatePlot(false);
    }

    private DataPoint CalcTextPoint(RectangleAnnotation anno) {
      return new DataPoint(
        anno.MinimumX + (model.Axes[0].ActualMaximum - model.Axes[0].ActualMinimum) * 0.007,
        model.Axes[1].ActualMaximum - (model.Axes[1].ActualMaximum - model.Axes[1].ActualMinimum)*0.009 * anno.Text.Length - (model.Axes[1].ActualMaximum - model.Axes[1].ActualMinimum) * 0.01
        );
    }

    private void RenderTextAnnotaions() {
      foreach(RectangleAnnotation anno in model.Annotations) {
        anno.TextPosition = CalcTextPoint(anno);
      }
    }

    private void AllToFalse() {
      if (AddingStage) AddingStage = false;
      if (DelingStage) DelingStage = false;
    }

    private bool addingStage = false;
    private bool AddingStage {
      get { return addingStage; }
      set {
        if (value) AllToFalse();
        addingStage = value;
        if (addingStage) {
          controller.Unbind(PlotCommands.PointsOnlyTrack);
          btnAddStage.Background = new SolidColorBrush(Colors.Coral);
        } else {
          btnAddStage.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
          new Thread(new ThreadStart(BindMouseTrack)).Start();
        }
      }
    }

    private bool delingStage = false;
    private bool DelingStage {
      get { return delingStage; }
      set {
        if (value) AllToFalse();
        delingStage = value;
        if (delingStage) {
          controller.Unbind(PlotCommands.PointsOnlyTrack);
          btnDelStage.Background = new SolidColorBrush(Colors.Coral);
        } else {
          btnDelStage.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
          new Thread(new ThreadStart(BindMouseTrack)).Start();
        }
      }
    }


    private void BindMouseTrack() {
      Thread.Sleep(100);
      controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.PointsOnlyTrack);
    }

    private RectangleAnnotation RangeMark = new RectangleAnnotation();
    private int idStage = 0;
    private bool ranging = false;

    private void Model_MouseDown(object sender, OxyMouseDownEventArgs e) {
      if (AddingStage) {
        if (ranging) {
          ranging = false;
          AddingStage = false;
          int indexStage = 0;
          DateTime x2 = DateTimeAxis.ToDateTime(RangeMark.MaximumX);
          for (int i = 0; i < stages.Count; i++) {
            if (stages[i].DateTime < x2) {
              indexStage = i+1;
            } else {
              break;
            }
          }
          stages.Insert(indexStage, new Stage {
            ID = idStage,
            DateTime = x2,
            Text = RangeMark.Tag.ToString(),
            IsSecond = true,
          });
          model.Annotations.Remove(RangeMark);
          RenderAnno();
        } else {
          PlotModel plot = model as PlotModel;
          ElementCollection<Axis> axisList = plot.Axes;
          Axis X_Axis = null, Y_Axis = null;
          foreach (Axis ax in axisList) {
            if (ax.Position == AxisPosition.Bottom)
              X_Axis = ax;
            else if (ax.Position == AxisPosition.Left)
              Y_Axis = ax;
          }
          RangeMark = new RectangleAnnotation();
          string text = Microsoft.VisualBasic.Interaction.InputBox("Введите название этапа", "");
          if (text == "")
            return;
          LineAnnotation anno = new LineAnnotation();
          anno.Text = text;
          anno.X = RoundToSecond(Axis.InverseTransform(e.Position, X_Axis, Y_Axis).X);
          anno.Type = LineAnnotationType.Vertical;
          anno.Color = OxyColors.Green;
          anno.MouseDown += Stage_MouseDown;
          anno.FontSize = TextFontSize;
          anno.TextColor = OxyColors.Black;
          anno.Tag = idStage;
          model.Annotations.Add(anno);
          RangeMark.SelectionMode = OxyPlot.SelectionMode.Single;          
          RangeMark.MinimumX = RoundToSecond(Axis.InverseTransform(e.Position, X_Axis, Y_Axis).X);
          RangeMark.MaximumX = Axis.InverseTransform(e.Position, X_Axis, Y_Axis).X;
          RangeMark.Layer = AnnotationLayer.BelowSeries;
          model.Annotations.Add(RangeMark);
          idStage = new Random().Next(1, int.MaxValue);
          RangeMark.Tag = text;
          ranging = true;
          plotter.InvalidatePlot(false);
          int indexStage = 0;
          DateTime x1 = DateTimeAxis.ToDateTime(RangeMark.MinimumX);
          for(int i = 0; i < stages.Count; i++) {
            if (stages[i].DateTime < x1) {
              indexStage = i+1;
            } else {
              break;
            }
          }
          stages.Insert(indexStage, new Stage {
            ID = idStage,
            DateTime = x1,
            Text = text,
            IsSecond = false,
            IsChecked = true
          });
        }
      } 
    }

    private double RoundToSecond(double input) {
      DateTime dt = DateTimeAxis.ToDateTime(input);
      double result = DateTimeAxis.ToDouble(dt);
      return result;
    }

    private void Model_MouseMove(object sender, OxyMouseEventArgs e) {
      Axis X_Axis = model.Axes[0], Y_Axis = model.Axes[1];
      if (ranging) {
        PlotModel plot = model as PlotModel;
        ElementCollection<Axis> axisList = plot.Axes;
        RangeMark.MaximumX = Axis.InverseTransform(e.Position, X_Axis, Y_Axis).X;
      }
      plotter.InvalidatePlot(false);
    }

    private void Stage_MouseDown(object sender, OxyMouseDownEventArgs e) {
      if (DelingStage) {
        if (e.ChangedButton == OxyMouseButton.Left) {
          if (MessageBox.Show("Вы точно хотите удалить этап?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
            Annotation an = sender as Annotation;
            for (int i = 0; i < stages.Count;) {
              if (stages[i].ID == Convert.ToInt32(an.Tag)) {
                stages.Remove(stages[i]);
              } else
                i++;
            }
            model.Annotations.Remove(an);
            //plotter.InvalidatePlot(false);
            DelingStage = false;
            RenderAnno();
          }
        }
      }
    }   

    private void AddStage_Click(object sender, RoutedEventArgs e) {
      AddingStage = !AddingStage;
      isNeedSave = true;
    }

    private void DelStage_Click(object sender, RoutedEventArgs e) {
      DelingStage = !DelingStage;
      isNeedSave = true;
    }

    private void DelAllStage_Click(object sender, RoutedEventArgs e) {
      stages.Clear();
      btnShowGraph_Click(null, null);
    }

    private void Save_Click(object sender, RoutedEventArgs e) {
      isNeedSave = false;
      SaveData();
    }

    private void SaveData() {
      GS_SPA.Core.Archive.OverrideData((WorkInfo)lvArchives.SelectedItem, dataParams, stages);
    }

    private Stage FindSecondStage(int id) {
      foreach (Stage stage in stages)
        if (stage.ID == id && stage.IsSecond)
          return stage;
      return null;
    }

    private void Report_Click(object sender, RoutedEventArgs e) {
      if (lvArchives.SelectedItem == null) {
        MessageBox.Show("Не выбраны архивные файлы", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      Report r = new Report();
      r.Do(
        (WorkInfo)lvArchives.SelectedItem,
        dataParams,
        stages
        );
    }

    private void SaveGraph_Click(object sender, RoutedEventArgs e) {
      if (plotter.Model == null)
        return;
      WorkInfo wi = (WorkInfo)lvArchives.SelectedItem;
      System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
      sfd.Filter = "PNG file (*.png)|*.png";
      sfd.Title = "Сохранить";
      if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
        plotter.SaveBitmap(sfd.FileName, 1920, 1260, plotter.ActualModel.Background);
      }
    }

    private List<int> selectedGraph = new List<int>();
    private int indexPage = 0;

    private void PrintGraph_Click(object sender, RoutedEventArgs e) {
      if (Directory.Exists(Constants.ENVPATH + "temp")) {
        Directory.Delete(Constants.ENVPATH + "temp\\", true);
      }
      int indexOld = lbParams.SelectedIndex;
      selectedGraph = new List<int>();
      for (int i = 0; i < 6; i++) {
        selectedGraph.Add(0);
      }
      ArchivePrintWindow apw = new ArchivePrintWindow(selectedGraph);
      if (apw.ShowDialog() == false)
        return;
      for (int i = 0; i < selectedGraph.Count; i++) {
        if (selectedGraph[i] > 0) {
          if (!Directory.Exists(Constants.ENVPATH + "temp\\")) {
            Directory.CreateDirectory(Constants.ENVPATH + "temp\\");
          }
          lbParams.SelectedIndex = i;
          btnShowGraphStage_Click(null, null);
          try {
            plotter.SaveBitmap(Constants.ENVPATH + @"temp\" + selectedGraph[i].ToString() + ".png", 1920, 1260, plotter.ActualModel.Background);
          } catch {
            return;
          }
        }
      }
      lbParams.SelectedIndex = indexOld;
      btnShowGraphStage_Click(null, null);

      System.Windows.Forms.PrintDialog printdg = new System.Windows.Forms.PrintDialog();
      System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
      printdg.AllowSomePages = true;
      printdg.AllowSelection = true;
      pd.PrintPage += D_PrintPage;
      pd.DefaultPageSettings.Landscape = true;

      if (printdg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
        pd.Print();
        pd.Dispose();
      }
    }

    private void D_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e) {
      System.Drawing.Printing.PrintDocument pd = sender as System.Drawing.Printing.PrintDocument;
      for (; indexPage < selectedGraph.Count;) {
        if (selectedGraph[indexPage] > 0) {
          System.Drawing.Image image = new System.Drawing.Bitmap(Constants.ENVPATH + @"temp\" + selectedGraph[indexPage].ToString() + ".png");
          e.Graphics.DrawImage(image, 30, 30, pd.DefaultPageSettings.Bounds.Width - 60, pd.DefaultPageSettings.Bounds.Height - 60);
          indexPage++;
          break;
        } else {
          indexPage++;
        }
      }
      if (indexPage >= selectedGraph.Count - 1)
        e.HasMorePages = false;
      else {
        e.HasMorePages = false;
        for (int i = indexPage; i < selectedGraph.Count; i++) {
          if (selectedGraph[i] > 0)
            e.HasMorePages = true;
        }
      }
    }
  }  
}