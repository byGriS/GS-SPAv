using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GS_SPAv {
  public class License {
    private static string KeyData;

    private static string GetData() {
      string resultKey = null;
      try {
        ManagementObjectSearcher searcher8 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
        foreach (ManagementObject queryObj in searcher8.Get()) {
          KeyData = queryObj["Name"].ToString() + queryObj["NumberOfCores"].ToString() + queryObj["ProcessorId"].ToString();
        }
      } catch { }
      try {
        ManagementObjectSearcher searcher5 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
        foreach (ManagementObject queryObj in searcher5.Get()) {
          KeyData += queryObj["BuildNumber"].ToString() +
            queryObj["Caption"].ToString() +
            queryObj["Name"].ToString() +
            queryObj["OSType"].ToString() +
            queryObj["RegisteredUser"].ToString() +
            queryObj["SerialNumber"].ToString() +
            queryObj["ServicePackMajorVersion"].ToString() +
            queryObj["ServicePackMinorVersion"].ToString() +
            queryObj["Status"].ToString() +
            queryObj["SystemDevice"].ToString() +
            queryObj["SystemDirectory"].ToString() +
            queryObj["SystemDrive"].ToString() +
            queryObj["Version"].ToString() +
            queryObj["WindowsDirectory"].ToString();
        }
      } catch { }
      try {
        /*ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");
        ManagementObjectCollection a = searcher.Get();
        foreach (ManagementObject queryObj in searcher.Get()) {
          KeyData += queryObj["SerialNumber"].ToString();
        }*/
      } catch { }
      byte[] tmp = Encoding.Unicode.GetBytes(KeyData);
      for (int i = 0; i < tmp.Length; i++) {
        resultKey = resultKey + (tmp[i] >> 3).ToString();
      }
      return resultKey;
    }

    private static void CreateKey() {
      StreamWriter sw = File.CreateText("license.key");
      sw.WriteLine(GetData());
      sw.Close();
    }

    public static bool KeyChecking() {
      if (File.Exists("license.key")) {
        string readText = String.Concat<string>(File.ReadAllLines("license.key"));
        if (GetData() == readText) {
          return true;
        } else {
          return false;
        }
      }
      return false;
    }

    public static string WebCheck(string key) {
      WebRequest request = (HttpWebRequest)WebRequest.Create("http://www.spautomation.ru/gs/checkKey.php");
      request.Method = "POST";
      byte[] byteArray = Encoding.UTF8.GetBytes("key_number=" + key);
      request.ContentType = "application/x-www-form-urlencoded";
      request.ContentLength = byteArray.Length;
      Stream dataStream = request.GetRequestStream();
      dataStream.Write(byteArray, 0, byteArray.Length);
      dataStream.Close();
      WebResponse response = request.GetResponse();
      dataStream = response.GetResponseStream();
      StreamReader reader = new StreamReader(dataStream);
      var result = reader.ReadToEnd();
      if (result.ToString() == "1") {
        CreateKey();
        return "1";
      } else { return result; }
    }
  }
}
