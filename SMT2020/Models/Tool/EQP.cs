// using SharpSim;

// namespace SMT2020;

// public class EQP(Fab fab, FabHistory hist, int id, string name, ToolType type) 
// : SimNode<Fab, FabHistory>(fab, hist, id, name)
// {
// #region Definitions

//     public enum State { NotUsed, Idle, Busy, PM, Breakdown }
// #endregion

//     protected ToolType Type { get; private set; } = type;
//     protected bool IsReserved { get; set;}

//     // Lots by status : TBD 하나로 합치기
//     protected List<Lot> lots = new List<Lot>();
//     protected List<Lot> assignedLots = new List<Lot>();
//     protected List<Lot> loadingLots = new List<Lot>();
//     protected List<Lot> loadedLots = new List<Lot>();
//     protected Dictionary<Lot, SimTime> processingLots = new Dictionary<Lot, SimTime>();
//     protected Dictionary<Lot, SimTime> pausedLots = new Dictionary<Lot, SimTime>();
//     protected List<Lot> unloadingLots = new List<Lot>();
//     // Lots by status

//     protected string lastRecipeName = "";
//     protected string lastProductName = "";
//     protected SimTime lastProcessStartTime;
//     protected SimTime lastProcessFinishTime;
//     protected SimTime nextLotIntervalTime;
//     protected int trainSize = 0;

//     public override void Initialize()
//     {
//         base.Initialize();
//         IsReserved = false;
//         SetState(State.Idle);
//     }

//     public override void SetState(Enum newState)
//     {
//         var prevState = this.state;
//         base.SetState(newState);
//     }
    
//     protected double GetProcessTime(Step step)
//     {
//         // TBD
//         return 10;
//     }

//     protected double GetIntervalTime(Step step)
//     {
//         // TBD
//         return 10;
//     }

//     protected double GetSetupTime(Step step)
//     {
//         return 32.7;
//     }
//     #region Events
//     public void Reserve(Lot lot)
//     {
//         assignedLots.Add(lot);
//         IsReserved = true;
//     }

//     protected double loadTime = 0;
//     protected virtual void LoadFinished(Lot lot)
//     {
//         throw new NotImplementedException(); // Will be implemented in Concrete Classes
//     }
    
//     protected virtual void ProcessStart(SimObject simObj)
//     {
//         throw new NotImplementedException(); // Will be implemented in Concrete Classes
//     }

//     protected virtual void ProcessFinished(SimObject simObj)
//     {
//         throw new NotImplementedException(); // Will be implemented in Concrete Classes
//     }

//     /// Common for all EQP Types
//     protected void CompleteLot(Lot lot)
//     {
//         lot.ProcessEndTime = Simulation.Now;
//         lot.State = Lot.LotStatus.Processed;
//         processingLots.Remove(lot);
//         unloadingLots.Add(lot);

//         #region [TBD: Lot Logging]
        
//         // Tool Level Log

//         // Recipe Level Log
//         #endregion
//     }

//     protected double unloadTime = 600;
//     protected virtual void UnloadFinished(SimObject simObj)
//     { }

//     protected virtual void CallNextLot()
//     {
//         throw new NotImplementedException(); // Will be implemented in Concrete Classes
//     }

//     protected virtual void SetupFinished(SimObject simObj, double setupTime)
//     {
        
//     }
//     internal class LotEvent(SimTime time, Tool node, Lot lot, List<Action>? callbacks) : Event<Tool>(time, node, callbacks)
//     {
//         protected Lot lot = lot;
//         public override void Execute()
//         {
//             throw new NotImplementedException();
//         }
//     }

//     internal class EvtLoad(SimTime time, Tool node, Lot lot) : LotEvent(time, node, lot, null)
//     {
//         public override void Execute()
//         {
//             double loadTime = 0;
//             Node.Simulation.Delay(loadTime, new List<Action> { () => Node.LoadFinished(this.lot) });
//         }
//     }
//     #endregion
// }