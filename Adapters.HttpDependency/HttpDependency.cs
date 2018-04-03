namespace Adapters.HttpDependency
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Fabric;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Newtonsoft.Json;
    using Services.Contracts.Adapters.HttpDependency;
    using Shared.Logging;
    using Shared.ServiceFabric;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class HttpDependency : StatelessService, IHttpDependency
    {
        public HttpDependency(StatelessServiceContext context)
            : base(context)
        {
        }

        public async Task<IList<UserResponse>> GetUsers()
        {
            using (var client = new HttpClient(new HttpClientLoggingHandler(new HttpClientHandler())))
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["BaseUri"]);

                // users endpoint
                HttpResponseMessage httpResponse = await client.GetAsync("/users");
                if (httpResponse.IsSuccessStatusCode)
                {
                    string jsonString = await httpResponse.Content.ReadAsStringAsync();
                    List<UserResponse> users = JsonConvert.DeserializeObject<List<UserResponse>>(jsonString);

                    return users;
                }

                throw new Exception("TODO");
            }
        }

        public async Task<UserResponse> GetUser(string id)
        {
            using (var client = new HttpClient(new HttpClientLoggingHandler(new HttpClientHandler())))
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["BaseUri"]);

                // users endpoint
                HttpResponseMessage httpResponse = await client.GetAsync($"/users/{id}");
                if (httpResponse.IsSuccessStatusCode)
                {
                    string jsonString = await httpResponse.Content.ReadAsStringAsync();
                    UserResponse user = JsonConvert.DeserializeObject<UserResponse>(jsonString);

                    return user;
                }

                throw new Exception("TODO");
            }
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
                LoggingContext.CreateLogger<HttpDependency>()
                    .LogError(ex, "Service exception in RunAsync");
            }
        }
    }
}
