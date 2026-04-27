using SharpSim;

namespace SMT2020;

public class Dispatcher : IDispatcher
{
    public Dictionary<Tool, List<Lot>> Do(SimTime now, ToolGroup toolGroup)
    {
        if (toolGroup.Toolype != ToolType.Batch)
        {
            return toolGroup.AreaType == AreaType.Litho
                ? PhotoLogic(now, toolGroup)
                : BaseLogic(now, toolGroup);
        }
        else
            return BaseLogic(now, toolGroup);
    }

    /// <summary>
    /// 
    /// 기본 단일-Lot 디스패칭 로직.
    /// 1. 가용 Tool 선별 (Breakdown/PM 제외)
    /// 2. LotQueue를 DispatchingRuleSet(Main→Ranking1→2→3)으로 정렬
    /// 3. 가용 Tool과 정렬된 Lot을 순서대로 1:1 매칭
    /// </summary>
    private static Dictionary<Tool, List<Lot>> BaseLogic(SimTime now, ToolGroup toolGroup)
    {
        var result = new Dictionary<Tool, List<Lot>>();

        if (toolGroup.LotQueue.Count == 0)
            return result;

        // 가용 Tool: Breakdown/PM이 아닌 것, 현재 Track-In 중이지 않은 것
        var availTools = toolGroup.Tools
            .Where(t => t.State is not (ToolState.Breakdown or ToolState.PM) && t.AssignedLots.Count < t.Ports.Count)
            .ToList();

        if (availTools.Count == 0)
            return result;

        // DispatchingRuleSet으로 Lot 우선순위 정렬
        var sortedLots = toolGroup.DispatchingRuleSet.Sort([.. toolGroup.LotQueue], now);

        // 가용 Tool 순서대로 최우선 Lot 배정 (1 Tool : 1 Lot)
        int lotIdx = 0;
        foreach (var tool in availTools)
        {
            if (lotIdx >= sortedLots.Count) break;
            result[tool] = [sortedLots[lotIdx++]];
        }

        return result;
    }

    private static Dictionary<Tool, List<Lot>> PhotoLogic(SimTime now, ToolGroup toolGroup)
    {
        // TBD: Litho 전용 로직 (Setup 고려 등)
        return BaseLogic(now, toolGroup);
    }

    private static Dictionary<Tool, List<Lot>> BatchLogic(SimTime now, ToolGroup toolGroup)
    {
        // TBD: Batch 묶음 로직
        return [];
    }
}
