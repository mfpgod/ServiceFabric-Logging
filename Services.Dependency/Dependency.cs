namespace Services.Dependency
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Fabric;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Services.Contracts.Adapters.DocumentDbDependency;
    using Services.Contracts.Adapters.FailedDependency;
    using Services.Contracts.Adapters.HttpDependency;
    using Services.Contracts.Adapters.ServiceBusDependency;
    using Services.Contracts.Services.Dependency;
    using Shared;
    using Shared.Logging;
    using Shared.ServiceFabric;

    public class Dependency : StatelessService, IDependency
    {
        private IFailedDependency failedDependencyClient;
        private IHttpDependency httpDependencyClient;
        private IServiceBusDependency serviceBusDependencyClient;
        private IDocumentDbDependency documentDbDependencyClient;

        public Dependency(StatelessServiceContext context)
            : base(context)
        {
            string adapterFailedDependencyAddress = ConfigurationManager.AppSettings["AdapterFailedDependencyAddress"];
            failedDependencyClient = ServiceFabricHelper.CreateServiceProxyWithCorrelation<IFailedDependency>(adapterFailedDependencyAddress);

            string adapterDependencyAddress = ConfigurationManager.AppSettings["AdapterHttpDependencyAddress"];
            httpDependencyClient = ServiceFabricHelper.CreateServiceProxyWithCorrelation<IHttpDependency>(adapterDependencyAddress);

            string adapterServiceBusDependencyAddress = ConfigurationManager.AppSettings["AdapterServiceBusDependencyAddress"];
            serviceBusDependencyClient = ServiceFabricHelper.CreateServiceProxyWithCorrelation<IServiceBusDependency>(adapterServiceBusDependencyAddress);

            string adapterDocumentDbDependencyAddress = ConfigurationManager.AppSettings["AdapterDocumentDbDependencyAddress"];
            documentDbDependencyClient = ServiceFabricHelper.CreateServiceProxyWithCorrelation<IDocumentDbDependency>(adapterDocumentDbDependencyAddress);
        }

        public async Task<IList<User>> GetUsers()
        {
            IList<UserEntity> docDbUsers = await documentDbDependencyClient.GetUsers();
            IEnumerable<User> dbUsers = docDbUsers.Select(x => new User
            {
                Id = x.Id,
                Name = x.Name,
                Processed = x.Processed
            });

            IList<UserResponse> serviceUsers = await httpDependencyClient.GetUsers();
            IEnumerable<User> httpUsers = serviceUsers.Select(x => new User
            {
                Id = x.Id,
                Name = x.Name
            });

            return httpUsers.Union(dbUsers).ToList();
        }

        public async Task<User> GetUser(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
            {
                UserResponse user = await httpDependencyClient.GetUser(id);
                return new User
                {
                    Id = user.Id,
                    Name = user.Name
                };
            }
            else
            {
                UserEntity user = await documentDbDependencyClient.GetUser(id);
                return new User
                {
                    Id = user.Id,
                    Name = user.Name,
                    Processed = user.Processed
                };
            }
        }

        public async Task<User> AddUser(User user)
        {
            UserEntity dbUser = await documentDbDependencyClient.AddUser(new UserEntity
            {
                Id = Guid.NewGuid().ToString(),
                Name = user.Name
            });

            await serviceBusDependencyClient.QueueRequest(new QueueRequest
            {
                Id = dbUser.Id,
                Name = dbUser.Name
            });

            return new User
            {
                Id = dbUser.Id,
                Name = dbUser.Name,
                Processed = dbUser.Processed
            };
        }

        public async Task<User> UpdateUser(User user)
        {
            if (!Guid.TryParse(user.Id, out Guid guid))
            {
                throw new BusinessException($"User with id='{user.Id}' could not be updated");
            }

            UserEntity dbUser = await documentDbDependencyClient.GetUser(user.Id);
            if (dbUser == null)
            {
                throw new BusinessException(string.Format($"User with id='{user.Id}' could not be found"));
            }

            dbUser.Name = user.Name;
            await documentDbDependencyClient.UpdateUser(dbUser);

            return new User
            {
                Id = dbUser.Id,
                Name = dbUser.Name,
                Processed = dbUser.Processed
            };
        }

        public async Task DeleteUser(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
            {
                throw new BusinessException($"User with id='{id}' could not be deleted");
            }

            await documentDbDependencyClient.DeleteUser(id);
        }

        public async Task Fail()
        {
            await failedDependencyClient.Fail();
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[1]
            {
                ServiceFabricHelper.CreateServiceInstanceListenerWithCorrelation(this)
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                // init service fabric context
                ServiceFabricServiceContext.Set(Context);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LoggingContext.CreateLogger<Dependency>()
                    .LogError(ex, "Service exception in RunAsync");
            }
        }
    }
}
