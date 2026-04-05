using DTSim;
using DTSim.Core;

namespace DTSim.Demo;

public class Machine (Simulation<DemoHistory> sim, string name): SimNode<Simulation<DemoHistory>> (sim, name)
{
    private Distribution processingTime = new Poisson(5); // Average processing time of 5 seconds
    public int ProcessingJobId { get; private set; } = -1;
    public bool IsBusy { get; private set; } = false;

    public override void Initialize()
    {
        Console.WriteLine($"{this} initialized.");
    }

    public double GetProcessingTime()
    {
        return 10;
        //return new Random().NextDouble() * 5 + 1; // Random processing time between 1 and 6 seconds
    }

    public class ProcessJob(SimTime time, Machine machine, int jobId, List<Action>? callbacks) 
        : Event<Machine>(time, machine, callbacks)
    {
        public override void Execute()
        {
            Console.WriteLine($"{Node.Simulation.Now.TotalSeconds,-7} : {Node.Name} starts to process");
            double processingTime = Node.GetProcessingTime();
            Node.ProcessingJobId = jobId;
            Node.Simulation.Delay(processingTime, new List<Action> { ProcessFinished });
            Node.IsBusy = true;  
        }

        public void ProcessFinished()
        {
            Console.WriteLine($"{Node.Simulation.Now.TotalSeconds,-7} : {Node.Name} finishes processing job {Node.ProcessingJobId}");
            Node.IsBusy = false;
            Callbacks.ForEach(action => action());
        }
    }
}
