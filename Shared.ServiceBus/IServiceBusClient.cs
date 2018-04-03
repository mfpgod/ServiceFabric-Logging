namespace Shared.ServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IServiceBusClient
    {
        Task SendAsync<TMessage>(TMessage messageBody, TimeSpan delay = default(TimeSpan));

        void Receive<TMessage>(Func<TMessage, IServiceBusClient, CancellationToken, Task<QueueItemProcessResult>> receiver);

        Task CloseAsync();
    }
}
