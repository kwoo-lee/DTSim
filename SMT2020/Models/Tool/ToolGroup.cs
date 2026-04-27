using SharpSim;

namespace SMT2020;

public class ToolGroup(int id, string name, AreaType areaType, ToolType toolType, ProcessingUnit processingUnit, double loadingTime, double unloadingTime)
    : SimObject(id, name)
{
    #region [Attributes]
    public AreaType AreaType { get; private set; } = areaType;
    public ToolType Toolype { get; private set; } = toolType;
    public ProcessingUnit ProcessingUnit { get; private set; } = processingUnit;
    public double LoadingTime { get; } = loadingTime;
    public double UnloadingTiem { get; } = unloadingTime;
    public DispatchingRuleSet DispatchingRuleSet { get; private set; } = new(DispatchingRuleType.FIFO);
    public List<Tool> Tools { get; } = [];
    public List<List<Lot>> AssignLots { get; } = [];
    public List<Lot> LotQueue { get; } = [];
    #endregion [Attributes End]

    public void AddTool(Fab fab, FabHistory fabHistory, int numberOfTools)
    {
        for (int i = 0; i < numberOfTools; i++)
        {
            var tool = new Tool(fab, fabHistory, Tools.Count, Name + $"_{i}", Toolype, this);
            for (int j = 0; j < 4; j++)
                new LoadPort(fab, fabHistory, j, tool.Name + $"_{j}", tool);

            Tools.Add(tool);
            AssignLots.Add([]);
        }
    }

    public void SetDispatchingRule(DispatchingRuleSet ruleSet) =>
        DispatchingRuleSet = ruleSet;


    // public List<Down> Downs { get; private set; }
    // private Breakdown _breakdown;
    // private BatchRule _batchRule;
    // public void AddPM(PM pm) { ... }
    // public void SetBreakDown(Breakdown bd) { ... }
}
