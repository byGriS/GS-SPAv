using Service;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Archive {
  public class Archive {

    private string Head = "";
    private string FileName = "";
    private ObservableCollection<DataParam> ListDataParams = null;

    public Archive(ObservableCollection<DataParam> listDataParams) {
      ListDataParams = listDataParams;
      System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
      //CreateHead();
    }

    public void NewInjection(WorkInfo wi) {
      if (!Directory.Exists(Constants.ENVPATH + "Archives\\")) {
        Directory.CreateDirectory(Constants.ENVPATH + "Archives\\");
      }
      if (!Directory.Exists(Constants.ENVPATH + "ADT\\")) {
        Directory.CreateDirectory(Constants.ENVPATH + "ADT\\");
      }
      Head = wi.PhoneNumber +"|"+ wi.Well + "|" + wi.Field + "|" + wi.Builder + "|" + wi.FIO + "|" + wi.NumPKRS + "|"+wi.Start + "|" + wi.Company + "|" + wi.Bush + "\r\n";
      Head += "Время;";
      foreach (DataParam param in ListDataParams) {
        Head += param.Title + ";";
      }
      Head += "Этап;Метка\r\n";
      FileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") ;
      File.WriteAllText(Constants.ENVPATH + "Archives\\" + FileName + ".csv", Head);
    }

    public void Write(ObservableCollection<Stage> stages) {
      string path = Constants.ENVPATH + "Archives\\" + FileName + ".csv";
      string result = "";
      for (int i = 0; i < ListDataParams.Count; i++) {
        result += (ListDataParams[i].LastValue.Value).ToString() + ";";
      }
      foreach (Stage stage in stages) {
        result += stage.ID + "|";
        result += stage.Text;
        result += "|" + stage.IsSecond;
        result += ";";
      }
      File.AppendAllText(path, DateTime.Now + ";" + result + "\r\n", System.Text.Encoding.UTF8);
      WriteADT();
    }

    public WorkInfo[] GetListWorks() {
      FileInfo[] files = new DirectoryInfo(Constants.ENVPATH + "Archives\\").GetFiles("*.csv");
      WorkInfo[] result = new WorkInfo[files.Length];

      //string[] filesName = new string[files.Length];
      for (int i = files.Length-1; i >= 0; i--) {
        string[] allLines = File.ReadAllLines(files[files.Length - 1 - i].FullName);
        string[] info = allLines[0].Split('|');
        string startEnd = "";
        try {
          DateTime dtStart = DateTime.Parse(info[6]);
          string stringEnd = allLines[allLines.Length - 1].Split(';')[0];
          DateTime dtEnd = DateTime.Parse(stringEnd);

          if (dtStart.Day == dtEnd.Day) {
            startEnd += dtStart.ToString("HH:mm");
            startEnd += "-";
            startEnd += dtEnd.ToString("HH:mm dd/MM/yy");
          } else {
            startEnd += dtStart.ToString("HH:mm dd/MM/yy");
            startEnd += "-";
            startEnd += dtEnd.ToString("HH:mm dd/MM/yy");
          }
        } catch {
          startEnd = info[6];
        }

        result[i] = new WorkInfo {
          PhoneNumber = info[0],
          Well = info[1],
          Field = info[2],
          Builder = info[3],
          FIO = info[4],
          NumPKRS = info[5],
          Start = info[6],
          StartEnd = startEnd,
        };
      }
      
      return result;
    }

    public void GetData(WorkInfo fi, ObservableCollection<DataParam> dataParams, ObservableCollection<Stage> stages) {
      dataParams.Clear();
      foreach (DataParam param in ListDataParams) {
        dataParams.Add(param.CopyNewDataParam());
      }
      string[] lines = File.ReadAllLines(Constants.ENVPATH + "Archives\\" + fi.Start.Replace(':', '_').Replace(' ', '_').Replace('-', '_') + ".csv", System.Text.Encoding.UTF8);
      for (int line = 2; line < lines.Count(); line++) {
        string[] data = lines[line].Split(';');
        for (int i = 1; i < data.Length; i++) {
          if (i < dataParams.Count + 1) {
            try {
              dataParams[i - 1].Points.Add(new DataParamPoint { DateTime = Convert.ToDateTime(data[0]), Value = Convert.ToSingle(data[i]) });
            } catch {
              dataParams[i - 1].Points.Add(new DataParamPoint { DateTime = Convert.ToDateTime(data[0]), Value = 0 });
            }
          } else {
            if (data[i] != "") {
              string[] stageData = data[i].Split('|');
              stages.Add(new Stage {
                ID = Convert.ToInt32(stageData[0]),
                Text = stageData[1],
                DateTime = Convert.ToDateTime(data[0]),
                IsSecond = Convert.ToBoolean(stageData[2])
              });
            }
          }
        }
      }
    }

    public void OverrideData(WorkInfo wi, ObservableCollection<DataParam> dataParams, ObservableCollection<Stage> inStages) {
      ObservableCollection<Stage> stages = new ObservableCollection<Stage>();
      foreach (Stage stage in inStages)
        stages.Add(stage.Clone());

      string result = "";
      string[] lines = File.ReadAllLines(Constants.ENVPATH + "Archives\\" + wi.Start.Replace(':', '_').Replace(' ', '_').Replace('-', '_') + ".csv", System.Text.Encoding.UTF8);
      result += lines[0] + "\r\n";
      result += lines[1] + "\r\n";

      for (int lineIndex = 2; lineIndex < lines.Length; lineIndex++) {
        string[] data = lines[lineIndex].Split(';');
        string line = data[0] + ";";
        for (int i = 1; i < dataParams.Count+1; i++) {
          line += data[i] + ";";
        }
        DateTime now = DateTime.Parse(data[0]);
        for (int j = 0; j < stages.Count; j++) {
          if (stages[j].DateTime <= now || Math.Abs((stages[j].DateTime- now).TotalSeconds)<=1) {
            line += stages[j].ID + "|";
            line += stages[j].Text;
            line += "|" + stages[j].IsSecond;
            line += ";";
            stages.Remove(stages[j--]);
          } else {
            break;
          }
        }
        line += "\r\n";
        result += line;
      }
      File.WriteAllText(Constants.ENVPATH + "Archives\\" + wi.Start.Replace(':', '_').Replace(' ', '_').Replace('-', '_') + ".csv", result, System.Text.Encoding.UTF8);
    }
    
    public void WriteADT() {
      string path = Constants.ENVPATH + "ADT\\" + FileName + ".adt";
      string result = "";
      for (int i = 0; i < ListDataParams.Count; i++) {
        result += (ListDataParams[i].LastValue.Value).ToString().Replace(',','.') + ",";
      }
      File.AppendAllText(path, DateTime.Now + ";" + result + "\r\n", System.Text.Encoding.UTF8);
    }
  }
}