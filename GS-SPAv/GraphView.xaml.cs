using Core;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using Service;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;

namespace GS_SPAv {
  public partial class GraphView : UserControl {

    public ObservableCollection<DataParam> DataParams { get; set; } = new ObservableCollection<DataParam>();
    private PlotModel Model { get; set; } = new PlotModel();
    public Setting Setting = new Setting();

    public GraphView() {
      InitializeComponent();
      /*DataContext = Model;
      DateTimeAxis axisX = new DateTimeAxis() {
        Position = AxisPosition.Bottom,
        StringFormat = "HH:mm:ss",
        Key = "time"
      };
      axisX.MajorGridlineStyle = LineStyle.Solid;
      axisX.MajorGridlineThickness = 1;
      Model.Axes.Add(axisX);*/
      DataParams.CollectionChanged += DataParams_CollectionChanged;
    }

    private void DataParams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
        foreach (DataParam dataParam in DataParams) {
          dataParam.OnChangeValue -= Param_OnChangeValue;
          dataParam.OnChangeTitle -= DataParam_OnChangeTitle;
          dataParam.OnChangeValue += Param_OnChangeValue;
          dataParam.OnChangeTitle += DataParam_OnChangeTitle;
        }
        InitGraph();
        // InitGraphSetting();
      }
    }

    private void DataParam_OnChangeTitle(DataParam param) {
      for(int i=0;i< DataParams.Count; i++) {
        Model.Series[i].Title = DataParams[i].Title;
      }

      plotter.InvalidatePlot(false);
    }

    public void InitGraph() {
      Model = new PlotModel();
      Model.LegendPosition = LegendPosition.RightBottom;
      DateTimeAxis axisX = new DateTimeAxis() {
        Title = "Время",
        Position = AxisPosition.Bottom,
        StringFormat = "HH:mm:ss"
      };
      axisX.Minimum = DateTimeAxis.ToDouble(DateTime.Now);
      axisX.MajorGridlineStyle = LineStyle.Solid;
      axisX.MajorGridlineThickness = 1;
      Model.Axes.Add(axisX);

      bool first = true;
      int indexTier = 0;
      foreach (DataParam param in DataParams) {
        bool addAxe = true;
        /*foreach (LinearAxis axe in Model.Axes) {
          if (axe.Title == param.Title.Split(',')[1]) {
            addAxe = false;
          }
        }*/
        if (addAxe) {
          LinearAxis axeY = new LinearAxis() {
            //Title = param.Title.Split(' ')[0] + ", " + param.Title.Split(',')[1],
            Key = param.Title.Split(',')[0],
            Position = AxisPosition.Left,
            PositionTier = indexTier++,
            TextColor = OxyColor.FromRgb(param.ColorLine.Color.R, param.ColorLine.Color.G, param.ColorLine.Color.B),
            AxislineColor = OxyColor.FromRgb(param.ColorLine.Color.R, param.ColorLine.Color.G, param.ColorLine.Color.B),
            AxislineStyle = LineStyle.Solid
          };
          if (first) {
            first = false;
            //axeY.AxisChanged += AxeY_AxisChanged;
            axeY.MajorGridlineStyle = LineStyle.Solid;
            axeY.MajorGridlineThickness = 1;
          }
          Model.Axes.Add(axeY);
        }
        LineSeries series = new LineSeries() {
          Title = param.Title,
          YAxisKey = param.Title.Split(',')[0],
          Color = OxyColor.FromRgb(param.ColorLine.Color.R, param.ColorLine.Color.G, param.ColorLine.Color.B)
        };
        //series.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;
        Model.Series.Add(series);
      }
      for (; indexTier < 4;) {
        LinearAxis axeY = new LinearAxis() {
          //Title = param.Title.Split(' ')[0] + ", " + param.Title.Split(',')[1],
          Position = AxisPosition.Left,
          PositionTier = indexTier++,
          TextColor = OxyColors.White,
          AxislineColor = OxyColors.White,
          TitleColor = OxyColors.White,
          TicklineColor = OxyColors.White,
        };
        Model.Axes.Add(axeY);
      }

      var controller = new PlotController();
      controller.UnbindAll();
      plotter.Controller = controller;

      plotter.Model = Model;
      plotter.Model.PlotMargins = new OxyThickness(150, double.NaN, double.NaN, double.NaN);
    }

    private void Param_OnChangeValue(DataParam dataParam) {
      this.Dispatcher.Invoke(new ThreadStart(delegate {
        foreach (LineSeries line in plotter.Model.Series) {
          if (line.Title == dataParam.Title) {
            if (Setting.DepthOnlineGraph > 0) {
              Model.Axes[0].Minimum = DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(-1 * Setting.DepthOnlineGraph));
              /*while (line.Points.Count > Setting.DepthOnlineGraph / Setting.intervalReadGS)
                line.Points.RemoveAt(0);*/
            }
            if (dataParam.LastValue.ValueString != "Н/Д")
              line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), dataParam.LastValue.Value));
            plotter.InvalidatePlot(true);
          }
        }
        Model.Axes[0].Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(DateTime.Now);
      }));
    }

    public void AddStageAnnotation(Stage stage) {
      this.Dispatcher.Invoke(new ThreadStart(delegate {
        LineAnnotation anno = new LineAnnotation();
        anno.Text = stage.Text;
        anno.TextOrientation = AnnotationTextOrientation.AlongLine;
        //anno.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
        anno.TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom;
        anno.X = plotter.Model.Axes[0].ActualMaximum;
        anno.Type = LineAnnotationType.Vertical;
        anno.Color = OxyColors.Red;
        plotter.Model.Annotations.Add(anno);
        plotter.InvalidatePlot(false);
      }));
    }

    public void ClearData() {
      foreach (LineSeries line in plotter.Model.Series) {
        line.Points.Clear();
      }
    }
  }
}