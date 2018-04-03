namespace Shared.ServiceFabric.Correlation
{
    using System.Threading.Tasks;

    /// <summary>
    /// Class with extension helper methods for working with the Task class
    /// </summary>
    internal static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
        }
    }
}
