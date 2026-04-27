using DocumentFormat.OpenXml.Spreadsheet;
using SharpSim;

namespace SMT2020;

public class LotRelease(Fab fab, FabHistory hist, int id, string name, Dictionary<string, List<ReleasePlan>> releasePlanByRoute, Dictionary<string, List<Lot>> futureLotsByRoute) 
    //: Tool(fab, hist, id, name, ToolType.LotRelease)
    : SimNode<Fab, FabHistory>(fab, hist, id, name)
{
    public static int LastLotId;
    public Dictionary<string, List<ReleasePlan>> ReleasePlanByRoute { get; private set; } = releasePlanByRoute;
    public Dictionary<string, List<Lot>> FutureLotsByRoute { get; private set; } = futureLotsByRoute;

    public override void Initialize()
    {
        base.Initialize();

        // Release type 1. Distribution based Release
        foreach (var (routeName, plans) in ReleasePlanByRoute)
        {
            foreach(ReleasePlan plan in plans)
            {
                var arrivalTime = new SimTime((plan.StartDateTime - Sim.StartDateTime).TotalSeconds);
                Sim.Delay(arrivalTime, new List<Action>() { () => { ReleaseByPlan(plan); } });

                break; // Temp
            }

            break; // Temp
        }

        // Release type 2. Lot by Lot Release
        foreach (var (routeName, plans) in FutureLotsByRoute)
        {
            if(plans.Count > 0)
            {
                Sim.DelayUntil(plans[0].StartTime, new List<Action>() { () => { ReleaseByLotList(routeName); } });
            }
        }
    }

    // public override void AddEQPPort(EQPPort port)
    // {
    //     base.AddEQPPort(port);
    //     this.SetInitLocaiton(port.Location);
    // }


    private void ReleaseByPlan(ReleasePlan plan)
    {
        for(int i = 0; i < plan.LotByRelease; i++)
        {
            Lot lot = new Lot(
                id: ++LastLotId, 
                name: plan.LotType + $"_{++plan.Count}", 
                productName: plan.ProductName, 
                route: plan.Route, 
                wafersPerLot: plan.WafersPerLot, 
                priroity: plan.Priority, 
                startTime: Sim.Now,
                endTime: Sim.Now + plan.CycleTime
            );

            ReleaseLot(lot);
        }

        // var delayTime = plan.Dist.GetNumber();
        // Sim.Delay(delayTime, new List<Action>() { () => { ReleaseByPlan(plan); }} );
    }
    
    private void ReleaseByLotList(string routeName)
    {
        Lot lot = FutureLotsByRoute[routeName][0];
        FutureLotsByRoute[routeName].RemoveAt(0);

        ReleaseLot(lot);

        if (FutureLotsByRoute[routeName].Count > 0)
        {
            SimTime arrivalTime = FutureLotsByRoute[routeName][0].StartTime;
            Sim.DelayUntil(arrivalTime, new List<Action>() { () => { ReleaseByLotList(routeName); } });
        }
        else
        {
            LogHandler.Info($"{Sim.Now, -11:F1} | Lot Release Finish {routeName}");
        }
    }

    private void ReleaseLot(Lot lot)
    {
        var foup = new Foup(lot.Id, "Foup_" + lot.Id);// New Foup In
        foup.LoadLot(lot);
        foup.SetCurrentPort(this);

        this.Entities.Add(foup);

        LogHandler.Info($"{Sim.Now, -11:F1} | {this.Name, -10} | Lot Release | {lot.Name}");

        // Temp
        Sim.MES.SendLotToNextStep(foup);
    }
}