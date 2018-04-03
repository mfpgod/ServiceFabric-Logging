namespace Services.Contracts.Adapters.ServiceBusDependency
{
    using System;

    [Serializable]
    public class QueueRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
