namespace Adapters.DocumentDbDependency
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Services.Contracts.Adapters.DocumentDbDependency;
    using Shared.DocumentDb;
    using Shared.Logging;
    using Shared.ServiceFabric;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DocumentDbDependency : StatelessService, IDocumentDbDependency
    {
        private IDocumentDbClientFactory documentDbClientFactory;

        public DocumentDbDependency(StatelessServiceContext context)
            : base(context)
        {
        }

        public async Task<UserEntity> GetUser(string id)
        {
            var docDbRepository = new UserRepository(documentDbClientFactory);
            DbResponse<UserEntity> response = await docDbRepository.QuerySingleAsync(x => x.Id == id);
            return response.Entity;
        }

        public async Task<IList<UserEntity>> GetUsers()
        {
            var docDbRepository = new UserRepository(documentDbClientFactory);
            DbListResponse<UserEntity> response = await docDbRepository.QueryAsync();
            return response.Entities.ToList();
        }

        public async Task<UserEntity> AddUser(UserEntity user)
        {
            var docDbRepository = new UserRepository(documentDbClientFactory);
            DbResponse<UserEntity> response = await docDbRepository.CreateAsync(user);
            return response.Entity;
        }

        public async Task<UserEntity> UpdateUser(UserEntity user)
        {
            var docDbRepository = new UserRepository(documentDbClientFactory);
            DbResponse<UserEntity> response = await docDbRepository.UpsertAsync(user);
            return response.Entity;
        }

        public async Task DeleteUser(string id)
        {
            var docDbRepository = new UserRepository(documentDbClientFactory);
            await docDbRepository.DeleteAsync(id);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[1]
            {
                ServiceFabricHelper.CreateServiceInstanceListenerWithCorrelation(this)
            };

            // return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                // init service fabric context
                ServiceFabricServiceContext.Set(Context);

                // initailize DocumentDb client factory
                var config = new DocumentDbConnectionConfig();
                documentDbClientFactory = new DocumentDbClientFactory(config);

                // initilize the DOcument database & collection
                await UserCollectionInitializer.Initialize(documentDbClientFactory);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LoggingContext.CreateLogger<DocumentDbDependency>()
                    .LogError(ex, "Service exception in RunAsync");
            }
        }
    }
}
