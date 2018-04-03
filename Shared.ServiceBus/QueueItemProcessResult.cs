namespace Shared.ServiceBus
{
    using System;

    public class QueueItemProcessResult
    {
        public static readonly QueueItemProcessResult Complete = new QueueItemProcessResult(Action.Complete);
        public static readonly QueueItemProcessResult Retry = new QueueItemProcessResult(Action.Retry);

        public QueueItemProcessResult()
        {
        }

        public QueueItemProcessResult(Action action)
        {
            ResultAction = action;
        }

        public QueueItemProcessResult(TimeSpan delay)
        {
            ResultAction = Action.Retry;
            RetryDelay = delay;
        }

        public Action ResultAction { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public enum Action
        {
            Complete,
            Retry
        }
    }
}
