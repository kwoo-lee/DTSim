using SharpSim;

namespace SMT2020;

public interface IPortTool
{
    List<LoadPort> Ports { get;}
    void LotStage(LoadPort port, Lot lot);
    void LotLeave(LoadPort port);
}

public class LoadPort : Port 
{
    public IPortTool Tool { get; private set; }

    public LoadPort(Fab fab, FabHistory hist, int id, string name, IPortTool tool) : base(fab, hist, id, name, PortType.LoadPort)
    {
        Tool = tool;
        Tool.Ports.Add(this);
    }

    protected override void LoadFinish(Transport transport, Foup foup)
    {
        // Foup: Transport --> Buffer
        base.LoadFinish(transport, foup);
        
        if (foup.Lot is not null) //[TBD]
        {
            // foup.Lot.SetMovingState(LotMovingState.OnPort);

            // Lot Staged
            Tool.LotStage(this, foup.Lot);
        }
    }

    protected override void UnloadFinish(Transport transport, Foup foup)
    {
        // Foup: Buffer --> Transport
        base.UnloadFinish(transport, foup);
        Tool.LotLeave(this);
    }

    
}