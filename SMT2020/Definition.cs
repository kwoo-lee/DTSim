public enum AreaType { Dry_Etch, Def_Met, Delay_32, Dielectric, Diffusion, Implant, Litho, Litho_Met, Planar, TF, TF_Met, Wet_Etch }
public enum ToolType { Table, Cascade, Batch, LotRelease }
public enum ProcessingUnit { Wafer, Lot, Batch }

public enum DispatchingRuleType
{
    FIFO,       // First In, First Out — 큐 도착 시간 순
    LIFO,       // Last In, First Out
    Priority,   // Lot 우선순위 높은 순
    SPT,        // Shortest Processing Time
    LPT,        // Longest Processing Time
    EDD,        // Earliest Due Date
    CR,         // Critical Ratio = 잔여시간 / 처리시간
    LeastSetup,
}