using SharpSim;

namespace SMT2020;

public interface IDispatcher
{
    Dictionary<Tool, List<Lot>> Do(SimTime now, ToolGroup toolGroup);
}

// public class DispatchResults
// {
//     public Dictionary<Tool, List<Lot>> Results;
// }