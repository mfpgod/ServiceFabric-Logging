namespace Services.Contracts.Services.Dependency
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface IDependency : IService
    {
        Task<IList<User>> GetUsers();

        Task<User> GetUser(string id);

        Task<User> AddUser(User id);

        Task<User> UpdateUser(User id);

        Task DeleteUser(string id);

        Task Fail();
    }
}
