using SharpSim;

namespace SMT2020;

public class Transport(Fab fab, FabHistory hist, int id, string name) 
    : SimNode<Fab, FabHistory>(fab, hist, id, name)
{
    /// <summary>Set of valid location names — populated from Transport sheet (FROM ∪ TO).</summary>
    public HashSet<string> Locations { get; } = new();

    public Dictionary<(string From, string To), Distribution> DeliveryTimes { get; } = new ();

    public void Delivery(string toLocation, Tool destTool, Lot lot)
    {
        double deliveryTime = 600;
        if(DeliveryTimes.TryGetValue((lot.Location, toLocation), out Distribution? dist) && dist != null)
        {
            deliveryTime = dist.GetNumber();
        }

        Sim.Delay(deliveryTime, [() => { 
            lot.Location = toLocation;
            destTool.LoadStart(this, lot);
        } ]);
        
    }
}