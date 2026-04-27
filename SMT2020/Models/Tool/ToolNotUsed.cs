using SharpSim;

namespace SMT2020;

public class ToolNotUsed
{
    #region [PM]
    // public PM ReservedPM {get; private set;}
    // public Dictionary<string, int> WafersLimitForPM => _wafersLimitForPM;
    // public Dictionary<string, int> WafersCountForPM => _wafersCountForPM;
    // public double TotalValueAddedTime => _totalValueAddedTime;
    // public double TotalDownTime => _totalDownTime;
    // public SimTime LastProcessEndTime => _lastProcessEndTime;
    // public SimTime ExpectedProcessEndTime => _expectedProcessEndTime;
    // protected Dictionary<string, int> _wafersLimitForPM = new();
    // protected Dictionary<string, int> _wafersCountForPM = new();
    // protected bool _isProcessingBreakdown;

    // protected void CountProcessingWafers()
    // {
    //     if (_wafersLimitForPM.Count > 0)
    //     {
    //         int waferCount = this.Entities.Sum(l => ((Lot)l).WafersPerLot);
    //         foreach (string pmName in _wafersCountForPM.Keys.ToArray())
    //             _wafersCountForPM[pmName] += waferCount;

    //         PM nextPM = null;
    //         foreach (string pmName in _wafersCountForPM.Keys.ToArray())
    //         {
    //             if (_wafersCountForPM[pmName] > _wafersLimitForPM[pmName])
    //             {
    //                 PM tempPM = this.Spec.PMs[pmName];
    //                 _wafersCountForPM[pmName] = 0;
    //                 _wafersLimitForPM[pmName] = (int)tempPM.MTBPM.GetNumber();
    //                 if (nextPM is null || nextPM.MTTR.GetNumber() < tempPM.MTTR.GetNumber())
    //                     nextPM = tempPM;
    //             }
    //         }
    //     }
    // }

    // public void SetReservedPM(string pmName)
    // {
    //     if (!string.IsNullOrEmpty(pmName))
    //     {
    //         var toolgroup = ToolGroup as ToolGroup;
    //         if (toolgroup?.Spec.PMs.ContainsKey(pmName) == true)
    //             _reservedPM = toolgroup.Spec.PMs[pmName];
    //         else
    //             throw new Exception("Wrong Toolgroup");
    //     }
    // }
// protected void PMStart(SimTime timeNow, SimPort port)
    // {
    //     string pmName = (string)port.Data;
    //     PM pm = this.Spec.PMs[pmName];
    //     switch (this.ToolState)
    //     {
    //         case ToolState.Idle:
    //             this.SetState(timeNow, ToolState.PM);
    //             SimTime pmFinishTime = timeNow + pm.MTTR.GetNumber();
    //             EvtCalendar.AddEvent(pmFinishTime, this, new SimPort(EQPIntPort.PMFinish) { Data = pmName });
    //             foreach (var LoadPort in _ports)
    //                 CancelReservedProcess(LoadPort, timeNow);
    //             break;
    //         case ToolState.Reserved:
    //         case ToolState.Busy:
    //         case ToolState.SetUp:
    //             if (pm.Type != PMType.TimeBased)
    //                 LogHandler.Error($"{timeNow,-7} / {this.Name} / {this.ToolState} / Impossible PM");
    //             if (_reservedPM is null)
    //                 _reservedPM = pm;
    //             else if (pm.MTTR.GetNumber() > _reservedPM.MTTR.GetNumber())
    //             {
    //                 EvtCalendar.AddEvent(timeNow + _reservedPM.MTBPM.GetNumber(), this, new SimPort(EQPIntPort.PMStart) { Data = _reservedPM.Name });
    //                 _reservedPM = pm;
    //             }
    //             else
    //                 EvtCalendar.AddEvent(timeNow + pm.MTBPM.GetNumber(), this, new SimPort(EQPIntPort.PMStart) { Data = pmName });
    //             break;
    //         case ToolState.PM:
    //         case ToolState.Breakdown:
    //             if (pm.Type is PMType.TimeBased)
    //                 EvtCalendar.AddEvent(timeNow + pm.MTBPM.GetNumber(), this, new SimPort(EQPIntPort.PMStart) { Data = pmName });
    //             else if (pm.Type is PMType.CounterBased)
    //                 LogHandler.Error($"{timeNow,-7} / {this.Name} / {this.ToolState} / Cannot be counter PM during PM ToolState");
    //             break;
    //         default:
    //             LogHandler.Error($"{timeNow,-7} / {this.Name} / {this.ToolState} / M(PMStart): Not Implemented ToolState");
    //             break;
    //     }
    // }

    // protected void PMFinish(SimTime timeNow, SimPort port)
    // {
    //     if (ToolState is ToolState.PM)
    //     {
    //         var pmName = (string)port.Data;
    //         PM pm = this.Spec.PMs[pmName];
    //         this.SetState(timeNow, ToolState.Idle);

    //         if (pm.Type is PMType.TimeBased)
    //         {
    //             var nextPmStart = timeNow + pm.MTBPM.GetNumber();
    //             EvtCalendar.AddEvent(nextPmStart, this, new SimPort(EQPIntPort.PMStart) { Data = pmName });
    //         }
    //         else if (pm.Type is PMType.CounterBased)
    //             _wafersCountForPM[pmName] = 0;

    //         // _toolGroup 미선언으로 인한 주석처리
    //         // _toolGroup.ExternalTransition(timeNow, new SimPort(TGExtPort.RequestDispatching));
    //     }
    //     else
    //     {
    //         LogHandler.Error($"{timeNow,-7} / {this.Name} / {this.ToolState} / PM Finish ToolState Error!");
    //     }
    // }

    // protected void BreakdownStart(SimTime timeNow, SimPort port)
    // {
    //     Breakdown breakdown = this.Spec.Breakdown;
    //     switch (this.ToolState)
    //     {
    //         case ToolState.Idle:
    //             this.SetState(ToolState.Breakdown);
    //             EvtCalendar.AddEvent(timeNow + breakdown.MTTR.GetNumber(), this, new SimPort(EQPIntPort.Repair));
    //             foreach (var LoadPort in _ports)
    //                 CancelReservedProcess(LoadPort, timeNow);
    //             break;
    //         case ToolState.Busy:
    //             this.SetState(ToolState.Breakdown);
    //             LogHandler.Error($"{timeNow,-7} / {this.Name} / {this.ToolState} / Breakdown Busy No Remaining Time Error!");
    //             _isProcessingBreakdown = true;
    //             EvtCalendar.AddEvent(timeNow + breakdown.MTTR.GetNumber(), this, new SimPort(EQPIntPort.Repair));
    //             break;
    //         case ToolState.Breakdown:
    //             LogHandler.Error($"{timeNow,-7} / {this.Name} / {this.ToolState} / Duplicate failures cannot occur.");
    //             break;
    //         case ToolState.PM:
    //             EvtCalendar.AddEvent(timeNow + breakdown.MTTF.GetNumber(), this, port);
    //             break;
    //         case ToolState.SetUp:
    //             break;
    //         default:
    //             break;
    //     }
    // }
    
    // protected void Repair()
    // {
    //     if (this.ToolState is ToolState.Breakdown)
    //     {
    //         if (_isProcessingBreakdown)
    //             this.SetState(timeNow, ToolState.Busy);
    //         else
    //             this.SetState(timeNow, ToolState.Idle);

    //         Breakdown breakdown = this.Spec.Breakdown;
    //         EvtCalendar.AddEvent(timeNow + breakdown.MTTF.GetNumber(), this, new SimPort(EQPIntPort.Breakdown));

    //         _isProcessingBreakdown = false;
    //     }
    //     else
    //     {
    //         LogHandler.Error($"{timeNow,-7} / {this.Name} / {this.ToolState} / Repair ToolState Error!");
    //     }
    // }
    #endregion

    #region [Setup]
    protected string _lastLotName = "";
    public string LastLotName => _lastLotName;
    public string CurrentSetUpName { get; private set;}
    // protected virtual void SetUpFinish() =>
    //     throw new NotImplementedException();

    // protected void CancelReservedProcess(LoadPort port, SimTime timeNow)
    // {
    //     if (port.Entities.Any())
    //     {
    //         var foup = port.Entities[0] as Foup;
    //         if (foup?.IsEmpty == false)
    //         {
    //             // _toolGroup 미선언으로 인한 주석처리
    //             // _toolGroup.ExternalTransition(timeNow, new SimPort(TGExtPort.CancelReservation, this, foup));
    //         }
    //     }
    // }

    // protected bool CheckToSetUp(SetUp setUp)
    // {
    //     if (setUp is null) return false;
    //     return _currentSetUpName is null
    //         || setUp.Timing is SetUpTiming.Always
    //         || (setUp.Timing is SetUpTiming.Need && setUp.Name != _currentSetUpName);
    // }
    #endregion

    #region KPI
    protected double _totalValueAddedTime;
    protected double _totalDownTime;
    // protected int _reworkCount;
    protected SimTime _lastProcessEndTime;
    protected SimTime _expectedProcessEndTime;
    #endregion

    #region KPI Method

    public void SetProcessEndTime(double expectedProcessEndTime, double lastProcessEndTime)
    {
        _expectedProcessEndTime = new SimTime(expectedProcessEndTime);
        _lastProcessEndTime = new SimTime(lastProcessEndTime);
    }
    public double CalcUtilization(SimTime timeNow)
    {
        double valueAddedTime = _totalValueAddedTime;
        return Math.Truncate(10000 * valueAddedTime / (timeNow.TotalSeconds - _totalDownTime)) / 100;
    }

    public double CalcAvailability(SimTime timeNow)
    {
        return Math.Truncate(10000 * (timeNow.TotalSeconds - _totalDownTime) / timeNow.TotalSeconds) / 100;
    }
    public void SetEqpKPIBySnapshot(double totalValueAddedTimes, double totalDownTimes)
    {
        _totalValueAddedTime = totalValueAddedTimes;
        _totalDownTime = totalDownTimes;
    }
    #endregion

}
