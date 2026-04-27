using SharpSim;

namespace SMT2020;

public class Port(Fab fab, FabHistory hist, int id, string name, PortType type) 
    : SimNode<Fab, FabHistory> (fab, hist, id, name)
{
    public PortType Type { get; private set; } = type;
    //public string BayName { get; private set; }
    public Foup? ReservedFoup { get; private set; } = null;
    public Foup? Foup { get; private set;} = null;

    #region Initialize
    public void SetCurrentBay(string bayName)
    {
        //BayName = bayName;
    }

    // public override void SetInitLocaiton(Location location)
    // {
    //     base.SetInitLocaiton(location);
    //     this.SetPosition(location.GetPosition());
    // }
    #endregion Initialize End

    #region Simulation Mehtods
    public override void Initialize()
    {
        base.Initialize();
        this.SetState(PortState.Empty);
        ReservedFoup = null;
    }

    /// <summary>
    /// Reserve Port to Load Foup
    /// </summary>
    public void Reserve(Foup foup)
    {
        this.SetState(PortState.Reserved);   
        ReservedFoup = foup;
    }

    public void LoadStart(Transport transport, Foup foup)
    {
        Sim.Delay(10, new List<Action>() { () => { LoadFinish(transport, foup); }});
    }

    protected virtual void LoadFinish(Transport transport, Foup foup)
    {
        this.SetState(PortState.Full);
        this.Entities.Add(foup);
        
        ReservedFoup = null;

        foup.SetCurrentPort(this);
        
        if (foup.Lot is not null) //[TBD]
        {
            var lot = foup.Lot;
            //lot.SetMovingState(LotMovingState.OnBuffer);
        }

        // Unload Finish In transport perspective
        transport.UnloadFinish(foup); 
        //LogHandler.AddLog(LogLevel.Info, $"{timeNow} / {this.Name} / {foup.Name} / LoadFinish");
    }

    public void UnloadStart(Transport transport, Foup foup)
    {
        Sim.Delay(10, new List<Action>() { () => { UnloadFinish(transport, foup); }});
    }

    protected virtual void UnloadFinish(Transport transport, Foup foup)
    {
        this.SetState(PortState.Empty);
        this.Entities.Remove(foup);

        foup.SetCurrentPort(null);

        if (foup.Lot is not null) //[TBD]
        {
            var lot = foup.Lot;
            //lot.SetMovingState(LotMovingState.OnOHT);
        }

        // Load Finish In transport perspective
        transport.LoadFinish(foup);
        //LogHandler.AddLog(LogLevel.Info, $"{timeNow} / {this.Name} / {foup.Name} / UnloadFinish");

    }
    #endregion Simulation Mehtods End
}

