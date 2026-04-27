using SharpSim;

namespace SMT2020;

/// <summary>
/// This is tool for Table Type & Cascading
/// </summary>
public class Tool(Fab fab, FabHistory hist, int id, string name, ToolType type, ToolGroup toolGroup)
    : SimNode<Fab, FabHistory>(fab, hist, id, name), IPortTool
{    
    #region [Attributes]
    protected double LoadingTime = toolGroup.LoadingTime;
    protected double UnloadingTime = toolGroup.UnloadingTiem;
    protected ToolType Type = type;
    public ToolGroup ToolGroup { get; private set; } = toolGroup;
    public List<LoadPort> Ports {get; private set;} = [];
    #endregion [Attributes End]

    #region [Lots]
    public Dictionary<Lot, bool> AssignedLots { get; private set; } = new();
    protected List<Lot> StagedLots { get; private set; } = [];
    protected Lot? TrackingInLot { get; private set; } = null;
    protected List<Lot> LoadedLots { get; private set; } = [];
    protected Dictionary<Lot, SimTime> RunningLots { get; private set; } = new();
    protected Lot? TrackingOutLot { get; private set; } = null;

    //protected LoadPort? NextTrackInPort = null;
    protected SimTime nextRunnableTime;
    #endregion [Lots End]

    #region Simulation Methods
    public override void SetState(Enum newState)
    {
        //_currentSetUpName = setUpName;
        //_lastLotName = lastLotName;

        var elapsedTime = Sim.Now - lastStateUpdatedTime;
        switch (this.State)
        {
            // case ToolState.Busy:
            //     _totalValueAddedTime += elapsedTime;
            //     break;
            // case ToolState.Breakdown:
            // case ToolState.PM:
            //     _totalDownTime += elapsedTime;
            //     break;
        }
        base.SetState(newState);
    }

    public override void Initialize()
    {
        base.Initialize();
        this.SetState(ToolState.Idle);
        //ReservedPM = null;
    }
    #endregion

    #region [Events]
    public virtual void LotStage(LoadPort port, Lot lot)   
    {
        StagedLots.Add(lot);
        if((Sim.Now + this.LoadingTime > nextRunnableTime) && !IsLotLoading())
            TrackInStart(port, lot);
    }

    protected virtual void TrackInStart(LoadPort port, Lot lot)
    {
        TrackingInLot = lot;
        Sim.Delay(this.LoadingTime, new List<Action>() { () => { TrackInFinish(port, lot); }});
    }

    protected virtual void TrackInFinish(LoadPort port, Lot lot) 
    {
        if(port.Foup == null)
        {
            LogHandler.Error("There is no Foup to unload");
            return;
        }

        port.Foup.UnloadLot();
        TrackingInLot = null;
        LoadedLots.Add(lot);

        if(Sim.Now > nextRunnableTime)
        {
            ProcessStart(lot);
        }
        else // Tracked In First
        {
            SimTime delay = Sim.Now - nextRunnableTime;
            Sim.Delay(delay, new List<Action>() { () => { ProcessStart(lot); }});
        }
    }

    protected virtual void ProcessStart(SimObject simObject)
    {
        Lot? lot = simObject as Lot;
        if(lot == null)
        {
            LogHandler.Error("Wrong Sim Object Type to process Start");
            return;
        }

        SimTime processingTime = lot.GetProcessingTime();
        SimTime estimatedRunTime = Sim.Now + processingTime;

        this.LoadedLots.Remove(lot);
        this.RunningLots.Add(lot, estimatedRunTime);

        Sim.DelayUntil(estimatedRunTime, [() => { ProcessFinish(lot); }]);

        // Lot Cascading Only
        if(this.Type == ToolType.Cascade && this.ToolGroup.ProcessingUnit == ProcessingUnit.Lot)
            nextRunnableTime = Sim.Now + lot.CurrentStep.CascadingInterval.GetNumber();
        else
            nextRunnableTime = estimatedRunTime;
        
        // Next Lot Track In Request.
        Sim.DelayUntil(nextRunnableTime - this.LoadingTime, [() => { TriggerNextTrackIn(); }]);
    }

    protected virtual void ProcessFinish(SimObject simObject)
    {
        Lot lot = simObject as Lot;
        LoadPort port = GetPortHavingEmptyFoup();

        // TBD: Lot Status
        RunningLots.Remove(lot);

        TrackOutStart(port, lot);
    }

    protected virtual void TrackOutStart(LoadPort port, Lot lot)
    {
        TrackingOutLot = lot;
        Sim.Delay(this.UnloadingTime, [() => { TrackOutFinish(port, lot); }]);
    }

    protected virtual void TrackOutFinish(LoadPort port, Lot lot)
    {
        port.Foup.LoadLot(lot);
        TrackingOutLot = null;

        Sim.MES.SendLotToNextStep(port.Foup);
    }

    public virtual void LotLeave(LoadPort port)
    {
        // Request Lots.
        Sim.MES.RequestNextLot(this);
    }

    protected virtual void JobStart(Lot lot) =>
        throw new NotImplementedException();
        
    protected virtual void JobFinish(Lot lot) =>
        throw new NotImplementedException();

    protected void TriggerNextTrackIn()
    {
        foreach(var port in Ports)
        {
            if(port.Foup != null && port.Foup.Lot != null)
            {
                TrackInStart(port, port.Foup.Lot);
                break;
            }
        }
    }
    #endregion [Process]

    #region [Port related]
    protected LoadPort GetPortHavingEmptyFoup() =>
        Ports.First(p => p.Foup != null && p.Foup.Lot == null);

    public LoadPort GetEmptyPort() =>
        Ports.First(p => p.Foup == null && p.ReservedFoup == null);
    // protected Foup GetEmptyFoup()
    // {
    //     var port = GetPortHavingEmptyFoup();
    //     return port.Foup;
    // }

    // protected Port FindLoadPort(Lot lot)
    // {
    //     foreach (var port in this.Ports)
    //     {
    //         if (port.Entities.Count > 0)
    //         {
    //             var foup = port.Entities[0] as Foup;
    //             if (lot == foup?.Lot)
    //                 return port;
    //         }
    //     }
    //     return null;
    // }
    
    // public LoadPort GetEmptyPort()
    // {
    //     var emptyPorts = GetPorts(PortState.Empty);
    //     return emptyPorts.Count > 0 ? emptyPorts[0] : null;
    // }

    // protected List<LoadPort> GetPorts(PortState portToolState) =>
    //     Ports.FindAll(p => ((PortState)p.State) ==  portToolState);

    // public bool HasEmptyPorts() => GetPorts(PortToolState.Empty).Count > 0;
    #endregion [Port related finish]

    #region Status
    public bool IsLotLoading() => TrackingInLot != null;
    #endregion
}
