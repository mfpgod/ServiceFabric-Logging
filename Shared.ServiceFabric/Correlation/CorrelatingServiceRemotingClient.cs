namespace Shared.ServiceFabric.Correlation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.ServiceFabric.Services.Remoting.V1;
    using Microsoft.ServiceFabric.Services.Remoting.V1.Client;
    using Shared.Logging;

    /// <summary>
    /// Service remoting client that wraps another service remoting client and adds correlation id and context propagation support. This allows
    /// traces the client and the service to log traces with the same relevant correlation id and context.
    /// </summary>
    internal class CorrelatingServiceRemotingClient : IServiceRemotingClient, IWrappingClient
    {
        private Lazy<DataContractSerializer> baggageSerializer = new Lazy<DataContractSerializer>(() => new DataContractSerializer(typeof(IEnumerable<KeyValuePair<string, string>>)));

        private Uri serviceUri;
        private IMethodNameProvider methodNameProvider;

        public CorrelatingServiceRemotingClient(IServiceRemotingClient innerClient, Uri serviceUri, IMethodNameProvider methodNameProvider)
        {
            InnerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));

            this.serviceUri = serviceUri ?? throw new ArgumentNullException(nameof(serviceUri));
            this.methodNameProvider = methodNameProvider;
        }

        public ResolvedServicePartition ResolvedServicePartition { get => InnerClient.ResolvedServicePartition; set => InnerClient.ResolvedServicePartition = value; }

        public string ListenerName { get => InnerClient.ListenerName; set => InnerClient.ListenerName = value; }

        public ResolvedServiceEndpoint Endpoint { get => InnerClient.Endpoint; set => InnerClient.Endpoint = value; }

        public IServiceRemotingClient InnerClient { get; private set; }

        public Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            return SendAndTrackRequestAsync(messageHeaders, requestBody, () => InnerClient.RequestResponseAsync(messageHeaders, requestBody));
        }

        public void SendOneWay(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            SendAndTrackRequestAsync(messageHeaders, requestBody, () =>
            {
                InnerClient.SendOneWay(messageHeaders, requestBody);
                return Task.FromResult<byte[]>(null);
            }).Forget();
        }

        private async Task<byte[]> SendAndTrackRequestAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody, Func<Task<byte[]>> doSendRequest)
        {
            string methodName = methodNameProvider.GetMethodName(messageHeaders.InterfaceId, messageHeaders.MethodId);
            if (string.IsNullOrEmpty(methodName))
            {
                methodName = messageHeaders.MethodId.ToString();
            }

            if (!messageHeaders.ContainsHeader(ServiceRemotingStrings.SourceHeaderName))
            {
                messageHeaders.AddHeader(ServiceRemotingStrings.SourceHeaderName, ServiceFabricServiceContext.Current?.Name?.ToString());
            }

            if (!messageHeaders.ContainsHeader(ServiceRemotingStrings.CorrelationContextHeaderName) && Activity.Current != null)
            {
                Activity currentActivity = Activity.Current;
                if (currentActivity.Baggage.Any())
                {
                    using (var ms = new MemoryStream())
                    {
                        var dictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(ms);
                        baggageSerializer.Value.WriteObject(dictionaryWriter, currentActivity.Baggage);
                        dictionaryWriter.Flush();
                        messageHeaders.AddHeader(ServiceRemotingStrings.CorrelationContextHeaderName, ms.ToArray());
                    }
                }
            }

            Uri operationUri = ServiceFabricHelper.MakeOperationUri(serviceUri, methodName);
            string operationName = ServiceFabricHelper.MakeOperationName(serviceUri, methodName);
            string serviceName = ServiceFabricHelper.GetServiceName(serviceUri);

            // activity is mainly required here to generate the correct dependency id
            Activity activity = new Activity(operationName);
            activity.Start();

            // this step should be after activity.id is initialized - it happens after starting the activity
            if (!messageHeaders.ContainsHeader(ServiceRemotingStrings.ParentIdHeaderName))
            {
                messageHeaders.AddHeader(ServiceRemotingStrings.ParentIdHeaderName, activity.Id);
            }

            bool success = true;
            try
            {
                return await doSendRequest().ConfigureAwait(false);
            }
            catch (AggregateException ax)
            {
                success = false;
                // SF rethrow exception that is returned from remoting operation and wraps it into AggregateException
                throw new InternalDependencyException(serviceName, operationUri.ToString(), ax.InnerException);
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                // cleaning the activity
                activity.Stop();
                var dependencyLog = new DependencyLog
                {
                    Name = activity.OperationName,
                    Type = ServiceFabricHelper.RemotingTypeName,
                    Uri = operationUri,
                    Target = serviceName,
                    Success = success
                };
                LoggingContext.CreateLogger<CorrelatingServiceRemotingClient>().LogDependency(dependencyLog, activity);
            }
        }
    }
}
