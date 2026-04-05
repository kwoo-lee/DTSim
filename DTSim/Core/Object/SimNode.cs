namespace DTSim.Core;

public interface ISimNode
{
    int Id { get; }
    string Name { get; }
    void Initialize();      
}

public abstract class SimNode<TSimulation> : SimObject, ISimNode where TSimulation : ISimulation
{
    protected readonly TSimulation Simulation;
    public SimNode(TSimulation simulation, string name) : base(simulation.Nodes.Count, name)
    {
        Simulation = simulation;
        Simulation.AddNode(this);
    }

    public abstract void Initialize();
}


