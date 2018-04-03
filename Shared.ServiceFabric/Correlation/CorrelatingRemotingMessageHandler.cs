namespace Shared.ServiceFabric.Correlation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.V1;
    using Microsoft.ServiceFabric.Services.Remoting.V1.Runtime;
    using Shared.Logging;

    /// <summary>
    /// Service remoting handler that wraps a service and parses correlation id and context, if they have been passed by the caller as
    /// message headers. This allows traces the client and the service to log traces with the same relevant correlation id and context.
    /// </summary>
    public class CorrelatingRemotingMessageHandler : IServiceRemotingMessageHandler, IDisposable
    {
        private readonly Lazy<DataContractSerializer> baggageSerializer = new Lazy<DataContractSerializer>(() => new DataContractSerializer(typeof(IEnumerable<KeyValuePair<string, string>>)));

        private IServiceRemotingMessageHandler innerHandler;
        private MethodNameProvider methodNameProvider;
        private ServiceContext serviceContext;

        public CorrelatingRemotingMessageHandler(ServiceContext serviceContext, IService service)
        {
            this.serviceContext = serviceContext;

            // Populate our method name provider with methods from the IService interfaces
            methodNameProvider = new MethodNameProvider(false /* threadSafe */);
            methodNameProvider.AddMethodsForProxyOrService(service.GetType().GetInterfaces(), typeof(IService));

            innerHandler = new ServiceRemotingDispatcher(serviceContext, service);
        }

        public void HandleOneWay(IServiceRemotingRequestContext requestContext, ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            HandleAndTrackRequestAsync(messageHeaders, () =>
            {
                innerHandler.HandleOneWay(requestContext, messageHeaders, requestBody);
                return Task.FromResult<byte[]>(null);
            }).Forget();
        }

        public Task<byte[]> RequestResponseAsync(IServiceRemotingRequestContext requestContext, ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            return HandleAndTrackRequestAsync(messageHeaders, () => innerHandler.RequestResponseAsync(requestContext, messageHeaders, requestBody));
        }

        private async Task<byte[]> HandleAndTrackRequestAsync(ServiceRemotingMessageHeaders messageHeaders, Func<Task<byte[]>> doHandleRequest)
        {
            // set service context
            ServiceFabricServiceContext.Set(serviceContext);

            // create logger per operation
            ILogger logger = LoggingContext.CreateLogger<CorrelatingRemotingMessageHandler>();

            // Do our best effort in setting the request name.
            string methodName = methodNameProvider.GetMethodName(messageHeaders.InterfaceId, messageHeaders.MethodId);

            // Weird case, we couldn't find the method in the map. Just use the numerical id as the method name
            if (string.IsNullOrEmpty(methodName))
            {
                methodName = messageHeaders.MethodId.ToString();
            }

            // get service & operation names
            Uri operationUri = ServiceFabricHelper.MakeOperationUri(serviceContext.ServiceName, methodName);
            string operationName = ServiceFabricHelper.MakeOperationName(serviceContext.ServiceName, methodName);
            string serviceName = ServiceFabricHelper.GetServiceName(serviceContext.ServiceName);

            // create request activity
            Activity activity = new Activity(operationName);
            if (messageHeaders.TryGetHeaderValue(ServiceRemotingStrings.ParentIdHeaderName, out string parentId))
            {
                activity.SetParentId(parentId);
            }

            // get parent operation source
            messageHeaders.TryGetHeaderValue(ServiceRemotingStrings.SourceHeaderName, out string source);

            // set activity baggage
            if (messageHeaders.TryGetHeaderValue(ServiceRemotingStrings.CorrelationContextHeaderName, out byte[] correlationBytes))
            {
                var baggageBytesStream = new MemoryStream(correlationBytes, writable: false);
                var dictionaryReader = XmlDictionaryReader.CreateBinaryReader(baggageBytesStream, XmlDictionaryReaderQuotas.Max);
                var baggage = baggageSerializer.Value.ReadObject(dictionaryReader) as IEnumerable<KeyValuePair<string, string>>;
                foreach (KeyValuePair<string, string> pair in baggage)
                {
                    activity.AddBaggage(pair.Key, pair.Value);
                }
            }

            activity.Start();
            bool success = true;
            try
            {
                byte[] result = await doHandleRequest().ConfigureAwait(false);
                return result;
            }
            catch (AppException ex)
            {
                success = false;
                logger.LogError(ex, "{service} request {uri} failed", serviceName, operationUri);
                throw;
            }
            catch (Exception ex)
            {
                success = false;
                logger.LogError(ex, "{service} request {uri} failed", serviceName, operationUri);
                throw new Exception(ex.Message);
            }
            finally
            {
                // cleaning the activity
                activity.Stop();
                var requestLog = new RequestLog
                {
                    Name = activity.OperationName,
                    Type = ServiceFabricHelper.RemotingTypeName,
                    Uri = operationUri,
                    Source = source,
                    Success = success
                };
                logger.LogRequest(requestLog, activity);
            }
        }

        #region IDisposable Support

#pragma warning disable SA1201 // Elements must appear in the correct order
        private bool disposedValue = false; // To detect redundant calls
#pragma warning restore SA1201 // Elements must appear in the correct order

        /// <summary>
        /// Overridden by derived class to dispose and clean up resources used by <see cref="CorrelatingRemotingMessageHandler"/>.
        /// </summary>
        /// <param name="disposing">Whether it should dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var disposableHandler = innerHandler as IDisposable;
                    if (disposableHandler != null)
                    {
                        disposableHandler.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

#pragma warning disable SA1202 // Elements must be ordered by access
                              /// <summary>
                              /// Dispose and clean up resources used by <see cref="CorrelatingRemotingMessageHandler"/>
                              /// </summary>
        public void Dispose()
#pragma warning restore SA1202 // Elements must be ordered by access
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion // IDisposable Support
    }
}
