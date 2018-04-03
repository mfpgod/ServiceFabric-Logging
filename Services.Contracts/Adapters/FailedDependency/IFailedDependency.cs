namespace Services.Contracts.Adapters.FailedDependency
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface IFailedDependency : IService
    {
        Task Fail();
    }
}
