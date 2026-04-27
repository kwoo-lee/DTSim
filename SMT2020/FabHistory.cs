using SharpSim;

namespace SMT2020;

public class FabHistory : IHistory
{
    public double TotalProcessingTime { get; set; } = 0;
    public double TotalIdleTime { get; set; } = 0;
    public double TotalMaintenanceTime { get; set; } = 0;
}