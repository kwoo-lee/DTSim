using SharpSim;

namespace SMT2020;

public static class Program
{
    public static void Main()
    {
        LogHandler.LogInfoHandle += System.Console.WriteLine;

        var evtList = new EventList();
        var fab = new Fab(evtList);
        fab.LoadData();

        fab.Run(86400 * 30);
    }
}
