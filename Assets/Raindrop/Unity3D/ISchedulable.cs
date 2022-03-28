namespace Raindrop.Unity3D
{
    // allow periodic tasks to be done by implementing this and registering the instance to the scheduler.
    public interface ISchedulable
    {
        public bool canRun();
        public void Run();
    }
}