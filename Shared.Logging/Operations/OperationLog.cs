namespace Shared.Logging
{
    using System;

    public class OperationLog
    {
        public OperationLog(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            Name = name;

            StartTime = startTime;
            Duration = duration;

            ResponseCode = responseCode;
            Success = success;
        }

        public OperationLog()
        {
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Uri Uri { get; set; }

        public DateTimeOffset StartTime { get; set; }
        public TimeSpan Duration { get; set; }

        public string ResponseCode { get; set; }
        public bool? Success { get; set; }

        public string OperationId { get; set; }
        public string OperationName { get; set; }
        public string OperationParentId { get; set; }
    }
}
