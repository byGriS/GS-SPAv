using Service;

namespace Input {
  public class InputParam {
    public DataParam Param { get; set; } = new DataParam();
    public ushort SlaveID { get; set; } = 1;
    public double Address { get; set; } = 1;
    public Command Command { get; set; } = Command.ReadHoldingRegisters;
    public TypeParam TypeParam { get; set; } = TypeParam.FLOAT;
  }
}
