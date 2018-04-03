namespace Services.Contracts.Adapters.ServiceBusDependency
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface IServiceBusDependency : IService
    {
        Task QueueRequest(QueueRequest id);
    }
}
