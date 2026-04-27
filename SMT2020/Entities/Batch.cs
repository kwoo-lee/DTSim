namespace SMT2020;

public class Batch(int id, string name) : SharpSim.SimObject(id, name)
{
    public List<Lot> Lots { get; private set; } = new List<Lot>();
    //public double TotalWaferCount => Lots.Sum(lot => lot.WafersPerLot);
}