using Core.Properties;
using Service;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Timers;
using System.Windows;

namespace Core {
  public class Core {
    public Setting Setting { get; private set; } = new Setting();
    private Timer TimerArchive = new Timer(1000);
    private Input.Work inputWork = null;
    public Archive.Archive Archive = null;
    private int jobID = -1;
    private int countStages = 0;


    public delegate void ChangedState(string source, int state, string error);
    public event ChangedState OnChangedState;

    public Core() {
      Setting.LoadSetting();
      Setting.LoadWorkInfo();
      TimerArchive.Elapsed += TimerArchive_Elapsed;
      TimerArchive.AutoReset = true;
      Archive = new Archive.Archive(Setting.ListDataParams);
      inputWork = new Input.Work(Setting.inputCUDR, Setting.inputGS, Setting.ListStages);
      inputWork.OnChangedState += InputWork_OnChangedState;
    }

    private void InputWork_OnChangedState(string source, int state, string error) {
      OnChangedState?.Invoke(source, state, error);
    }

    public void Start() {
      if (Setting.IsWEB){
        SendToServerStart(Setting.WorkInfo);
      }
      inputWork.Start(Setting.IsCUDR, Setting.IsGS, Setting.intervalReadGS, Setting.intervalReadCUDR);
      TimerArchive.Start();
      countStages = Setting.ListStages.Count;
    }

    public void Stop() {
      TimerArchive.Stop();
      inputWork.Stop();
      StopAlarm();
    }

    private void TimerArchive_Elapsed(object sender, ElapsedEventArgs e) {
      ObservableCollection<Stage> stages = new ObservableCollection<Stage>();
      ObservableCollection<AlarmMark> marks = new ObservableCollection<AlarmMark>();
      CheckAlarmValue(marks);
      try {
        Application.Current.Dispatcher.Invoke(new System.Threading.ThreadStart(delegate {
          for(int i = 0; i < marks.Count; i++) { 
            if (marks[i].GetType() == typeof(AlarmMark)) {
              if (!((AlarmMark)marks[i]).IsSecond) {
                for (int j = 0; j < Setting.EventLogs.Count; j++) {
                  if (marks[j].Text == Setting.EventLogs[j].Text) {
                    Setting.EventLogs.Remove(Setting.EventLogs[j]);
                  }
                }
                Setting.EventLogs.Insert(0, marks[i]);
              }
            }
          }
        }));
      } catch { }
      if (countStages != Setting.ListStages.Count) {
        countStages = Setting.ListStages.Count;
        if (countStages != 1) {
          stages.Add(new Stage {
            Text = Setting.ListStages[countStages - 2].Text,
            ID = Setting.ListStages[countStages - 2].ID,
            DateTime = DateTime.Now,
            IsSecond = true
          });
        }
        stages.Add(new Stage {
          Text = Setting.ListStages[countStages - 1].Text,
          ID = Setting.ListStages[countStages - 1].ID,
          DateTime = DateTime.Now,
          IsSecond = false
        });
      }
      Archive.Write(stages);
      Archive.WriteADT();
      if (Setting.IsWEB) {
        SendToServerData();
      }

      if (Setting.ListStages.Count > 0) {
        Setting.ListStages[Setting.ListStages.Count - 1].Capacity1End = Setting.GetParamByTitleSmall("V1").LastValue.Value;
        Setting.ListStages[Setting.ListStages.Count - 1].Capacity2End = Setting.GetParamByTitleSmall("V2").LastValue.Value;
      }
    }

    private void CheckAlarmValue(ObservableCollection<AlarmMark> marks) {
      Random r = new Random();
      bool alarm = false;
      foreach (DataParam param in Setting.ListDataParams) {
        if (!param.isND && param.AlarmMin != param.AlarmMax) {
          if (param.LastValue.Value < param.AlarmMin) {
            if (!param.InAlarm)
              marks.Add(new AlarmMark {
                ID = r.Next(int.MaxValue),
                DateTime = DateTime.Now,
                Text = param.Title + " - сработал нижний порог"
              });
            param.InAlarm = true;
            if (param.IsAlarmSound)
              alarm = true;
          } else {
            if (param.LastValue.Value >= param.AlarmMax) {
              if (!param.InAlarm)
                marks.Add(new AlarmMark {
                  ID = r.Next(int.MaxValue),
                  DateTime = DateTime.Now,
                  Text = param.Title + " - сработал верхний порог"
                });
              param.InAlarm = true;
              if (param.IsAlarmSound)
                alarm = true;
            } else {
              if (param.InAlarm) {
                int id = -1;
                foreach (AlarmMark mark in Setting.EventLogs) {
                  if (mark.IsSecond != true && mark.Text.IndexOf(param.Title) > -1) {
                    id = mark.ID;
                    break;
                  }
                }
                if (id > -1)
                  marks.Add(new AlarmMark {
                    ID = id,
                    DateTime = DateTime.Now,
                    IsSecond = true
                  });
              }
              param.InAlarm = false;
            }
          }
        } else {
          param.InAlarm = false;
        }
      }
      if (alarm)
        AlarmSound();
      else {
        countAlarm = 0;
        StopAlarm();
      }
    }

    private int countAlarm = 0;
    private WMPLib.WindowsMediaPlayer WMP = null;
    private void AlarmSound() {
      countAlarm++;
      if (countAlarm < 5) return;
      if (WMP == null) {
        WMP = new WMPLib.WindowsMediaPlayer();
        WMP.URL = @"alarm.mp3";
      }
      if (WMP.playState != WMPLib.WMPPlayState.wmppsPlaying && Setting.isTotalSoundAlarm) {
        WMP.controls.play();
      }
    }

    public void StopAlarm() {
      if (WMP != null) {
        WMP.controls.stop();
      }
    }

    public void ChangeVolumeWMP() {
      if (WMP != null) {
        WMP.settings.volume = Setting.volumeSoundAlarm;
      }
    }

    private void SendToServerStart(WorkInfo wi) {
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://pkrs.awardto.me/v1/jobs");
      request.Method = "POST";
      request.Headers.Add("Authorization", "Basic Y29udHJvbGxlcjphc2VsYXU2YWVsb294MFhhaDhFY2g3Y2FpczZhZXI=");
      request.ContentType = "application/json";
      string data = "{" +
          "\"controllerId\": \"10524efa-debb-4509-9f42-be168a862e55\"," +
          "\"wellname\":\"" + wi.Well + "\"," +
          "\"fieldname\":\"" + wi.Field + "\"," +
          "\"contrCompany\": \"" + wi.Builder + "\"," +
          "\"responsible\": \"" + wi.FIO + "\"," +
          "\"process\": \"ОПЗ\"," +
          "\"serial\": \"" + wi.NumPKRS + "\"," +
          "\"configuration\": {" +
            "\"values\": [" +
              "{\"name\": \"P1\"}," +
              "{\"name\": \"P2\"}," +
              "{\"name\": \"P3\"}," +
              "{\"name\": \"T1\"}," +
              "{\"name\": \"R1\"}," +
              "{\"name\": \"Q1\"}," +
              "{\"name\": \"V1\"}," +
              "{\"name\": \"Q2\"}," +
              "{\"name\": \"V2\"}" +
            "]}}";

      try {
        using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
          writer.Write(data);
        }
        WebResponse response = request.GetResponse();
        using (Stream stream = response.GetResponseStream()) {
          using (StreamReader reader = new StreamReader(stream)) {
            string result = reader.ReadToEnd();
            jobID = Convert.ToInt32(result.Split(':')[2].Split('}')[0]);
          }
        }
        response.Close();
        OnChangedState?.Invoke("Web", 1, "");
      } catch (Exception e) {
        //MessageBox.Show("Не удалось создать закачку в удаленном сервер\n" + e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        OnChangedState?.Invoke("Web", 0, e.Message);
      }
    }

    private void SendToServerData() {
      if (jobID < 0)
        return;
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://pkrs.awardto.me/v1/jobs/" + jobID.ToString());
      request.ContentType = "application/json";
      request.Method = "POST";
      request.Headers["Authorization"] = "Basic Y29udHJvbGxlcjphc2VsYXU2YWVsb294MFhhaDhFY2g3Y2FpczZhZXI=";

      string data = "[" +
        "{" +
          "\"P1\": " + Setting.GetParamByTitleSmall("P1").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"P2\": " + Setting.GetParamByTitleSmall("P2").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"P3\": " + Setting.GetParamByTitleSmall("P3").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"T1\": " + Setting.GetParamByTitleSmall("T").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"R1\": " + Setting.GetParamByTitleSmall("po").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"V1\": " + Setting.GetParamByTitleSmall("V1").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"Q1\": " + Setting.GetParamByTitleSmall("Q1").LastValue.Value.ToString().Replace(',','.') + "," +
          "\"V2\": " + Setting.GetParamByTitleSmall("V1").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"Q2\": " + Setting.GetParamByTitleSmall("Q1").LastValue.Value.ToString().Replace(',', '.') + "," +
          "\"t\": " + (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + "000" +
        "}" +
      "]";

      try {
        using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
          writer.Write(data);
        }
        WebResponse response = request.GetResponse();
        using (Stream stream = response.GetResponseStream()) {
          using (StreamReader reader = new StreamReader(stream)) {
            string result = reader.ReadToEnd();
          }
        }
        response.Close();
        OnChangedState?.Invoke("Web", 1, "");
      } catch(Exception e) {
        OnChangedState?.Invoke("Web", 0, e.Message);
      }
    }
  }
}
