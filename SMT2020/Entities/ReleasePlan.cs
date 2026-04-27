using SharpSim;

namespace SMT2020;

public class ReleasePlan(string productName, Route route, string lotType, int wafersPerLot, int priroity, DateTime startDateTime, SimTime cycleTime, Distribution dist, int lotByRelease) 
{
    public string ProductName { get; private set; } = productName;
    public Route Route { get; private set; } = route;
    public string LotType { get; private set; } = lotType;
    public int WafersPerLot { get; private set; } = wafersPerLot;
    public int Priority { get; private set; } = priroity;
    public DateTime StartDateTime { get; private set; } = startDateTime;
    public SimTime CycleTime { get; private set;} = cycleTime;
    public Distribution Dist { get; private set; } = dist;
    public int LotByRelease {get; private set; } = lotByRelease;
    public int Count { get; set;} = 0;

    public override string ToString()
    {
        return LotType;
    }
}