using Service;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace GS_SPAv {
  public partial class TableStageView : UserControl {
    public TableStageView() {
      InitializeComponent();
    }

    private ObservableCollection<Stage> stages = new ObservableCollection<Stage>();
    public ObservableCollection<Stage> Stages {
      get { return stages; }
      set {
        stages = value;
        DataContext = stages;
      }
    }
  }
}
