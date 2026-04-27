using SharpSim;

namespace SMT2020;

public class Transport(Fab fab, FabHistory hist, int id, string name, Dictionary<(string From, string To), Distribution> deliveryTimes) 
    : SimNode<Fab, FabHistory>(fab, hist, id, name)
{
    public Dictionary<(string From, string To), Distribution> DeliveryTimes { get; } = deliveryTimes;

    public void UnloadFinish(Foup foup)
    {
        this.Entities.Remove(foup);
    }

/// <summary>
/// 
/// </summary>
/// <param name="foup"></param>
    public void LoadFinish(Foup foup)
    {
        this.Entities.Add(foup);
    }

    public void Delivery(string fromLocation, string toLocation, LoadPort destPort, Foup foup)
    {
        this.Entities.Add(foup);
        double deliveryTime = DeliveryTimes[(fromLocation, toLocation)].GetNumber();
        Sim.Delay(deliveryTime, [() => {destPort.LoadStart(this, foup);}])
    }
}