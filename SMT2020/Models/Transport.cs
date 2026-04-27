using SharpSim;

namespace SMT2020;

public class Transport(Fab fab, FabHistory hist, int id, string name) 
    : SimNode<Fab, FabHistory>(fab, hist, id, name)
{
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
}