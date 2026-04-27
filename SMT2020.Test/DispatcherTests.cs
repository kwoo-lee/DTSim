using SharpSim;

namespace SMT2020.Test;

public class DispatcherTests
{
    private readonly IDispatcher _dispatcher = new Dispatcher();
    private readonly SimTime _now = new SimTime(0.0);

    // ──────────────────────────────────────────────
    // Empty-input edge cases
    // ──────────────────────────────────────────────

    [Fact]
    public void Do_EmptyLotQueue_ReturnsEmpty()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);

        var result = _dispatcher.Do(_now, tg);

        Assert.Empty(result);
    }

    [Fact]
    public void Do_NoTools_ReturnsEmpty()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 0);
        tg.Enqueue(TestHelpers.CreateLot(tg, 1));

        var result = _dispatcher.Do(_now, tg);

        Assert.Empty(result);
    }

    [Fact]
    public void Do_AllToolsBreakdown_ReturnsEmpty()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);
        foreach (var t in tg.Tools) t.SetState(ToolState.Breakdown);
        tg.Enqueue(TestHelpers.CreateLot(tg, 1));

        var result = _dispatcher.Do(_now, tg);

        Assert.Empty(result);
    }

    [Fact]
    public void Do_AllToolsPM_ReturnsEmpty()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);
        foreach (var t in tg.Tools) t.SetState(ToolState.PM);
        tg.Enqueue(TestHelpers.CreateLot(tg, 1));

        var result = _dispatcher.Do(_now, tg);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────
    // Basic 1:1 matching shape
    // ──────────────────────────────────────────────

    [Fact]
    public void Do_SingleLotSingleTool_AssignsOneToOne()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1);
        var lot = TestHelpers.CreateLot(tg, 1);
        tg.Enqueue(lot);

        var result = _dispatcher.Do(_now, tg);

        Assert.Single(result);
        var (tool, lots) = Assert.Single(result);
        Assert.Same(tg.Tools[0], tool);
        Assert.Single(lots);
        Assert.Same(lot, lots[0]);
    }

    [Fact]
    public void Do_MoreLotsThanTools_AssignsTopLotsOnly()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);

        var l1 = TestHelpers.CreateLot(tg, 1, enqueueSec: 0);
        var l2 = TestHelpers.CreateLot(tg, 2, enqueueSec: 1);
        var l3 = TestHelpers.CreateLot(tg, 3, enqueueSec: 2);
        tg.Enqueue(l1, l2, l3);

        var result = _dispatcher.Do(_now, tg);

        Assert.Equal(2, result.Count);
        var assignedLots = result.SelectMany(kv => kv.Value).ToList();
        Assert.Contains(l1, assignedLots);
        Assert.Contains(l2, assignedLots);
        Assert.DoesNotContain(l3, assignedLots);
    }

    [Fact]
    public void Do_MoreToolsThanLots_OnlyAssignsAvailableLots()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 3);

        var l1 = TestHelpers.CreateLot(tg, 1);
        var l2 = TestHelpers.CreateLot(tg, 2);
        tg.Enqueue(l1, l2);

        var result = _dispatcher.Do(_now, tg);

        Assert.Equal(2, result.Count);
        Assert.All(result.Values, lots => Assert.Single(lots));
    }

    [Fact]
    public void Do_AssignsExactlyOneLotPerTool()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 3);
        for (int i = 1; i <= 5; i++) tg.LotQueue.Add(TestHelpers.CreateLot(tg, i));

        var result = _dispatcher.Do(_now, tg);

        Assert.All(result.Values, lots => Assert.Single(lots));
        Assert.Equal(3, result.Count);
    }

    // ──────────────────────────────────────────────
    // Tool availability filters
    // ──────────────────────────────────────────────

    [Fact]
    public void Do_BreakdownToolIsSkipped_HealthyToolGetsLot()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);
        tg.Tools[0].SetState(ToolState.Breakdown);

        var lot = TestHelpers.CreateLot(tg, 1);
        tg.Enqueue(lot);

        var result = _dispatcher.Do(_now, tg);

        Assert.Single(result);
        Assert.True(result.ContainsKey(tg.Tools[1]));
        Assert.False(result.ContainsKey(tg.Tools[0]));
    }

    [Fact]
    public void Do_PMToolIsSkipped_HealthyToolGetsLot()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);
        tg.Tools[1].SetState(ToolState.PM);

        var lot = TestHelpers.CreateLot(tg, 1);
        tg.Enqueue(lot);

        var result = _dispatcher.Do(_now, tg);

        Assert.Single(result);
        Assert.True(result.ContainsKey(tg.Tools[0]));
        Assert.False(result.ContainsKey(tg.Tools[1]));
    }

    [Fact]
    public void Do_IdleAndBusyTools_BothEligibleWhenPortsFree()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);
        tg.Tools[0].SetState(ToolState.Idle);
        tg.Tools[1].SetState(ToolState.Busy);

        var l1 = TestHelpers.CreateLot(tg, 1);
        var l2 = TestHelpers.CreateLot(tg, 2);
        tg.Enqueue(l1, l2);

        var result = _dispatcher.Do(_now, tg);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Do_ToolWithAllPortsTaken_IsSkipped()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);
        TestHelpers.FillTool(tg.Tools[0]);

        var lot = TestHelpers.CreateLot(tg, 1);
        tg.Enqueue(lot);

        var result = _dispatcher.Do(_now, tg);

        Assert.Single(result);
        Assert.True(result.ContainsKey(tg.Tools[1]));
        Assert.False(result.ContainsKey(tg.Tools[0]));
    }

    // ──────────────────────────────────────────────
    // Dispatching rules — ordering
    // ──────────────────────────────────────────────

    [Fact]
    public void Do_FifoRule_PicksEarliestEnqueueFirst()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1, rank1: DispatchingRuleType.FIFO);

        var late = TestHelpers.CreateLot(tg, 1, enqueueSec: 100);
        var early = TestHelpers.CreateLot(tg, 2, enqueueSec: 10);
        var mid = TestHelpers.CreateLot(tg, 3, enqueueSec: 50);
        tg.Enqueue(late, early, mid);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(early, Assert.Single(result.Values).Single());
    }

    [Fact]
    public void Do_LifoRule_PicksLatestEnqueueFirst()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1, rank1: DispatchingRuleType.LIFO);

        var late = TestHelpers.CreateLot(tg, 1, enqueueSec: 100);
        var early = TestHelpers.CreateLot(tg, 2, enqueueSec: 10);
        tg.Enqueue(early, late);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(late, Assert.Single(result.Values).Single());
    }

    [Fact]
    public void Do_PriorityRule_PicksHighestPriorityFirst()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1, rank1: DispatchingRuleType.Priority);

        var low = TestHelpers.CreateLot(tg, 1, priority: 1);
        var high = TestHelpers.CreateLot(tg, 2, priority: 99);
        var mid = TestHelpers.CreateLot(tg, 3, priority: 50);
        tg.Enqueue(low, high, mid);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(high, Assert.Single(result.Values).Single());
    }

    [Fact]
    public void Do_SptRule_PicksShortestProcessingFirst()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1, rank1: DispatchingRuleType.SPT);

        var slow = TestHelpers.CreateLot(tg, 1, processingSec: 500);
        var fast = TestHelpers.CreateLot(tg, 2, processingSec: 30);
        var mid = TestHelpers.CreateLot(tg, 3, processingSec: 100);
        tg.Enqueue(slow, fast, mid);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(fast, Assert.Single(result.Values).Single());
    }

    [Fact]
    public void Do_LptRule_PicksLongestProcessingFirst()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1, rank1: DispatchingRuleType.LPT);

        var slow = TestHelpers.CreateLot(tg, 1, processingSec: 500);
        var fast = TestHelpers.CreateLot(tg, 2, processingSec: 30);
        tg.Enqueue(fast, slow);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(slow, Assert.Single(result.Values).Single());
    }

    [Fact]
    public void Do_EddRule_PicksEarliestDueDateFirst()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1, rank1: DispatchingRuleType.EDD);

        var lateDue = TestHelpers.CreateLot(tg, 1, dueSec: 10_000);
        var earlyDue = TestHelpers.CreateLot(tg, 2, dueSec: 200);
        var midDue = TestHelpers.CreateLot(tg, 3, dueSec: 1_000);
        tg.Enqueue(lateDue, earlyDue, midDue);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(earlyDue, Assert.Single(result.Values).Single());
    }

    [Fact]
    public void Do_CrRule_PicksMostCriticallyOverdueFirst()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 1, rank1: DispatchingRuleType.CR);

        // CR = (due - now) / processing.  Smaller = more critical.
        // safe : (1000 - 0) / 100 = 10
        // tight: (200  - 0) / 100 = 2
        // crit : (50   - 0) / 100 = 0.5  ← most critical
        var safe = TestHelpers.CreateLot(tg, 1, processingSec: 100, dueSec: 1_000);
        var tight = TestHelpers.CreateLot(tg, 2, processingSec: 100, dueSec: 200);
        var crit = TestHelpers.CreateLot(tg, 3, processingSec: 100, dueSec: 50);
        tg.Enqueue(safe, tight, crit);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(crit, Assert.Single(result.Values).Single());
    }

    // ──────────────────────────────────────────────
    // Multi-level ranking tie-breakers
    // ──────────────────────────────────────────────

    [Fact]
    public void Do_PriorityTiesBrokenByFifo_AcrossSecondaryRule()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(
            fab,
            numberOfTools: 1,
            rank1: DispatchingRuleType.Priority,
            rank2: DispatchingRuleType.FIFO);

        // Same priority — FIFO breaks the tie (earlier enqueue wins).
        var laterSamePri = TestHelpers.CreateLot(tg, 1, priority: 50, enqueueSec: 100);
        var earlierSamePri = TestHelpers.CreateLot(tg, 2, priority: 50, enqueueSec: 10);
        var lowestPri = TestHelpers.CreateLot(tg, 3, priority: 1, enqueueSec: 0);
        tg.Enqueue(laterSamePri, lowestPri, earlierSamePri);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(earlierSamePri, Assert.Single(result.Values).Single());
    }

    [Fact]
    public void Do_TwoToolsTwoLots_OrderedByRule_HighestFirstToFirstTool()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2, rank1: DispatchingRuleType.Priority);

        var p1 = TestHelpers.CreateLot(tg, 1, priority: 1);
        var p9 = TestHelpers.CreateLot(tg, 2, priority: 9);
        var p5 = TestHelpers.CreateLot(tg, 3, priority: 5);
        tg.Enqueue(p1, p9, p5);

        var result = _dispatcher.Do(_now, tg);

        // First available tool (Tools[0]) gets the highest-priority lot.
        Assert.Same(p9, result[tg.Tools[0]].Single());
        Assert.Same(p5, result[tg.Tools[1]].Single());
        Assert.Equal(2, result.Count);
    }

    // ──────────────────────────────────────────────
    // ToolGroup type / area routing
    // ──────────────────────────────────────────────

    [Fact]
    public void Do_LithoArea_StillProducesOneToOneAssignment()
    {
        // PhotoLogic currently delegates to BaseLogic — verify Litho path is wired
        // and produces the same shape (single lot per available tool).
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(
            fab,
            numberOfTools: 2,
            area: AreaType.Litho,
            type: ToolType.Table,
            rank1: DispatchingRuleType.FIFO);

        var l1 = TestHelpers.CreateLot(tg, 1, enqueueSec: 0);
        var l2 = TestHelpers.CreateLot(tg, 2, enqueueSec: 1);
        tg.Enqueue(l1, l2);

        var result = _dispatcher.Do(_now, tg);

        Assert.Equal(2, result.Count);
        Assert.All(result.Values, lots => Assert.Single(lots));
    }

    [Fact]
    public void Do_BatchToolType_FollowsBaseLogic()
    {
        // Even when Toolype == Batch, the dispatcher currently routes through BaseLogic.
        // Pin that contract so a future rewrite to use BatchLogic surfaces here.
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(
            fab,
            numberOfTools: 1,
            type: ToolType.Batch,
            rank1: DispatchingRuleType.Priority);

        var low = TestHelpers.CreateLot(tg, 1, priority: 1);
        var high = TestHelpers.CreateLot(tg, 2, priority: 9);
        tg.Enqueue(low, high);

        var result = _dispatcher.Do(_now, tg);

        Assert.Same(high, Assert.Single(result.Values).Single());
    }

    // ──────────────────────────────────────────────
    // Result-shape invariants
    // ──────────────────────────────────────────────

    [Fact]
    public void Do_AssignmentsAreUniqueLotsAcrossTools()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 3);
        for (int i = 1; i <= 5; i++) tg.LotQueue.Add(TestHelpers.CreateLot(tg, i));

        var result = _dispatcher.Do(_now, tg);

        var allAssigned = result.SelectMany(kv => kv.Value).ToList();
        Assert.Equal(allAssigned.Count, allAssigned.Distinct().Count());
    }

    [Fact]
    public void Do_DoesNotMutateLotQueue()
    {
        var fab = TestHelpers.CreateFab();
        var tg = TestHelpers.CreateToolGroup(fab, numberOfTools: 2);
        for (int i = 1; i <= 3; i++) tg.LotQueue.Add(TestHelpers.CreateLot(tg, i));
        var before = tg.LotQueue.ToList();

        _dispatcher.Do(_now, tg);

        Assert.Equal(before, tg.LotQueue);
    }
}
