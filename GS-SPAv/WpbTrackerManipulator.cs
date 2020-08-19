using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_SPAv {
  public class WpbTrackerManipulator : MouseManipulator {
    private DataPointSeries currentSeries;

    public WpbTrackerManipulator(IPlotView plotView)
        : base(plotView) {
    }

    /// <summary>
    /// Occurs when a manipulation is complete.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Completed(OxyMouseEventArgs e) {
      base.Completed(e);
      e.Handled = true;

      currentSeries = null;
      PlotView.HideTracker();
    }

    /// <summary>
    /// Occurs when the input device changes position during a manipulation.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Delta(OxyMouseEventArgs e) {
      base.Delta(e);
      e.Handled = true;
      if (currentSeries == null) {
        PlotView.HideTracker();
        return;
      }
      var actualModel = PlotView.ActualModel;
      if (actualModel == null) {
        return;
      }
      if (!actualModel.PlotArea.Contains(e.Position.X, e.Position.Y)) {
        return;
      }
      var time = currentSeries.InverseTransform(e.Position).X;
      var points = currentSeries.Points;
      if (points == null) return;
      DataPoint dp = points.FirstOrDefault(d => d.X >= time);
      if (dp.X != 0 || dp.Y != 0) {
        int index = points.IndexOf(dp);
        var ss = PlotView.ActualModel.Series.Cast<DataPointSeries>();
        TrackParam[] values = new TrackParam[PlotView.ActualModel.Series.Count+1];
        values[0] = new TrackParam() { Title = "" };
        int i = 0;
        for (i = 0; i < PlotView.ActualModel.Series.Count; i++) {
          values[i+1] = new TrackParam() { Title = PlotView.ActualModel.Series[i].Title };
        }
        values[0].Value = dp.X;
        i = 1;
        foreach (var series in ss) {
          values[i++].Value = series.Points[index].Y;
        }

        var position = XAxis.Transform(dp.X, dp.Y, currentSeries.YAxis);
        position = new ScreenPoint(position.X, e.Position.Y);

        var result = new WpbTrackerHitResult(values) {
          Series = currentSeries,
          DataPoint = dp,
          Index = index,
          Item = dp,
          Position = position,
          PlotModel = PlotView.ActualModel
        };
        PlotView.ShowTracker(result);
      }
    }

    /// <summary>
    /// Occurs when an input device begins a manipulation on the plot.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Started(OxyMouseEventArgs e) {
      base.Started(e);
      currentSeries = PlotView?.ActualModel?.Series
                       .FirstOrDefault(s => s.IsVisible) as DataPointSeries;
      Delta(e);
    }

  }

  public class WpbTrackerHitResult : TrackerHitResult {
    public TrackParam[] Values { get; private set; }
    
    [System.Runtime.CompilerServices.IndexerName("ValueString")]
    public string this[int index] {
      get {
        string result = "";
        for(int i = 0; i < Values.Length; i++) {
          if (i == 0) {
            result += OxyPlot.Axes.DateTimeAxis.ToDateTime(Values[i].Value).ToString();
          } else {
            result += Values[i].Title.Split(new string[] { ", " }, StringSplitOptions.None)[0] + ": " + Values[i].Value.ToString("0.###");
          }
          if (i < Values.Length - 1)
            result += "\n";
        }
        return result;
      }
    }

    public WpbTrackerHitResult(TrackParam[] values) {
      Values = values;
    }
  }

  public class TrackParam {
    public string Title { get; set; } = "";
    public double Value { get; set; } = 1;
  }

}
