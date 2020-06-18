using System.Collections.ObjectModel;
using System.IO.Ports;

namespace Input {
  public class Input {
    public SerialPort SerialPort { get; set; } = new SerialPort();
    public ObservableCollection<InputParam> InputParams { get; set; } = new ObservableCollection<InputParam>();
    public NModbus.IModbusSerialMaster Master { get; set; }
  }
}
