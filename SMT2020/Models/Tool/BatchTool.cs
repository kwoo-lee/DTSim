using SharpSim;

namespace SMT2020;

/// <summary>
/// Tool that processes lots in batches. A new Batch is opened when a lot is assigned
/// while no batch is open, and processing only starts once every lot in the batch
/// has finished staging.
/// </summary>
public class BatchTool(Fab fab, FabHistory hist, int id, string name, ToolType type, ToolGroup toolGroup)
    : Tool(fab, hist, id, name, type, toolGroup)
{
    #region [Batches]
    private int batchSeq = 0;
    protected List<Batch> Batches { get; } = [];
    protected Batch? CurrentBatch { get; private set; }
    protected Dictionary<Batch, List<Lot>> StagedLotsByBatch { get; } = new();
    #endregion [Batches End]

    public override void AssignLot(Lot lot)
    {
        int batchMax = lot.CurrentStep?.BatchMaximum > 0 ? lot.CurrentStep.BatchMaximum : 1;

        if (CurrentBatch == null)
        {
            IsReserved = true;
            CurrentBatch = new Batch(batchSeq, $"{Name}_Batch_{batchSeq++}");
            Batches.Add(CurrentBatch);
            StagedLotsByBatch[CurrentBatch] = [];
        }

        CurrentBatch.Lots.Add(lot);
        AssignedLots.Add(lot);
    }

    protected override void LoadFinish(Lot lot)
    {
        Entities.Add(lot);
        AssignedLots.Remove(lot);
        StagedLots.Add(lot);

        if (!CurrentBatch.Lots.Contains(lot))
        {
            LogHandler.Error($"LoadFinish: lot {lot.Name} has no batch assignment");
            return;
        }

        StagedLotsByBatch[CurrentBatch].Add(lot);

        if (StagedLotsByBatch[CurrentBatch].Count != CurrentBatch.Lots.Count) // All Lots Staged
            return;

        if (Sim.Now > nextRunnableTime)
            ProcessStart(CurrentBatch);
        else
            Sim.Delay(nextRunnableTime - Sim.Now, [() => { ProcessStart(CurrentBatch); }]);
    }

    protected override void ProcessStart(SimObject simObject)
    {
        Batch? batch = simObject as Batch;
        if (batch == null || batch.Lots.Count == 0)
        {
            LogHandler.Error("ProcessStart: empty batch");
            return;
        }

        Lot lead = batch.Lots[0];
        if (lead.CurrentStep == null)
        {
            LogHandler.Error($"ProcessStart: No Current Step {lead.Name}");
            return;
        }

        double processingTime = lead.GetProcessingTime();
        SimTime estimatedRunTime = Sim.Now + processingTime;

        foreach (var lot in batch.Lots)
        {
            this.StagedLots.Remove(lot);
            this.RunningLots.Add(lot, estimatedRunTime);
        }
        this.StagedLotsByBatch.Remove(batch);

        Sim.DelayUntil(estimatedRunTime, [() => { ProcessFinish(batch); }]);

        LogHandler.Debug($"{Sim.Now,-11:F1} | {Name,-21} | {batch.Name,-21} | ProcessStart ({batch.Lots.Count} lots)");

        nextRunnableTime = estimatedRunTime;

        // Call next Batch
        CurrentBatch = null;
        IsReserved = false;
        Sim.MES.RequestNextLot(this);
    }

    protected override void ProcessFinish(SimObject simObject)
    {
        Batch? batch = simObject as Batch;

        if(batch == null)
        {
            LogHandler.Error("ProcessStart: Wrong Sim Object Type");
            return;
        }

        LogHandler.Debug($"{Sim.Now,-11:F1} | {Name,-21} | {batch.Name,-21} | ProcessFinish");

        foreach (var lot in batch.Lots)
        {
            this.RunningLots.Remove(lot);
            this.FinishedLots.Add(lot);
        }

        Sim.Delay(UnloadingTime, [() => { UnloadFinish(FinishedLots[0]); }]);

        Batches.Remove(batch);
    }

    protected override void UnloadFinish(Lot lot)
    {
        base.UnloadFinish(lot);
        if(FinishedLots.Count > 0) // Recursive Unload
            Sim.Delay(UnloadingTime, [() => { UnloadFinish(FinishedLots[0]); }]);
    }
}
