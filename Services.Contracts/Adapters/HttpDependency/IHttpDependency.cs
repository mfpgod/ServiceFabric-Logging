namespace Services.Contracts.Adapters.HttpDependency
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface IHttpDependency : IService
    {
        Task<IList<UserResponse>> GetUsers();

        Task<UserResponse> GetUser(string id);
    }
}
