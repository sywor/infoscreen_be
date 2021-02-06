namespace Common.Bootstrap
{
    public interface IBootstrapService<T> where T : IRunnable
    {
        void Launch();
    }
}