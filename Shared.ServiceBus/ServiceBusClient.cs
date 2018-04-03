namespace Shared.ServiceBus
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class ServiceBusClient : IServiceBusClient
    {
        private readonly ILogger logger = LoggingContext.CreateLogger<ServiceBusClient>();

        private readonly QueueClient queueClient;

        public ServiceBusClient(string connectionString, string queueName)
        {
            ConnectionString = connectionString;
            QueueName = queueName;
            queueClient = new QueueClient(ConnectionString, QueueName);
        }

        public string ConnectionString { get; set; }
        public string QueueName { get; set; }

        public async Task SendAsync<TMessage>(TMessage messageBody, TimeSpan delay = default(TimeSpan))
        {
            Type type = typeof(TMessage);
            string messageString = JsonConvert.SerializeObject(messageBody);
            var sbMessage = new Message(Encoding.UTF8.GetBytes(messageString))
            {
                // change to the newly formatted value
                CorrelationId = Activity.Current.Id,
                ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(delay)
            };
            sbMessage.UserProperties.Add("TypeName", type.ToString());

            await queueClient.SendAsync(sbMessage);

            logger.LogDebug(
                "Message queued: queueName={queueName}, type={type}, message=\n{message}",
                QueueName,
                typeof(TMessage).Name,
                messageString);
        }

        public void Receive<TMessage>(Func<TMessage, IServiceBusClient, CancellationToken, Task<QueueItemProcessResult>> receiver)
        {
            var messageHandlerOptions = new MessageHandlerOptions(ProcessMessageExceptionInternalAsync)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false,
        };
            queueClient.RegisterMessageHandler(async (message, token) => { await ProcessMessageInternalAsync(message, receiver, token); }, messageHandlerOptions);
        }

        public async Task CloseAsync()
        {
            if (queueClient != null)
            {
                await queueClient.CloseAsync();
            }
        }

        private async Task ProcessMessageInternalAsync<TMessage>(Message message, Func<TMessage, IServiceBusClient, CancellationToken, Task<QueueItemProcessResult>> receiver, CancellationToken token)
        {
            Activity activity = null;

            // if Activity.Current is not empty, it means that Service Bus library restored it.
            // It happens when App Insights DependencyTrackingTelemetryModule is configured with 'Microsoft.Azure.ServiceBus'
            // <IncludeDiagnosticSourceActivities>
            //   <Add>Microsoft.Azure.ServiceBus</Add>
            // </IncludeDiagnosticSourceActivities>
            if (Activity.Current == null)
            {
                activity = new Activity("Process");
                if (message.CorrelationId != null)
                {
                    activity.SetParentId(message.CorrelationId);
                }
            }

            if (activity != null)
            {
                activity.Start();
            }

            string messageString = null;
            try
            {
                messageString = Encoding.UTF8.GetString(message.Body);
                TMessage tMessage = JsonConvert.DeserializeObject<TMessage>(messageString);
                QueueItemProcessResult result = await receiver(tMessage, this, token);
                switch (result.ResultAction)
                {
                    case QueueItemProcessResult.Action.Complete:
                        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                        break;
                    case QueueItemProcessResult.Action.Retry:
                        if (result.RetryDelay != default(TimeSpan))
                        {
                            message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(result.RetryDelay);
                        }

                        await queueClient.AbandonAsync(message.SystemProperties.LockToken);
                        break;
                }

                logger.LogDebug(
                    "Message processing completed: queueName={queueName}, type={type}, messageId={messageId}, action={action}, message=\n{message}",
                    QueueName,
                    typeof(TMessage).Name,
                    message.MessageId,
                    result.ResultAction,
                    messageString);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Message processing failed: queueName={queueName}, type={type}, id={id}, message=\n{message}",
                    QueueName,
                    typeof(TMessage).Name,
                    message.MessageId,
                    messageString);
            }
            finally
            {
                activity?.Stop();
            }
        }

        private async Task ProcessMessageExceptionInternalAsync(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            logger.LogError(exceptionReceivedEventArgs.Exception, "Message processing internal exception: queueName={queueName}", QueueName);
            await Task.CompletedTask;
        }
    }
}
