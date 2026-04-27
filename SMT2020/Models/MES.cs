using SharpSim;

namespace SMT2020;

public class MES(Fab fab, FabHistory hist, int id, string name) : SimNode<Fab, FabHistory>(fab, hist, id, name)
{
    private Random r = new Random();
    private IDispatcher dispatcher = new Dispatcher();

    #region [Manufacturing Information]
    public List<string> Products { get; private set; } = [];
    public Dictionary<string, Route> Routes { get; private set; } = new ();
    public List<ToolGroup> ToolGroups { get; private set; } = [];
    public List<Tool> Tools { get; private set; } = [];
    public List<LoadPort> LoadPorts {get; } = [];
    public Dictionary<string, ToolGroup> ToolGroupByName { get; private set; } = new ();
    #endregion 

    public List<List<Lot>> AssignedLots { get;} = [];
    public List<int> dispatchToolGroups = new List<int>();
    public double dispatchDelay = 1;

    #region [Initialize]
    public void AddToolGroup(ToolGroup toolGroup, int numberOfTools)
    {
        ToolGroups.Add(toolGroup);
        ToolGroupByName[toolGroup.Name] = toolGroup;

        // Generate Tools
        if(toolGroup.Name != "Delay_32")
        {
            toolGroup.AddTool(Sim, History, numberOfTools);
        }
    }
    #endregion

    public void SendLotToNextStep(Foup foup)
    {
        Lot lot = foup.Lot;

        if (lot.StepIndex == -1) // Fab In
        {
            lot.StepIndex++;
            //NewLot(lot);
            //_foups.Add(foup.Name, foup);
        }
        else
        {
            //Step currentStep = lot.CurrentStep;
            //EndStep(timeNow, lot);

            double reworkRatio = lot.CurrentStep.ReworkProbability;
            double probability = r.NextDouble();
            probability = 1;
            if (probability >= reworkRatio) // Proceed to Next Step
            {
                // lot.RemainingProcessingTime -= lot.Route[lot.CurrentStep].ProcessingTime.Mean;
                // if (lot.RemainingProcessingTime < 0)
                //     lot.RemainingProcessingTime = 0;
                lot.StepIndex++;
            }
            else // Rework, Go back to Rework Step
            {
                throw new NotImplementedException();
                // for (uint i = lot.ReworkStep; i < lot.Step; i++)
                // {
                //     var mean = lot.Route[lot.Step].ProcessingTime.Mean;
                //     lot.RemainingProcessingTime += mean;
                // }
                // lot.StepIndex = lot.ReworkStep;
                // lot.NeedRework = false;
            }
        }

        if (lot.Route.Count > lot.StepIndex) // Next Step
        {
            LogHandler.Info($"{Sim.Now, -11:F1} | {this.Name, -8} | {lot.Name} | ({lot.CurrentStep.Order}){lot.CurrentStep.Description}");
            Step nextStep = lot.CurrentStep;
            ToolGroup nextTG = nextStep.ToolGroup;

            double samplingPct = r.NextDouble();
            if(samplingPct > nextStep.ProcessingProbability)
            {
                SendLotToNextStep(foup);
                return;
            }

            if (nextTG.Name == "Delay_32")
            {
                double delayTime = nextStep.ProcessingTime.GetNumber();
                Sim.Delay(delayTime, new List<Action>() { () => { SendLotToNextStep(foup); } });
                return;
            }

            nextTG.LotQueue.Add(lot);
            if(!dispatchToolGroups.Contains(nextTG.Id))
            {
                if(dispatchToolGroups.Count == 0)
                    Sim.Delay(dispatchDelay, [() => { Dispatch(); }]);

                dispatchToolGroups.Add(nextTG.Id);
            }

            // ------ Temporary Test ------ 
            // double processingTime = nextStep.ProcessingTime.GetNumber();
            // Sim.Delay(processingTime, new List<Action>() { () => { SendLotToNextStep(foup); } });
            // ---------------------------- 
            
            // TBDs
            // StartStep(timeNow, lot, nextStep);

            // ToolGroup nextToolGroup = _toolGroups[nextStep.ToolGroupName];
            // nextToolGroup.ExternalTransition(timeNow, new SimPort(TGExtPort.NewJob, foup));
        }
        else // Finish. Go To Complete
        {
            LogHandler.Info($"{Sim.Now, -11:F1} | {this.Name, -8} | {lot.Name} | FabOut");
            System.Console.WriteLine(lot.Route.TotalProcessingTime);

            // TBDs
            // Complete complete = _completes.Find(x => x.Spec.RoutePlans.ContainsKey(lot.Route));
            // RequestOHT(timeNow, foup.CurrentPort, complete, foup);
        }
    }

    private void ProceedToNextStep(Foup foup)
    {

    }

    public void RequestNextLot(Tool Tool)
    {
        // Send Assigned Lots to Tool's Loadport

    }

    public void Dispatch()
    {
        for(int i = 0; i < dispatchToolGroups.Count; i++)
        {
            int id = dispatchToolGroups[i];
            ToolGroup toolGroup = ToolGroups[id];

            Dictionary<Tool, List<Lot>> results = dispatcher.Do(Sim.Now, toolGroup);
            //toolGroup.AssignLots()
        }

        dispatchToolGroups.Clear();
    }
}