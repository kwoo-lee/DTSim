namespace DTSim.Core;


public interface ISimulation
{
    List<ISimNode> Nodes { get; }
    void Schedule(IEvent evt);
    void AddNode(ISimNode node);
    void Run(SimTime endOfSimulation);
    void Delay(SimTime delay, List<Action> actions);
}

public interface IHistory
{
}

public class Simulation<THistory> : ISimulation where THistory : IHistory
{
    private IEventList evtList;
    public SimTime Now { get; private set; } = new SimTime(0);
    public List<ISimNode> Nodes { get; private set; } = new List<ISimNode>();
    public THistory History { get; private set; }

    public Simulation(IEventList evtList, THistory history)
    {
        this.evtList = evtList;
        this.History = history;
    }

    public void AddNode(ISimNode node)
    {
        Nodes.Add(node);
    }

    public void Schedule(IEvent evt)
    {
        evtList.Add(evt);
    }

    public void Run(SimTime endOfSimulation)
    {
        foreach (var node in Nodes)
        {
            node.Initialize();
        }

        while (evtList.Count > 0)
        {
            var evt = evtList.RetrieveNext();
            if (evt is null || evt.Time > endOfSimulation)
                break;

            Now = evt.Time;
            evt.Execute();
        }
    }

    public void Delay(SimTime delay, List<Action> actions)
    {
        Schedule(new TimeDelayEvent(Now + delay, actions));
    }
}
