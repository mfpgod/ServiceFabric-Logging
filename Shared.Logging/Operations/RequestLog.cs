namespace Shared.Logging
{
    using System;
    using System.Text;

    public sealed class RequestLog : OperationLog
    {
        public RequestLog(string name, string source, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
            : this(name, startTime, duration, responseCode, success)
        {
            Source = source;
        }

        public RequestLog(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
            : this(name)
        {
            Name = name;

            StartTime = startTime;
            Duration = duration;

            ResponseCode = responseCode;
            Success = success;
        }

        public RequestLog(string name)
        {
            Name = name;
        }

        public RequestLog()
        {
        }

        public string Source { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder($"(Name={Name}");

            if (!string.IsNullOrEmpty(Source))
            {
                sb.AppendFormat($", Source={Source}");
            }

            if (StartTime != default(DateTime))
            {
                sb.AppendFormat($", StartTime={StartTime}");
            }

            if (Duration != default(TimeSpan))
            {
                sb.AppendFormat($", Duration={StartTime}");
            }

            if (!string.IsNullOrEmpty(ResponseCode))
            {
                sb.AppendFormat($", ResponseCode={ResponseCode}");
            }

            if (Success.HasValue)
            {
                sb.AppendFormat($", Success={Success}");
            }

            sb.AppendFormat(")");

            return sb.ToString();
        }
    }
}
