using SharpSim;

namespace SMT2020;

public class Lot(int id, string name, string productName, Route route, int wafersPerLot, int priroity, SimTime startTime, SimTime endTime) 
    : SharpSim.SimObject(id, name)
{
    public string ProductName { get; private set; } = productName;
    public Route Route { get; private set; } = route;
    public int StepIndex { get; set; } = -1;
    public Step? CurrentStep { get => Route.Steps[StepIndex]; } 
    public int WafersPerLot { get; private set; } = wafersPerLot;
    public int Priority { get; private set; } = priroity;
    public SimTime StartTime { get; private set; } = startTime;
    public SimTime DueTime { get; private set; } = endTime;
    public LotStatus State { get; set; }
    public SimTime EnqueueTime{get; set;}
    public SimTime EstimatedProcessEndTime {get;set;}
    public SimTime ProcessStartTime{get; set;}
    public SimTime ProcessEndTime {get; set;}
    public Foup? CurrentFoup { get; set;}

    public double GetProcessingTime()
    {
        double processingTime = CurrentStep.ProcessingTime.GetNumber();

        // [TBD] : check what is processing Prob
        // if (CurrentStep.ProcessingProbability != 0 && CurrentStep.ProcessingProbability != 100)
        //     processingTime *= (double)CurrentStep.ProcessingProbability / 100;

        if (CurrentStep.ProcessingUnit is ProcessingUnit.Wafer)
        {
            double interval = CurrentStep.CascadingInterval.GetNumber();
            if (Math.Abs(interval) > double.Epsilon)
            {
                // Cascading
                // p = p + int * (w -1);
                processingTime = processingTime + CurrentStep.CascadingInterval.GetNumber() * (WafersPerLot - 1);
            } 
            else // Table
            {
                // p* = p * w
                processingTime = processingTime * WafersPerLot;
            }
        }
        return processingTime;
    }
}