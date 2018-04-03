namespace Shared.Logging
{
    using System;
    using System.Text;

    public sealed class DependencyLog : OperationLog
    {
        public DependencyLog(string name, string target, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
            : this(name, startTime, duration, responseCode, success)
        {
            Target = target;
        }

        public DependencyLog(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
            : this(name)
        {
            Name = name;

            StartTime = startTime;
            Duration = duration;

            ResponseCode = responseCode;
            Success = success;
        }

        public DependencyLog(string name)
        {
            Name = name;
        }

        public DependencyLog()
        {
        }

        public string Target { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder($"(Name={Name}");

            if (!string.IsNullOrEmpty(Target))
            {
                sb.AppendFormat($", Target={Target}");
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
