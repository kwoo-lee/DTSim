
using SharpSim;

namespace SMT2020;

public class Foup(int id, string name) : SimObject (id, name)
{
    public Lot? Lot { get; private set; }
    public ISimNode? CurrentNode { get; private set; }

    public void LoadLot(Lot lot)
    {
        if(Lot == null)
        {
            Lot = lot;
            Lot.CurrentFoup = this;
        }
        else
            LogHandler.Error("Foup : Empty Lot Load");
    }

    public void UnloadLot()
    {
        if(Lot != null)
        {
            Lot.CurrentFoup = null;
            Lot = null;
        }
        else
            LogHandler.Error("Foup : Not Lot to Unload");
    }
    
    public void SetCurrentPort(ISimNode? simNode)
    {
        CurrentNode = simNode;
    }
}


