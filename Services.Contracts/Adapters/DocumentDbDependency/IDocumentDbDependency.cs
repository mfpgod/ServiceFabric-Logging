namespace Services.Contracts.Adapters.DocumentDbDependency
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface IDocumentDbDependency : IService
    {
        Task<IList<UserEntity>> GetUsers();

        Task<UserEntity> GetUser(string id);

        Task<UserEntity> AddUser(UserEntity user);

        Task<UserEntity> UpdateUser(UserEntity user);

        Task DeleteUser(string id);
    }
}
