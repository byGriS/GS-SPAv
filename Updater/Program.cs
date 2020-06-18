using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Updater {
  class Program {
    private static string EnvPath = "";
    private static string[] appName = { "GS-SPAv" };
    private static string[] modulesName = { "Archive.dll", "Core.dll", "Service.dll", "Input.dll", "alarm.mp3", "GS-SPAv.exe"  };
    private static string hostName = "http://spautomation.ru/gs-spav/";

    static void Main(string[] args) {
      if (args.Length == 1)
        return;
      try {
        EnvPath = args[1];
      } catch { }
      Console.WriteLine("Обновление:");
      if (args[0] == "d") {
        try {
          CloseApp();
          Console.WriteLine("5%");
          for (int i = 0; i < modulesName.Length; i++) {
            DeleteFile(modulesName[i]);
            Console.WriteLine((5 + 25 / modulesName.Length * i).ToString("#") + "%");
          }
          //DeleteFile("setting.dat");
          for (int i = 0; i < modulesName.Length; i++) {
            DownloadFile(modulesName[i]);
            Console.WriteLine((5 + 65 / modulesName.Length * i).ToString("#") + "%");
          }
          Process start = new Process();
          start.StartInfo.FileName = modulesName[modulesName.Length-1];
          start.Start();
          Console.WriteLine("100%");
          Console.WriteLine("Обновление успешно завершено");
        } catch (Exception ex) {
          string s = DateTime.Now.ToString("########## dd.MM.yyyy HH:mm:ss");
          StreamWriter sw = new StreamWriter(EnvPath + "\\errors.txt", true, Encoding.Unicode);
          sw.WriteLine(s + "\n " + ex.Message + "\n" + ex.ToString());
          sw.Close();
        }
      }
    }

    static private void DownloadFile(string fileName) {
      string myStringWebResource = null;
      WebClient myWebClient = new WebClient();
      myWebClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
      myStringWebResource = hostName + fileName;
      myWebClient.DownloadFile(myStringWebResource, EnvPath + fileName);
    }

    static private void CloseApp() {
      foreach (string name in appName) {
        foreach (Process proc in Process.GetProcessesByName(name)) {
          proc.Kill();
        }
      }
    }

    static private void DeleteFile(string fileName) {
      if (File.Exists(fileName)) {
        for (int i = 0; i < 10; i++) {
          try {
            File.Delete(EnvPath + fileName);
            break;
          } catch {
            Thread.Sleep(1000);
          }
        }
      }
    }
  }
}
