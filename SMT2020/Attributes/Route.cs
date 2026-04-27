using SharpSim;

namespace SMT2020;

public class Route
{
    private string _name;
    public string Name { get => _name; }
    public double TotalProcessingTime { get; private set; } = 0;
    public List<Step> Steps { get; private set; } = new List<Step>();
    public int Count { get => Steps.Count; }

    public Route(string name)
    {
        _name = name;
    }

    public void AddStep(Step step)
    {
        if (Steps.Count + 1 == step.Order)
        {
            Steps.Add(step);
            TotalProcessingTime += step.ProcessingTime.Mean;
        }
        else
        {
            LogHandler.Error($"Route({Name}) : Wrong Step Order");
        }
    }
}


